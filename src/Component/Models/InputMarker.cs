/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using ParameterDictionary = System.Collections.Generic.Dictionary<string, Ligral.Component.Parameter>;
using Ligral.Simulation;


namespace Ligral.Component.Models
{
    class InputMarker : Model, IFixable
    {
        protected override string DocString
        {
            get
            {
                return "This model marks the input signal as control input.";
            }
        }
        private string varName;
        private int rowNo;
        private int colNo;
        private ControlInputHandle handle;
        protected override void SetUpParameters()
        {
            Parameters = new ParameterDictionary()
            {
                {"name", new Parameter(ParameterType.String , value=>
                {
                    varName = (string) value;
                }, ()=>{})},
                {"col", new Parameter(ParameterType.Signal , value=>
                {
                    colNo = System.Convert.ToInt32(value);
                    InPortList[0].ColNo = colNo;
                }, ()=>{})},
                {"row", new Parameter(ParameterType.Signal , value=>
                {
                    rowNo = System.Convert.ToInt32(value);
                    InPortList[0].RowNo = rowNo;
                }, ()=>{})}
            };
        }
        public bool FixConnection()
        {
            ILinkable constant = ModelManager.Create("Constant");
            var dict = new Dictionary<string, object>();
            if (rowNo > 0 && colNo > 0)
            {
                MatrixBuilder<double> m = Matrix<double>.Build;
                Matrix<double> matrix = m.Dense(rowNo, colNo, 0);
                dict.Add("value", matrix);
            }
            else
            {
                dict.Add("value", 0);
            }
            constant.Configure(dict);
            constant.Connect(this);
            return true;
        }
        public override void Prepare()
        {
            string inputSignalName = InPortList[0].Source is null ? null : InPortList[0].Source.SignalName;
            varName = varName ?? GivenName ?? inputSignalName ?? Name;
            handle = ControlInput.CreateInput(varName??Name, rowNo, colNo);
        }
        protected override List<Signal> DefaultCalculate(List<Signal> values)
        {
            Signal inputSignal = values[0];
            Signal outputSignal = Results[0];
            try
            {
                handle.SetClosedLoopInput(inputSignal);
            }
            catch (LigralException)
            {
                throw logger.Error(new ModelException(this));
            }
            outputSignal.Clone(handle.GetInput());
            return Results;
        }
    }
}