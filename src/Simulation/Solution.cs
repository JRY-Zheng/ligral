/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using System.Linq;
using Ligral.Component;

namespace Ligral.Simulation
{
    public class Solution
    {
        public double ActualValue;
        public double InitialValue;
        public double GuessedValue;
        private static Logger logger = new Logger("Solution");
        public static List<Solution> SolutionPool = new List<Solution>();
        public static Dictionary<string, SolutionHandle> SolutionHandles = new Dictionary<string, SolutionHandle>();
        public string Name;
        public static Solution CreateSolution(string name)
        {
            name = name??$"Solution{SolutionPool.Count}";
            if (SolutionPool.Exists(solution => solution.Name == name))
            {
                throw logger.Error(new LigralException($"Solution {name} has already existed."));
            }
            else
            {
                Solution solution = new Solution(name);
                SolutionPool.Add(solution);
                return solution;
            }
        }
        public static SolutionHandle CreateSolution(string name, int rowNo, int colNo, Signal initial)
        {
            name = name??$"Solution{SolutionPool.Count}";
            if (SolutionHandles.ContainsKey(name))
            {
                throw logger.Error(new LigralException($"Solution handle {name} has already existed."));
            }
            else
            {
                var handle = new SolutionHandle(name, rowNo, colNo, initial);
                SolutionHandles.Add(name, handle);
                return handle;
            }
        }
        public static string GetNames()
        {
            return $"[{string.Join(", ", SolutionPool.Select((solution, index)=>solution.Name))}]";
        }
        private Solution(string name) 
        {
            Name = name;
        }
    }
    public class SolutionHandle : Handle<Solution>
    {
        public SolutionHandle(string name, int rowNo, int colNo, Signal initialSignal) : 
        base(name, rowNo, colNo, name => Solution.CreateSolution(name)) 
        {
            SetSignal(initialSignal, (solution, initial) => 
            {
                solution.InitialValue = initial;
                solution.ActualValue = initial;
            });
        }

        public void SetActualValue(Signal signal)
        {
            SetSignal(signal, (solution, actualValue) => solution.ActualValue = actualValue);
        }
        public Signal GetGuessedValue()
        {
            return GetSignal(solution => solution.GuessedValue);
        }
    }
}