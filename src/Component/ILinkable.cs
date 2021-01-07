/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using Dict=System.Collections.Generic.Dictionary<string,object>;

namespace Ligral.Component
{
    public interface ILinkable
    {
        bool IsConfigured {get; set;}
        void Connect(ILinkable linkable)
        {
            for (int i=0; i<OutPortCount() || i<linkable.InPortCount(); i++)
            {
                Connect(i, linkable.Expose(i));
            }
        }
        void Connect(int outPortNO, InPort inPort);
        InPort Expose(int inPortNO);
        int InPortCount();
        int OutPortCount();
        void Configure(Dict dictionary);
        Port Expose(string portName);
        string GetTypeName();
    }
}