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
        public Parameter(Action<object> onset, Action onDefault)
        {
            Required = false;
            OnSet = onset;
            OnDefault = onDefault;
        }
    }
}