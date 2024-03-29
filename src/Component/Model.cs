/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using System.Linq;
using Dict=System.Collections.Generic.Dictionary<string,object>;
using ParameterDictionary=System.Collections.Generic.Dictionary<string,Ligral.Component.Parameter>;
using System.Text;
using MathNet.Numerics.LinearAlgebra;
using Ligral.Syntax;
using Ligral.Syntax.CodeASTs;
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
        protected Dict configuration;
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
        public string GlobalName
        {get
            {
                if (Scope != null)
                {
                    return Scope.Replace('.', '_') + "_" + Name;
                }
                else
                {
                    return Name;
                }
            }
        }
        public string GivenName;
        public Token ModelToken {get; set;}
        protected virtual bool HasConfiguration {get => false; }
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
        public List<Matrix<double>> Results;
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
            Interpreter.Completed += Prepare;
            JsonLoader.Completed += Prepare;
            Inspector.Confirmed += Confirm;
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
                Document = DocString,
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
            Results = OutPortList.ConvertAll<Matrix<double>>(outPort => null);
        }
        protected virtual void AfterConfigured(){}
        public void Configure(Dict dictionary) 
        {
            if (IsConfigured)
            {
                return;
            }
            configuration = dictionary;
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
                try
                {
                    parameter.OnSet(value);
                }
                catch (System.Exception e)
                {
                    throw logger.Error(new ModelException(this, e.Message));
                }
            });
            AfterConfigured();
        }
        public virtual void Prepare() { }
        public virtual void Confirm() { }
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
        public virtual OutPort ExposeOutPort(int outPortNO)
        {
            if (outPortNO>=OutPortCount())
            {
                throw logger.Error(new ModelException(this, $"Out port number {outPortNO} exceeds boundary."));
            }
            else
            {
                return OutPortList[outPortNO];
            }
        }
        public virtual InPort ExposeInPort(int inPortNO)
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
        public virtual Port Expose(string portName)
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
        public virtual void Check()
        {
            try
            {
                OutPortList[0].SetShape(InPortList[0].RowNo, InPortList[0].ColNo);
                Results[0] = Matrix<double>.Build.Dense(InPortList[0].RowNo, InPortList[0].ColNo);
            }
            catch (LigralException)
            {
                throw logger.Error(new ModelException(this));
            }
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
        protected virtual List<Matrix<double>> Calculate(List<Matrix<double>> values)
        {
            for(int i=0; i<values.Count; i++)
            {
                values[i].CopyTo(Results[i]);
            }
            return Results;
        }
        public void Propagate()
        {
            List<Matrix<double>> inputs = InPortList.ConvertAll(inPort=>inPort.GetValue());
            List<Matrix<double>> outputs;
            try
            {
                outputs = Calculate(inputs);
            }
            catch (LigralException)
            {
                throw logger.Error(new ModelException(this));
            }
            catch (System.Exception e)
            {
                throw logger.Error(new ModelException(this, $"Error occurs while propagating (time={Solver.Time}): {e.Message}"));
            }
            if (outputs.Any(m => m.HasNAN()))
            {
                int i = outputs.FindIndex(m => m.HasNAN());
                throw logger.Error(new ModelException(this, $"NAN occurs while propagating (time={Solver.Time}) in output {OutPortList[i]}"));
            }
            if(outputs.Count != OutPortList.Count)
            {
                throw logger.Error(new ModelException(this, $"The number of outputs {outputs.Count} doesn't match that of the ports {OutPortList.Count} (time={Solver.Time})."));
            }
            else
            {
                for (int i=0; i<OutPortList.Count; i++)
                {
                    OutPortList[i].SetValue(outputs[i]);
                    OutPortList[i].Output();
                }
            }
        }
        public string GetTypeName()
        {
            return GetType().Name;
        }
        public static string GetVarName(string varName, Model model)
        {
            string inputSignalName = model.InPortCount() == 0 ? null :
                model.InPortList[0].Source is null ? null : model.InPortList[0].Source.SignalName;
            varName = varName ?? model.GivenName;
            if (varName != null && model.Scope != null)
            {
                varName = model.Scope +"_" + varName;
            }
            else if (varName == null && inputSignalName!= null) 
            {
                varName = inputSignalName;
            }
            else if (varName == null)
            {
                varName = model.Name;
                if (model.Scope != null) varName = model.Scope+"_"+varName;
            }
            return varName;
        }
        public List<DeclareCodeAST> ConstructTempVarDeclarationAST()
        {
            var declareCodeASTs = new List<DeclareCodeAST>();
            foreach (OutPort outPort in OutPortList)
            {
                DeclareCodeAST declareCodeAST = new DeclareCodeAST();
                declareCodeAST.Type = $"Matrix<double, {outPort.RowNo}, {outPort.ColNo}>";
                declareCodeAST.Instance = $"{GlobalName}_{outPort.Name}";
                declareCodeASTs.Add(declareCodeAST);
            }
            return declareCodeASTs;
        }
        public CallCodeAST ConstructConnectionAST()
        {
            CallCodeAST callCodeAST = new CallCodeAST();
            callCodeAST.FunctionName =  $"{GlobalName}.calculate";
            callCodeAST.Parameters = InPortList.ConvertAll(inPort => $"{inPort.Source.FatherModel.GlobalName}_{inPort.Source.Name}");
            callCodeAST.Results = OutPortList.ConvertAll(outPort => $"{GlobalName}_{outPort.Name}");
            return callCodeAST;
        }
        public virtual List<int> GetCharacterSize()
        {
            List<int> characterSize =  new List<int>();
            foreach (var inPort in InPortList)
            {
                characterSize.Add(inPort.RowNo);
                characterSize.Add(inPort.ColNo);
            }
            return characterSize;
        }
        public DeclareCodeAST ConstructDeclarationAST()
        {
            DeclareCodeAST declareCodeAST = new DeclareCodeAST();
            var size = GetCharacterSize();
            if (size.Count > 0)
            {
                declareCodeAST.Type = $"{GetTypeName()}<{string.Join(',', size.ConvertAll(n => n.ToString()))}>";
            }
            else
            {
                declareCodeAST.Type = GetTypeName();
            }
            declareCodeAST.Instance= GlobalName;
            return declareCodeAST;
        }
        public virtual List<CodeAST> ConstructConfigurationAST()
        {
            return null;
        }
        public virtual List<CodeAST> ConstructRefreshAST()
        {
            return null;
        }
        public override string ToString()
        {
            return $"{GetType().Name}({Name})";
        }
    }
}