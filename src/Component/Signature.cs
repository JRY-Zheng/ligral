/* Copyright (C) 2019-2020 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using System.Linq;
using Ligral.Syntax;

namespace Ligral.Component
{
    class Signature
    {
        public List<string> InPorts;
        public List<string> OutPorts;
        public string Name;
        public Signature(string name, List<string> inPorts, List<string> outPorts)
        {
            Name = name;
            InPorts = inPorts;
            OutPorts = outPorts;
        }
        public bool Derive(Model model)
        {
            return Validate(model.InPortsName, model.OutPortsName);
        }
        public bool Derive(Route route)
        {
            TypeSymbol typeSymbol = route.RouteScope.Lookup(route.Type) as TypeSymbol;
            if (typeSymbol == null || typeSymbol.Type == null || typeSymbol.Type.Name != Name || 
                typeSymbol.Type.Type == null || typeSymbol.Type.Type.Name != "SIGN")
            {
                return false;
            }
            return typeSymbol.Type.GetValue() == this;
        }
        public bool Validate(List<string> inPortsName, List<string> outPortsName)
        {
            return inPortsName.SkipWhile(inPort => InPorts.Contains(inPort)).Count() == 0
                && InPorts.SkipWhile(inPort => inPortsName.Contains(inPort)).Count() == 0
                && outPortsName.SkipWhile(outPort => OutPorts.Contains(outPort)).Count() == 0
                && OutPorts.SkipWhile(outPort => outPortsName.Contains(outPort)).Count() == 0;
        }
    }
}