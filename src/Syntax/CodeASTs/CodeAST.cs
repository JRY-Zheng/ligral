/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

namespace Ligral.Syntax.CodeASTs
{
    class CodeAST 
    {
        public virtual CodeToken FindToken()
        {
            return null;
        }
    }
}