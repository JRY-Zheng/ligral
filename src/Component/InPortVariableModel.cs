/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Linq;

namespace Ligral.Component 
{
    public class InPortVariableModel : Model
    {
        public override InPort ExposeInPort(int inPortNO)
        {
            if (inPortNO < InPortCount())
            {
                return base.ExposeInPort(inPortNO);
            }
            else while (inPortNO >= InPortCount())
            {
                InPort inPort = new InPort($"in{InPortCount()}", this);
                InPortList.Add(inPort);
            }
            return InPortList.Last();
        }
        public override Port Expose(string portName)
        {
            try
            {
                return base.Expose(portName);
            }
            catch (ModelException)
            {
                InPort inPort = new InPort(portName, this);
                InPortList.Add(inPort);
                logger.Solve();
                return inPort;
            }
        }
        protected override void SetUpPorts()
        {
            //InPortList.Add(new InPort("in0", this));
        }
        public override void Check()
        {
            throw new System.NotImplementedException();
        }
    }
}