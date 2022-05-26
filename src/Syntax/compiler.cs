/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using Ligral.Component;
using Ligral.Syntax.CodeASTs;

namespace Ligral.Syntax
{
    class Compiler
    {
        private Logger logger = new Logger("Compiler");
        public void Compile(List<Model> routine)
        {
            var configurationASTs = routine.ConvertAll(model => model.ConstructConfigurationAST());
            foreach (var ast in configurationASTs)
            {
                if (ast == null) continue;
                System.Console.WriteLine(Visit(ast));
            }
            var declarationASTs = routine.ConvertAll(model => model.ConstructTempVarDeclarationAST());
            var connectionASTs = routine.ConvertAll(model => model.ConstructConnectionAST());
            var inputUpdateASTs = routine.FindAll(model => model is InitializeableModel).ConvertAll(model => ((InitializeableModel)model).ConstructInputUpdateAST());
            var functionCodeAST = new FunctionCodeAST();
            functionCodeAST.ReturnType = "void";
            functionCodeAST.FunctionName = "project::step";
            functionCodeAST.Parameters = new List<string>();
            functionCodeAST.codeASTs = new List<CodeAST>();
            foreach (var asts in declarationASTs)
            {
                functionCodeAST.codeASTs.AddRange(asts);
            }
            functionCodeAST.codeASTs.AddRange(connectionASTs);
            functionCodeAST.codeASTs.AddRange(inputUpdateASTs);
            System.Console.WriteLine(Visit(functionCodeAST));
        }
        private string Visit(CodeAST codeAST)
        {
            switch (codeAST)
            {
            case CallCodeAST callCodeAST:
                return Visit(callCodeAST);
            case CopyCodeAST copyCodeAST:
                return Visit(copyCodeAST);
            case ConfigCodeAST configCodeAST:
                return Visit(configCodeAST);
            case DeclareCodeAST declareCodeAST:
                return Visit(declareCodeAST);
            default:
                throw logger.Error(new CompileException(codeAST.FindToken(), $"No CodeAST named {codeAST.GetType().Name}"));
            }
        }
        private string Visit(FunctionCodeAST functionCodeAST)
        {
            string head = $"{functionCodeAST.ReturnType} {functionCodeAST.FunctionName}";
            string parameter = "("+string.Join(", ", functionCodeAST.Parameters)+") {\n";
            string content = "\t"+string.Join("\n\t", functionCodeAST.codeASTs.ConvertAll(c => Visit(c)));
            string tail = "\n}";
            return head+parameter+content+tail;
        }
        private string Visit(CallCodeAST callCodeAST)
        {
            string parameters = string.Join(", ", callCodeAST.Parameters.ConvertAll(parameter => $"{parameter}"));
            string results = string.Join(", ", callCodeAST.Results.ConvertAll(result => $"&{result}"));
            if (callCodeAST.Results.Count == 0)
            {
                return $"{callCodeAST.FunctionName}({parameters});";
            }
            else if (callCodeAST.Parameters.Count == 0)
            {
                return $"{callCodeAST.FunctionName}({results});";
            }
            else
            {
                return $"{callCodeAST.FunctionName}({parameters}, {results});";
            }
        }
        private string Visit(CopyCodeAST copyCodeAST)
        {
            return $"{copyCodeAST.Destination} = {copyCodeAST.Source};";
        }
        private string Visit(ConfigCodeAST configuarionCodeAST)
        {
            string functionStatement = Visit(configuarionCodeAST.declareCodeAST);
            var statements = configuarionCodeAST.copyCodeASTs.ConvertAll(copyCodeAST => Visit(copyCodeAST));
            statements.Insert(0, functionStatement);
            return string.Join('\n', statements);
        }
        private string Visit(DeclareCodeAST declareCodeAST)
        {
            return $"{declareCodeAST.Type} {declareCodeAST.Instance};";
        }
    }
}