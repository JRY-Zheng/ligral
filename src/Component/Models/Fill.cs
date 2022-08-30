/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using ParameterDictionary = System.Collections.Generic.Dictionary<string, Ligral.Component.Parameter>;

namespace Ligral.Component.Models
{
    class Fill : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model out put constant matrix with given size and same filling values.";
            }
        }
        protected override void SetUpPorts()
        {
            OutPortList.Add(new OutPort("value", this));
        }
        int rowNo = 0;
        int colNo = 0;
        double value = 0;
        protected override void SetUpParameters()
        {
            Parameters = new ParameterDictionary()
            {
                {"col", new Parameter(ParameterType.String , value=>
                {
                    colNo = value.ToInt();
                })},
                {"row", new Parameter(ParameterType.String , value=>
                {
                    rowNo = value.ToInt();
                })},
                {"value", new Parameter(ParameterType.String , value=>
                {
                    this.value = value.ToScalar();
                }, ()=> {})}
            };
        }
        protected override void AfterConfigured()
        {
            Results[0] = Matrix<double>.Build.Dense(rowNo, colNo, value);
        }
        public override void Check()
        {
            OutPortList[0].SetShape(Results[0].RowCount, Results[0].ColumnCount);
        }
        protected override List<Matrix<double>> Calculate(List<Matrix<double>> values)
        {
            return Results;
        }
    }
}