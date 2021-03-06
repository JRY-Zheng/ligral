/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

namespace Ligral.Component 
{
    public class InPortVariableModel : Model
    {
        public override InPort Expose(int inPortNO)
        {
            if (inPortNO == InPortCount())
            {
                InPort inPort = new InPort($"in{inPortNO}", this);
                InPortList.Add(inPort);
                return inPort;
            }
            else
            {
                return base.Expose(inPortNO);
            }
        }
        protected override void SetUpPorts()
        {
            InPortList.Add(new InPort("in0", this));
        }
        public override void Check()
        {
            throw new System.NotImplementedException();
        }
    }
}