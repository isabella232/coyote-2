﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Coyote.Testing
{
    /// <summary>
    /// Interface of a test report.
    /// </summary>
    internal interface ITestReport
    {
        /// <summary>
        /// Sets the specified scheduling statistics.
        /// </summary>
        void SetSchedulingStatistics(bool isBugFound, string bugReport, int scheduledSteps,
            bool isMaxScheduledStepsBoundReached, bool isScheduleFair, int numSpawnTasks, int numContinuationTasks);

        /// <summary>
        /// Sets the specified unhandled exception.
        /// </summary>
        void SetUnhandledException(Exception exception);

        /// <summary>
        /// Sets the specified uncontrolled invocations.
        /// </summary>
        void SetUncontrolledInvocations(HashSet<string> invocations);
    }
}
