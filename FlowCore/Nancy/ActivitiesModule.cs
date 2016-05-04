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
using Newtonsoft.Json.Linq;

namespace FlowCore.Nancy
{
    public class ActivitiesModule : NancyModule
    {
        public ActivitiesModule()
        {
            // Get name and version of all activities
            Get["/activities"] = parameters =>
            {
                using (var db = new Database())
                {
                    var array = new JArray();
                    foreach (var wf in db.GetActivities())
                        array.Add(JObject.FromObject(wf));

                    var response = (Response)array.ToString(Formatting.None);
                    response.ContentType = "application/json";
                    return response;
                }
            };

            // Get name and version of all activities with a given name
            Get["/activities/{name}"] = parameters =>
            {
                using (var db = new Database())
                {
                    var array = new JArray();
                    foreach (var wf in db.GetActivities(parameters.name))
                        array.Add(JObject.FromObject(wf));

                    var response = (Response)array.ToString(Formatting.None);
                    response.ContentType = "application/json";
                    return response;
                }
            };

            // Get the JSON definition of a specific activity
            Get["/activities/{name}/versions/{version}"] = parameters =>
            {
                using (var db = new Database())
                {
                    Activity wf = db.GetActivity(parameters.name, parameters.version);
                    if(wf == null)
                        return new Response().WithStatusCode(HttpStatusCode.NoContent);

                    var response = (Response)wf.Json;
                    response.ContentType = "application/json";
                    return response;
                }
            };

            // Store the JSON definition of a specific activity
            Put["/activities/{name}/versions/{version}"] = parameters =>
            {
                using(var db = new Database())
                {
                    using(var reader = new StreamReader(Request.Body))
                    {
                        try
                        {
                            var json = reader.ReadToEnd();

                            var ac = JsonConvert.DeserializeObject<ActivityObj>(json);

                            return db.StoreActivity(ac)
                                ? new Response {StatusCode = HttpStatusCode.OK}
                                : new Response {StatusCode = HttpStatusCode.InternalServerError};
                        }
                        catch(Exception)
                        {
                            return new Response {StatusCode = HttpStatusCode.InternalServerError};
                        }
                    }
                }
            };
        }
    }
}
