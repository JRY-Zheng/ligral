using Ligral.Component;

namespace Ligral.Component.Models
{
    class Output : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model is automatically used inside a route. Do not call it manually.";
            }
        }
    }
}