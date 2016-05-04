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
using Nancy;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FlowCore.Nancy
{
    public class TaskListModule : NancyModule
    {
        public TaskListModule()
        {
            // Get an array of tasklist names
            Get["/tasklists"] = parameters =>
            {
                using(var db = new Database())
                {
                    var lists = new JArray((from tl in db.TaskLists orderby tl.ListName select tl.ListName).Distinct());
                    var response = (Response)(lists.ToString(Formatting.None));
                    response.ContentType = "application/json";
                    return response;
                }
            };
        }

        public class TaskModule : NancyModule
        {
            public TaskModule()
            {
                //Get details of a single task 
                Get["/tasks/{taskToken}"] = parameters =>
                {
                    using(var db = new Database())
                    {
                        using(var reader = new StreamReader(Request.Body))
                        {
                            Guid taskToken = Guid.Parse(parameters.taskToken);
                            var task = db.TaskLists.SingleOrDefault(t => t.TaskToken == taskToken);
                            if(task == null)
                                return new Response { StatusCode = HttpStatusCode.NoContent };

                            var json = JsonConvert.SerializeObject(task);
                            var response = (Response)json;
                            response.ContentType = "application/json";
                            return response;
                        }
                    }
                };

                // Get the tasks on a specific list
                Get["/tasks"] = parameters =>
                {
                    using(var db = new Database())
                    {
                        string list = Request.Query["list"];

                        var json = new JArray(
                            from t in db.TaskLists
                            where t.ListName == list
                            from e in db.Executions
                            where t.ExecutionId == e.ExecutionId
                            let ev = t.TaskScheduledEventId != 0
                                ? JObject.Parse(
                                    (from h in db.Histories
                                        where t.ExecutionId == h.ExecutionId && t.TaskScheduledEventId == h.Id
                                        select h.Json).Single())
                                : null 
                            select new JObject(
                                new JProperty("taskList", t.ListName),
                                new JProperty("executionId", t.ExecutionId),
                                new JProperty("jobId", e.JobId),
                                new JProperty("workflowName", e.WorkflowName),
                                new JProperty("workflowVersion", Database.ConvertVersion(e.WorkflowVersion)),
                                new JProperty("priority", t.Priority),
                                new JProperty("taskScheduledEventId", t.TaskScheduledEventId),
                                new JProperty("taskToken", t.TaskToken),
                                new JProperty("taskAlarm", t.TaskAlarm),
                                new JProperty("heartbeatTimeout", t.HeartbeatTimeout),
                                new JProperty("heartbeatAlarm", t.HeartbeatAlarm),
                                new JProperty("workerId", t.WorkerId),
                                new JProperty("cancelling", t.Cancelling),
                                new JProperty("scheduledAt", t.ScheduledAt),
                                new JProperty("startedAt", t.StartedAt),
                                new JProperty("taskSheduleToCloseTimeout", t.TaskSheduleToCloseTimeout),
                                new JProperty("taskStartToCloseTimeout", t.TaskStartToCloseTimeout),
                                new JProperty("progress", t.Progress),
                                new JProperty("progressMessage", t.ProgressMessage),
                                new JProperty("notificationData", t.NotificationData),
                                new JProperty("progressData", t.ProgressData),
                                new JProperty("schedulingEvent", ev)
                                )).ToString(Formatting.None);

                        if(json == null)
                            return new Response { StatusCode = HttpStatusCode.NoContent };

                        var response = (Response)json;
                        response.ContentType = "application/json";
                        return response;
                    }
                };

                // Post the response for a finished task from a worker.
                // Not to be used by any other client.
                Post["/tasks/{taskToken}"] = parameters =>
                {
                    using(var db = new Database())
                    {
                        using (var reader = new StreamReader(Request.Body))
                        {
                            var json = db.ProcessTaskResponse(parameters.taskToken, reader.ReadToEnd());

                            if(json == null)
                                return new Response {StatusCode = HttpStatusCode.NoContent};

                            var response = (Response)json;
                            response.ContentType = "application/json";
                            return response;
                        }
                    }
                };

                // Special request for workers to obtain work.
                // Not to be used by any other client.
                Get["/tasks/poll"] = parameters =>
                {
                    using(var db = new Database())
                    {
                        var list = Request.Query["list"];
                        var worker = Request.Query["worker"];

                        if(string.IsNullOrWhiteSpace(list) || string.IsNullOrWhiteSpace(worker))
                            return  new Response { StatusCode = HttpStatusCode.BadRequest };

                        var json = db.SelectAndStartTask(worker, list);

                        if(json == null)
                            return new Response { StatusCode = HttpStatusCode.NoContent };

                        var response = (Response)json;
                        response.ContentType = "application/json";
                        return response;
                    } 
                };
            }
        }
    }
}
