/* Copyright 2019-2020 Junruoyu Zheng. All rights reserved.

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;

namespace Ligral.Syntax.ASTs
{
    class SignatureAST : AST
    {
        public RoutePortAST InPorts;
        public RoutePortAST OutPorts;
        public WordAST TypeName;
        public SignatureAST(WordAST signatureName, RoutePortAST inPorts, RoutePortAST outPorts)
        {
            TypeName = signatureName;
            InPorts = inPorts;
            OutPorts = outPorts;
        }
        public override Token FindToken()
        {
            return TypeName.FindToken();
        }
    }
}