/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

namespace Ligral.Syntax.ASTs
{
    class PortAST : AST
    {
        public WordAST PortId;
        public WordAST PortName;
        public PortAST(WordAST portID, WordAST portName)
        {
            PortId = portID;
            PortName = portName;
        }
        public override Token FindToken()
        {
            return PortId.FindToken();
        }
    }
}