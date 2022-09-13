/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Linq;

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
            if (outPortNO < OutPortCount())
            {
                base.Connect(outPortNO, inPort);
            }
            else while (outPortNO >= OutPortCount())
            {
                OutPort outPort = new OutPort($"out{OutPortCount()}", this);
                OutPortList.Add(outPort);
                Results.Add(null);
            }
            OutPortList.Last().Bind(inPort);
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
        public override Port Expose(string portName)
        {
            try
            {
                return base.Expose(portName);
            }
            catch (ModelException)
            {
                OutPort outPort = new OutPort(portName, this);
                OutPortList.Add(outPort);
                Results.Add(null);
                logger.Solve();
                return outPort;
            }
        }
        public override OutPort ExposeOutPort(int outPortNO)
        {
            if (outPortNO == OutPortCount())
            {
                OutPort outPort = new OutPort($"out{outPortNO}", this);
                OutPortList.Add(outPort);
                Results.Add(null);
            }
            return base.ExposeOutPort(outPortNO);
        }
        public override void Check()
        {
            throw new System.NotImplementedException();
        }
    }
}