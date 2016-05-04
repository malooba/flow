//Copyright 2016 Malooba Ltd

//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at

//    http://www.apache.org/licenses/LICENSE-2.0

//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Threading;
using FlowShared.Db;

namespace FlowCore
{
    internal class DeciderTimeoutChecker : PeriodicTask
    {
        private const int SCHEDULE_PERIOD = 30; // Seconds

        public DeciderTimeoutChecker() : base("TimeoutDecider", TimeoutDecisions, SCHEDULE_PERIOD)
        { }

        /// <summary>
        ///  Check timeout for any decisions that have a token (i.e. that are under consideration)
        /// </summary>
        private static void TimeoutDecisions(CancellationToken ct)
        {
            log.Info("Running Decider Timeout Checker");
            IEnumerable<Guid> executions;
            var now = DateTime.UtcNow;
            using(var db = new Database())
            {
                executions =
                    (from e in db.Executions
                     where e.DeciderToken != null && e.DeciderAlarm < now
                     select e.ExecutionId).ToList();
            }
            foreach(var e in executions)
            {
                TimeoutDecision(e, now);
                if(ct.IsCancellationRequested)
                    break;
            }
        }

        /// <summary>
        /// Timeout a decision
        /// Each timeout update takes place in its own context so that we can discard the context on failure
        /// </summary>
        /// <param name="executionId"></param>
        /// <param name="now"></param>
        private static void TimeoutDecision(Guid executionId, DateTime now)
        {
            using(var db = new Database())
            {
                try
                {
                    var execution = db.Executions.SingleOrDefault(e => e.ExecutionId == executionId);
                    if(execution == null) return;
                    // Ignore any future response
                    execution.DeciderToken = null;
                    execution.AwaitingDecision = true;
                    db.SubmitChanges();
                    log.Info($"Decider timeout on execution {execution.ExecutionId}");
                }
                catch(ChangeConflictException ex)
                {
                    // Just ignore failures for now
                    log.Warn($"Failed to timeout decider task on execution: {executionId}", ex);
                }
            }
        }
    }
}
