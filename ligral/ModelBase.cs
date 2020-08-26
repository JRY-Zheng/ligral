using Dict=System.Collections.Generic.Dictionary<string,object>;

namespace Ligral
{
    abstract class ModelBase
    {
        protected bool Configured = false;
        public void Connect(ModelBase modelBase)
        {
            for (int i=0; i<OutPortCount(); i++)
            {
                Connect(i, modelBase.Expose(i));
            }
        }
        public abstract void Connect(int outPortNO, InPort inPort);
        public abstract InPort Expose(int inPortNO);
        public abstract int InPortCount();
        public abstract int OutPortCount();
        public abstract void Configure(Dict dictionary);
        public abstract Port Expose(string portName);
        public abstract string GetTypeName();
        public static Group operator+(ModelBase left, ModelBase right)
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
        protected object ObtainKeyValue(Dict dictionary, string key)
        {
            if (dictionary.ContainsKey(key))
            {
                return dictionary[key];
            }
            else
            {
                throw new LigralException(string.Format("Parameter {0} is required but not provided.", key));
            }
        }
        public static Group operator+(ModelBase left, Group right)
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
        public static Group operator+(ModelBase left, Model right)
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
        public static Group operator-(ModelBase left, ModelBase right)
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
        public static Group operator-(ModelBase left, Group right)
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
        public static Group operator-(ModelBase left, Model right)
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
        public static Group operator*(ModelBase left, ModelBase right)
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
        public static Group operator*(ModelBase left, Group right)
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
        public static Group operator*(ModelBase left, Model right)
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
        public static Group operator/(ModelBase left, ModelBase right)
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
        public static Group operator/(ModelBase left, Group right)
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
        public static Group operator/(ModelBase left, Model right)
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
        public static Group operator+(ModelBase modelBase)
        {
            if (modelBase.GetType().Name=="Group")
            {
                return modelBase as Group;
            }
            else
            {
                return +(modelBase as Model);
            }
        }
        public static Group operator-(ModelBase modelBase)
        {
            if (modelBase.GetType().Name=="Group")
            {
                return -(modelBase as Group);
            }
            else
            {
                return -(modelBase as Model);
            }
        }
    }
}