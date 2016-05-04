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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FlowCore.Nancy
{
    public class VariablesModule : NancyModule
    {
        private ILog log;

        public VariablesModule()
        {
            log = LogManager.GetLogger(typeof(VariablesModule));

            Get["/variables"] = parameters =>
            {

                Response response;
                var qexecutionId = Request.Query["executionId"]; // The executionId to get variables for (required)

                Guid executionId;
                if(!Guid.TryParse(qexecutionId, out executionId))
                {
                    response = new Response().WithStatusCode(HttpStatusCode.BadRequest);
                }
                else
                {
                    try
                    {
                        using(var db = new Database())
                        {
                            var variables = new JObject(
                                from v in db.Variables
                                where v.ExecutionId == executionId
                                select new JProperty(v.Name, JToken.Parse(v.Json))
                                );

                            var responseJson = variables.ToString(Formatting.None);
                            response =
                                ((Response)responseJson)
                                    .WithContentType("application/json");
                        }
                    }
                    catch(Exception ex)
                    {
                        log.Error($"Error fetching variables for execution {executionId}", ex);
                        response = new Response().WithStatusCode(HttpStatusCode.InternalServerError);
                    }
                }
                return response;
            };
        }
    }
}


