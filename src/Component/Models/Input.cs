/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;

namespace Ligral.Component.Models
{
    class Input : Model, IFixable
    {
        private double? source;
        protected override string DocString
        {
            get
            {
                return "This model is automatically used inside a route. Do not call it manually.";
            }
        }
        public void SetDefaultSource(double source)
        {
            this.source = source;
        }
        public bool FixConnection()
        {
            if (source is double src)
            {
                ILinkable constant = ModelManager.Create("Constant");
                var dict = new Dictionary<string, object>() {{"value", src}};
                constant.Configure(dict);
                constant.Connect(this);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}