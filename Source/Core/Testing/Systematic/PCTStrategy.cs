// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Coyote.IO;
using Microsoft.Coyote.Runtime;
using Microsoft.Coyote.Specifications;

namespace Microsoft.Coyote.Testing.Systematic
{
    /// <summary>
    /// A probabilistic priority-based scheduling strategy.
    /// </summary>
    /// <remarks>
    /// This strategy is described in the following paper:
    /// https://www.microsoft.com/en-us/research/wp-content/uploads/2016/02/asplos277-pct.pdf.
    /// </remarks>
    internal sealed class PCTStrategy : SystematicStrategy
    {
        /// <summary>
        /// Random value generator.
        /// </summary>
        private readonly IRandomValueGenerator RandomValueGenerator;

        /// <summary>
        /// The maximum number of steps to explore.
        /// </summary>
        private readonly int MaxSteps;

        /// <summary>
        /// The number of exploration steps.
        /// </summary>
        private int StepCount;

        /// <summary>
        /// Max number of priority switch points.
        /// </summary>
        private readonly int MaxPrioritySwitchPoints;

        /// <summary>
        /// Approximate length of the schedule across all iterations.
        /// </summary>
        private int ScheduleLength;

        /// <summary>
        /// List of prioritized operations.
        /// </summary>
        private readonly List<AsyncOperation> PrioritizedOperations;

        /// <summary>
        /// Scheduling points in the current execution where a priority change should occur.
        /// </summary>
        private readonly HashSet<int> PriorityChangePoints;

        private int ActualNumberOfPrioritySwitches = 0;

        private readonly HashSet<AsyncOperation> registeredOps;

        private int ContextSwitchNumber;

        /// <summary>
        /// Initializes a new instance of the <see cref="PCTStrategy"/> class.
        /// </summary>
        internal PCTStrategy(int maxSteps, int maxPrioritySwitchPoints, IRandomValueGenerator generator)
        {
            this.ActualNumberOfPrioritySwitches = 0;
            this.RandomValueGenerator = generator;
            this.MaxSteps = maxSteps;
            this.StepCount = 0;
            this.ScheduleLength = 0;
            this.MaxPrioritySwitchPoints = maxPrioritySwitchPoints;
            this.PrioritizedOperations = new List<AsyncOperation>();
            this.PriorityChangePoints = new HashSet<int>();
            this.registeredOps = new HashSet<AsyncOperation>();
            this.ContextSwitchNumber = 0;
        }

        internal void PrintTaskPCTStatsForIteration(uint iteration)
        {
            Console.WriteLine(string.Empty);
            Console.WriteLine($"===========<IMP_TaskPCTStrategy> [PrintTaskPCTStatsForIteration] TASK-PCT STATS for ITERATION: {iteration}");
            Console.WriteLine($"                  TOTAL ASYNC OPS (#PRIORITIES):: {this.PrioritizedOperations.Count}");
            Console.WriteLine($"                  #PRIORITY_SWITCHES: {this.ActualNumberOfPrioritySwitches}");
            this.DebugPrintOperationPriorityList();
        }

        /// <inheritdoc/>
        internal override bool InitializeNextIteration(uint iteration)
        {
            // The first iteration has no knowledge of the execution, so only initialize from the second
            // iteration and onwards. Note that although we could initialize the first length based on a
            // heuristic, its not worth it, as the strategy will typically explore thousands of iterations,
            // plus its also interesting to explore a schedule with no forced priority switch points.
            if (iteration > 0)
            {
                this.ActualNumberOfPrioritySwitches = 0;

                // FN_TODO: print the stat for the last iteration also
                this.PrintTaskPCTStatsForIteration(iteration - 1);

                this.ScheduleLength = Math.Max(this.ScheduleLength, this.StepCount);
                this.StepCount = 0;

                this.PrioritizedOperations.Clear();
                this.PriorityChangePoints.Clear();

                var range = Enumerable.Range(0, this.ScheduleLength);
                foreach (int point in this.Shuffle(range).Take(this.MaxPrioritySwitchPoints))
                {
                    this.PriorityChangePoints.Add(point);
                }

                this.DebugPrintPriorityChangePoints();
                this.registeredOps.Clear();
                this.ContextSwitchNumber = 0;
            }

            return true;
        }

