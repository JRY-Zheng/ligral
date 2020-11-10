using System.Collections.Generic;

namespace Ligral.Component
{
    class Port
    {
        public string Name;
        public Model FatherModel;
        protected Signal Value;
        public Port(string name, Model model)
        {
            Name = name;
            FatherModel = model;
        }
        public override string ToString()
        {
            return string.Format("Port({0}={1})", Name, Value);
        }
    }

    class InPort : Port
    {
        public bool Visited = false;
        public OutPort Source;
        public delegate void InPortValueReceivedHandler(Signal value);
        public event InPortValueReceivedHandler InPortValueReceived;
        public InPort(string name, Model model) : base(name, model) {}
        public void Input(Signal value)
        {
            Value = value;
            if (InPortValueReceived != null) InPortValueReceived(value);
        }
        public Signal GetValue()
        {
            return Value;
        }
        public override string ToString()
        {
            return string.Format("InPort({0}={1})", Name, Value);
        }
    }

    class OutPort : Port
    {
        private List<InPort> destinationList;
        public string SignalName;
        public OutPort(string name, Model model) : base(name, model)
        {
            destinationList = new List<InPort>();
        }
        public void Bind(InPort inPort)
        {
            if(inPort.Source!=null)
            {
                throw new LigralException("Duplicated binding of InPort.");
            }
            else
            {
                destinationList.Add(inPort);
                inPort.Source = this;
            }
        }
        public void SetValue(Signal value)
        {
            Value = value;
        }
        public void Output()
        {
            destinationList.ForEach(inPort=>inPort.Input(Value));
        }
        public List<Model> Visit()
        {
            destinationList.ForEach(inPort=>{inPort.Visited = true;});
            return destinationList.ConvertAll(inPort=>inPort.FatherModel);
        }
        public override string ToString()
        {
            return string.Format("OutPort({0}={1})", Name, Value);
        }
    }
}