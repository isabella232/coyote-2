﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Coyote.Runtime;

namespace Microsoft.Coyote.Testing.Interleaving
{
    /// <summary>
    /// A probabilistic scheduling strategy that uses Q-learning.
    /// </summary>
    /// <remarks>
    /// This strategy is described in the following paper:
    /// https://dl.acm.org/doi/10.1145/3428298.
    /// </remarks>
    internal sealed class QLearningStrategy : RandomStrategy
    {
        /// <summary>
        /// Map from program states to a map from next operations to their quality values.
        /// </summary>
        private readonly Dictionary<int, Dictionary<ulong, double>> OperationQTable;

        /// <summary>
        /// The path that is being executed during the current iteration. Each
        /// step of the execution is represented by an operation and a value
        /// representing the program state after the operation executed.
        /// </summary>
        private readonly LinkedList<(ulong op, SchedulingPointType sp, int state)> ExecutionPath;

        /// <summary>
        /// Map from values representing program states to their transition
        /// frequency in the current execution path.
        /// </summary>
        private readonly Dictionary<int, ulong> TransitionFrequencies;

        /// <summary>
        /// The last chosen operation.
        /// </summary>
        private ulong LastOperation;

        /// <summary>
        /// The value of the learning rate.
        /// </summary>
        private readonly double LearningRate;

        /// <summary>
        /// The value of the discount factor.
        /// </summary>
        private readonly double Gamma;

        /// <summary>
        /// The op value denoting a true boolean choice.
        /// </summary>
        private readonly ulong TrueChoiceOpValue;

        /// <summary>
        /// The op value denoting a false boolean choice.
        /// </summary>
        private readonly ulong FalseChoiceOpValue;

        /// <summary>
        /// The op value denoting the min integer choice.
        /// </summary>
        private readonly ulong MinIntegerChoiceOpValue;

        /// <summary>
        /// The basic action reward.
        /// </summary>
        private readonly double BasicActionReward;

        /// <summary>
        /// The failure injection reward.
        /// </summary>
        private readonly double FailureInjectionReward;

        /// <summary>
        /// The number of explored executions.
        /// </summary>
        private int Epochs;

        /// <summary>
        /// Initializes a new instance of the <see cref="QLearningStrategy"/> class.
        /// It uses the specified random number generator.
        /// </summary>
        public QLearningStrategy(Configuration configuration, IRandomValueGenerator generator)
            : base(configuration, generator, false)
        {
            this.OperationQTable = new Dictionary<int, Dictionary<ulong, double>>();
            this.ExecutionPath = new LinkedList<(ulong, SchedulingPointType, int)>();
            this.TransitionFrequencies = new Dictionary<int, ulong>();
            this.LastOperation = 0;
            this.LearningRate = 0.3;
            this.Gamma = 0.7;
            this.TrueChoiceOpValue = ulong.MaxValue;
            this.FalseChoiceOpValue = ulong.MaxValue - 1;
            this.MinIntegerChoiceOpValue = ulong.MaxValue - 2;
            this.BasicActionReward = -1;
            this.FailureInjectionReward = -1000;
            this.Epochs = 0;
        }

        /// <inheritdoc/>
        internal override bool GetNextOperation(IEnumerable<ControlledOperation> ops, ControlledOperation current,
            bool isYielding, out ControlledOperation next)
        {
            int state = this.CaptureExecutionStep(current);
            this.InitializeOperationQValues(state, ops);

            next = this.GetNextOperationByPolicy(state, ops);
            this.LastOperation = next.Id;

            this.StepCount++;
            return true;
        }

        /// <inheritdoc/>
        internal override bool GetNextBooleanChoice(ControlledOperation current, int maxValue, out bool next)
        {
            int state = this.CaptureExecutionStep(current);
            this.InitializeBooleanChoiceQValues(state);

            next = this.GetNextBooleanChoiceByPolicy(state);

            this.LastOperation = next ? this.TrueChoiceOpValue : this.FalseChoiceOpValue;
            this.StepCount++;
            return true;
        }

