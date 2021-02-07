/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using Ligral.Component;

namespace Ligral.Simulation
{
    abstract class ConditionSetter : IConfigurable
    {
        protected Logger logger;
        protected string HandleName;
        public ConditionSetter(string handleName)
        {
            HandleName = handleName;
            logger = new Logger($"ConditionSetter({handleName})");
        }
        protected abstract void SetValue(Signal signal);
        protected abstract void SetUpperBound(Signal signal);
        protected abstract void SetLowerBound(Signal signal);
        public void Configure(Dictionary<string, object> dict)
        {
            foreach (string item in dict.Keys)
            {
                object val = dict[item];
                try
                {
                    switch (item.ToLower())
                    {
                    case "val":
                    case "value":
                        SetValue(new Signal(val));
                        break;
                    case "upper":
                    case "max":
                        SetUpperBound(new Signal(val));
                        break;
                    case "lower":
                    case "min":
                        SetLowerBound(new Signal(val));
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
    class StateCondition : ConditionSetter
    {
        private StateHandle handle;
        public StateCondition(string handleName) : base(handleName)
        {
            if (!State.StateHandles.ContainsKey(handleName))
            {
                throw logger.Error(new SettingException(handleName, null, $"No state handle named {handleName}"));
            }
            handle = State.StateHandles[handleName];
        }

        protected override void SetLowerBound(Signal signal)
        {
            handle.SetStateLowerBound(signal);
        }

        protected override void SetUpperBound(Signal signal)
        {
            handle.SetStateUpperBound(signal);
        }

        protected override void SetValue(Signal signal)
        {
            handle.SetStateVariable(signal);
        }
    }
    class StateDerivativeCondition : ConditionSetter
    {
        private StateHandle handle;
        public StateDerivativeCondition(string handleName) : base(handleName)
        {
            if (!State.StateHandles.ContainsKey(handleName))
            {
                throw logger.Error(new SettingException(handleName, null, $"No state handle named {handleName}"));
            }
            handle = State.StateHandles[handleName];
        }

        protected override void SetLowerBound(Signal signal)
        {
            handle.SetDerivativeLowerBound(signal);
        }

        protected override void SetUpperBound(Signal signal)
        {
            handle.SetDerivativeUpperBound(signal);
        }

        protected override void SetValue(Signal signal)
        {
            handle.SetDerivative(signal);
        }
    }
    class OutputCondition : ConditionSetter
    {
        private ObservationHandle handle;
        public OutputCondition(string handleName) : base(handleName)
        {
            if (!Observation.ObservationHandles.ContainsKey(handleName))
            {
                throw logger.Error(new SettingException(handleName, null, $"No observation handle named {handleName}"));
            }
            handle = Observation.ObservationHandles[handleName];
        }

        protected override void SetLowerBound(Signal signal)
        {
            handle.SetOutputUpperBound(signal);
        }

        protected override void SetUpperBound(Signal signal)
        {
            handle.SetOutputLowerBound(signal);
        }

        protected override void SetValue(Signal signal)
        {
            handle.SetOutputVariable(signal);
        }
    }
    class InputCondition : ConditionSetter
    {
        private ControlInputHandle handle;
        public InputCondition(string handleName) : base(handleName)
        {
            if (!ControlInput.InputHandles.ContainsKey(handleName))
            {
                throw logger.Error(new SettingException(handleName, null, $"No control input handle named {handleName}"));
            }
            handle = ControlInput.InputHandles[handleName];
        }

        protected override void SetLowerBound(Signal signal)
        {
            handle.SetInputLowerBound(signal);
        }

        protected override void SetUpperBound(Signal signal)
        {
            handle.SetInputUpperBound(signal);
        }

        protected override void SetValue(Signal signal)
        {
            handle.SetOpenLoopInput(signal);
        }
    }
}