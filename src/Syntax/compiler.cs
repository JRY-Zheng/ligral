/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

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
            var codeASTs = routine.ConvertAll(model => model.ConstructAST());
            foreach (var ast in codeASTs)
            {
                System.Console.WriteLine(Visit(ast));
            }
        }
        private string Visit(CodeAST codeAST)
        {
            switch (codeAST)
            {
            case ModelCodeAST modelCodeAST:
                return Visit(modelCodeAST);
            case FunctionCodeAST functionCodeAST:
                return Visit(functionCodeAST);
            case CopyCodeAST copyCodeAST:
                return Visit(copyCodeAST);
            default:
                throw logger.Error(new ComplieException(codeAST.FindToken(), $"No CodeAST named {codeAST.GetType().Name}"));
            }
        }
        private string Visit(ModelCodeAST modelCodeAST)
        {
            string functionStatement = Visit(modelCodeAST.functionCodeAST);
            var statements = modelCodeAST.copyCodeASTs.ConvertAll(copyCodeAST => Visit(copyCodeAST));
            statements.Insert(0, functionStatement);
            return string.Join('\n', statements);
        }
        private string Visit(FunctionCodeAST functionCodeAST)
        {
            string parameters = string.Join(", ", functionCodeAST.Parameters.ConvertAll(parameter => $"{parameter.Value}"));
            string results = string.Join(", ", functionCodeAST.Results.ConvertAll(result => $"&{result.Value}"));
            if (functionCodeAST.Results.Count == 0)
            {
                return $"{functionCodeAST.FunctionName.Value}({parameters});";
            }
            else if (functionCodeAST.Parameters.Count == 0)
            {
                return $"{functionCodeAST.FunctionName.Value}({results});";
            }
            else
            {
                return $"{functionCodeAST.FunctionName.Value}({parameters}, {results});";
            }
        }
        private string Visit(CopyCodeAST copyCodeAST)
        {
            return $"{copyCodeAST.Destination.Value} = {copyCodeAST.Source.Value};";
        }
    }
}