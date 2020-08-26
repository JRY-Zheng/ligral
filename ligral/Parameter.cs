using System;

namespace Ligral
{
    class Parameter
    {
        public bool Required = true;
        public Action<object> OnSet;
        public Action OnDefault;
        public Parameter(Action<object> onset)
        {
            OnSet = onset;
            OnDefault = ()=>{};
        }
        public Parameter(bool required, Action<object> onset, Action onDefault)
        {
            Required = required;
            OnSet = onset;
            OnDefault = onDefault;
        }
    }
}