/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;

namespace Ligral.Component
{
    public class Port : ILinkable
    {
        public string Name;
        public Model FatherModel;
        public int RowNo = 0;
        public int ColNo = 0;
        protected Matrix<double> Value;
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

        public virtual InPort ExposeInPort(int inPortNO)
        {
            throw new System.NotImplementedException();
        }
        public virtual OutPort ExposeOutPort(int outPortNO)
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

    public class InPort : Port
    {
        public bool Visited = false;
        public OutPort Source;
        public delegate void InPortValueReceivedHandler(Matrix<double> value);
        public event InPortValueReceivedHandler InPortValueReceived;
        public InPort(string name, Model model) : base(name, model) {}
        public void Input(Matrix<double> value)
        {
            Value = value;
            if (InPortValueReceived != null) InPortValueReceived(value);
        }
        public Matrix<double> GetValue()
        {
            return Value;
        }
        public override void Connect(int outPortNO, InPort inPort)
        {
            throw logger.Error(new ModelException(FatherModel, "Cannot link in port to in port"));
        }

        public override InPort ExposeInPort(int inPortNO)
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

    public class OutPort : Port
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
        private List<string> destinationWordList;
        public List<string> DestinationWordList
        {
            get
            {
                if (destinationWordList == null)
                {
                    destinationWordList = destinationList.ConvertAll(inPort => $"{inPort.FatherModel.Name}_{inPort.Name}");
                }
                return destinationWordList;
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
                throw logger.Error(new ModelException(FatherModel, "Duplicated binding of InPort."));
            }
            else
            {
                destinationList.Add(inPort);
                inPort.Source = this;
            }
        }
        public override OutPort ExposeOutPort(int outPortNO)
        {
            return this;
        }
        public void SetShape(int rowNo, int colNo)
        {
            ColNo = colNo;
            RowNo = rowNo;
            foreach (var inPort in destinationList)
            {
                if (inPort.RowNo<=0)
                {
                    inPort.RowNo = rowNo;
                }
                else if (rowNo != inPort.RowNo)
                {
                    throw logger.Error(new ModelException(FatherModel, $"Shape check fail, row numbers should be {inPort.RowNo}, but got {rowNo}"));
                }
                if (inPort.ColNo<=0)
                {
                    inPort.ColNo = colNo;
                }
                else if (colNo != inPort.ColNo)
                {
                    throw logger.Error(new ModelException(FatherModel, $"Shape check fail, column numbers should be {inPort.ColNo}, but got {colNo}"));
                }
            }
        }
        public void SetValue(Matrix<double> value)
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
                throw logger.Error(new ModelException(FatherModel, "Out port has only single Matrix<double>"));
            }
        }

        public override InPort ExposeInPort(int inPortNO)
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