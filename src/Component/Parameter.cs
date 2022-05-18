/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System;

namespace Ligral.Component
{
    public enum ParameterType
    {
        Signal, String
    }
    public class Parameter
    {
        public bool Required = true;
        public ParameterType Type;
        public Action<object> OnSet;
        public Action OnDefault;
        public Parameter(ParameterType type, Action<object> onset)
        {
            Type = type;
            OnSet = onset;
            OnDefault = ()=>{};
        }
        public Parameter(ParameterType type, Action<object> onset, Action onDefault)
        {
            Type = type;
            Required = false;
            OnSet = onset;
            OnDefault = onDefault;
        }
    }
}