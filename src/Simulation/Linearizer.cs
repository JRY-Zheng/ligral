/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using Ligral.Component;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;

namespace Ligral.Simulation
{
    class StateSetter : IConfigurable
    {
        private Logger logger = new Logger("StateSetter");
        public void Configure(Dictionary<string, object> dict)
        {
            foreach (string item in dict.Keys)
            {
                object val = dict[item];
                if (!State.StateHandles.ContainsKey(item))
                {
                    throw logger.Error(new SettingException(item, val, $"No state handle named {item}"));
                }
                StateHandle handle = State.StateHandles[item];
                Signal origin;
                try
                {
                    origin = new Signal(val);
                    handle.SetStateVariable(origin);
                }
                catch (LigralException)
                {
                    throw logger.Error(new SettingException(item, val));
                }
                catch (System.InvalidCastException)
                {
                    throw logger.Error(new SettingException(item, val, $"Invalid type {val.GetType()} in state setter."));
                }
            }
        }
    }
    class InputSetter : IConfigurable
    {
        private Logger logger = new Logger("InputSetter");
        public void Configure(Dictionary<string, object> dict)
        {
            foreach (string item in dict.Keys)
            {
                object val = dict[item];
                if (!ControlInput.InputHandles.ContainsKey(item))
                {
                    throw logger.Error(new SettingException(item, val, $"No input handle named {item}"));
                }
                ControlInputHandle handle = ControlInput.InputHandles[item];
                Signal origin;
                try
                {
                    origin = new Signal(val);
                }
                catch (LigralException)
                {
                    throw logger.Error(new SettingException(item, val));
                }
                catch (System.InvalidCastException)
                {
                    throw logger.Error(new SettingException(item, val, $"Invalid type {val.GetType()} in input setter."));
                }
            }
        }
    }
    class Linearizer : IConfigurable
    {
        private Logger logger = new Logger("Linearizer");
        private StateSetter stateSetter = new StateSetter();
        private InputSetter inputSetter = new InputSetter();
        public void Linearize()
        {
            throw new System.NotImplementedException();
        }
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
                        stateSetter.Configure((Dictionary<string, object>) val);
                        break;
                    case "input":
                    case "u":
                        inputSetter.Configure((Dictionary<string, object>) val);
                        break;
                    case "time":
                    case "t":
                        Solver.Time = System.Convert.ToDouble(val);
                        break;
                    default:
                        throw logger.Error(new SettingException(item, val, "Unsupported setting in linearizer."));
                    }
                }
                catch (System.InvalidCastException)
                {
                    throw logger.Error(new SettingException(item, val, $"Invalid type {val.GetType()} in linearizer."));
                }
            }
        }
    }
}