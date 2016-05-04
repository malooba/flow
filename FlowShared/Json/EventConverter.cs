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
using FlowShared.Db;
using Newtonsoft.Json;

namespace FlowShared.Json
{
    /// <summary>
    /// This uses the recurring template pattern to allow the Create method to exist once in this base class
    /// whilst returning instances of derived classes
    /// This means that the derived classes only need define the mapped Json parameters
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class EventConverter<T> where T: EventConverter<T>, new()
    {
        [JsonIgnore] 
        public Guid ExecutionId;
        [JsonIgnore]
        public string Json;
        [JsonIgnore]
        public int Id;
        [JsonIgnore]
        public DateTime Timestamp;
        [JsonIgnore]
        public string EventType;


        /// <summary>
        /// Create an object of a derived type with the Json expanded into C# properties
        /// </summary>
        /// <param name="h">History event to be converted</param>
        /// <returns></returns>
        public static T Create(History h)
        {
            if(typeof(T).Name != h.EventType)
                throw new ApplicationException($"Wrong type {typeof(T).Name} != {h.EventType}");

            var obj = new T
            {
                ExecutionId = h.ExecutionId,
                Id = h.Id,
                Timestamp = h.Timestamp,
                EventType = h.EventType,
                Json = h.Json
            };

            JsonConvert.PopulateObject(obj.Json, obj);
            return obj;
        }

        /// <summary>
        /// Convert back into a history item
        /// </summary>
        /// <param name="evt"></param>
        /// <returns></returns>
        public static implicit operator History(EventConverter<T> evt)
        {
            // Special "Builder" types should have the Builder suffix removed
            var eventType  = evt.GetType().Name;
            if(eventType.EndsWith("Builder"))
                eventType = eventType.Substring(0, eventType.Length - 7);

            return new History
            {
                ExecutionId = evt.ExecutionId,
                Id = evt.Id,
                Timestamp = evt.Timestamp,
                EventType = eventType,
                Json = JsonConvert.SerializeObject(evt)
            };
        }

        public static bool CanCreate(History h)
        {
            return typeof(T).Name == h.EventType;
        }
    }
}
