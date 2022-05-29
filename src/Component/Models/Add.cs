/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using System;

namespace Ligral.Component.Models
{
    class Add : Model
    {
        protected override string DocString
        {
            get
            {
                return "This model outputs the sum of the two inputs";
            }
        }
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("left", this));
            InPortList.Add(new InPort("right", this));
            OutPortList.Add(new OutPort("result", this));
        }
        public override void Check()
        {
            try
            {
                (int xRowNo, int xColNo) = MatrixIteration.BroadcastShape(InPortList[0].RowNo, InPortList[0].ColNo, InPortList[1].RowNo, InPortList[1].ColNo);
                OutPortList[0].SetShape(xRowNo, xColNo);
            }
            catch (Exception e)
            {
                throw logger.Error(new ModelException(this, e.Message));
            }
        }
        protected override List<Matrix<double>> Calculate(List<Matrix<double>> values)
        {
            try
            {
                Results[0] = values[0].MatAdd(values[1]);
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
        public override List<int> GetCharacterSize()
        {
            // #define BROADCAST_RCRC 0
            // #define BROADCAST_RC11 1
            // #define BROADCAST_11RC 2
            // #define BROADCAST_RCR1 3
            // #define BROADCAST_R1RC 4
            // #define BROADCAST_RC1C 5
            // #define BROADCAST_1CRC 6
            int type = 0;
            int LR = InPortList[0].RowNo;
            int LC = InPortList[0].ColNo;
            int RR = InPortList[1].RowNo;
            int RC = InPortList[1].ColNo;
            if (RR == 1 && RC == 1) type = 1;
            else if (LR == 1 && LC == 1) type = 2;
            else if (RR != 1 && RC == 1) type = 3;
            else if (LR != 1 && LC == 1) type = 4;
            else if (RR == 1 && RC != 1) type = 5;
            else if (LR == 1 && LC != 1) type = 6;
            return new List<int>() {OutPortList[0].RowNo, OutPortList[0].ColNo, type};
        }
    }
}