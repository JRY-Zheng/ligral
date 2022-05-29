/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;

namespace Ligral.Component.Models
{
    class HStack : InPortVariableModel
    {
        protected override string DocString
        {
            get
            {
                return "This model stacks inputs horizontally to a matrix";
            }
        }
        protected override void SetUpPorts()
        {
            base.SetUpPorts();
            OutPortList.Add(new OutPort("matrix", this));
        }
        public override void Check()
        {
            int rowNo = InPortList[0].RowNo;
            int colNo = InPortList[0].ColNo;
            rowNo = rowNo == 0? 1 : rowNo;
            colNo = colNo == 0? 1 : colNo;
            foreach (InPort inPort in InPortList.Skip(1))
            {
                int nextRowNo = inPort.RowNo;
                int nextColNo = inPort.ColNo;
                nextRowNo = nextRowNo == 0? 1 : nextRowNo;
                nextColNo = nextColNo == 0? 1 : nextColNo;
                if (rowNo != nextRowNo)
                {
                    int index = InPortList.IndexOf(inPort);
                    throw logger.Error(new ModelException(this, $"The in port {index} has inconsistent row number."));
                }
                colNo += nextColNo;
            }
            OutPortList[0].SetShape(rowNo, colNo);
        }
        protected override List<Matrix<double>> Calculate(List<Matrix<double>> values)
        {
            Matrix<double> firstMatrix = values[0];
            foreach(Matrix<double> matrix in values.Skip(1))
            {
                try
                {
                    firstMatrix = firstMatrix.Append(matrix);
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
            }
            Results[0] = firstMatrix;
            return Results;
        }
        public override List<int> GetCharacterSize()
        {
            var characterSize = new List<int>() {OutPortList[0].RowNo, OutPortList[0].ColNo};
            characterSize.AddRange(InPortList.ConvertAll(inPort => inPort.ColNo));
            return characterSize;
        }
    }
}