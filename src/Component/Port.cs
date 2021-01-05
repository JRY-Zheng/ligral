/* Copyright 2019-2020 Junruoyu Zheng. All rights reserved.

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;

namespace Ligral.Component
{
    class Port : ILinkable
    {
        public string Name;
        public Model FatherModel;
        protected Signal Value;
        protected Logger logger;
        protected Port(string name, Model model)
        {
            Name = name;
            FatherModel = model;
            logger = new Logger($"Port({Name})");
        }

        public bool IsConfigured { get; set; }

        public void Configure(Dictionary<string, object> dictionary)
        {
            
        }

        public virtual void Connect(int outPortNO, InPort inPort)
        {
            throw new System.NotImplementedException();
        }

        public virtual InPort Expose(int inPortNO)
        {
            throw new System.NotImplementedException();
        }

        public virtual Port Expose(string portName)
        {
            throw new System.NotImplementedException();
        }

        public virtual string GetTypeName()
        {
            throw new System.NotImplementedException();
        }

        public virtual int InPortCount()
        {
            throw new System.NotImplementedException();
        }

        public virtual int OutPortCount()
        {
            throw new System.NotImplementedException();
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
        public override void Connect(int outPortNO, InPort inPort)
        {
            throw logger.Error(new ModelException(FatherModel, "Cannot link in port to in port"));
        }

        public override InPort Expose(int inPortNO)
        {
            if (inPortNO == 0)
            {
                return this;
            }
            else
            {
                throw logger.Error(new ModelException(FatherModel, "Cannot link multiple signals to single in port"));
            }
        }

        public override Port Expose(string portName)
        {
            throw logger.Error(new ModelException(FatherModel, "Cannot expose ports from out port"));
        }

        public override string GetTypeName()
        {
            return "InPort";
        }

        public override int InPortCount()
        {
            return 1;
        }

        public override int OutPortCount()
        {
            return 0;
        }

        public override string ToString()
        {
            return string.Format("InPort({0}={1})", Name, Value);
        }
    }

    class OutPort : Port
    {
        private List<InPort> destinationList;
        private string signalName;
        public string SignalName
        {
            get 
            {
                if (signalName != null)
                {
                    return signalName;
                }
                else if (FatherModel.GivenName != null)
                {
                    string referenceName;
                    if (FatherModel.OutPortCount() == 1)
                    {
                        referenceName = FatherModel.GivenName;
                    }
                    else
                    {
                        referenceName = $"{FatherModel.GivenName}.{Name}";
                    }
                    if (FatherModel.Scope != null)
                    {
                        return FatherModel.Scope + "." + referenceName;
                    }
                    else
                    {
                        return referenceName;
                    }
                }
                else
                {
                    return null;
                }
            }
            set
            {
                signalName = value;
            }
        }
        public OutPort(string name, Model model) : base(name, model)
        {
            destinationList = new List<InPort>();
        }
        public void Bind(InPort inPort)
        {
            if(inPort.Source!=null)
            {
                throw logger.Error(new LigralException("Duplicated binding of InPort."));
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
        public override void Connect(int outPortNO, InPort inPort)
        {
            if (outPortNO == 0)
            {
                Bind(inPort);
            }
            else
            {
                throw logger.Error(new ModelException(FatherModel, "Out port has only single signal"));
            }
        }

        public override InPort Expose(int inPortNO)
        {
            throw logger.Error(new ModelException(FatherModel, "Out port has no in port"));
        }

        public override Port Expose(string portName)
        {
            throw logger.Error(new ModelException(FatherModel, "Cannot expose ports from out port"));
        }

        public override string GetTypeName()
        {
            return "OutPort";
        }

        public override int InPortCount()
        {
            return 0;
        }

        public override int OutPortCount()
        {
            return 1;
        }
        public override string ToString()
        {
            return string.Format("OutPort({0}={1})", Name, Value);
        }
    }
}