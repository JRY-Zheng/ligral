/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using ParameterDictionary = System.Collections.Generic.Dictionary<string, Ligral.Component.Parameter>;
using MathNet.Numerics.LinearAlgebra;
using Ligral.Syntax.CodeASTs;

namespace Ligral.Component.Models
{
    class Gain : Model
    {
        private Matrix<double> gain;
        private bool leftProduct = true;
        protected override string DocString
        {
            get
            {
                return "This model amplifies the input by a given constant.";
            }
        }
        protected override void SetUpParameters()
        {
            Parameters = new ParameterDictionary()
            {
                {"value", new Parameter(ParameterType.Signal , value=>
                {
                    gain = value.ToMatrix();
                })},
                {"prod", new Parameter(ParameterType.String , value=>
                {
                    string prod = (string) value;
                    switch (prod)
                    {
                    case "left":
                        leftProduct = true; break;
                    case "right":
                        leftProduct = false; break;
                    default:
                        throw logger.Error(new ModelException(this, $"Invalid enum prod {prod}"));
                    }
                }, ()=>{})}
            };
        }
        protected override List<Matrix<double>> Calculate(List<Matrix<double>> values)
        {
            try
            {
                if (leftProduct)
                {
                    Results[0] = gain.MatMul(values[0]);
                }
                else
                {
                    Results[0] = values[0].MatMul(gain);
                }
            }
            catch (System.ArgumentException e)
            {
                string message = e.Message;
                int indexOfParenthesis = message.IndexOf('(');
                if (indexOfParenthesis>=0)
                {
                    message = message.Substring(0, indexOfParenthesis);
                }
                throw logger.Error(new ModelException(this, message));
            }
            return Results;
        }
        public override void Check()
        {
            if (InPortList[0].RowNo == 1 && InPortList[0].ColNo == 1)
            {
                OutPortList[0].SetShape(gain.RowCount, gain.ColumnCount);
            }
            else if (gain.RowCount == 1 && gain.ColumnCount == 1)
            {
                OutPortList[0].SetShape(InPortList[0].RowNo, InPortList[0].ColNo);
            }
            else if (leftProduct && gain.ColumnCount == InPortList[0].RowNo)
            {
                OutPortList[0].SetShape(gain.RowCount, InPortList[0].ColNo);
            }
            else if (!leftProduct && gain.RowCount == InPortList[0].ColNo)
            {
                OutPortList[0].SetShape(InPortList[0].RowNo, gain.ColumnCount);
            }
            else if (leftProduct)
            {
                throw logger.Error(new ModelException(this, $"Shape inconsistency ({gain.RowCount},{gain.ColumnCount})x({InPortList[0].RowNo},{InPortList[0].ColNo})"));
            }
            else
            {
                throw logger.Error(new ModelException(this, $"Shape inconsistency ({InPortList[0].RowNo},{InPortList[0].ColNo})x({gain.RowCount},{gain.ColumnCount})"));
            }
        }
        public override List<int> GetCharacterSize()
        {
            if (gain.ColumnCount == InPortList[0].RowNo &&
                gain.RowCount == InPortList[0].ColNo &&
                gain.ColumnCount == gain.RowCount)
            {
                if (leftProduct) return new List<int>() {gain.RowCount, 0, 0};
                else return new List<int>() {0, 0, gain.RowCount};
            }
            else if (InPortList[0].RowNo == 1 && InPortList[0].ColNo == 1)
            {
                return new List<int>() {gain.RowCount, gain.ColumnCount, 0};
            }
            else if (gain.RowCount == 1 && gain.ColumnCount == 1)
            {
                return new List<int>() {0, InPortList[0].RowNo, InPortList[0].ColNo};
            }
            else if (leftProduct)
            {
                return new List<int>() {gain.RowCount, InPortList[0].RowNo, InPortList[0].ColNo};
            }
            else
            {
                return new List<int>() {InPortList[0].RowNo, InPortList[0].ColNo, gain.ColumnCount};
            }
        }
        public override List<CodeAST> ConstructConfigurationAST()
        {
            var codeASTs = new List<CodeAST>();
            LShiftCodeAST valueConfiguration = new LShiftCodeAST();
            valueConfiguration.Destination = $"{GlobalName}.value";
            if (leftProduct && 
                !(gain.ColumnCount == InPortList[0].RowNo &&
                gain.RowCount == InPortList[0].ColNo &&
                gain.ColumnCount == gain.RowCount) && 
                !(InPortList[0].RowNo == 1 && InPortList[0].ColNo == 1) && 
                !(gain.RowCount == 1 && gain.ColumnCount == 1))
            {
                valueConfiguration.Source = string.Join(',', gain.Transpose().ToRowMajorArray());
            }
            else
            {
                valueConfiguration.Source = string.Join(',', gain.ToRowMajorArray());
            }
            codeASTs.Add(valueConfiguration);
            return codeASTs;
        }
    }
}