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
using FlowShared.Db;
using FlowShared.Json;
using Nancy;
using Newtonsoft.Json;

namespace FlowCore.Nancy
{ 
    public class WorkflowsModule : NancyModule
    {
        public WorkflowsModule()
        {
            // Get name and version of all workflows
            Get["/workflows"] = parameters =>
            {
                using(var db = new Database())
                {
                    var json = JsonConvert.SerializeObject(db.GetWorkflows());
                    var response = (Response)json;
                    response.ContentType = "application/json";
                    return response;
                }
            };

            // Get name and version of all workflows with a given name
            Get["/workflows/{name}"] = parameters =>
            {
                using (var db = new Database())
                {
                    var json = JsonConvert.SerializeObject(db.GetWorkflows(parameters.name));
                    var response = (Response)json;
                    response.ContentType = "application/json";
                    return response;
                }
            };

            // Get the JSON definition of a specific workflow
            Get["/workflows/{name}/versions/{version}"] = parameters =>
            {
                using(var db = new Database())
                {
                    Workflow wf = db.GetWorkflow(parameters.name, parameters.version);
                    var response = (Response)wf.Json;
                    response.ContentType = "application/json";
                    return response;
                }
            };

            // Store the JSON definition of a specific workflow
            Put["/workflows"] = parameters =>
            {
                try
                {

                    using(var db = new Database())
                    {
                        using(var reader = new StreamReader(Request.Body))
                        {
                            var json = reader.ReadToEnd();
                            var wf = JsonConvert.DeserializeObject<WorkflowObj>(json);

                            json = db.StoreWorkflow(wf);
                            // Response is a JSON string - "OK" if no error, else the error message
                            return ((Response) json).WithStatusCode(HttpStatusCode.OK).WithContentType("application/json");
                        }
                    }
                }
                catch(Exception ex)
                {
                    return ((Response)$"\"{ex.Message}\"").WithStatusCode(HttpStatusCode.InternalServerError).WithContentType("application/json");
                }
            };

            // Delete all versions of a workflow
            Delete["/workflows/{name}"] = parameters =>
            {
                using(var db = new Database())
                {
                    if(db.DeleteWorkflows(parameters.name))
                        return new Response().WithStatusCode(HttpStatusCode.NoContent);

                    return new Response().WithStatusCode(HttpStatusCode.NotFound);
                }
            };

            // Delete one version of a workflow
            Delete["/workflows/{name}/versions/{version}"] = parameters =>
            {
                using(var db = new Database())
                {
                    if(db.DeleteWorkflow(parameters.name, parameters.version))
                        return new Response().WithStatusCode(HttpStatusCode.NoContent);
                    
                    return new Response().WithStatusCode(HttpStatusCode.NotFound);
                }
            };

        }
    } 
}
