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
using FlowShared.Json;

namespace FlowCore
{
    /// <summary>
    /// Check for timed out activity tasks
    /// This includes the overall task timeouts and heartbeat timeouts
    /// </summary>
    class ActivityTaskTimeoutChecker : PeriodicTask
    {
        private const int SCHEDULE_PERIOD = 60; // Seconds

        public ActivityTaskTimeoutChecker() : base("TimeoutTasks", TimeoutTasks, SCHEDULE_PERIOD)
        { }

        private static void TimeoutTasks(CancellationToken ct)
        {
            log.Info("Running Activity Task Timeout Checker");
            IEnumerable<Guid> timeouts;
            var now = DateTime.UtcNow;
            using (var db = new Database())
            {
                timeouts =
                    (from t in db.TaskLists
                     where t.HeartbeatAlarm < now || t.TaskAlarm < now
                     select t.TaskToken).ToList();
            }
            foreach(var timeout in timeouts)
            {
                if(ct.IsCancellationRequested) break;
                TimeoutTask(timeout, now);
            }
        }

        /// <summary>
        /// Timeout a task
        /// Each timeout update takes place in its own context so that we can discard the context on failure
        /// </summary>
        /// <param name="taskToken"></param>
        /// <param name="now"></param>
        private static void TimeoutTask(Guid taskToken, DateTime now)
        {
            using(var db = new Database())
            {
                try
                {
                    var task = db.TaskLists.SingleOrDefault(t => t.TaskToken == taskToken);
                    if(task == null) return;
                    // Cull the task
                    // Any late response from the worker will be ignored
                    db.TaskLists.DeleteOnSubmit(task);
                    var atte = new ActivityTaskTimeoutEvent
                    {
                        SchedulingEventId = task.TaskScheduledEventId,
                        // Heartbeat alarm if Task alarm not triggered (Task alarm takes priority)
                        HeartbeatMissed = task.TaskAlarm >= now
                    };
                    task.Execution.AwaitingDecision = true;
                    Database.InsertHistory(db, task.Execution, atte);
                    db.SubmitChanges();
                    log.Info($"Task timeout of task {task.TaskToken} on execution {task.ExecutionId}");
                }
                catch(ChangeConflictException ex)
                {
                    // Just ignore failures for now
                    log.Warn($"Failed to timeout task: {taskToken}", ex);
                }
            }
        }
    }
}

