using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System;
using System.IO;
using Ligral.Component;

namespace Ligral.Syntax
{
    struct JParameter
    {
        [JsonPropertyName("name")]
        public string Name {get; set;}
        [JsonPropertyName("value")]
        public object Value {get; set;}
    }
    struct JOutPort
    {
        [JsonPropertyName("name")]
        public string OutPortName {get; set;}
        [JsonPropertyName("destination")]
        public JInPort[] Destination {get; set;}
    }
    struct JInPort
    {
        [JsonPropertyName("id")]
        public string Id {get; set;}
        [JsonPropertyName("in-port")]
        public string InPortName {get; set;}
    }
    struct JModel
    {
        [JsonPropertyName("id")]
        public string Id {get; set;}
        [JsonPropertyName("type")]
        public string Type {get; set;}
        [JsonPropertyName("parameters")]
        public JParameter[] Parameters {get; set;}
        [JsonPropertyName("out-ports")]
        public JOutPort[] OutPorts {get; set;}
    }
    class JsonLoader
    {
        private Logger logger = new Logger("JsonCoder");
        private ScopeSymbolTable symbolTable = new ScopeSymbolTable("<global>", 0);
        public void Load(string fileName)
        {
            if (!File.Exists(fileName))
            {
                throw logger.Error(new LigralException($"File {fileName} not found."));
            }
            string text = File.ReadAllText(fileName);
            JModel[] models;
            try
            {
                models = JsonSerializer.Deserialize<JModel[]>(text);
            }
            catch (Exception e)
            {
                throw new LigralException($"Json deserialize failed.\n{e.Message}");
            }
            foreach (JModel model in models)
            {
                Rigister(model);
            }
            foreach (JModel model in models)
            {
                Construct(model);
            }
        }
        private void Rigister(JModel jModel)
        {
            Model model = ModelManager.Create(jModel.Type);
            model.Name = jModel.Id;
            TypeSymbol typeSymbol = symbolTable.Lookup("MODEL") as TypeSymbol;
            ModelSymbol modelSymbol = new ModelSymbol(jModel.Id, typeSymbol, model);
            symbolTable.Insert(modelSymbol);
            var parameters = Config(jModel.Parameters);
            model.Configure(parameters);
        }
        private void Construct(JModel jModel)
        {
            ModelSymbol modelSymbol = symbolTable.Lookup(jModel.Id) as ModelSymbol;
            if (modelSymbol is null)
            {
                throw logger.Error(new LigralException($"Reference {jModel.Id} not exist"));
            }
            Model model = modelSymbol.GetValue() as Model;
            foreach (JOutPort jOutPort in jModel.OutPorts)
            {
                Connect(jOutPort, model);
            }
        }
        private Dictionary<string, object> Config(JParameter[] jParameters)
        {
            var dict = new Dictionary<string, object>();
            foreach (JParameter jParameter in jParameters)
            {
                JsonElement jsonElement = (JsonElement) jParameter.Value;
                if (jsonElement.ValueKind == JsonValueKind.Number)
                {
                    double digit = jsonElement.GetDouble();
                    dict[jParameter.Name] = digit;
                }
                else if (jsonElement.ValueKind == JsonValueKind.String)
                {
                    string text = jsonElement.GetString();
                    dict[jParameter.Name] = text;
                }
            }
            return dict;
        }
        private void Connect(JOutPort jOutPort, Model model)
        {
            foreach (JInPort jInPort in jOutPort.Destination)
            {
                model.Connect(jOutPort.OutPortName, Find(jInPort));
            }
        }
        private InPort Find(JInPort jInPort)
        {
            ModelSymbol modelSymbol = symbolTable.Lookup(jInPort.Id) as ModelSymbol;
            if (modelSymbol is null)
            {
                throw logger.Error(new LigralException($"Reference {jInPort.Id} not exist"));
            }
            Model model = modelSymbol.GetValue() as Model;
            InPort inPort = model.Expose(jInPort.InPortName) as InPort;
            if (inPort is null)
            {
                throw logger.Error(new LigralException($"{jInPort.InPortName} is not in port."));
            }
            return inPort;
        }
    }
}