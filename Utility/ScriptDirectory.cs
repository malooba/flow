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

namespace Flow.Utility
{
    /// <summary>
    /// Utility class for Script worker
    /// </summary>
    public static class ScriptDirectory
    {
        /// <summary>
        /// Ensure that a complete directory path exists
        /// </summary>
        /// <param name="path">path</param>
        /// <returns>true if the path now exists</returns>
        public static bool Create(string path)
        {
            try
            {
                Directory.CreateDirectory(path);
            }
            catch(Exception)
            {
               return false;
            }
            return true;
        }

        /// <summary>
        /// Check that a directory path exists
        /// </summary>
        /// <param name="path">path</param>
        /// <returns>true if the path exists</returns>
        public static bool Exists(string path)
        {
            return Directory.Exists(path);
        }

        /// <summary>
        /// Delete a directory path and, optionally, all content, recursively
        /// This function will return true (success) if the path does not exist
        /// </summary>
        /// <param name="path">path to delete</param>
        /// <param name="recursive">true to delete recursively</param>
        /// <returns>true if the path no longer exists</returns>
        public static bool Delete(string path, bool recursive = false)
        {
            try
            {
                Directory.Delete(path, recursive);
            }
            catch(DirectoryNotFoundException)
            {
                return true;
            }
            catch(Exception)
            {
                return false;
            }
            return true;
        }
    }
}
