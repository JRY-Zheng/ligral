namespace Ligral.Component 
{
    class InPortVariableModel : Model
    {
        public override InPort Expose(int inPortNO)
        {
            if (inPortNO == InPortCount())
            {
                InPort inPort = new InPort($"in{inPortNO}", this);
                InPortList.Add(inPort);
                return inPort;
            }
            else
            {
                return base.Expose(inPortNO);
            }
        }
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("in0", this));
        }
    }
}