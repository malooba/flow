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
using System.Threading;
using Flow.Utility;
using Microsoft.ClearScript;
using Microsoft.ClearScript.V8;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Flow.Workers.ScriptWorker
{
    class ScriptWorker : WorkerBase.WorkerBase
    {
        private readonly string scriptContainer;

        public ScriptWorker()
        {
            scriptContainer = Properties.Resources.ScriptContainer;
        }

        protected override void DoTask(WorkerBase.ActivityTask task)
        {
            try
            {
                RespondSuccess(task, ExecuteScript(task.Input));
            }
            catch(ScriptEngineException e)
            {
                logger.Error("Script error", e);
                RespondFailure(task, "Script failed with error: " + e.Message, e.ErrorDetails);
            }
            catch(Exception e)
            {
                logger.Error("Script error", e);
                RespondFailure(task, "Script failed with error: " + e.Message, e.StackTrace);
            } 
        }

        private JObject ExecuteScript(JObject input)
        {
            var script = (string)input["$script"];
            if(string.IsNullOrWhiteSpace(script))
                throw new ApplicationException("Missing script");
            input.Remove("$script");

            using(var engine = new V8ScriptEngine())
            {
                // Insert script and input data into engine environment
                var inputjson = input.ToString(Formatting.None);
                var scriptText = scriptContainer
                    .Replace("_inputjson_", EscapeSingleQuotedString(inputjson))
                    .Replace("_script_", EscapeSingleQuotedString(script));

                // Additional host types and data added to global environment
                engine.AddHostType("Console", typeof(Console));
                engine.AddHostType("String", typeof(string));
                engine.AddHostType("Environment", typeof(Environment));
                engine.AddHostType("Xml", typeof(ScriptXml));
                engine.AddHostType("File", typeof(ScriptFile));
                engine.AddHostType("Directory", typeof(ScriptDirectory));
                engine.AddHostType("Util", typeof(ScriptUtil));
                engine.AddHostType("Thread", typeof(Thread));

                // Run the script
                engine.Execute(scriptText);

                // return the result as JSON
                var result = engine.Script["result"];
                return JObject.FromObject(result);
            }
        }

        private static string EscapeSingleQuotedString(string js)
        {
            return js
                .Replace("\\", "\\\\")
                .Replace("'", "\\'")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n")
                .Replace("\t", "\\t")
                .Replace("\b", "\\b")
                .Replace("\f", "\\f");
        }
    }
}
