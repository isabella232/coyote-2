﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
// using System.Collections.Generic;

namespace Microsoft.Coyote.Runtime
{
    /// <summary>
    /// Represents an abstract asynchronous operation that can be controlled during systematic testing.
    /// </summary>
    internal abstract class AsyncOperation : IEquatable<AsyncOperation>
    {
        public AsyncOperation ParentTask = null;

        public bool IsContinuationTask = false;

        // TODO: make it internal or private
        // public AsyncOperation Spawner = null;

        // TODO: make it internal or private, initialize properly inside a constructor
        // public List<AsyncOperation> Spawnees = new List<AsyncOperation>();

        // public int SpawnChainNumber = 0;

        // public int DepthInSpawnChain = 0;

        /// <summary>
        /// The unique id of the operation.
        /// </summary>
        internal ulong Id { get; }

        /// <summary>
        /// The unique name of the operation.
        /// </summary>
        internal string Name { get; }

        /// <summary>
        /// The status of the operation. An operation can be scheduled only
        /// if it is <see cref="AsyncOperationStatus.Enabled"/>.
        /// </summary>
        internal AsyncOperationStatus Status;

        /// <summary>
        /// The type of the operation.
        /// </summary>
        internal AsyncOperationType Type;

        /// <summary>
        /// A value that represents the hashed program state when
        /// this operation last executed.
        /// </summary>
        internal int HashedProgramState;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncOperation"/> class.
        /// </summary>
        internal AsyncOperation(ulong operationId, string name)
        {
            this.Id = operationId;
            this.Name = name;
            this.Status = AsyncOperationStatus.None;
            this.Type = AsyncOperationType.Start;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj is AsyncOperation op)
            {
                return this.Id == op.Id;
            }

            return false;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        public override int GetHashCode() => this.Id.GetHashCode();

        /// <summary>
        /// Returns a string that represents the current actor id.
        /// </summary>
        public override string ToString() => this.Name;

        /// <summary>
        /// Indicates whether the specified <see cref="AsyncOperation"/> is equal
        /// to the current <see cref="AsyncOperation"/>.
        /// </summary>
        public bool Equals(AsyncOperation other) => this.Equals((object)other);

        /// <summary>
        /// Indicates whether the specified <see cref="AsyncOperation"/> is equal
        /// to the current <see cref="AsyncOperation"/>.
        /// </summary>
        bool IEquatable<AsyncOperation>.Equals(AsyncOperation other) => this.Equals(other);
    }
}
