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
using System.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// ReSharper disable LocalizableElement

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace FlowShared.Json
{
    /// <summary>
    /// Definition of an entire workflow
    /// </summary>
    [JsonObject]
    public class WorkflowObj
    {
        /// <summary>
        /// ObjType is always "workflow"
        /// </summary>
        [JsonProperty(PropertyName = "objtype", Required = Required.Always, DefaultValueHandling = DefaultValueHandling.Include, Order = 0)]
        [DefaultValue("workflow")]
        public string ObjType { get; set; }

        /// <summary>
        /// The workflow name 
        /// </summary>
        [JsonProperty(PropertyName = "name", Required = Required.Always, Order = 1)]
        public string Name { get; set; }

        /// <summary>
        /// The workflow version 
        /// </summary>
        [JsonProperty(PropertyName = "version", Required = Required.Always, Order = 2)]
        public string Version { get; set; }

        /// <summary>
        /// Decider default tasklist
        /// </summary>
        [JsonProperty(PropertyName = "decisionList", DefaultValueHandling = DefaultValueHandling.Populate, Order = 3)]
        [DefaultValue("decider")]
        public string DecisionList { get; set; }

        /// <summary>
        /// Overall workflow timeout in seconds
        /// All workflows timeout after a year regardless of this setting
        /// </summary>
        [JsonProperty(PropertyName = "defaultExecutionStartToCloseTimeout", NullValueHandling = NullValueHandling.Ignore, Order = 4)]
        public uint? DefaultExecutionStartToCloseTimeout { get; set; }

        /// <summary>
        /// Default task timeout
        /// </summary>
        [JsonProperty(PropertyName = "defaultTaskStartToCloseTimeout", DefaultValueHandling = DefaultValueHandling.Populate, Order = 5)]
        public uint? DefaultTaskStartToCloseTimeout { get; set; }

        /// <summary>
        /// Name of JSON schema to validate workflow start data
        /// </summary>
        [JsonProperty(PropertyName = "inputSchema", NullValueHandling = NullValueHandling.Ignore, Order = 6)]
        public string InputSchema { get; set; }

        /// <summary>
        /// Version of JSON schema to validate workflow start data
        /// </summary>
        [JsonProperty(PropertyName = "inputSchemaVersion", NullValueHandling = NullValueHandling.Ignore, Order = 7)]
        public string InputSchemaVersion { get; set; }

        /// <summary>
        /// Workflow variables and how they are initialised
        /// </summary>
        [JsonProperty(PropertyName = "variables", NullValueHandling = NullValueHandling.Ignore, Order = 8)]
        public Dictionary<string, VariableObj> Variables { get; set; }

        /// <summary>
        /// Workflow tasks
        /// </summary>
        [Browsable(false), JsonProperty(PropertyName = "tasks", Required = Required.Always, Order = 9)]
        public List<TaskObj> Tasks { get; set; }
    }

    public delegate void TaskChangedEventHandler(object sender, EventArgs e);

    /// <summary>
    /// One workflow task
    /// </summary>
    public class TaskObj
    {
        /// <summary>
        /// Task ID (unique in workflow)
        /// </summary>
        [JsonProperty(PropertyName = "taskId", Required = Required.Always, Order = 0)]
        public string TaskId 
        { get; set; }

        /// <summary>
        /// Activity Name
        /// </summary>
        [JsonProperty(PropertyName = "activityName", Required = Required.Always, Order = 1)]
        public string ActivityName { get; set; }

        /// <summary>
        /// Activity Version
        /// </summary>
        [JsonProperty(PropertyName = "activityVersion", Required = Required.Always, Order = 2)]
        public string ActivityVersion { get; set; }

        /// <summary>
        /// Asynchronous task signal name
        /// </summary>
        [JsonProperty(PropertyName = "asyncSignal", NullValueHandling = NullValueHandling.Ignore, Order = 3)]
        public string AsyncSignal { get; set; }

        /// <summary>
        /// Inputs to the Task Activity
        /// </summary>
        [JsonProperty(PropertyName = "inputs", NullValueHandling = NullValueHandling.Ignore, Order = 4)]
        public Dictionary<string, InputObj> Inputs { get; set; }

        /// <summary>
        /// Outputs from Task Activity
        /// </summary>
        [JsonProperty(PropertyName = "outputs", NullValueHandling = NullValueHandling.Ignore, Order = 5)]
        public Dictionary<string, OutputObj> Outputs { get; set; }

        /// <summary>
        /// Task outflows
        /// This will be one outflow named "Out" or multiple outflows with Activity specific names
        /// Terminal tasks will have no outflow
        /// </summary>
        [JsonProperty(PropertyName = "outflows", NullValueHandling = NullValueHandling.Ignore, Order = 6)]
        public FlowObj[] Outflows { get; set; }

        /// <summary>
        /// Outflow for Activty failure or timeout
        /// </summary>
        [JsonProperty(PropertyName = "failOutflow", NullValueHandling = NullValueHandling.Ignore, Order = 7)]
        public FlowObj FailOutflow { get; set; }

        [JsonProperty(PropertyName = "taskList", NullValueHandling = NullValueHandling.Ignore, Order = 8)]
        public string TaskList { get; set; }

        [JsonProperty(PropertyName = "heartbeatTimeout", NullValueHandling = NullValueHandling.Ignore, Order = 9)]
        public uint? HeartbeatTimeout { get; set; }

        /// <summary>
        /// Activity timeout parameter override
        /// </summary>
        [JsonProperty(PropertyName = "scheduleToCloseTimeout", NullValueHandling = NullValueHandling.Ignore, Order = 10)]
        public uint? ScheduleToCloseTimeout { get; set; }

        /// <summary>
        /// Activity timeout parameter override
        /// </summary>
        [JsonProperty(PropertyName = "scheduleToStartTimeout", NullValueHandling = NullValueHandling.Ignore, Order = 11)]
        public uint? ScheduleToStartTimeout { get; set; }

        /// <summary>
        /// Activity timeout parameter override
        /// </summary>
        [JsonProperty(PropertyName = "startToCloseTimeout", NullValueHandling = NullValueHandling.Ignore, Order = 12)]
        public uint? StartToCloseTimeout { get; set; }

        /// <summary>
        /// Task priority
        /// </summary>
        [JsonProperty(PropertyName = "taskPriority", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Populate, Order = 13)]
        [DefaultValue(0)]
        public int? TaskPriority { get; set; }

        /// <summary>
        /// Internal symbol graphic data
        /// </summary>
        [JsonProperty(PropertyName = "symbol", NullValueHandling = NullValueHandling.Ignore, Order = 14)]
        public SymbolObj Symbol { get; set; }

    }

    /// <summary>
    /// Activity definition
    /// </summary>
    public class ActivityObj
    {
        /// <summary>
        /// ObjType is always "activity"
        /// </summary>
        [JsonProperty(PropertyName = "objtype", Required = Required.Always, Order = 0)]
        public string ObjType { get; set; }

        /// <summary>
        /// Activity name
        /// </summary>
        [JsonProperty(PropertyName = "name", Required = Required.Always, Order = 1)]
        public string Name { get; set; }

        /// <summary>
        /// Activity version
        /// </summary>
        [JsonProperty(PropertyName = "version", Required = Required.Always, Order = 2)]
        public string Version { get; set; }

        /// <summary>
        /// Activity default queue
        /// </summary>
        [JsonProperty(PropertyName = "defaultTaskList", Required = Required.Always, Order = 3)]
        public string DefaultTaskList { get; set; }

        /// <summary>
        /// Activity timeout parameter default
        /// </summary>
        [JsonProperty(PropertyName = "defaultTaskScheduleToCloseTimeout", NullValueHandling = NullValueHandling.Ignore, Order = 4)]
        public uint? DefaultTaskScheduleToCloseTimeout { get; set; }

        /// <summary>
        /// Activity timeout parameter default
        /// </summary>
        [JsonProperty(PropertyName = "defaultTaskScheduleToStartTimeout", NullValueHandling = NullValueHandling.Ignore, Order = 5)]
        public uint? DefaultTaskScheduleToStartTimeout { get; set; }

        /// <summary>
        /// Activity timeout parameter default
        /// </summary>
        [JsonProperty(PropertyName = "defaultTaskStartToCloseTimeout", NullValueHandling = NullValueHandling.Ignore, Order = 6)]
        public uint? DefaultTaskStartToCloseTimeout { get; set; }

        /// <summary>
        /// Activity timeout parameter default
        /// </summary>
        [JsonProperty(PropertyName = "defaultTaskHeartbeatTimeout", NullValueHandling = NullValueHandling.Ignore, Order = 7)]
        public uint? DefaultTaskHeartbeatTimeout { get; set; }

        /// <summary>
        /// Activity priority default
        /// </summary>
        [JsonProperty(PropertyName = "defaultPriority", NullValueHandling = NullValueHandling.Ignore, Order = 7)]
        public int? DefaultPriority { get; set; }

        /// <summary>
        /// Flag to indicate that the inputs may be augmented by the user so that, for example, inputs may be supplied to an arbitrary script activity
        /// </summary>
        [JsonProperty(PropertyName = "allowUserInputs", NullValueHandling = NullValueHandling.Ignore, Order = 8)]
        public bool AllowUserInputs { get; set; }

        /// <summary>
        /// Activity inputs
        /// </summary>
        [JsonProperty(PropertyName = "inputs", NullValueHandling = NullValueHandling.Ignore, Order = 9)]
        public Dictionary<string, InputObj> Inputs { get; set; }

        /// <summary>
        /// Activity outputs
        /// </summary>
        [JsonProperty(PropertyName = "outputs", NullValueHandling = NullValueHandling.Ignore, Order = 10)]
        public Dictionary<string, OutputObj> Outputs { get; set; }

        /// <summary>
        /// Activity description
        /// </summary>
        [JsonProperty(PropertyName = "description", NullValueHandling = NullValueHandling.Ignore, Order = 11)]
        public string Description { get; set; }
    }

    [JsonObject]
    public class VariableObj
    {
        /// <summary>
        /// The literal JSON value assigned to this variable
        /// </summary>
        [JsonProperty(PropertyName = "lit", NullValueHandling = NullValueHandling.Ignore, Order = 1)]
        public string Lit { get; set; }

        /// <summary>
        /// Input data type
        /// </summary>
        [JsonProperty(PropertyName = "type", NullValueHandling = NullValueHandling.Ignore, Order = 2)]
        public string Type { get; set; }

        /// <summary>
        /// Optional JSON path to select part of the input data
        /// </summary>
        [JsonProperty(PropertyName = "path", NullValueHandling = NullValueHandling.Ignore, Order = 3)]
        public string Path { get; set; }

        /// <summary>
        /// If true then this input must be satisfied by the Var and Path settings
        /// </summary>
        [JsonProperty(PropertyName = "required", NullValueHandling = NullValueHandling.Ignore, Order = 4)]
        public bool Required { get; set; }

        /// <summary>
        /// Default value if the Lit and Path settings do not produce a value 
        /// </summary>
        [JsonProperty(PropertyName = "default", NullValueHandling = NullValueHandling.Ignore, Order = 5)]
        public string Default { get; set; }

        /// <summary>
        /// Description of input
        /// </summary>
        [JsonProperty(PropertyName = "description", NullValueHandling = NullValueHandling.Ignore, Order = 6)]
        public string Description { get; set; }
    }

    /// <summary>
    /// One input to a task
    /// </summary>
    [JsonObject]
    public class InputObj
    {
        /// <summary>
        /// The variable assigned to this input
        /// </summary>
        [JsonProperty(PropertyName = "var", NullValueHandling = NullValueHandling.Ignore, Order = 0)]
        public string Var
        { get; set; }

        /// <summary>
        /// The literal JSON value assigned to this input
        /// </summary>
        [JsonProperty(PropertyName = "lit", NullValueHandling = NullValueHandling.Ignore, Order = 1)]
        public JToken Lit
        { get; set; }

        /// <summary>
        /// Input data type
        /// </summary>
        [JsonProperty(PropertyName = "type", NullValueHandling = NullValueHandling.Ignore, Order = 2)]
        public string Type
        { get; set; }

        /// <summary>
        /// Optional JSON path to select part of the input variable
        /// </summary>
        [JsonProperty(PropertyName = "path", NullValueHandling = NullValueHandling.Ignore, Order = 3)]
        public string Path
        { get; set; }

        /// <summary>
        /// If true then this input must be satisfied by the Var and Path settings
        /// </summary>
        [JsonProperty(PropertyName = "required", NullValueHandling = NullValueHandling.Ignore, Order = 4)]
        public bool Required
        { get; set; }

        /// <summary>
        /// Default value if the Var and Path settings do not produce a value 
        /// </summary>
        [JsonProperty(PropertyName = "default", NullValueHandling = NullValueHandling.Ignore, Order = 5)]
        public JToken Default
        { get; set; }

        /// <summary>
        /// Description of input
        /// </summary>
        [JsonProperty(PropertyName = "description", NullValueHandling = NullValueHandling.Ignore, Order = 6)]
        public string Description
        { get; set; }

        /// <summary>
        /// User defined inputs can be deleted or have their type changed
        /// </summary>
        [JsonProperty(PropertyName = "userDefined", NullValueHandling = NullValueHandling.Ignore, Order = 6)]
        public bool UserDefined
        { get; set; }

        /// <summary>
        /// If true then this input must be satisfied by the Var and Path settings
        /// </summary>
        [JsonProperty(PropertyName = "hidden", NullValueHandling = NullValueHandling.Ignore, Order = 4)]
        public bool Hidden
        { get; set; }
    }

    /// <summary>
    /// One output from a task
    /// </summary>
    [JsonObject]
    public class OutputObj
    {
        /// <summary>
        /// The variable to receive the output value or null if the value is to be discarded
        /// </summary>
        [JsonProperty(PropertyName = "var", NullValueHandling = NullValueHandling.Ignore, Order = 0)] 
        public string Var
        { get; set; }

        /// <summary>
        /// Output data type
        /// </summary>
        [JsonProperty(PropertyName = "type", NullValueHandling = NullValueHandling.Ignore, Order = 1)]
        public string Type
        { get; set; }

        /// <summary>
        /// Description of output
        /// </summary>
        [JsonProperty(PropertyName = "description", NullValueHandling = NullValueHandling.Ignore, Order = 2)]
        public JToken Description
        { get; set; }

        /// <summary>
        /// User defined outputs can be deleted or have their type or description changed
        /// </summary>
        [JsonProperty(PropertyName = "userDefined", NullValueHandling = NullValueHandling.Ignore, Order = 3)]
        public bool UserDefined
        { get; set; }


        [JsonIgnore]
        [Browsable(false)]
        public string ValueString
        {
            get { return Var; }
        }
    }

    /// <summary>
    /// Symbol graphical data
    /// </summary>
    public class SymbolObj
    {
        /// <summary>
        /// Symbol name
        /// </summary>
        [JsonProperty(PropertyName = "name", Required = Required.Always, Order = 0)]
        public string Name
        { get; set; }

        /// <summary>
        /// Symbol label
        /// </summary>
        [JsonProperty(PropertyName = "label", Required = Required.Always, Order = 1)]
        public string Label 
        { get; set; }

        /// <summary>
        /// Symbol style
        /// </summary>
        [JsonProperty(PropertyName = "style", Required = Required.Always, Order = 1)]
        public string Style
        { get; set; }

        /// <summary>
        /// Symbol X coordinate
        /// </summary>
        [JsonProperty(PropertyName = "locationX", Order = 2)]
        [DefaultValue(100)]
        public int LocationX
        { get; set; }

        /// <summary>
        /// Symbol Y coordinate
        /// </summary>
        [JsonProperty(PropertyName = "locationY", Order = 3)]
        [DefaultValue(100)]
        public int LocationY
        { get; set; }
    }

    public class FlowObj
    {
        /// <summary>
        /// Pin name
        /// </summary>
        [JsonProperty(PropertyName = "name", Required = Required.Always, Order = 0)]
        public string Name
        { get; set; }

	    /// <summary>
	    /// Target name
	    /// </summary>
	    [JsonProperty(PropertyName = "target", NullValueHandling = NullValueHandling.Ignore, Order = 1)]
	    public string Target
        { get; set; }

        /// <summary>
        /// Target pin name
        /// </summary>
        [JsonProperty(PropertyName = "targetPin", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Populate, Order = 1)]
        [DefaultValue("In")]
        public string TargetPin
        { get; set; }

        /// <summary>
        /// Connector route points
        /// </summary>
        [JsonProperty(PropertyName = "route", NullValueHandling = NullValueHandling.Ignore, Order = 1)]
        public int[] Route
        { get; set; }

    }
}
