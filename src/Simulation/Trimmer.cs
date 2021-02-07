/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;

namespace Ligral.Simulation
{
    class Trimmer : IConfigurable
    {
        private Logger logger = new Logger("Trimmer");
        public void Configure(Dictionary<string, object> dict)
        {
            foreach (string item in dict.Keys)
            {
                object val = dict[item];
                try
                {
                    switch (item.ToLower())
                    {
                    case "state":
                    case "x":
                        ConditionsSetter<StateCondition> stateSetter = new ConditionsSetter<StateCondition>();
                        stateSetter.Configure((Dictionary<string, object>) val);
                        break;
                    case "state_derivative":
                    case "derivative":
                    case "dx":
                        ConditionsSetter<StateDerivativeCondition> derivativeSetter = new ConditionsSetter<StateDerivativeCondition>();
                        derivativeSetter.Configure((Dictionary<string, object>) val);
                        break;
                    case "input":
                    case "u":
                        ConditionsSetter<InputCondition> inputSetter = new ConditionsSetter<InputCondition>();
                        inputSetter.Configure((Dictionary<string, object>) val);
                        break;
                    case "output":
                    case "y":
                        ConditionsSetter<OutputCondition> outputSetter = new ConditionsSetter<OutputCondition>();
                        outputSetter.Configure((Dictionary<string, object>) val);
                        break;
                    case "time":
                    case "t":
                        Solver.Time = System.Convert.ToDouble(val);
                        break;
                    default:
                        throw logger.Error(new SettingException(item, val, "Unsupported setting in linearizer."));
                    }
                }
                catch (LigralException)
                {
                    throw logger.Error(new SettingException(item, val));
                }
                catch (System.InvalidCastException)
                {
                    throw logger.Error(new SettingException(item, val, $"Invalid type {val.GetType()} in linearizer."));
                }
            }
        }
    }
}