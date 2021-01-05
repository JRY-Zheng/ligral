/* Copyright (C) 2019-2020 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
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
        public JInPort[] Destinations {get; set;}
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
    struct JConfig
    {
        [JsonPropertyName("item")]
        public string Item {get; set;}
        [JsonPropertyName("value")]
        public object Value {get; set;}
    }
    struct JProject
    {
        [JsonPropertyName("settings")]
        public JConfig[] Settings {get; set;}
        [JsonPropertyName("models")]
        public JModel[] Models {get; set;}
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
            JProject project;
            try
            {
                project = JsonSerializer.Deserialize<JProject>(text);
            }
            catch (Exception e)
            {
                throw logger.Error(new LigralException($"Json deserialize failed.\n{e.Message}"));
            }
            if (project.Settings == null)
            {
                throw new LigralException("Property `settings` not found.");
            }
            if (project.Models == null)
            {
                throw new LigralException("Property `settings` not found.");
            }
            logger.Info($"JsonLoader started at {fileName}");
            Apply(project.Settings);
            Declare(project.Models);
        }
        private void Apply(JConfig[] configs)
        {
            Settings settings = Settings.GetInstance();
            foreach (JConfig config in configs)
            {
                if (config.Item == null)
                {
                    throw new LigralException("Property `item` not found.");
                }
                if (config.Value == null)
                {
                    throw new LigralException("Property `value` not found.");
                }
                var jsonElement = (JsonElement) config.Value;
                switch (jsonElement.ValueKind)
                {
                case JsonValueKind.Number:
                    settings.AddSetting(config.Item, jsonElement.GetDouble());
                    break;
                case JsonValueKind.String:
                    settings.AddSetting(config.Item, jsonElement.GetString());
                    break;
                case JsonValueKind.True:
                case JsonValueKind.False:
                    settings.AddSetting(config.Item, jsonElement.GetBoolean());
                    break;
                case JsonValueKind.Object:
                    Dictionary<string, object> dict = new Dictionary<string, object>();
                    foreach (var item in jsonElement.EnumerateObject())
                    {
                        dict[item.Name] = GetSettingValue(item.Name, item.Value);
                    }
                    settings.AddSetting(config.Item, dict);
                    break;
                default:
                    throw logger.Error(new SettingException(config.Item, jsonElement, "Unsupported type"));
                }
            }
        }
        private object GetSettingValue(string name, JsonElement jsonElement)
        {
            switch (jsonElement.ValueKind)
            {
            case JsonValueKind.Number:
                return jsonElement.GetDouble();
            case JsonValueKind.String:
                return jsonElement.GetString();
            case JsonValueKind.True:
            case JsonValueKind.False:
                return jsonElement.GetBoolean();
            case JsonValueKind.Object:
                Dictionary<string, object> dict = new Dictionary<string, object>();
                foreach (var item in jsonElement.EnumerateObject())
                {
                    dict[item.Name] = GetSettingValue(name, item.Value);
                }
                return dict;
            default:
                throw logger.Error(new SettingException(name, jsonElement, "Unsupported type"));
            }
        }
        private void Declare(JModel[] models)
        {
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
            if (jModel.Type is null) 
            {
                throw logger.Error(new LigralException("Property `type` not found."));
            }
            Model model = ModelManager.Create(jModel.Type);
            if (jModel.Id is null) 
            {
                throw logger.Error(new ModelException(model, "Property `id` not found."));
            }
            model.Name = jModel.Id;
            TypeSymbol typeSymbol = symbolTable.Lookup("MODEL") as TypeSymbol;
            ModelSymbol modelSymbol = new ModelSymbol(jModel.Id, typeSymbol, model);
            symbolTable.Insert(modelSymbol);
            if (jModel.Parameters is null) 
            {
                throw logger.Error(new ModelException(model, "Property `parameters` not found."));
            }
            Config(jModel.Parameters, model);
        }
        private void Construct(JModel jModel)
        {
            ModelSymbol modelSymbol = symbolTable.Lookup(jModel.Id) as ModelSymbol;
            if (modelSymbol is null)
            {
                throw logger.Error(new LigralException($"Reference {jModel.Id} not exist"));
            }
            Model model = modelSymbol.GetValue() as Model;
            if (jModel.OutPorts is null) 
            {
                throw logger.Error(new ModelException(model, "Property `out-ports` not found."));
            }
            foreach (JOutPort jOutPort in jModel.OutPorts)
            {
                Connect(jOutPort, model);
            }
        }
        private void Config(JParameter[] jParameters, Model model)
        {
            var dict = new Dictionary<string, object>();
            foreach (JParameter jParameter in jParameters)
            {
                if (jParameter.Value is null) 
                {
                    throw logger.Error(new ModelException(model, "Property `value` not found."));
                }
                if (jParameter.Name is null) 
                {
                    throw logger.Error(new ModelException(model, "Property `name` not found."));
                }
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
                else if (jsonElement.ValueKind == JsonValueKind.Array)
                {
                    Matrix<double> matrix = null;
                    foreach (var row in jsonElement.EnumerateArray())
                    {
                        if (row.ValueKind != JsonValueKind.Array)
                        {
                            throw logger.Error(new ModelException(model, "Parameter must be a matrix rather than vector of number or string or object."));
                        }
                        Matrix<double> vector = null;
                        foreach (var item in row.EnumerateArray())
                        {
                            if (item.ValueKind != JsonValueKind.Number)
                            {
                                throw logger.Error(new ModelException(model, "Parameter must be a matrix of number."));
                            }
                            if (vector == null)
                            {
                                vector = DenseMatrix.Create(1, 1, item.GetDouble());
                            }
                            else
                            {
                                vector = vector.Append(DenseMatrix.Create(1, 1, item.GetDouble()));
                            }
                        }
                        if (matrix == null)
                        {
                            matrix = vector;
                        }
                        else
                        {
                            matrix = matrix.Stack(vector);
                        }
                    }
                    dict[jParameter.Name] = matrix;
                }
                else
                {
                    throw logger.Error(new ModelException(model, $"Unsupported type {jsonElement.ValueKind} in parameter"));
                }
            }
            model.Configure(dict);
        }
        private void Connect(JOutPort jOutPort, Model model)
        {
            if (jOutPort.OutPortName is null) 
            {
                throw logger.Error(new ModelException(model, "Property `name` not found."));
            }
            if (jOutPort.Destinations is null) 
            {
                throw logger.Error(new ModelException(model, "Property `destinations` not found."));
            }
            foreach (JInPort jInPort in jOutPort.Destinations)
            {
                model.Connect(jOutPort.OutPortName, Find(jInPort, model));
            }
        }
        private InPort Find(JInPort jInPort, Model sourceModel)
        {
            if (jInPort.Id is null) 
            {
                throw logger.Error(new ModelException(sourceModel, "Property `id` not found."));
            }
            if (jInPort.InPortName is null) 
            {
                throw logger.Error(new ModelException(sourceModel, "Property `in-port` not found."));
            }
            ModelSymbol modelSymbol = symbolTable.Lookup(jInPort.Id) as ModelSymbol;
            if (modelSymbol is null)
            {
                throw logger.Error(new LigralException($"Reference {jInPort.Id} not exist"));
            }
            Model destinationModel = modelSymbol.GetValue() as Model;
            InPort inPort = destinationModel.Expose(jInPort.InPortName) as InPort;
            if (inPort is null)
            {
                throw logger.Error(new LigralException($"{jInPort.InPortName} is not in port."));
            }
            return inPort;
        }
    }
}