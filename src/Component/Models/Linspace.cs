/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using ParameterDictionary = System.Collections.Generic.Dictionary<string, Ligral.Component.Parameter>;

namespace Ligral.Component.Models
{
    class Linspace : Model
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
        int count = 0;
        double start = 0;
        double step = 0;
        double stop = 0;
        protected override void SetUpParameters()
        {
            Parameters = new ParameterDictionary()
            {
                {"count", new Parameter(ParameterType.String , value=>
                {
                    count = value.ToInt();
                }, ()=> {})},
                {"step", new Parameter(ParameterType.String , value=>
                {
                    step = value.ToScalar();
                }, ()=> {})},
                {"start", new Parameter(ParameterType.String , value=>
                {
                    start = value.ToScalar();
                })},
                {"stop", new Parameter(ParameterType.String , value=>
                {
                    stop = value.ToScalar();
                })}
            };
        }
        protected override void AfterConfigured()
        {
            if (step==0 && count==0)
            {
                step = 1;
            }
            if (step!=0 && count>0)
            {
                throw logger.Error(new ModelException(this, "At most one of step and count should be set"));
            }
            else if (count < 0)
            {
                throw logger.Error(new ModelException(this, $"Count must greater than 0 but got {count}"));
            }
            else if (count > 0)
            {
                step = (stop-start)/(count-1);
            }
            else // (count == 0 && step != 0)
            {
                if ((stop-start)*step < 0)
                {
                    throw logger.Error(new ModelException(this, $"Start({start}) cannot reach stop({stop}) via step({step})."));
                }
                while ((stop - start-count*step)*step > 0) count++;
            }
            Results[0] = Matrix<double>.Build.Dense(1, count, start);
            for (int i=1; i<count; i++)
            {
                Results[0][0, i] += i*step;
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