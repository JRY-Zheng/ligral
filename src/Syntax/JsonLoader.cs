/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

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
    class JsonLoader
    {
        private Logger logger = new Logger("JsonCoder");
        private ScopeSymbolTable symbolTable = new ScopeSymbolTable("<global>", 0);
        private JProject project;
        public void Load(string fileName)
        {
            if (!File.Exists(fileName))
            {
                throw logger.Error(new NotFoundException($"File {fileName}"));
            }
            string text = File.ReadAllText(fileName);
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
                project.Settings = new JConfig[0];
            }
            if (project.Models == null)
            {
                throw logger.Error(new NotFoundException("Property `models` is not found."));
            }
            if (project.Groups == null)
            {
                project.Groups = new JGroup[0];
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
                    throw logger.Error(new NotFoundException("Property `item`"));
                }
                if (config.Value == null)
                {
                    throw logger.Error(new NotFoundException("Property `value`"));
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
        private bool IsGroup(string groupName)
        {
            return project.Groups.Any(group => group.Name == groupName);
        }
        private JGroup Find(string groupName)
        {
            return project.Groups.First(group => group.Name == groupName);
        }
        private void Declare(JModel[] models)
        {
            foreach (JModel model in models)
            {
                Register(model);
            }
            foreach (JModel model in models)
            {
                Construct(model);
            }
        }
        private void Register(JModel jModel)
        {
            if (jModel.Type is null) 
            {
                throw logger.Error(new NotFoundException("Property `type`"));
            }
            if (IsGroup(jModel.Type))
            {
                JGroup jGroup = Find(jModel.Type);
                var scopedSymbolTable = new ScopeSymbolTable(jGroup.Name, symbolTable.ScopeLevel+1, symbolTable);
                var originSymbolTable = symbolTable;
                symbolTable = scopedSymbolTable;
                foreach (var jSubModel in jGroup.Models)
                {
                    Register(jSubModel);
                }
                foreach (var jSubModel in jGroup.Models)
                {
                    Construct(jSubModel);
                }
                Group group = new Group();
                foreach (var jInPort in jGroup.InPorts)
                {
                    ModelSymbol modelSymbol = symbolTable.Lookup(jInPort.InputID) as ModelSymbol;
                    if (modelSymbol is null)
                    {
                        throw logger.Error(new NotFoundException($"Reference {jInPort.InputID} not found"));
                    }
                    if (modelSymbol.GetValue() is ILinkable linkable)
                    {
                        if (linkable.Expose(jInPort.InputPort) is InPort inPort)
                        {
                            group.AddInputModel(inPort);
                        }
                        else
                        {
                            throw logger.Error(new LigralException($"No in port named {jInPort.InputPort} in {jInPort.InputID}"));
                        }
                        group.AddInPortName(jInPort.Name);
                    }
                    else
                    {
                        throw logger.Error(new LigralException($"{jInPort.InputPort} is not model or group"));
                    }
                }
                foreach (var jOutPort in jGroup.OutPorts)
                {
                    ModelSymbol modelSymbol = symbolTable.Lookup(jOutPort.OutputID) as ModelSymbol;
                    if (modelSymbol is null)
                    {
                        throw logger.Error(new NotFoundException($"Reference {jOutPort.OutputID} not found"));
                    }
                    if (modelSymbol.GetValue() is ILinkable linkable)
                    {
                        if (linkable.Expose(jOutPort.OutputPort) is OutPort outPort)
                        {
                            group.AddOutputModel(outPort);
                        }
                        else
                        {
                            throw logger.Error(new LigralException($"No out port named {jOutPort.OutputPort} in {jOutPort.OutputID}"));
                        }
                        group.AddOutPortName(jOutPort.Name);
                    }
                    else
                    {
                        throw logger.Error(new LigralException($"{jOutPort.OutputPort} is not model or group"));
                    }
                }
                symbolTable = originSymbolTable;
                TypeSymbol typeSymbol = symbolTable.Lookup("GROUP") as TypeSymbol;
                ModelSymbol groupSymbol = new ModelSymbol(jModel.Id, typeSymbol, group);
                symbolTable.Insert(groupSymbol);
            }
            else
            {
                Model model = ModelManager.Create(jModel.Type, null);
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
        }
        private void Construct(JModel jModel)
        {
            ModelSymbol modelSymbol = symbolTable.Lookup(jModel.Id) as ModelSymbol;
            if (modelSymbol is null)
            {
                throw logger.Error(new NotFoundException($"Reference {jModel.Id}"));
            }
            ILinkable model = modelSymbol.GetValue() as ILinkable;
            if (jModel.OutPorts is null) 
            {
                throw logger.Error(new LigralException("Property `out-ports` not found."));
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
        private void Connect(JOutPort jOutPort, ILinkable model)
        {
            if (jOutPort.OutPortName is null) 
            {
                throw logger.Error(new LigralException("Property `name` not found."));
            }
            if (jOutPort.Destinations is null) 
            {
                throw logger.Error(new LigralException("Property `destinations` not found."));
            }
            foreach (JInPort jInPort in jOutPort.Destinations)
            {
                // model.Connect(jOutPort.OutPortName, Find(jInPort, model));
                var outPort = model.Expose(jOutPort.OutPortName) as OutPort;
                if (outPort == null)
                {
                    throw logger.Error(new LigralException($"{jOutPort.OutPortName} is not out port"));
                }
                outPort.Bind(Find(jInPort, model));
            }
        }
        private InPort Find(JInPort jInPort, ILinkable sourceModel)
        {
            if (jInPort.Id is null) 
            {
                throw logger.Error(new LigralException("Property `id` not found."));
            }
            if (jInPort.InPortName is null) 
            {
                throw logger.Error(new LigralException("Property `in-port` not found."));
            }
            ModelSymbol modelSymbol = symbolTable.Lookup(jInPort.Id) as ModelSymbol;
            if (modelSymbol is null)
            {
                throw logger.Error(new LigralException($"Reference {jInPort.Id} not exist"));
            }
            ILinkable destinationModel = modelSymbol.GetValue() as ILinkable;
            InPort inPort = destinationModel.Expose(jInPort.InPortName) as InPort;
            if (inPort is null)
            {
                throw logger.Error(new LigralException($"{jInPort.InPortName} is not in port."));
            }
            return inPort;
        }
    }
}