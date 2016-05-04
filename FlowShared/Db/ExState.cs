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

namespace FlowShared.Db
{
    /// <summary>
    /// A utility class to represent, convert and validate ExecutionStates
    /// </summary>
    public class ExState
    {
        // static unique instances so we can compare with the default reference equality
        public static readonly ExState Running = new ExState("running");
        public static readonly ExState Paused = new ExState("paused");
        public static readonly ExState Cleanup = new ExState("cleanup");
        public static readonly ExState Stopped = new ExState("stopped");

        private readonly string state;

        /// <summary>
        /// Private constructor 
        /// </summary>
        /// <param name="state"></param>
        private ExState(string state)
        {
            this.state = state;
        }


        /// <summary>
        /// Convert a string to one of the unique instances above
        /// </summary>
        /// <param name="st">state string (from database)</param>
        /// <returns>a unique state instance (or null if the string is invalid)</returns>
        public static ExState Create(string st)
        {
            if(st == null)
                return null;
            switch(st.Trim("\" ".ToCharArray()).ToLower())
            {
                case "running":
                    return Running;
                case "paused":
                    return Paused;
                case "cleanup":
                    return Cleanup;
                case "stopped":
                    return Stopped;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Implicit string conversion so these values can be used directly in linq queries
        /// </summary>
        /// <param name="e"></param>
        public static implicit operator string(ExState e) => e.state;

        /// <summary>
        /// Trivial conversion to JSON
        /// </summary>
        public string Json => "\"" + state + "\"";
    }
    
}