        private void DebugPrintBeforeGetNextOperation(IEnumerable<AsyncOperation> opss)
        {
            this.ContextSwitchNumber += 1;
            var ops = opss.ToList();
            IO.Debug.WriteLine($"          ops.Count = {ops.Count}");
            int countt = 0;
            foreach (var op in ops)
            {
                if (countt == 0)
                {
                    IO.Debug.Write($"          {op}");
                }
                else
                {
                    IO.Debug.Write($", {op}");
                }

                countt++;
            }

            IO.Debug.WriteLine(string.Empty);

            countt = 0;
            foreach (var op in ops)
            {
                if (countt == 0)
                {
                    IO.Debug.Write($"          {op.Status}");
                }
                else
                {
                    IO.Debug.Write($", {op.Status}");
                }

                countt++;
            }

            IO.Debug.WriteLine(string.Empty);

            countt = 0;
            foreach (var op in ops)
            {
                if (countt == 0)
                {
                    IO.Debug.Write($"          {op.Type}");
                }
                else
                {
                    IO.Debug.Write($", {op.Type}");
                }

                countt++;
            }

            IO.Debug.WriteLine(string.Empty);

            HashSet<AsyncOperation> newConcurrentOps = new HashSet<AsyncOperation>();
            foreach (var op in ops)
            {
                if (!this.registeredOps.Contains(op))
                {
                    newConcurrentOps.Add(op);
                    this.registeredOps.Add(op);
                }
            }

            IO.Debug.WriteLine($"          # new operations added {newConcurrentOps.Count}");
            // Specification.Assert((newConcurrentOps.Count <= 1) || (newConcurrentOps.Count == 2 && this.ContextSwitchNumber == 1),
            //     $"     <TaskSummaryLog-ERROR> At most one new operation must be added across context switch.");
            if (!((newConcurrentOps.Count <= 1) || (newConcurrentOps.Count == 2 && this.ContextSwitchNumber == 1)))
            {
                Console.WriteLine($"     <TaskSummaryLog-ERROR> At most one new operation must be added across context switch.");
            }

            int cases = 0;

            if (newConcurrentOps.Count == 0)
            {
                Console.WriteLine($"     <TaskSummaryLog> T-case 1.): No new task added.");
                cases = 1;
            }

            foreach (var op in newConcurrentOps)
            {
                IO.Debug.WriteLine($"          newConcurrentOps: {op}, Spawner: {op.ParentTask}");
                if (op.IsContinuationTask)
                {
                    if (op.ParentTask == null)
                    {
                        Console.WriteLine($"     <TaskSummaryLog> T-case 3.): Continuation task {op} (id = {op.Id}) is the first task to be created!");
                    }
                    else
                    {
                        Console.WriteLine($"     <TaskSummaryLog> T-case 3.): Continuation task {op} (id = {op.Id}) created by {op.ParentTask} (id = {op.ParentTask.Id}).");
                    }

                    cases = 3;
                }
                else
                {
                    if (op.ParentTask == null)
                    {
                        Console.WriteLine($"     <TaskSummaryLog> T-case 2.): Spawn task {op} (id = {op.Id}) is the first task to be created!");
                    }
                    else
                    {
                        Console.WriteLine($"     <TaskSummaryLog> T-case 2.): Spawn task {op} (id = {op.Id}) created by {op.ParentTask} (id = {op.ParentTask.Id}).");
                    }

                    cases = 2;
                }
            }

            // Specification.Assert( (cases == 1) || (cases == 2) || (cases == 3),
            //     $"     <TaskSummaryLog-ERROR> At most one new operation must be added across context switch.");
            if (!((cases == 1) || (cases == 2) || (cases == 3)))
            {
                Console.WriteLine($"     <TaskSummaryLog-ERROR> At most one new operation must be added across context switch.");
            }

            // IO.Debug.WriteLine(string.Empty);
        }

        private static void DebugPrintAfterGetNextOperation(AsyncOperation next)
        {
            IO.Debug.WriteLine($"          next = {next}");
            Console.WriteLine($"     <TaskSummaryLog> Scheduled: {next}");
            // IO.Debug.WriteLine();
            // IO.Debug.WriteLine();
            // IO.Debug.WriteLine();
            // IO.Debug.WriteLine();
            // IO.Debug.WriteLine();
        }

        /// <inheritdoc/>
        internal override bool GetNextOperation(IEnumerable<AsyncOperation> ops, AsyncOperation current,
            bool isYielding, out AsyncOperation next)
        {
            this.DebugPrintBeforeGetNextOperation(ops);
            next = null;
            var enabledOps = ops.Where(op => op.Status is AsyncOperationStatus.Enabled).ToList();
            if (enabledOps.Count is 0)
            {
                return false;
            }

            this.SetNewOperationPriorities(enabledOps, current);
            this.DeprioritizeEnabledOperationWithHighestPriority(enabledOps, current, isYielding);
            this.DebugPrintOperationPriorityList();

            AsyncOperation highestEnabledOperation = this.GetEnabledOperationWithHighestPriority(enabledOps);
            next = enabledOps.First(op => op.Equals(highestEnabledOperation));
            this.StepCount++;
            DebugPrintAfterGetNextOperation(next);
            return true;
        }

        /// <summary>
        /// Sets the priority of new operations, if there are any.
        /// </summary>
        private void SetNewOperationPriorities(List<AsyncOperation> ops, AsyncOperation current)
        {
            if (this.PrioritizedOperations.Count is 0)
            {
                this.PrioritizedOperations.Add(current);
            }

            // Randomize the priority of all new operations.
            foreach (var op in ops.Where(op => !this.PrioritizedOperations.Contains(op)))
            {
                // Randomly choose a priority for this operation.
                int index = this.RandomValueGenerator.Next(this.PrioritizedOperations.Count) + 1;
                this.PrioritizedOperations.Insert(index, op);
                Debug.WriteLine("<PCTLog> chose priority '{0}' for new operation '{1}'.", index, op.Name);
            }
        }

