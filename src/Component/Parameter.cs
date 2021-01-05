/* Copyright 2019-2020 Junruoyu Zheng. All rights reserved.

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System;

namespace Ligral.Component
{
    enum ParameterType
    {
        Signal, String
    }
    class Parameter
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