/* Copyright (C) 2019-2020 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using Ligral.Component;

namespace Ligral.Component.Models
{
    class Node : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model outputs what it receives.";
            }
        }
    }
}