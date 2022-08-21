/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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
                new MacroCodeAST() { Macro = "define", Definition = $"STEPS {settings.StopTime/settings.StepSize+1}"}, 
                new MacroCodeAST() { Macro = "define", Definition = $"integral {settings.SolverName}_integral<N>"}, 
                new MacroCodeAST() { Macro = "define", Definition = "context struct context_struct<M, N, P>"}, 
                new MacroCodeAST() { Macro = "endif"}
            };
        }
        private List<CodeAST> GenerateProject(List<Model> routine)
        {
            var inputsCodeAST = new StructCodeAST();
            inputsCodeAST.StructName = "inputs";
            inputsCodeAST.contentASTs = new List<DeclareCodeAST>();
            foreach (var input in ControlInput.InputPool)
            {
                var inputDeclare = new DeclareCodeAST();
                inputDeclare.Type = "double";
                string name = Regex.Replace(input.Name, "[^\\w\\d]+", "_");
                inputDeclare.Instance = name;
                inputsCodeAST.contentASTs.Add(inputDeclare);
            }
            var statesCodeAST = new StructCodeAST();
            statesCodeAST.StructName = "states";
            statesCodeAST.contentASTs = new List<DeclareCodeAST>();
            foreach (var state in State.StatePool)
            {
                var stateDeclare = new DeclareCodeAST();
                stateDeclare.Type = "double";
                string name = Regex.Replace(state.Name, "[^\\w\\d]+", "_");
                stateDeclare.Instance = name;
                statesCodeAST.contentASTs.Add(stateDeclare);
            }
            var outputsCodeAST = new StructCodeAST();
            outputsCodeAST.StructName = "outputs";
            outputsCodeAST.contentASTs = new List<DeclareCodeAST>();
            foreach (var output in Observation.ObservationPool)
            {
                var outputDeclare = new DeclareCodeAST();
                outputDeclare.Type = "double";
                string name = Regex.Replace(output.Name, "[^\\w\\d]+", "_");
                outputDeclare.Instance = name;
                outputsCodeAST.contentASTs.Add(outputDeclare);
            }
            var projectCodeAST = new ClassCodeAST();
            projectCodeAST.ClassName = "project";
            var ctx = new DeclareCodeAST();
            ctx.Type = "context";
            ctx.Instance = "ctx";
            var up = new DeclareCodeAST();
            up.Type = "inputs*";
            up.Instance = "up";
            var xp = new DeclareCodeAST();
            xp.Type = "states*";
            xp.Instance = "xp";
            var xdotp = new DeclareCodeAST();
            xdotp.Type = "states*";
            xdotp.Instance = "xdotp";
            var yp = new DeclareCodeAST();
            yp.Type = "outputs*";
            yp.Instance = "yp";
            projectCodeAST.publicASTs = new List<DeclareCodeAST>(){ctx, up, xp, xdotp, yp};
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
            var refresh = new DeclareCodeAST();
            refresh.Type = "void";
            refresh.Instance = "refresh()";
            projectCodeAST.publicASTs.Add(refresh);
            var initCodeAST = new FunctionCodeAST();
            var configurationASTs = routine.ConvertAll(model => model.ConstructConfigurationAST());
            initCodeAST.ReturnType = "void";
            initCodeAST.FunctionName = "project::init";
            var inputsAssign = new AssignCodeAST();
            inputsAssign.Destination = "up";
            inputsAssign.Source = "(inputs*) &(ctx.u)";
            var statesAssign = new AssignCodeAST();
            statesAssign.Destination = "xp";
            statesAssign.Source = "(states*) &(ctx.x)";
            var derivativesAssign = new AssignCodeAST();
            derivativesAssign.Destination = "xdotp";
            derivativesAssign.Source = "(states*) &(ctx.xdot)";
            var outputsAssign = new AssignCodeAST();
            outputsAssign.Destination = "yp";
            outputsAssign.Source = "(outputs*) &(ctx.y)";
            initCodeAST.codeASTs = new List<CodeAST>() {inputsAssign, statesAssign, derivativesAssign, outputsAssign};
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
            var refreshCodeAST = new FunctionCodeAST();
            refreshCodeAST.ReturnType = "void";
            refreshCodeAST.FunctionName = "project::refresh";
            refreshCodeAST.codeASTs = new List<CodeAST>();
            var refreshASTs = routine.ConvertAll(model => model.ConstructRefreshAST());
            foreach (var asts in refreshASTs)
            {
                if (asts == null) continue;
                refreshCodeAST.codeASTs.AddRange(asts);
            }
            var fileContent = new List<CodeAST>() 
            {
                new MacroCodeAST() { Macro = "ifndef", Definition = "PROJECT_H"},
                new MacroCodeAST() { Macro = "define", Definition = "PROJECT_H"},
                new MacroCodeAST() { Macro = "include", Definition = "\"models.h\""},
                inputsCodeAST, statesCodeAST, outputsCodeAST,
                projectCodeAST, initCodeAST, stepCodeAST, refreshCodeAST,
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
            case StructCodeAST structCodeAST:
                return Visit(structCodeAST);
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
        private string Visit(StructCodeAST structCodeAST)
        {
            string head = $"struct {structCodeAST.StructName} {{\n";
            string content = "\t"+string.Join("\n\t", structCodeAST.contentASTs.ConvertAll(c => Visit(c)));
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