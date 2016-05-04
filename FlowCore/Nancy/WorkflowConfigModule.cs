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
using FlowShared.Db;
using log4net;
using Nancy;

namespace FlowCore.Nancy
{
    public class WorkflowConfigModule : NancyModule
    {
        private ILog log;

        public WorkflowConfigModule()
        {
            log = LogManager.GetLogger(typeof(WorkflowConfigModule));

            // Fetch the config for a workflow
            // If a version specific config exists then return it
            // else return the generic config for the workflow name
            Get["/workflowconfig"] = parameters =>
            {
                Response response;
                string wfName = Request.Query["workflowname"];
                string wfVersion = Request.Query["workflowversion"];
                long wfVersionNum;

                try
                {
                    if(string.IsNullOrWhiteSpace(wfName))
                        throw new ApplicationException("Missing required workflow name");
                    wfVersionNum = Database.ConvertVersion(wfVersion);
                }
                catch(Exception ex)
                {
                    log.Error($"Exception: {ex.Message}", ex);
                    return new Response().WithStatusCode(HttpStatusCode.BadRequest);
                }

                try
                {
                    using(var db = new Database())
                    {
                        // Config with a null version is the default for the workflow
                        // Special config can be supplied for specific versions
                        var configs =
                            from w in db.WorkflowConfigs
                            where w.WorkflowName == wfName
                            where w.WorkflowVersion == wfVersionNum || w.WorkflowVersion == null
                            select w;

                        if(!configs.Any())
                            return new Response().WithStatusCode(HttpStatusCode.NoContent);

                        var config = (configs.Count() == 1)
                            ? configs.Single()
                            : configs.Single(c => c.WorkflowVersion != null);

                        response = ((Response)config.Json).WithContentType("application/json");
                    }
                }
                catch(Exception ex)
                {
                    log.Error($"Exception: {ex.Message}", ex);
                    response = new Response().WithStatusCode(HttpStatusCode.InternalServerError);
                }

                return response;
            };
        }
    }
}


