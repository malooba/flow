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
using System.IO;
using System.Linq;
using FlowShared.Db;
using FlowShared.Json;
using log4net;
using Nancy;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FlowCore.Nancy
{
    public class ExecutionsModule : NancyModule
    {
        private ILog log;

        public ExecutionsModule()
        {
            log = LogManager.GetLogger(typeof(VariablesModule));

            Get["/executions/{guid}"] = parameters =>
            {
                Guid executionid;
                if(Guid.TryParse(parameters.guid, out executionid))
                {
                    using(var db = new Database())
                    {
                        var execution = 
                           (from e in db.Executions
                               where e.ExecutionId == executionid
                               select e).SingleOrDefault();

                        if(execution == null)
                            return new Response().WithStatusCode(HttpStatusCode.NotFound);

                       var executionObj = new
                        {
                            executionId = execution.ExecutionId,
                            jobId = execution.JobId,
                            state = execution.ExecutionState.State,
                            awaitingDecision = execution.AwaitingDecision,
                            deciderToken = execution.DeciderToken,
                            deciderAlarm = execution.DeciderAlarm,
                            decisionList = execution.DecisionList,
                            historySeen = execution.HistorySeen,
                            lastSeen = execution.LastSeen,
                            workflowName = execution.WorkflowName,
                            workflowVersion = execution.WorkflowVersion,
                            executionStartToCloseTimeout = execution.ExecutionStartToCloseTimeout,
                            taskScheduleToCloseTimeout = execution.TaskScheduleToCloseTimeout,
                            taskScheduleToStartTimeout = execution.TaskScheduleToStartTimeout,
                            taskStartToCloseTimeout = execution.TaskStartToCloseTimeout
                        };
                        var json = JsonConvert.SerializeObject(executionObj);
                        return ((Response)json).WithContentType("application/json");
                    }
                }
                return new Response().WithStatusCode(HttpStatusCode.BadRequest);
            };

            Get["/executions"] = parameters =>
            {
                Response response;
                string jobid = Request.Query["jobid"];
                string workflow = Request.Query["workflow"]; // The workflow to get executions for
                DateTime? after = Request.Query["after"] == null ? null : DateTime.Parse(Request.Query["after"]); // Get executions started after
                string wfstate = Request.Query["state"];

                try
                {
                    using(var db = new Database())
                    {
                        JArray executions;
                        {
                            // Return ExecutionId, JobId and Started date ordered by started date 
                            executions = new JArray(
                                from e in db.Executions
                                join state in db.ExecutionStates on e.ExecutionId equals state.ExecutionId
                                let start = (from h in db.Histories
                                    where h.ExecutionId == e.ExecutionId
                                    orderby h.Id
                                    select h.Timestamp).First()
                                where jobid == null || e.JobId.Contains(jobid) || e.ExecutionId.ToString().Contains(jobid)
                                where workflow == null || e.WorkflowName.Contains(workflow)
                                where wfstate == null || state.State == wfstate
                                where after == null || start >= after
                                orderby start 
                                select new JObject(
                                    new JProperty("executionId", e.ExecutionId),
                                    new JProperty("jobId", e.JobId),
                                    new JProperty("started", start))
                                );
                        }
                        var responseJson = executions.ToString(Formatting.None);
                        response =
                            ((Response)responseJson)
                                .WithContentType("application/json");
                    }
                }
                catch(Exception ex)
                {
                    log.Error($"Error fetching executions with a query", ex);
                    response = new Response().WithStatusCode(HttpStatusCode.InternalServerError);
                }
                return response;
            };

            // Signal an execution
            // Used by workers for asynchronous tasks
            // May have wider application in the future
            Post["/executions/{executionid}/signal"] = parameters =>
            {
                Guid executionid;
                if(!Guid.TryParse(parameters.executionid, out executionid))
                    return new Response { StatusCode = HttpStatusCode.BadRequest };

                using(var db = new Database())
                {
                    using (var reader = new StreamReader(Request.Body))
                    {
                        try
                        {
                            var json = reader.ReadToEnd();
                            var wse = JsonConvert.DeserializeObject<WorkflowSignalledEvent>(json);
                            db.SignalExecution(parameters.executionid, wse);

                            return new Response {StatusCode = HttpStatusCode.NoContent};
                        }
                        catch (Exception)
                        {
                            return new Response { StatusCode = HttpStatusCode.BadRequest };
                        }
                    }
                }
            };
        }
    }
}
