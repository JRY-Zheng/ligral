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