        /// <summary>
        /// Deprioritizes the enabled operation with the highest priority, if there is a
        /// priotity change point installed on the current execution step.
        /// </summary>
        private void DeprioritizeEnabledOperationWithHighestPriority(List<AsyncOperation> ops, AsyncOperation current, bool isYielding)
        {
            if (ops.Count <= 1)
            {
                // Nothing to do, there is only one enabled operation available.
                return;
            }

            AsyncOperation deprioritizedOperation = null;
            if (this.PriorityChangePoints.Contains(this.StepCount))
            {
                // This scheduling step was chosen as a priority switch point.
                deprioritizedOperation = this.GetEnabledOperationWithHighestPriority(ops);
                Debug.WriteLine("<PCTLog> operation '{0}' is deprioritized.", deprioritizedOperation.Name);
            }
            else if (isYielding)
            {
                // The current operation is yielding its execution to the next prioritized operation.
                deprioritizedOperation = current;
                Debug.WriteLine("<PCTLog> operation '{0}' yields its priority.", deprioritizedOperation.Name);
            }

            if (deprioritizedOperation != null)
            {
                this.ActualNumberOfPrioritySwitches++;
                // Deprioritize the operation by putting it in the end of the list.
                this.PrioritizedOperations.Remove(deprioritizedOperation);
                this.PrioritizedOperations.Add(deprioritizedOperation);
            }
        }

        /// <summary>
        /// Returns the enabled operation with the highest priority.
        /// </summary>
        private AsyncOperation GetEnabledOperationWithHighestPriority(List<AsyncOperation> ops)
        {
            foreach (var entity in this.PrioritizedOperations)
            {
                if (ops.Any(m => m == entity))
                {
                    return entity;
                }
            }

            return null;
        }

        /// <inheritdoc/>
        internal override bool GetNextBooleanChoice(AsyncOperation current, int maxValue, out bool next)
        {
            next = false;
            if (this.RandomValueGenerator.Next(maxValue) is 0)
            {
                next = true;
            }

            this.StepCount++;
            return true;
        }

        /// <inheritdoc/>
        internal override bool GetNextIntegerChoice(AsyncOperation current, int maxValue, out int next)
        {
            next = this.RandomValueGenerator.Next(maxValue);
            this.StepCount++;
            return true;
        }

        /// <inheritdoc/>
        internal override int GetStepCount() => this.StepCount;

        /// <inheritdoc/>
        internal override bool IsMaxStepsReached()
        {
            if (this.MaxSteps is 0)
            {
                return false;
            }

            return this.StepCount >= this.MaxSteps;
        }

        /// <inheritdoc/>
        internal override bool IsFair() => false;

        /// <inheritdoc/>
        internal override string GetDescription()
        {
            var text = $"pct[seed '" + this.RandomValueGenerator.Seed + "']";
            return text;
        }

        /// <summary>
        /// Shuffles the specified range using the Fisher-Yates algorithm.
        /// </summary>
        /// <remarks>
        /// See https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle.
        /// </remarks>
        private IList<int> Shuffle(IEnumerable<int> range)
        {
            var result = new List<int>(range);
            for (int idx = result.Count - 1; idx >= 1; idx--)
            {
                int point = this.RandomValueGenerator.Next(result.Count);
                int temp = result[idx];
                result[idx] = result[point];
                result[point] = temp;
            }

            return result;
        }

        /// <inheritdoc/>
        internal override void Reset()
        {
            this.ScheduleLength = 0;
            this.StepCount = 0;
            this.PrioritizedOperations.Clear();
            this.PriorityChangePoints.Clear();
        }

        /// <summary>
        /// Print the operation priority list, if debug is enabled.
        /// </summary>
        private void DebugPrintOperationPriorityList()
        {
            if (Debug.IsEnabled)
            {
                Debug.Write("<PCTLog> operation priority list: ");
                for (int idx = 0; idx < this.PrioritizedOperations.Count; idx++)
                {
                    if (idx < this.PrioritizedOperations.Count - 1)
                    {
                        Debug.Write("'{0}', ", this.PrioritizedOperations[idx].Name);
                    }
                    else
                    {
                        Debug.WriteLine("'{0}'.", this.PrioritizedOperations[idx].Name);
                    }
                }
            }
        }

        /// <summary>
        /// Print the priority change points, if debug is enabled.
        /// </summary>
        private void DebugPrintPriorityChangePoints()
        {
            if (Debug.IsEnabled)
            {
                // Sort them before printing for readability.
                var sortedChangePoints = this.PriorityChangePoints.ToArray();
                Array.Sort(sortedChangePoints);
                Debug.WriteLine("<PCTLog> next priority change points ('{0}' in total): {1}",
                    sortedChangePoints.Length, string.Join(", ", sortedChangePoints));
            }
        }
    }
}
