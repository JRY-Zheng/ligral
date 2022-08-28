/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using ParameterDictionary = System.Collections.Generic.Dictionary<string, Ligral.Component.Parameter>;

namespace Ligral.Component.Models
{
    class Slice : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model slices matrix.";
            }
        }
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("x", this));
            OutPortList.Add(new OutPort("y", this));
        }
        int hStart = 0;
        int hStop = int.MaxValue;
        int hStep = 1;
        int vStart = 0;
        int vStop = int.MaxValue;
        int vStep = 1;
        protected override void SetUpParameters()
        {
            Parameters = new ParameterDictionary()
            {
                {"hstart", new Parameter(ParameterType.String , value=>
                {
                    hStart = value.ToInt();
                }, ()=> {})},
                {"hstop", new Parameter(ParameterType.String , value=>
                {
                    hStop = value.ToInt();
                }, ()=> {})},
                {"hstep", new Parameter(ParameterType.String , value=>
                {
                    hStep = value.ToInt();
                }, ()=> {})},
                {"vstart", new Parameter(ParameterType.String , value=>
                {
                    vStart = value.ToInt();
                }, ()=> {})},
                {"vstop", new Parameter(ParameterType.String , value=>
                {
                    vStop = value.ToInt();
                }, ()=> {})},
                {"vstep", new Parameter(ParameterType.String , value=>
                {
                    vStep = value.ToInt();
                }, ()=> {})},
            };
        }
        private int CheckSlice(int start, int stop, int step)
        {
            if (step == 0)
            {
                throw logger.Error(new ModelException(this, "step cannot be zero"));
            }
            else if (step > 0)
            {
                if ((start>=0 && stop>=0 && start>=stop) ||
                    (start<0 && stop<0 && stop<=start))
                {
                    throw logger.Error(new ModelException(this, $"start({start}) cannot reach stop({stop}) via step({step})"));
                }
            }
            else// if (step < 0)
            {
                if ((start>=0 && stop>=0 && start<stop) ||
                    (start<0 && stop<0 && stop>start))
                {
                    throw logger.Error(new ModelException(this, $"start({start}) cannot reach stop({stop}) via step({step})"));
                }
            }
            // System.Console.WriteLine($"stop={stop}, start={start}, step={step}, count={((double)stop-start)/step}");
            return (int) System.Math.Ceiling(((double)stop-start)/step);
        }
        protected override void AfterConfigured()
        {
            CheckSlice(hStart, hStop, hStep);
            CheckSlice(vStart, vStop, vStep);
        }
        private int AnalyseSlicePosition(int position, int limit)
        {
            if (position == int.MaxValue) 
            {
                position = limit;
            }
            else if (position > limit)
            {
                throw logger.Error(new ModelException(this, $"slice position({position}) cannot be larger than limit({limit})"));
            }
            else if (position < -limit)
            {
                throw logger.Error(new ModelException(this, $"slice position({position}) cannot be smaller than limit({-limit})"));
            }
            else if (position < 0)
            {
                position += limit;
            }
            return position;
        }
        public override void Check()
        {
            int rowNo = InPortList[0].RowNo;
            int colNo = InPortList[0].ColNo;
            hStart = AnalyseSlicePosition(hStart, colNo);
            hStop = AnalyseSlicePosition(hStop, colNo);
            vStart = AnalyseSlicePosition(vStart, rowNo);
            vStop = AnalyseSlicePosition(vStop, colNo);
            int outColNo = CheckSlice(hStart, hStop, hStep);
            int outRowNo = CheckSlice(vStart, vStop, vStep);
            OutPortList[0].SetShape(outRowNo, outColNo);
        }
        protected override List<Matrix<double>> Calculate(List<Matrix<double>> values)
        {
            int rows = values[0].RowCount;
            var columnSliced = values[0].SubMatrix(0, rows, hStart, 1);
            for (int i=hStart+hStep; i<hStop; i+=hStep)
            {
                columnSliced = columnSliced.Append(values[0].SubMatrix(0, rows, i, 1));
            }
            int columns = columnSliced.ColumnCount;
            var rowSliced = columnSliced.SubMatrix(vStart, 1, 0, columns);
            for (int j=vStart+vStep; j<vStop; j+=vStep)
            {
                rowSliced = rowSliced.Stack(columnSliced.SubMatrix(j, 1, 0, columns));
            }
            Results[0] = rowSliced;
            return Results;
        }
    }
}