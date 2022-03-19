/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using MathNet.Numerics.LinearAlgebra;

namespace Ligral.Component
{
    public class OutPortVariableModel : Model
    {
        
        protected override void SetUpPorts()
        {
            // OutPortList.Add(new OutPort("out0", this));
        }
        public override void Connect(int outPortNO, InPort inPort)
        {
            if (outPortNO == OutPortCount())
            {
                OutPort outPort = new OutPort($"out{outPortNO}", this);
                OutPortList.Add(outPort);
                outPort.Bind(inPort);
                Results.Add(null);
            }
            else
            {
                base.Connect(outPortNO, inPort);
            }
        }
        public override void Connect(string outPortName, InPort inPort)
        {
            try
            {
                base.Connect(outPortName, inPort);
            }
            catch (ModelException)
            {
                OutPort outPort = new OutPort(outPortName, this);
                OutPortList.Add(outPort);
                outPort.Bind(inPort);
                Results.Add(null);
            }
        }
        public override void Check()
        {
            throw new System.NotImplementedException();
        }
    }
}