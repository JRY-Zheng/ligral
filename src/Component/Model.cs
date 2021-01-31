/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using System.Linq;
using Dict=System.Collections.Generic.Dictionary<string,object>;
using ParameterDictionary=System.Collections.Generic.Dictionary<string,Ligral.Component.Parameter>;
using System.Text;
using Ligral.Syntax;
using Ligral.Simulation;

namespace Ligral.Component
{
    public class Model : ILinkable
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
        public bool IsConfigured {get; set;}
        public string DefaultName;
        private string scope;
        public string Scope
        {
            get
            {
                return scope;
            }
            set
            {
                if (value != "<global>")
                {
                    scope = value;
                }
            }
        }
        public string ScopedName
        {
            get
            {
                if (Scope != null)
                {
                    return Scope + "." + Name;
                }
                else
                {
                    return Name;
                }
            }
        }
        public string GivenName;
        protected List<InPort> InPortList;
        protected List<OutPort> OutPortList;
        protected Logger loggerInstance;
        private static Logger modelLogger = new Logger("Model");
        protected Logger logger
        {
            get
            {
                if (loggerInstance is null)
                {
                    loggerInstance = new Logger(Name);
                }
                return loggerInstance;
            }
        }
        public List<string> InPortsName 
        {
            get {return InPortList.ConvertAll(InPort => InPort.Name);}
        }
        public List<string> OutPortsName 
        {
            get {return OutPortList.ConvertAll(InPort => InPort.Name);}
        }
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
            Interpreter.Completed += Prepare;
            Observation.Stepped += Refresh;
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
            stringBuilder.Append("Input Port:\t\t");
            stringBuilder.AppendLine(string.Join(',',InPortList.ConvertAll(inPort=>inPort.Name)));
            stringBuilder.Append("Output Port:\t\t");
            stringBuilder.AppendLine(string.Join(',',OutPortList.ConvertAll(outPort=>outPort.Name)));
            stringBuilder.Append("Input Port Variable:\t");
            stringBuilder.AppendLine((this is InPortVariableModel).ToString());
            stringBuilder.Append("Output Port Variable:\t");
            stringBuilder.AppendLine((this is OutPortVariableModel).ToString());
            stringBuilder.Append("Parameter list:\t\t");
            if (Parameters.Count == 0)
            {
                stringBuilder.AppendLine("No parameter");
            }
            else
            {
                stringBuilder.AppendLine("");
            }
            foreach(string parameterName in Parameters.Keys)
            {
                stringBuilder.Append("\t");
                stringBuilder.Append(parameterName);
                stringBuilder.Append("\tRequired:");
                stringBuilder.AppendLine(Parameters[parameterName].Required.ToString());
            }
            return stringBuilder.ToString();
        }
        public ModelDocument GetDocStruct()
        {
            return new ModelDocument() 
            {
                Type = GetType().Name,
                Parameters = Parameters.Keys.ToList().ConvertAll(key => new ParameterDocument()
                {
                    Name = key,
                    Type = Parameters[key].Type.ToString("f").ToLower(),
                    Required = Parameters[key].Required
                }).ToArray(),
                InPorts = InPortList.ConvertAll(inPort => inPort.Name).ToArray(),
                OutPorts = OutPortList.ConvertAll(outPort => outPort.Name).ToArray(),
                InPortVariable = this is InPortVariableModel,
                OutPortVariable = this is OutPortVariableModel
            };
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
        public void Configure(Dict dictionary) 
        {
            if (IsConfigured)
            {
                return;
            }
            IsConfigured = true;
            // ConfigureAction(dictionary);
            dictionary.Keys.Except(Parameters.Keys).ToList().ForEach(unknownParameterName=>
            {
                throw logger.Error(new ModelException(this, $"Unknown Parameter {unknownParameterName}"));
            });
            Parameters.Keys.Except(dictionary.Keys).ToList().ForEach(defaultParameterName=>
            {
                Parameter parameter = Parameters[defaultParameterName];
                if (parameter.Required)
                {
                    throw logger.Error(new ModelException(this, $"The parameter {defaultParameterName} requires value assignment"));
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
                parameter.OnSet(value);
            });
            AfterConfigured();
        }
        public virtual void Prepare() { }
        public virtual void Refresh() { }
        public virtual void Release() { }
        public virtual void Connect(int outPortNO, InPort inPort)
        {
            if (outPortNO >= OutPortCount())
            {
                throw logger.Error(new ModelException(this, $"Out port number {outPortNO} exceed boundary"));
            }
            else
            {
                OutPortList[outPortNO].Bind(inPort);
            }
        }
        public virtual void Connect(string outPortName, InPort inPort)
        {
            int outPortNO = OutPortList.FindIndex(outPort=>outPort.Name==outPortName);
            if (outPortNO < 0)
            {
                throw logger.Error(new ModelException(this, $"Out port {outPortName} not found."));
            }
            Connect(outPortNO, inPort);
        }
        public virtual InPort Expose(int inPortNO)
        {
            if (inPortNO>=InPortCount())
            {
                throw logger.Error(new ModelException(this, $"In port number {inPortNO} exceeds boundary."));
            }
            else
            {
                return InPortList[inPortNO];
            }
        }
        public Port Expose(string portName)
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
                throw logger.Error(new ModelException(this, $"Undefined port name {portName} in {GetType().Name}"));
            }
        }
        public int InPortCount()
        {
            return InPortList.Count;
        }
        public int OutPortCount()
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
                throw logger.Error(new ModelException(this, $"The number of outputs {outputs.Count} doesn't match that of the ports {OutPortList.Count}."));
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
        public string GetTypeName()
        {
            return GetType().Name;
        }
        public override string ToString()
        {
            return $"{GetType().Name}({Name})";
        }
    }
}