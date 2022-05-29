/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.IO;
using System.Collections.Generic;
using Ligral.Component;
using Ligral.Syntax.CodeASTs;
using Ligral.Simulation;
using System.Reflection;

namespace Ligral.Syntax
{
    class Compiler
    {
        private Logger logger = new Logger("Compiler");
        private List<string> codeFiles = new List<string> 
        {
            "CMakeLists.txt", "context.h", "models.h",
            "main.cc", "solvers.h"
        };
        public void Compile(List<Model> routine)
        {
            var settings = Settings.GetInstance();
            string folder = settings.OutputFolder;
            if (!Directory.Exists(settings.OutputFolder))
            {
                Directory.CreateDirectory(settings.OutputFolder);
            }
            Assembly assembly = Assembly.Load("Ligral");
            foreach (var codeFile in codeFiles)
            {
                Stream stream = assembly.GetManifestResourceStream(codeFile);
                StreamReader reader = new StreamReader(stream);
                File.WriteAllText(Path.Join(folder, codeFile), reader.ReadToEnd());
            }
            File.WriteAllLines(Path.Join(folder, "config.h"), 
                GenerateConfig().ConvertAll(block => Visit(block)));
            File.WriteAllLines(Path.Join(folder, "project.h"), 
                GenerateProject(routine).ConvertAll(block => Visit(block)));
        }
        private List<CodeAST> GenerateConfig()
        {
            var settings = Settings.GetInstance();
            return new List<CodeAST>() 
            {
                new MacroCodeAST() { Macro = "ifndef", Definition = "CONFIG_H"}, 
                new MacroCodeAST() { Macro = "define", Definition = "CONFIG_H"}, 
                new MacroCodeAST() { Macro = "include", Definition = "\"solvers.h\""}, 
                new MacroCodeAST() { Macro = "include", Definition = "\"context.h\""}, 
                new MacroCodeAST() { Macro = "define", Definition = $"H {settings.StepSize}"}, 
                new MacroCodeAST() { Macro = "define", Definition = $"N {State.StatePool.Count}"}, 
                new MacroCodeAST() { Macro = "define", Definition = $"M {Observation.ObservationPool.Count}"}, 
                new MacroCodeAST() { Macro = "define", Definition = $"P {ControlInput.InputPool.Count}"}, 
                new MacroCodeAST() { Macro = "define", Definition = $"STEPS {settings.StopTime/settings.StepSize}"}, 
                new MacroCodeAST() { Macro = "define", Definition = $"integral {settings.SolverName}_integral<N>"}, 
                new MacroCodeAST() { Macro = "define", Definition = "context struct context_struct<M, N, P>"}, 
                new MacroCodeAST() { Macro = "endif"}
            };
        }
        private List<CodeAST> GenerateProject(List<Model> routine)
        {
            var projectCodeAST = new ClassCodeAST();
            projectCodeAST.ClassName = "project";
            projectCodeAST.publicASTs = new List<DeclareCodeAST>();
            var ctx = new DeclareCodeAST();
            ctx.Type = "context";
            ctx.Instance = "ctx";
            projectCodeAST.publicASTs.Add(ctx);
            foreach (var model in routine)
            {
                projectCodeAST.publicASTs.Add(model.ConstructDeclarationAST());
            }
            var init = new DeclareCodeAST();
            init.Type = "void";
            init.Instance = "init()";
            projectCodeAST.publicASTs.Add(init);
            var step = new DeclareCodeAST();
            step.Type = "void";
            step.Instance = "step()";
            projectCodeAST.publicASTs.Add(step);
            var initCodeAST = new FunctionCodeAST();
            var configurationASTs = routine.ConvertAll(model => model.ConstructConfigurationAST());
            initCodeAST.ReturnType = "void";
            initCodeAST.FunctionName = "project::init";
            initCodeAST.codeASTs = new List<CodeAST>();
            foreach (var asts in configurationASTs)
            {
                if (asts == null) continue;
                initCodeAST.codeASTs.AddRange(asts);
            }
            var declarationASTs = routine.ConvertAll(model => model.ConstructTempVarDeclarationAST());
            var connectionASTs = routine.ConvertAll(model => model.ConstructConnectionAST());
            var inputUpdateASTs = routine.FindAll(model => model is InitializeableModel).ConvertAll(model => ((InitializeableModel)model).ConstructInputUpdateAST());
            var stepCodeAST = new FunctionCodeAST();
            stepCodeAST.ReturnType = "void";
            stepCodeAST.FunctionName = "project::step";
            stepCodeAST.codeASTs = new List<CodeAST>();
            foreach (var asts in declarationASTs)
            {
                stepCodeAST.codeASTs.AddRange(asts);
            }
            stepCodeAST.codeASTs.AddRange(connectionASTs);
            stepCodeAST.codeASTs.AddRange(inputUpdateASTs);
            var fileContent = new List<CodeAST>() 
            {
                new MacroCodeAST() { Macro = "ifndef", Definition = "PROJECT_H"},
                new MacroCodeAST() { Macro = "define", Definition = "PROJECT_H"},
                new MacroCodeAST() { Macro = "include", Definition = "\"models.h\""},
                projectCodeAST, initCodeAST, stepCodeAST,
                new MacroCodeAST() { Macro = "endif"},
            };
            return fileContent;
        }
        private string Visit(CodeAST codeAST)
        {
            switch (codeAST)
            {
            case CallCodeAST callCodeAST:
                return Visit(callCodeAST);
            case AssignCodeAST assignCodeAST:
                return Visit(assignCodeAST);
            case LShiftCodeAST lShiftCodeAST:
                return Visit(lShiftCodeAST);
            case DeclareCodeAST declareCodeAST:
                return Visit(declareCodeAST);
            case MacroCodeAST macroCodeAST:
                return Visit(macroCodeAST);
            case FunctionCodeAST functionCodeAST:
                return Visit(functionCodeAST);
            case ClassCodeAST classCodeAST:
                return Visit(classCodeAST);
            default:
                throw logger.Error(new LigralException($"No CodeAST named {codeAST.GetType().Name}"));
            }
        }
        private string Visit(FunctionCodeAST functionCodeAST)
        {
            string head = $"{functionCodeAST.ReturnType} {functionCodeAST.FunctionName}";
            string parameter = "("+(functionCodeAST.Parameters == null ? "" : string.Join(", ", functionCodeAST.Parameters))+") {\n";
            string content = "\t"+string.Join("\n\t", functionCodeAST.codeASTs.ConvertAll(c => Visit(c)));
            string tail = "\n}\n";
            return head+parameter+content+tail;
        }
        private string Visit(ClassCodeAST classCodeAST)
        {
            string head = $"class {classCodeAST.ClassName} {{\n";
            string content = "public:\n\t"+string.Join("\n\t", classCodeAST.publicASTs.ConvertAll(c => Visit(c)));
            string tail = "\n};\n";
            return head+content+tail;
        }
        private string Visit(CallCodeAST callCodeAST)
        {
            string parameters = callCodeAST.Parameters == null ? "" : string.Join(", ", callCodeAST.Parameters.ConvertAll(parameter => $"{parameter}"));
            string results = callCodeAST.Results == null ? "" :string.Join(", ", callCodeAST.Results.ConvertAll(result => $"&{result}"));
            if (callCodeAST.Results == null || callCodeAST.Results.Count == 0)
            {
                return $"{callCodeAST.FunctionName}({parameters});";
            }
            else if (callCodeAST.Parameters == null || callCodeAST.Parameters.Count == 0)
            {
                return $"{callCodeAST.FunctionName}({results});";
            }
            else
            {
                return $"{callCodeAST.FunctionName}({parameters}, {results});";
            }
        }
        private string Visit(AssignCodeAST assignCodeAST)
        {
            return $"{assignCodeAST.Destination} = {assignCodeAST.Source};";
        }
        private string Visit(LShiftCodeAST lShiftCodeAST)
        {
            return $"{lShiftCodeAST.Destination} << {lShiftCodeAST.Source};";
        }
        private string Visit(DeclareCodeAST declareCodeAST)
        {
            return $"{declareCodeAST.Type} {declareCodeAST.Instance};";
        }
        private string Visit(MacroCodeAST macroCodeAST)
        {
            return $"#{macroCodeAST.Macro} {macroCodeAST.Definition}";
        }
    }
}