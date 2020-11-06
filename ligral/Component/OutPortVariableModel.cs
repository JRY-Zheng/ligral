namespace Ligral.Component
{
    class OutPortVariableModel : Model
    {
        
        protected override void SetUpPorts()
        {
            OutPortList.Add(new OutPort("out0", this));
        }
        public override void Connect(int outPortNO, InPort inPort)
        {
            if (outPortNO == OutPortCount())
            {
                OutPort outPort = new OutPort($"out{outPortNO}", this);
                OutPortList.Add(outPort);
                outPort.Bind(inPort);
                Results.Add(new Signal(outPort.SignalName));
            }
            else
            {
                base.Connect(outPortNO, inPort);
            }
        }
    }
}