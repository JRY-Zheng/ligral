/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;

namespace Ligral.Syntax.CodeASTs
{
    public class ClassCodeAST : CodeAST 
    {
        public string ClassName;
        public List<DeclareCodeAST> publicASTs;
        public override string FindToken()
        {
            return ClassName;
        }
    }
}