        /// <inheritdoc/>
        internal override bool GetNextIntegerChoice(ControlledOperation current, int maxValue, out int next)
        {
            int state = this.CaptureExecutionStep(current);
            this.InitializeIntegerChoiceQValues(state, maxValue);

            next = this.GetNextIntegerChoiceByPolicy(state, maxValue);

            this.LastOperation = this.MinIntegerChoiceOpValue - (ulong)next;
            this.StepCount++;
            return true;
        }

        /// <summary>
        /// Returns the next operation to schedule by drawing from the probability
        /// distribution over the specified state and enabled operations.
        /// </summary>
        private ControlledOperation GetNextOperationByPolicy(int state, IEnumerable<ControlledOperation> ops)
        {
            var opIds = new List<ulong>();
            var qValues = new List<double>();
            foreach (var pair in this.OperationQTable[state])
            {
                if (ops.Any(op => op.Id == pair.Key))
                {
                    opIds.Add(pair.Key);
                    qValues.Add(pair.Value);
                }
            }

            int idx = this.ChooseQValueIndexFromDistribution(qValues);
            return ops.FirstOrDefault(op => op.Id == opIds[idx]);
        }

        /// <summary>
        /// Returns the next boolean choice by drawing from the probability
        /// distribution over the specified state and boolean choices.
        /// </summary>
        private bool GetNextBooleanChoiceByPolicy(int state)
        {
            double trueQValue = this.OperationQTable[state][this.TrueChoiceOpValue];
            double falseQValue = this.OperationQTable[state][this.FalseChoiceOpValue];

            var qValues = new List<double>(2)
            {
                trueQValue,
                falseQValue
            };

            int idx = this.ChooseQValueIndexFromDistribution(qValues);
            return idx == 0 ? true : false;
        }

        /// <summary>
        /// Returns the next integer choice by drawing from the probability
        /// distribution over the specified state and integer choices.
        /// </summary>
        private int GetNextIntegerChoiceByPolicy(int state, int maxValue)
        {
            var qValues = new List<double>(maxValue);
            for (ulong i = 0; i < (ulong)maxValue; i++)
            {
                qValues.Add(this.OperationQTable[state][this.MinIntegerChoiceOpValue - i]);
            }

            return this.ChooseQValueIndexFromDistribution(qValues);
        }

        /// <summary>
        /// Returns an index of a Q value by drawing from the probability distribution
        /// over the specified Q values.
        /// </summary>
        private int ChooseQValueIndexFromDistribution(List<double> qValues)
        {
            double sum = 0;
            for (int i = 0; i < qValues.Count; i++)
            {
                qValues[i] = Math.Exp(qValues[i]);
                sum += qValues[i];
            }

            for (int i = 0; i < qValues.Count; i++)
            {
                qValues[i] /= sum;
            }

            sum = 0;

            // First, change the shape of the distribution probability array to be cumulative.
            // For example, instead of [0.1, 0.2, 0.3, 0.4], we get [0.1, 0.3, 0.6, 1.0].
            var cumulative = qValues.Select(c =>
            {
                var result = c + sum;
                sum += c;
                return result;
            }).ToList();

            // Generate a random double value between 0.0 to 1.0.
            var rvalue = this.RandomValueGenerator.NextDouble();

            // Find the first index in the cumulative array that is greater
            // or equal than the generated random value.
            var idx = cumulative.BinarySearch(rvalue);

            if (idx < 0)
            {
                // If an exact match is not found, List.BinarySearch will return the index
                // of the first items greater than the passed value, but in a specific form
                // (negative) we need to apply ~ to this negative value to get real index.
                idx = ~idx;
            }

            if (idx > cumulative.Count - 1)
            {
                // Very rare case when probabilities do not sum to 1 because of
                // double precision issues (so sum is 0.999943 and so on).
                idx = cumulative.Count - 1;
            }

            return idx;
        }

        /// <summary>
        /// Captures metadata related to the current execution step, and returns
        /// a value representing the current program state.
        /// </summary>
        private int CaptureExecutionStep(ControlledOperation current)
        {
            int state = current.LastHashedProgramState;

            // Update the execution path with the current state.
            this.ExecutionPath.AddLast((this.LastOperation, current.LastSchedulingPoint, state));

            if (!this.TransitionFrequencies.ContainsKey(state))
            {
                this.TransitionFrequencies.Add(state, 0);
            }

            // Increment the state transition frequency.
            this.TransitionFrequencies[state]++;

            return state;
        }

