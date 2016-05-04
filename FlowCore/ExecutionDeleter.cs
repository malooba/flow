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
using System.Linq;
using System.Threading;
using FlowShared.Db;

namespace FlowCore
{
    public class ExecutionDeleter : PeriodicTask
    {
        private static int executionRetentionHours = 4;
        private const int SCHEDULE_PERIOD = 3600; // Seconds 

        public ExecutionDeleter() : base("DeleteExecutions", DeleteExecutions, SCHEDULE_PERIOD)
        {
        }

        private static void DeleteExecutions(CancellationToken ct)
        {
            using(var db = new Database())
            {
                var expiry = DateTime.UtcNow.AddHours(-executionRetentionHours);
                foreach(var hist in db.Histories.Where(h =>
                    (h.EventType == "WorkflowExecutionCompletedEvent" ||
                     h.EventType == "WorkflowExecutionTerminatedEvent" ||
                     h.EventType == "WorkflowExecutionCancelledEvent") &&
                    h.Timestamp > expiry))
                {
                    DeleteExecution(db, hist.Execution);
                    if(ct.IsCancellationRequested) break;
                }
            }
        }

        private static void DeleteExecution(Database db, Execution execution)
        {
            try
            {
                db.Variables.DeleteAllOnSubmit(db.Variables.Where(v => v.Execution == execution));
                db.Histories.DeleteAllOnSubmit(db.Histories.Where(h => h.Execution == execution));
                db.TaskLists.DeleteAllOnSubmit(db.TaskLists.Where(t => t.Execution == execution));
                db.ExecutionStates.DeleteAllOnSubmit(db.ExecutionStates.Where(t => t.Execution == execution));
                db.Executions.DeleteOnSubmit(execution);
                db.SubmitChanges();
            }
            catch
            {
                // Can this actually fail?  Just log it for the time being
                log.Error($"Failed to delete execution {execution.ExecutionId}");
            }
        }
    }
}
