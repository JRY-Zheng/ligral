/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;

namespace Ligral.Syntax.CodeASTs
{
    class ConfigCodeAST : CodeAST 
    {
        public DeclareCodeAST declareCodeAST;
        public List<CopyCodeAST> copyCodeASTs;
        public override string FindToken()
        {
            return declareCodeAST.FindToken();
        }
    }
}