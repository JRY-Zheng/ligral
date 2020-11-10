using System.Collections.Generic;
using System.Linq;
using Dict=System.Collections.Generic.Dictionary<string,object>;
using ParameterDictionary=System.Collections.Generic.Dictionary<string,Ligral.Component.Parameter>;
using System.Text;
using Ligral.Component.Models;
using Ligral.Simulation;

namespace Ligral.Component
{
    class Model : ModelBase
    {
        protected static int id = 0;
        public string Name 
        {
            get
            {
                return GivenName ?? DefaultName;
            }
            set
            {
                GivenName = value;
            }
        }
        public string DefaultName;
        public string GivenName;
        protected List<InPort> InPortList;
        protected List<OutPort> OutPortList;
        // public bool Initializeable = false;
        // protected bool Initialized = false;
        public List<Signal> Results;
        public delegate List<Signal> CalculateHandler(List<Signal> values);
        public CalculateHandler Calculate;
        protected virtual string DocString 
        {
            get
            {
                return "This is a model.";
            }
        }
        protected ParameterDictionary Parameters;
        public Model()
        {
            id += 1;
            DefaultName = GetType().Name + id.ToString();
            InPortList = new List<InPort>();
            OutPortList = new List<OutPort>();
            Calculate = DefaultCalculate;
            Observation.Stopped += Release;
            SetUpPorts();
            SetUpResults();
            SetUpParameters();
        }
        public string GetDoc()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(GetType().Name);
            stringBuilder.AppendLine(DocString);
            stringBuilder.Append("Input Port:\t");
            stringBuilder.AppendLine(string.Join(',',InPortList.ConvertAll(inPort=>inPort.Name)));
            stringBuilder.Append("Output Port:\t");
            stringBuilder.AppendLine(string.Join(',',OutPortList.ConvertAll(outPort=>outPort.Name)));
            stringBuilder.AppendLine("Parameter list:");
            foreach(string parameterName in Parameters.Keys)
            {
                stringBuilder.Append("\t");
                stringBuilder.Append(parameterName);
                stringBuilder.Append("\tRequired:");
                stringBuilder.AppendLine(Parameters[parameterName].Required.ToString());
            }
            return stringBuilder.ToString();
        }
        protected virtual void SetUpParameters() 
        {
            Parameters = new ParameterDictionary();
        }
        protected virtual void SetUpPorts()
        {
            InPortList.Add(new InPort("input", this));
            OutPortList.Add(new OutPort("output", this));
        }
        protected void SetUpResults()
        {
            Results = OutPortList.ConvertAll(outPort => new Signal(outPort));
        }
        protected virtual void AfterConfigured(){}
        public override void Configure(Dict dictionary) 
        {
            if (Configured)
            {
                return;
            }
            Configured = true;
            // ConfigureAction(dictionary);
            dictionary.Keys.Except(Parameters.Keys).ToList().ForEach(unknownParameterName=>
            {
                throw new LigralException($"Unknown Parameter {unknownParameterName}");
            });
            Parameters.Keys.Except(dictionary.Keys).ToList().ForEach(defaultParameterName=>
            {
                Parameter parameter = Parameters[defaultParameterName];
                if (parameter.Required)
                {
                    throw new LigralException($"The parameter {defaultParameterName} requires value assignment");
                }
                else
                {
                    parameter.OnDefault();
                }
            });
            Parameters.Keys.Intersect(dictionary.Keys).ToList().ForEach(parameterName=>
            {
                Parameter parameter = Parameters[parameterName];
                object value = dictionary[parameterName];
                // try
                // {
                    parameter.OnSet(value);
                // }
                // catch (System.Exception e)
                // {
                //     throw new LigralException($"type of {value} is unsupported for {parameterName} of {Name}\n{e}");
                // }
            });
            AfterConfigured();
        }
        protected virtual void ConfigureAction(Dict dictionary) {}
        public virtual void Release() { }
        public override void Connect(int outPortNO, InPort inPort)
        {
            if (outPortNO >= OutPortCount())
            {
                throw new LigralException($"Out port number {outPortNO} exceed boundary");
            }
            else
            {
                OutPortList[outPortNO].Bind(inPort);
            }
        }
        public override InPort Expose(int inPortNO)
        {
            if (inPortNO>=InPortCount())
            {
                throw new LigralException($"In port number {inPortNO} exceeds boundary");
            }
            else
            {
                return InPortList[inPortNO];
            }
        }
        public override Port Expose(string portName)
        {
            int inPortNO = InPortList.FindIndex(inPort=>inPort.Name==portName);
            int outPortNO = OutPortList.FindIndex(outPort=>outPort.Name==portName);
            if (inPortNO>=0)
            {
                return InPortList[inPortNO];
            }
            else if (outPortNO>=0)
            {
                return OutPortList[outPortNO];
            }
            else
            {
                throw new LigralException("Undefined port name "+portName);
            }
        }
        public override int InPortCount()
        {
            return InPortList.Count;
        }
        public override int OutPortCount()
        {
            return OutPortList.Count;
        }
        public virtual bool IsReady()
        {
            return InPortList.All(inPort=>inPort.Visited);
        }
        public bool IsConnected()
        {
            return InPortList.All(inPort=>inPort.Source!=null);
        }
        public List<Model> Inspect()
        {
            List<Model> destinationList = new List<Model>();
            foreach (List<Model> list in OutPortList.ConvertAll(outPort=>outPort.Visit()))
            {
                destinationList.AddRange(list);
            }
            return destinationList;
        }
        protected virtual List<Signal> DefaultCalculate(List<Signal> values)
        {
            foreach (var pair in Results.Zip(values))
            {
                Signal outputSignal = pair.First;
                Signal inputSignal = pair.Second;
                outputSignal.Clone(inputSignal);
            }
            return Results;
        }
        public void Propagate()
        {
            List<Signal> inputs = InPortList.ConvertAll(inPort=>inPort.GetValue());
            List<Signal> outputs = Calculate(inputs);
            if(outputs.Count != OutPortList.Count)
            {
                throw new LigralException($"The number of outputs {outputs.Count} doesn't match that of the ports {OutPortList.Count}. in {Name}");
            }
            else
            {
                OutPortList.Zip(outputs).ToList().ForEach(pair=>
                {
                    pair.First.SetValue(pair.Second);
                    pair.First.Output();
                });
            }
        }
        public override string GetTypeName()
        {
            return GetType().Name;
        }
        public override string ToString()
        {
            return $"{GetType().Name}({Name})";
        }
        public static Group operator+(Model left, Model right)
        {
            if (left.OutPortCount()!=1||right.OutPortCount()!=1)
            {
                throw new LigralException("Out port number should be 1 when adding together");
            }
            Calculator calculator = ModelManager.Create("Calculator") as Calculator;
            Dict dictionary = new Dict(){{"type", '+'}};
            calculator.Configure(dictionary);
            left.Connect(0, calculator.Expose(0));
            right.Connect(0, calculator.Expose(1));
            Group group = new Group();
            group.AddInputModel(left);
            group.AddInputModel(right);
            group.AddOutputModel(calculator);
            return group;
        }
        public static Group operator+(Model model)
        {
            Group group = new Group();
            group.AddInputModel(model);
            group.AddOutputModel(model);
            return group;
        }
        public static Group operator-(Model left, Model right)
        {
            if (left.OutPortCount()!=1||right.OutPortCount()!=1)
            {
                throw new LigralException("Out port number should be 1 when subtracting");
            }
            Calculator calculator = ModelManager.Create("Calculator") as Calculator;
            Dict dictionary = new Dict(){{"type", '-'}};
            calculator.Configure(dictionary);
            left.Connect(0, calculator.Expose(0));
            right.Connect(0, calculator.Expose(1));
            Group group = new Group();
            group.AddInputModel(left);
            group.AddInputModel(right);
            group.AddOutputModel(calculator);
            return group;
        }
        public static Group operator-(Model model)
        {
            Gain gain = ModelManager.Create("Gain") as Gain;
            Dict dictionary = new Dict(){{"value", -1}};
            gain.Configure(dictionary);
            model.Connect(gain);
            Group group = new Group();
            group.AddInputModel(model);
            group.AddOutputModel(gain);
            return group;
        }
        public static Group operator*(Model left, Model right)
        {
            if (left.OutPortCount()!=1||right.OutPortCount()!=1)
            {
                throw new LigralException("Out port number should be 1 when multiplying");
            }
            Calculator calculator = ModelManager.Create("Calculator") as Calculator;
            Dict dictionary = new Dict(){{"type", '*'}};
            calculator.Configure(dictionary);
            left.Connect(0, calculator.Expose(0));
            right.Connect(0, calculator.Expose(1));
            Group group = new Group();
            group.AddInputModel(left);
            group.AddInputModel(right);
            group.AddOutputModel(calculator);
            return group;
        }
        public static Group operator/(Model left, Model right)
        {
            if (left.OutPortCount()!=1||right.OutPortCount()!=1)
            {
                throw new LigralException("Out port number should be 1 when dividing");
            }
            Calculator calculator = ModelManager.Create("Calculator") as Calculator;
            Dict dictionary = new Dict(){{"type", '/'}};
            calculator.Configure(dictionary);
            left.Connect(0, calculator.Expose(0));
            right.Connect(0, calculator.Expose(1));
            Group group = new Group();
            group.AddInputModel(left);
            group.AddInputModel(right);
            group.AddOutputModel(calculator);
            return group;
        }
        public static Group operator^(Model left, Model right)
        {
            if (left.OutPortCount()!=1||right.OutPortCount()!=1)
            {
                throw new LigralException("Out port number should be 1 when powering");
            }
            Calculator calculator = ModelManager.Create("Calculator") as Calculator;
            Dict dictionary = new Dict(){{"type", '^'}};
            calculator.Configure(dictionary);
            left.Connect(0, calculator.Expose(0));
            right.Connect(0, calculator.Expose(1));
            Group group = new Group();
            group.AddInputModel(left);
            group.AddInputModel(right);
            group.AddOutputModel(calculator);
            return group;
        }
    }
}