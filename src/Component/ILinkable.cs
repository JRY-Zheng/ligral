/* Copyright (C) 2019-2020 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using Dict=System.Collections.Generic.Dictionary<string,object>;

namespace Ligral.Component
{
    interface ILinkable
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
        public static Group operator+(ILinkable left, ILinkable right)
        {
            if (right.GetType().Name=="Group")
            {
                return left+(right as Group);
            }
            else
            {
                return left+(right as Model);
            }
        }
        public static Group operator+(ILinkable left, Group right)
        {
            if (left.GetType().Name=="Group")
            {
                return (left as Group)+right;
            }
            else
            {
                return (left as Model)+right;
            }
        }
        public static Group operator+(ILinkable left, Model right)
        {
            if (left.GetType().Name=="Group")
            {
                return (left as Group)+right;
            }
            else
            {
                return (left as Model)+right;
            }
        }
        public static Group operator-(ILinkable left, ILinkable right)
        {
            if (right.GetType().Name=="Group")
            {
                return left-(right as Group);
            }
            else
            {
                return left-(right as Model);
            }
        }
        public static Group operator-(ILinkable left, Group right)
        {
            if (left.GetType().Name=="Group")
            {
                return (left as Group)-right;
            }
            else
            {
                return (left as Model)-right;
            }
        }
        public static Group operator-(ILinkable left, Model right)
        {
            if (left.GetType().Name=="Group")
            {
                return (left as Group)-right;
            }
            else
            {
                return (left as Model)-right;
            }
        }
        public static Group operator*(ILinkable left, ILinkable right)
        {
            if (right.GetType().Name=="Group")
            {
                return left*(right as Group);
            }
            else
            {
                return left*(right as Model);
            }
        }
        public static Group operator*(ILinkable left, Group right)
        {
            if (left.GetType().Name=="Group")
            {
                return (left as Group)*right;
            }
            else
            {
                return (left as Model)*right;
            }
        }
        public static Group operator*(ILinkable left, Model right)
        {
            if (left.GetType().Name=="Group")
            {
                return (left as Group)*right;
            }
            else
            {
                return (left as Model)*right;
            }
        }
        public static Group operator/(ILinkable left, ILinkable right)
        {
            if (right.GetType().Name=="Group")
            {
                return left/(right as Group);
            }
            else
            {
                return left/(right as Model);
            }
        }
        public static Group operator/(ILinkable left, Group right)
        {
            if (left.GetType().Name=="Group")
            {
                return (left as Group)/right;
            }
            else
            {
                return (left as Model)/right;
            }
        }
        public static Group operator/(ILinkable left, Model right)
        {
            if (left.GetType().Name=="Group")
            {
                return (left as Group)/right;
            }
            else
            {
                return (left as Model)/right;
            }
        }
        public static Group operator^(ILinkable left, ILinkable right)
        {
            if (right.GetType().Name=="Group")
            {
                return left^(right as Group);
            }
            else
            {
                return left^(right as Model);
            }
        }
        public static Group operator^(ILinkable left, Group right)
        {
            if (left.GetType().Name=="Group")
            {
                return (left as Group)^right;
            }
            else
            {
                return (left as Model)^right;
            }
        }
        public static Group operator^(ILinkable left, Model right)
        {
            if (left.GetType().Name=="Group")
            {
                return (left as Group)^right;
            }
            else
            {
                return (left as Model)^right;
            }
        }
        public static Group operator+(ILinkable linkable)
        {
            if (linkable.GetType().Name=="Group")
            {
                return +(linkable as Group);
            }
            else
            {
                return +(linkable as Model);
            }
        }
        public static Group operator-(ILinkable linkable)
        {
            if (linkable.GetType().Name=="Group")
            {
                return -(linkable as Group);
            }
            else
            {
                return -(linkable as Model);
            }
        }
    }
}