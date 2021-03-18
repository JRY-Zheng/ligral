/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using Ligral.Component;

namespace Ligral.Simulation
{
    abstract class ConditionSetter : IConfigurable
    {
        protected Logger logger;
        protected string HandleName;
        public virtual void SetHandleName(string handleName)
        {
            HandleName = handleName;
            logger = new Logger($"ConditionSetter({handleName})");
        }
        protected abstract void SetValue(Matrix<double> signal);
        protected abstract void SetConstrain(Matrix<double> signal);
        protected abstract void SetUpperBound(Matrix<double> signal);
        protected abstract void SetLowerBound(Matrix<double> signal);
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
                        SetValue(val.ToMatrix());
                        break;
                    case "con":
                    case "constrain":
                        SetConstrain(val.ToMatrix());
                        break;
                    case "upper":
                    case "max":
                        SetUpperBound(val.ToMatrix());
                        break;
                    case "lower":
                    case "min":
                        SetLowerBound(val.ToMatrix());
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
        public override void SetHandleName(string handleName)
        {
            base.SetHandleName(handleName);
            if (!State.StateHandles.ContainsKey(handleName))
            {
                throw logger.Error(new SettingException(handleName, null, $"No state handle named {handleName}"));
            }
            handle = State.StateHandles[handleName];
        }

        protected override void SetLowerBound(Matrix<double> signal)
        {
            handle.SetStateLowerBound(signal);
        }

        protected override void SetUpperBound(Matrix<double> signal)
        {
            handle.SetStateUpperBound(signal);
        }

        protected override void SetValue(Matrix<double> signal)
        {
            handle.SetStateVariable(signal);
        }

        protected override void SetConstrain(Matrix<double> signal)
        {
            handle.SetStateConstrain(signal);
        }
    }
    class StateDerivativeCondition : ConditionSetter
    {
        private StateHandle handle;
        public override void SetHandleName(string handleName)
        {
            base.SetHandleName(handleName);
            if (!State.StateHandles.ContainsKey(handleName))
            {
                throw logger.Error(new SettingException(handleName, null, $"No state handle named {handleName}"));
            }
            handle = State.StateHandles[handleName];
        }

        protected override void SetLowerBound(Matrix<double> signal)
        {
            handle.SetDerivativeLowerBound(signal);
        }

        protected override void SetUpperBound(Matrix<double> signal)
        {
            handle.SetDerivativeUpperBound(signal);
        }

        protected override void SetValue(Matrix<double> signal)
        {
            handle.SetDerivative(signal);
        }
        protected override void SetConstrain(Matrix<double> signal)
        {
            handle.SetDerivativeConstrain(signal);
        }
    }
    class OutputCondition : ConditionSetter
    {
        private ObservationHandle handle;
        public override void SetHandleName(string handleName)
        {
            base.SetHandleName(handleName);
            if (!Observation.ObservationHandles.ContainsKey(handleName))
            {
                throw logger.Error(new SettingException(handleName, null, $"No observation handle named {handleName}"));
            }
            handle = Observation.ObservationHandles[handleName];
        }

        protected override void SetLowerBound(Matrix<double> signal)
        {
            handle.SetOutputUpperBound(signal);
        }

        protected override void SetUpperBound(Matrix<double> signal)
        {
            handle.SetOutputLowerBound(signal);
        }

        protected override void SetValue(Matrix<double> signal)
        {
            handle.SetOutputVariable(signal);
        }
        protected override void SetConstrain(Matrix<double> signal)
        {
            handle.SetOutputConstrain(signal);
        }
    }
    class InputCondition : ConditionSetter
    {
        private ControlInputHandle handle;
        public override void SetHandleName(string handleName)
        {
            base.SetHandleName(handleName);
            if (!ControlInput.InputHandles.ContainsKey(handleName))
            {
                throw logger.Error(new SettingException(handleName, null, $"No control input handle named {handleName}"));
            }
            handle = ControlInput.InputHandles[handleName];
        }

        protected override void SetLowerBound(Matrix<double> signal)
        {
            handle.SetInputLowerBound(signal);
        }

        protected override void SetUpperBound(Matrix<double> signal)
        {
            handle.SetInputUpperBound(signal);
        }

        protected override void SetValue(Matrix<double> signal)
        {
            handle.SetOpenLoopInput(signal);
        }
        protected override void SetConstrain(Matrix<double> signal)
        {
            handle.SetInputConstrain(signal);
        }
    }
    class ConditionsSetter<T> : IConfigurable where T : ConditionSetter, new()
    {
        public void Configure(Dictionary<string, object> dict)
        {
            foreach (string item in dict.Keys)
            {
                object val = dict[item];
                T condition = new T();
                condition.SetHandleName(item);
                condition.Configure((Dictionary<string, object>) val);
            }
        }
    }
}