﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using SystemCompiler = System.Runtime.CompilerServices;

namespace Microsoft.Coyote.Runtime.CompilerServices
{
    /// <summary>
    /// Represents a builder for asynchronous methods that return a controlled task.
    /// This type is intended for compiler use only.
    /// </summary>
    /// <remarks>This type is intended for compiler use rather than use directly in code.</remarks>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    [StructLayout(LayoutKind.Auto)]
    public struct AsyncVoidMethodBuilder
    {
        /// <summary>
        /// Responsible for controlling the execution of tasks during systematic testing.
        /// </summary>
        private readonly CoyoteRuntime Runtime;

        /// <summary>
        /// The task builder to which most operations are delegated.
        /// </summary>
#pragma warning disable IDE0044 // Add readonly modifier
        private SystemCompiler.AsyncVoidMethodBuilder MethodBuilder;
#pragma warning restore IDE0044 // Add readonly modifier
        private ControlledOperation ParentOperation;

        // FN_ISSUE: Difference from the AsyncTaskMethodBuilder.
        // / <summary>
        // / Gets the task for this builder.
        // / </summary>
        // public Task Task => this.MethodBuilder.Task;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncVoidMethodBuilder"/> struct.
        /// </summary>
        private AsyncVoidMethodBuilder(CoyoteRuntime runtime)
        {
            this.Runtime = runtime;
            this.MethodBuilder = default;
            this.ParentOperation = CoyoteRuntime.GiveExecutingOperation();
            if (this.ParentOperation == null)
            {
                this.Runtime?.OnAsyncStateMachineStart(true);
                Console.WriteLine($"===========<F_AsyncBuilder-Error> [Constructor] this.ParentOperation: Null, setting parent missed, thread {Thread.CurrentThread.ManagedThreadId}.");
            }
            else
            {
                this.Runtime?.OnAsyncStateMachineStart(false);
                IO.Debug.WriteLine($"===========<F_AsyncBuilder> [Constructor] this.ParentOperation: {this.ParentOperation}, thread {Thread.CurrentThread.ManagedThreadId}.");
            }
        }

        /// <summary>
        /// Creates an instance of the <see cref="AsyncVoidMethodBuilder"/> struct.
        /// </summary>
        public static AsyncVoidMethodBuilder Create()
        {
            RuntimeProvider.TryGetFromSynchronizationContext(out CoyoteRuntime runtime);
            return new AsyncVoidMethodBuilder(runtime);
        }

        /// <summary>
        /// Begins running the builder with the associated state machine.
        /// </summary>
        [DebuggerStepThrough]
        public void Start<TStateMachine>(ref TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine
        {
            IO.Debug.WriteLine($"===========<F_AsyncBuilder> [Start] thread: {Thread.CurrentThread.ManagedThreadId}, Task: {Task.CurrentId}.");
            this.ParentOperation = CoyoteRuntime.GiveExecutingOperation();
            if (this.ParentOperation == null)
            {
                this.Runtime?.OnAsyncStateMachineStart(true);
                Console.WriteLine($"===========<F_AsyncBuilder-Error> [Start] ParentOperation=Null, setting parent missed stateMachine.ToString(): {stateMachine.ToString()} thread {Thread.CurrentThread.ManagedThreadId}.");
            }
            else
            {
                this.Runtime?.OnAsyncStateMachineStart(false);
                IO.Debug.WriteLine($"===========<F_AsyncBuilder> [Start] thread {Thread.CurrentThread.ManagedThreadId}.");
            }

            IO.Debug.WriteLine("<AsyncBuilder> Started state machine on runtime '{0}' and thread '{1}'.",
                this.Runtime?.Id, Thread.CurrentThread.ManagedThreadId);
            this.MethodBuilder.Start(ref stateMachine);
        }

        /// <summary>
        /// Associates the builder with the specified state machine.
        /// </summary>
        public void SetStateMachine(IAsyncStateMachine stateMachine) =>
            this.MethodBuilder.SetStateMachine(stateMachine);

        /// <summary>
        /// Marks the task as successfully completed.
        /// </summary>
        public void SetResult()
        {
            // FN_ISSUE: Difference from the AsyncTaskMethodBuilder.
            // IO.Debug.WriteLine("<AsyncBuilder> Set state machine task '{0}' from thread '{1}'.",
            //     this.MethodBuilder.Task.Id, Thread.CurrentThread.ManagedThreadId);
            this.MethodBuilder.SetResult();
        }

        /// <summary>
        /// Callback to AsyncVoidMethodBuilder before MoveNext method for IAsyncStateMachine class at the IL level.
        /// </summary>
        [DebuggerHidden]
        public void OnMoveNext()
        {
            IO.Debug.WriteLine($"===========<F_AsyncBuilder> [onMoveNext] ParentOperation: {this.ParentOperation}, thread: {Thread.CurrentThread.ManagedThreadId}, Task: {Task.CurrentId}.");
            this.Runtime?.SetParentOnMoveNext(this.ParentOperation);
        }

        /// <summary>
        /// Marks the task as failed and binds the specified exception to the task.
        /// </summary>
        public void SetException(Exception exception) => this.MethodBuilder.SetException(exception);

        /// <summary>
        /// Schedules the state machine to proceed to the next action when the specified awaiter completes.
        /// </summary>
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            this.MethodBuilder.AwaitOnCompleted(ref awaiter, ref stateMachine);
            if (this.Runtime != null && awaiter is IControllableAwaiter controllableAwaiter &&
                controllableAwaiter.IsControlled)
            {
                // FN_ISSUE: Difference from the AsyncTaskMethodBuilder.
                this.AssignStateMachineTask(Task.CompletedTask);
            }
        }

        /// <summary>
        /// Schedules the state machine to proceed to the next action when the specified awaiter completes.
        /// </summary>
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            IO.Debug.WriteLine($"===========<F_AsyncBuilder> [AwaitUnsafeOnCompleted] thread: {Thread.CurrentThread.ManagedThreadId}, Task: {Task.CurrentId}.");
            this.MethodBuilder.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine);
            if (this.Runtime != null && awaiter is IControllableAwaiter controllableAwaiter &&
                controllableAwaiter.IsControlled)
            {
                // FN_ISSUE: Difference from the AsyncTaskMethodBuilder.
                this.AssignStateMachineTask(Task.CompletedTask);
            }
        }

        /// <summary>
        /// Assigns the state machine task with the runtime.
        /// </summary>
        private void AssignStateMachineTask(Task builderTask)
        {
            IO.Debug.WriteLine("<AsyncBuilder> Assigned state machine task '{0}' from thread '{1}'.",
                builderTask.Id, Thread.CurrentThread.ManagedThreadId);
            this.Runtime.OnAsyncStateMachineScheduleMoveNext(builderTask);
        }

        // FN_ISSUE: Difference from the AsyncTaskMethodBuilder.
        // TODO: Write code for the generic version also.
    }
}
