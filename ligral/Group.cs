using System.Collections.Generic;
using System.Linq;
using Dict=System.Collections.Generic.Dictionary<string,object>;
using Ligral.Models;

namespace Ligral
{
    class Group : ModelBase
    {
        internal List<Model> inputModels = new List<Model>();
        internal List<Model> outputModels = new List<Model>();
        public void AddInputModel(Model model)
        {
            inputModels.Add(model);
        }
        public void AddInputModel(Group group)
        {
            if (group!=null)
            {
                inputModels.AddRange(group.inputModels);
            }
        }
        public void AddOutputModel(Model model)
        {
            outputModels.Add(model);
        }
        public void AddOutputModel(Group group)
        {
            if (group!=null)
            {
                outputModels.AddRange(group.outputModels);
            }
        }
        public override InPort Expose(int inPortNO)
        {
            int i = 0;
            foreach (Model model in inputModels)
            {
                int inPortCount = model.InPortCount();
                if (i+inPortCount>inPortNO)
                {
                    return model.Expose(inPortNO-i);
                }
                else
                {
                    i += inPortCount;
                }
            }
            throw new LigralException("In port number exceeds limit");
        }
        public override Port Expose(string portName)
        {
            return null;
        }
        public override void Connect(int outPortNO, InPort inPort)
        {
            int i = 0;
            foreach (Model model in outputModels)
            {
                int outPortCount = model.OutPortCount();
                if (i+outPortCount>outPortNO)
                {
                    model.Connect(outPortNO-i, inPort);
                    return;
                }
                else
                {
                    i += outPortCount;
                }
            }
            throw new LigralException("Out port number exceeds limit");
        }
        public override int InPortCount()
        {
            return inputModels.Sum(model=>model.InPortCount());
        }
        public override int OutPortCount()
        {
            return outputModels.Sum(model=>model.OutPortCount());
        }
        public void Merge(Group group)
        {
            inputModels.AddRange(group.inputModels);
            outputModels.AddRange(group.outputModels);
        }
        public override void Configure(Dict dictionary)
        {

        }
        public override string GetTypeName()
        {
            return "GROUP";
        }
        public static Group operator+(Group left, Group right)
        {
            if (left.OutPortCount()!=1||right.OutPortCount()!=1)
            {
                throw new LigralException("Out port number should be 1 when adding together");
            }
            Calculator calculator = ModelManager.Create("Calculator") as Calculator;
            Dict dictionary = new Dict(){{"type", '+'}};
            calculator.Configure(dictionary);
            left.Connect(0, calculator.Expose(0));
            right.Connect(0, calculator.Expose(1));
            Group group = new Group();
            group.AddInputModel(left);
            group.AddInputModel(right);
            group.AddOutputModel(calculator);
            return group;
        }
        public static Group operator+(Group left, Model right)
        {
            if (left.OutPortCount()!=1||right.OutPortCount()!=1)
            {
                throw new LigralException("Out port number should be 1 when adding together");
            }
            Calculator calculator = ModelManager.Create("Calculator") as Calculator;
            Dict dictionary = new Dict(){{"type", '+'}};
            calculator.Configure(dictionary);
            left.Connect(0, calculator.Expose(0));
            right.Connect(0, calculator.Expose(1));
            Group group = new Group();
            group.AddInputModel(left);
            group.AddInputModel(right);
            group.AddOutputModel(calculator);
            return group;
        }
        public static Group operator+(Model left, Group right)
        {
            if (left.OutPortCount()!=1||right.OutPortCount()!=1)
            {
                throw new LigralException("Out port number should be 1 when adding together");
            }
            Calculator calculator = ModelManager.Create("Calculator") as Calculator;
            Dict dictionary = new Dict(){{"type", '+'}};
            calculator.Configure(dictionary);
            left.Connect(0, calculator.Expose(0));
            right.Connect(0, calculator.Expose(1));
            Group group = new Group();
            group.AddInputModel(left);
            group.AddInputModel(right);
            group.AddOutputModel(calculator);
            return group;
        }
        public static Group operator-(Group left, Group right)
        {
            if (left.OutPortCount()!=1||right.OutPortCount()!=1)
            {
                throw new LigralException("Out port number should be 1 when adding together");
            }
            Calculator calculator = ModelManager.Create("Calculator") as Calculator;
            Dict dictionary = new Dict(){{"type", '-'}};
            calculator.Configure(dictionary);
            left.Connect(0, calculator.Expose(0));
            right.Connect(0, calculator.Expose(1));
            Group group = new Group();
            group.AddInputModel(left);
            group.AddInputModel(right);
            group.AddOutputModel(calculator);
            return group;
        }
        public static Group operator-(Group left, Model right)
        {
            if (left.OutPortCount()!=1||right.OutPortCount()!=1)
            {
                throw new LigralException("Out port number should be 1 when adding together");
            }
            Calculator calculator = ModelManager.Create("Calculator") as Calculator;
            Dict dictionary = new Dict(){{"type", '-'}};
            calculator.Configure(dictionary);
            left.Connect(0, calculator.Expose(0));
            right.Connect(0, calculator.Expose(1));
            Group group = new Group();
            group.AddInputModel(left);
            group.AddInputModel(right);
            group.AddOutputModel(calculator);
            return group;
        }
        public static Group operator-(Model left, Group right)
        {
            if (left.OutPortCount()!=1||right.OutPortCount()!=1)
            {
                throw new LigralException("Out port number should be 1 when adding together");
            }
            Calculator calculator = ModelManager.Create("Calculator") as Calculator;
            Dict dictionary = new Dict(){{"type", '-'}};
            calculator.Configure(dictionary);
            left.Connect(0, calculator.Expose(0));
            right.Connect(0, calculator.Expose(1));
            Group group = new Group();
            group.AddInputModel(left);
            group.AddInputModel(right);
            group.AddOutputModel(calculator);
            return group;
        }
        public static Group operator*(Group left, Group right)
        {
            if (left.OutPortCount()!=1||right.OutPortCount()!=1)
            {
                throw new LigralException("Out port number should be 1 when adding together");
            }
            Calculator calculator = ModelManager.Create("Calculator") as Calculator;
            Dict dictionary = new Dict(){{"type", '*'}};
            calculator.Configure(dictionary);
            left.Connect(0, calculator.Expose(0));
            right.Connect(0, calculator.Expose(1));
            Group group = new Group();
            group.AddInputModel(left);
            group.AddInputModel(right);
            group.AddOutputModel(calculator);
            return group;
        }
        public static Group operator*(Group left, Model right)
        {
            if (left.OutPortCount()!=1||right.OutPortCount()!=1)
            {
                throw new LigralException("Out port number should be 1 when adding together");
            }
            Calculator calculator = ModelManager.Create("Calculator") as Calculator;
            Dict dictionary = new Dict(){{"type", '*'}};
            calculator.Configure(dictionary);
            left.Connect(0, calculator.Expose(0));
            right.Connect(0, calculator.Expose(1));
            Group group = new Group();
            group.AddInputModel(left);
            group.AddInputModel(right);
            group.AddOutputModel(calculator);
            return group;
        }
        public static Group operator*(Model left, Group right)
        {
            if (left.OutPortCount()!=1||right.OutPortCount()!=1)
            {
                throw new LigralException("Out port number should be 1 when adding together");
            }
            Calculator calculator = ModelManager.Create("Calculator") as Calculator;
            Dict dictionary = new Dict(){{"type", '*'}};
            calculator.Configure(dictionary);
            left.Connect(0, calculator.Expose(0));
            right.Connect(0, calculator.Expose(1));
            Group group = new Group();
            group.AddInputModel(left);
            group.AddInputModel(right);
            group.AddOutputModel(calculator);
            return group;
        }
        public static Group operator/(Group left, Group right)
        {
            if (left.OutPortCount()!=1||right.OutPortCount()!=1)
            {
                throw new LigralException("Out port number should be 1 when adding together");
            }
            Calculator calculator = ModelManager.Create("Calculator") as Calculator;
            Dict dictionary = new Dict(){{"type", '/'}};
            calculator.Configure(dictionary);
            left.Connect(0, calculator.Expose(0));
            right.Connect(0, calculator.Expose(1));
            Group group = new Group();
            group.AddInputModel(left);
            group.AddInputModel(right);
            group.AddOutputModel(calculator);
            return group;
        }
        public static Group operator/(Group left, Model right)
        {
            if (left.OutPortCount()!=1||right.OutPortCount()!=1)
            {
                throw new LigralException("Out port number should be 1 when adding together");
            }
            Calculator calculator = ModelManager.Create("Calculator") as Calculator;
            Dict dictionary = new Dict(){{"type", '/'}};
            calculator.Configure(dictionary);
            left.Connect(0, calculator.Expose(0));
            right.Connect(0, calculator.Expose(1));
            Group group = new Group();
            group.AddInputModel(left);
            group.AddInputModel(right);
            group.AddOutputModel(calculator);
            return group;
        }
        public static Group operator/(Model left, Group right)
        {
            if (left.OutPortCount()!=1||right.OutPortCount()!=1)
            {
                throw new LigralException("Out port number should be 1 when adding together");
            }
            Calculator calculator = ModelManager.Create("Calculator") as Calculator;
            Dict dictionary = new Dict(){{"type", '/'}};
            calculator.Configure(dictionary);
            left.Connect(0, calculator.Expose(0));
            right.Connect(0, calculator.Expose(1));
            Group group = new Group();
            group.AddInputModel(left);
            group.AddInputModel(right);
            group.AddOutputModel(calculator);
            return group;
        }
        public static Group operator^(Group left, Group right)
        {
            if (left.OutPortCount()!=1||right.OutPortCount()!=1)
            {
                throw new LigralException("Out port number should be 1 when adding together");
            }
            Calculator calculator = ModelManager.Create("Calculator") as Calculator;
            Dict dictionary = new Dict(){{"type", '^'}};
            calculator.Configure(dictionary);
            left.Connect(0, calculator.Expose(0));
            right.Connect(0, calculator.Expose(1));
            Group group = new Group();
            group.AddInputModel(left);
            group.AddInputModel(right);
            group.AddOutputModel(calculator);
            return group;
        }
        public static Group operator^(Group left, Model right)
        {
            if (left.OutPortCount()!=1||right.OutPortCount()!=1)
            {
                throw new LigralException("Out port number should be 1 when adding together");
            }
            Calculator calculator = ModelManager.Create("Calculator") as Calculator;
            Dict dictionary = new Dict(){{"type", '^'}};
            calculator.Configure(dictionary);
            left.Connect(0, calculator.Expose(0));
            right.Connect(0, calculator.Expose(1));
            Group group = new Group();
            group.AddInputModel(left);
            group.AddInputModel(right);
            group.AddOutputModel(calculator);
            return group;
        }
        public static Group operator^(Model left, Group right)
        {
            if (left.OutPortCount()!=1||right.OutPortCount()!=1)
            {
                throw new LigralException("Out port number should be 1 when adding together");
            }
            Calculator calculator = ModelManager.Create("Calculator") as Calculator;
            Dict dictionary = new Dict(){{"type", '^'}};
            calculator.Configure(dictionary);
            left.Connect(0, calculator.Expose(0));
            right.Connect(0, calculator.Expose(1));
            Group group = new Group();
            group.AddInputModel(left);
            group.AddInputModel(right);
            group.AddOutputModel(calculator);
            return group;
        }
        public static Group operator+(Group group)
        {
            return group;
        }
        public static Group operator-(Group group)
        {
            if (group.OutPortCount()!=1)
            {
                throw new LigralException();
            }
            Gain gain = ModelManager.Create("Gain") as Gain;
            Dict dictionary = new Dict(){{"value", -1}};
            gain.Configure(dictionary);
            group.Connect(0, gain.Expose(0));
            Group newGroup = new Group();
            newGroup.AddInputModel(group);
            newGroup.AddOutputModel(gain);
            return newGroup;
        }
    }
}