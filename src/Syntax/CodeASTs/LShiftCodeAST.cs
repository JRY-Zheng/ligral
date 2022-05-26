/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/


namespace Ligral.Syntax.CodeASTs
{
    public class LShiftCodeAST : CodeAST 
    {
        public string Source;
        public string Destination;
        public override string FindToken()
        {
            return Source;
        }
    }
}