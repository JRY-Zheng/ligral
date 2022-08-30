/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using ParameterDictionary = System.Collections.Generic.Dictionary<string, Ligral.Component.Parameter>;

namespace Ligral.Component.Models
{
    class Diagonal : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model out put constant matrix with given vector oas diagonal elements.";
            }
        }
        double[] vector;
        int count = 0;
        double value = 1;
        protected override void SetUpPorts()
        {
            OutPortList.Add(new OutPort("value", this));
        }
        protected override void SetUpParameters()
        {
            Parameters = new ParameterDictionary()
            {
                {"vec", new Parameter(ParameterType.String , value=>
                {
                    var mat = value.ToMatrix();
                    if (mat.RowCount>1)
                    {
                        throw logger.Error(new ModelException(this, "Vector should has at most one row"));
                    }
                    vector = mat.Row(0).ToArray();
                }, ()=> {})},
                {"count", new Parameter(ParameterType.String , value=>
                {
                    count = value.ToInt();
                }, ()=> {})},
                {"value", new Parameter(ParameterType.String , value=>
                {
                    this.value = value.ToScalar();
                }, ()=> {})}
            };
        }
        protected override void AfterConfigured()
        {
            if (count==0)
            {
                if (vector != null)
                {
                    Results[0] = Matrix<double>.Build.Diagonal(vector);
                }
                else 
                {
                    throw logger.Error(new ModelException(this, "Must define either vector or count & value pair"));
                }
            }
            else if (vector != null)
            {
                throw logger.Error(new ModelException(this, "Cannot define both vector and count & value pair"));
            }
            else if (count <= 0)
            {
                throw logger.Error(new ModelException(this, $"Count must greater than 0 but got {count}"));
            }
            else // (count == 0 && step != 0)
            {
                Results[0] = Matrix<double>.Build.DenseIdentity(count, count)*value;
            }
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