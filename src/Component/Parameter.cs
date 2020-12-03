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