        /// <summary>
        /// Initializes the Q values of all operations that can be chosen at the
        /// specified state that have not been previously encountered.
        /// </summary>
        private void InitializeOperationQValues(int state, IEnumerable<ControlledOperation> ops)
        {
            if (!this.OperationQTable.TryGetValue(state, out Dictionary<ulong, double> qValues))
            {
                qValues = new Dictionary<ulong, double>();
                this.OperationQTable.Add(state, qValues);
            }

            foreach (var op in ops)
            {
                // Assign the same initial probability for all new operations.
                if (!qValues.ContainsKey(op.Id))
                {
                    qValues.Add(op.Id, 0);
                }
            }
        }

        /// <summary>
        /// Initializes the Q values of all boolean choices that can be chosen
        /// at the specified state that have not been previously encountered.
        /// </summary>
        private void InitializeBooleanChoiceQValues(int state)
        {
            if (!this.OperationQTable.TryGetValue(state, out Dictionary<ulong, double> qValues))
            {
                qValues = new Dictionary<ulong, double>();
                this.OperationQTable.Add(state, qValues);
            }

            if (!qValues.ContainsKey(this.TrueChoiceOpValue))
            {
                qValues.Add(this.TrueChoiceOpValue, 0);
            }

            if (!qValues.ContainsKey(this.FalseChoiceOpValue))
            {
                qValues.Add(this.FalseChoiceOpValue, 0);
            }
        }

        /// <summary>
        /// Initializes the Q values of all integer choices that can be chosen
        /// at the specified state that have not been previously encountered.
        /// </summary>
        private void InitializeIntegerChoiceQValues(int state, int maxValue)
        {
            if (!this.OperationQTable.TryGetValue(state, out Dictionary<ulong, double> qValues))
            {
                qValues = new Dictionary<ulong, double>();
                this.OperationQTable.Add(state, qValues);
            }

            for (ulong i = 0; i < (ulong)maxValue; i++)
            {
                ulong opValue = this.MinIntegerChoiceOpValue - i;
                if (!qValues.ContainsKey(opValue))
                {
                    qValues.Add(opValue, 0);
                }
            }
        }

        /// <inheritdoc/>
        internal override bool InitializeNextIteration(uint iteration)
        {
            this.LearnQValues();
            this.ExecutionPath.Clear();
            this.LastOperation = 0;
            this.Epochs++;

            return base.InitializeNextIteration(iteration);
        }

        /// <summary>
        /// Learn Q values using data from the current execution.
        /// </summary>
        private void LearnQValues()
        {
            int idx = 0;
            var node = this.ExecutionPath.First;
            while (node?.Next != null)
            {
                var (_, _, state) = node.Value;
                var (nextOp, nextSp, nextState) = node.Next.Value;

                // Compute the max Q value.
                double maxQ = double.MinValue;
                foreach (var nextOpQValuePair in this.OperationQTable[nextState])
                {
                    if (nextOpQValuePair.Value > maxQ)
                    {
                        maxQ = nextOpQValuePair.Value;
                    }
                }

                // Compute the reward. Program states that are visited with higher frequency result into lesser rewards.
                var freq = this.TransitionFrequencies[nextState];
                double reward = (nextSp == SchedulingPointType.InjectFailure ?
                    this.FailureInjectionReward : this.BasicActionReward) * freq;
                if (reward > 0)
                {
                    // The reward has underflowed.
                    reward = double.MinValue;
                }

                // Get the operations that are available from the current execution step.
                var currOpQValues = this.OperationQTable[state];
                if (!currOpQValues.ContainsKey(nextOp))
                {
                    currOpQValues.Add(nextOp, 0);
                }

                // Update the Q value of the next operation.
                // Q = [(1-a) * Q]  +  [a * (rt + (g * maxQ))]
                currOpQValues[nextOp] = ((1 - this.LearningRate) * currOpQValues[nextOp]) +
                    (this.LearningRate * (reward + (this.Gamma * maxQ)));

                node = node.Next;
                idx++;
            }
        }

        /// <inheritdoc/>
        internal override string GetDescription() => $"rl[seed:{this.RandomValueGenerator.Seed}]";
    }
}
