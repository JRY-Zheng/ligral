/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;

namespace Ligral.Simulation
{
    public class Function
    {
        public double Value;
        private static Logger logger = new Logger("Function");
        public static List<Function> FunctionPool = new List<Function>();
        public static Dictionary<string, FunctionHandle> FunctionHandles = new Dictionary<string, FunctionHandle>();
        public string Name;
        public static Function CreateFunction(string name)
        {
            name = name??$"Function{FunctionPool.Count}";
            if (FunctionPool.Exists(function => function.Name == name))
            {
                throw logger.Error(new LigralException($"Function {name} has already existed."));
            }
            else
            {
                Function function = new Function(name);
                FunctionPool.Add(function);
                return function;
            }
        }
        public static FunctionHandle CreateFunction(string name, int rowNo, int colNo)
        {
            name = name??$"Function{FunctionPool.Count}";
            if (FunctionHandles.ContainsKey(name))
            {
                throw logger.Error(new LigralException($"Function handle {name} has already existed."));
            }
            else
            {
                var handle = new FunctionHandle(name, rowNo, colNo);
                FunctionHandles.Add(name, handle);
                return handle;
            }
        }
        public static string GetNames()
        {
            return $"[{string.Join(", ", FunctionPool.Select((function, index)=>function.Name))}]";
        }
        private Function(string name) 
        {
            Name = name;
        }
    }
    public class FunctionHandle : Handle<Function>
    {
        public FunctionHandle(string name, int rowNo, int colNo) : 
        base(name, rowNo, colNo, Function.CreateFunction) 
        {}

        public void SetActualValue(Matrix<double> signal)
        {
            SetSignal(signal, (function, actualValue) => function.Value = actualValue);
        }
    }
}