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

using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Flow.Utility
{
    /// <summary>
    /// Utility class for the Script worker
    /// </summary>
    public static class ScriptXml
    {
        public static string[] XPathGet(string xml, string xpath)
        {
            var root = XElement.Parse(xml);
            return (from el in root.XPathSelectElements(xpath) select el.Value).ToArray();
        }

        public static string XPathGetOne(string xml, string xpath)
        {
            var root = XElement.Parse(xml);
            var el = root.XPathSelectElement(xpath);
            return el?.Value ?? "";
        }
    }
}
