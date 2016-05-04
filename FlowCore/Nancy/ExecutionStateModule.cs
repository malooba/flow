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
using System.Data.Linq;
using System.IO;
using System.Linq;
using FlowShared.Db;
using FlowShared.Json;
using log4net;
using Nancy;
using Newtonsoft.Json.Linq;

namespace FlowCore.Nancy
{
    public class ExecutionStateModule : NancyModule
    {
        private const int DB_RETRIES = 4;
        private ILog log;

        /// <summary>
        /// Change the current execution state
        /// POSTed json object:
        /// {"command": cmd}
        /// where cmd is a string and one of
        /// "run", "pause", "cancel" or "stop".
        /// </summary>
        public ExecutionStateModule()
        {
            log = LogManager.GetLogger(typeof(ExecutionStateModule));
            History history = null;

            Post["/executionstate"] = parameters =>
            {
                var qexecutionId = Request.Query["executionid"];

                ExState newState;
                string command;

                using(var reader = new StreamReader(Request.Body))
                {
                    var json = reader.ReadToEnd();
                    JObject obj;
                    try
                    {
                        obj = JObject.Parse(json);   
                    }
                    catch
                    {
                        var response = new Response();
                        response.StatusCode = HttpStatusCode.BadRequest;
                        response.ReasonPhrase = "Invalid JSON";
                        return response;
                    }
                    command = ((string)obj.SelectToken("$.state")).ToLower();

                    switch(command)
                    {
                        case "run":
                            history = new WorkflowExecutionResumedEvent { };
                            newState = ExState.Running;
                            break;

                        case "pause":
                            history = new WorkflowExecutionPausedEvent { };
                            newState = ExState.Paused;
                            break;

                        case "cancel":
                            history = new WorkflowExecutionCancelledEvent { };
                            newState = ExState.Running;
                            break;

                        case "stop":
                            history = new WorkflowStoppedEvent { };
                            newState = ExState.Stopped;
                            break;

                        default:
                            return new Response().WithStatusCode(HttpStatusCode.BadRequest);
                    }
                }
                Guid executionId;
                if(!Guid.TryParse(qexecutionId, out executionId))
                    return new Response().WithStatusCode(HttpStatusCode.BadRequest);

                using(var db = new Database())
                {
                    var execution = db.Executions.SingleOrDefault(e => e.ExecutionId == executionId);
                    var executionState = db.ExecutionStates.SingleOrDefault(st => st.ExecutionId == executionId);

                    if(execution == null || executionState == null)
                        return new Response { StatusCode = HttpStatusCode.NotFound };

                    var state = ExState.Create(executionState.State);
                    
                    // Do not change terminal states and ignore if newState is the current state
                    if(state == ExState.Stopped ||                                          // Cannot change STOPPED state
                       (state == ExState.Cleanup && newState != ExState.Stopped) ||         // Can only go to STOPPED from CLEANUP
                       (state == newState && command != "cancel"))                          // No state change and not cancelling
                        return ((Response)state.Json).WithContentType("application/json");

                    Database.InsertHistory(execution, history);

                    if(state == newState)
                        return ((Response)newState.Json).WithContentType("application/json");

                    // Now update the execution state and retry upon failure
                    executionState.State = newState;

                    int i;
                    for(i = 0; i < DB_RETRIES; i++)
                    {
                        try
                        {
                            db.SubmitChanges();
                        }
                        catch(ChangeConflictException)
                        {
                            // Surely there can be, at most, only one conflict on the executionState
                            foreach(var conflict in db.ChangeConflicts.Where(c => c.Object == executionState))
                            {
                                // Conflict must be the state value
                                var mc = conflict.MemberConflicts.Single();

                                // Should never happen
                                if(mc.Member.Name != "State")
                                {
                                    log.Error($"ExecutionState update conflict member name is {mc.Member.Name}");
                                    throw new ApplicationException($"ExecutionState update conflict member name is {mc.Member.Name}");
                                }

                                var dbState = ExState.Create((string)mc.DatabaseValue);

                                // The desired state was set elsewhere, so leave it
                                if(dbState == newState)
                                    break;

                                if(dbState == ExState.Stopped)
                                {
                                    // Do not overwrite STOPPED state
                                    // (but do overwrite newState with the value from the database so that the correct
                                    // current state is reported back to the client)
                                    newState = dbState;
                                    break; 
                                }
                                // Presumably the state went from PAUSED to RUNNING or vice versa, try again
                                conflict.Resolve(RefreshMode.KeepCurrentValues);
                            }
                        }
                    }
                    
                    if(i == DB_RETRIES)
                    {
                        log.Info($"Too many retries to set state of execution {executionId} to {newState}");
                        // return the current (and original) state
                        var originalState = ExState.Create(db.ExecutionStates.GetOriginalEntityState(executionState).State);
                        return ((Response)originalState.Json).WithContentType("application/json");
                    }
                    return ((Response)newState.Json).WithContentType("application/json");
                }
            };
        }
    }
}
