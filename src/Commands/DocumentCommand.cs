/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.IO;
using System.Text.Json;
using System;
using System.Collections.Generic;
using Ligral.Component;
using Ligral.Extension;

namespace Ligral.Commands
{
    class DocumentCommand : Command
    {
        public string ModelName;
        public bool? ToJson;
        public string OutputFolder;
        public override string HelpInfo {get => @"Command: doc & document
    Position parameter:
        ModelName           optional string
            if exist        return the document of this specific model.
            else            return documents of all models.
    Named parameters:
        --json & -j         boolean
            if true         output the definition(s) in JSON format.
            else            print document(s) on the screen.
        --output & -o       string
            if given        output JSON file in the given folder.
            else            output JSON file in the startup folder.
    Examples:
        ligral doc          print all documents on the screen
        ligral doc --json -o def
                            output all JSON definitions to the `def` folder.
        ligral doc Sin      print the document of the model `Sin`.
";}
        public override void Run()
        {
            Settings settings = Settings.GetInstance();
            try
            {
                settings.GetDefaultSettings();
            }
            catch (LigralException)
            {
                logger.Error(new LigralException("Default settings is not valid, ligral exited with errors."));
                logger.Throw();
                return;
            }
            PluginLoader pluginLoader = new PluginLoader();
            pluginLoader.Load();
            List<Model> models = new List<Model>();
            if (ModelName is string modelName)
            {
                if (ModelManager.ModelTypePool.ContainsKey(modelName))
                {
                    models.Add(ModelManager.Create(modelName));
                }
                else
                {
                    logger.Error(new OptionException(modelName, $"No model named {modelName}"));
                    logger.Throw();
                    return;
                }
            }
            else
            {
                foreach (string modelType in ModelManager.ModelTypePool.Keys)
                {
                    if (modelType.Contains('<')) continue;
                    models.Add(ModelManager.Create(modelType));
                }
            }
            if (ToJson is bool toJson && toJson)
            {
                if (OutputFolder is string outputFolder)
                {
                    settings.OutputFolder = outputFolder;
                }
                if (!Directory.Exists(settings.OutputFolder))
                {
                    Directory.CreateDirectory(settings.OutputFolder);
                }
                foreach (Model model in models)
                {
                    ModelDocument modelDocument = model.GetDocStruct();
                    string modelJson = JsonSerializer.Serialize<ModelDocument>(
                        modelDocument, new JsonSerializerOptions() {WriteIndented = true}
                    );
                    string modelJsonFileName = Path.Join(settings.OutputFolder, $"{modelDocument.Type}.mdl.json");
                    try
                    {
                        File.WriteAllText(modelJsonFileName, modelJson);
                    }
                    catch (Exception e)
                    {
                        logger.Error(new LigralException($"Cannot write to {modelJsonFileName}: {e.Message}"));
                        logger.Throw();
                        return;
                    }
                }
            }
            else
            {
                if (!(OutputFolder is null))
                {
                    logger.Error(new OptionException("Output folder is only needed when mdl.json is requested."));
                    logger.Throw();
                    return;
                }
                foreach (Model model in models)
                {
                    Console.WriteLine(model.GetDoc());
                }
            }
        }
    }
}