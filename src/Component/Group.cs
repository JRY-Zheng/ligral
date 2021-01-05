/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using System.Linq;
using Dict=System.Collections.Generic.Dictionary<string,object>;
using Ligral.Component.Models;

namespace Ligral.Component
{
    class Group : ILinkable
    {
        internal List<ILinkable> inputModels = new List<ILinkable>();
        internal List<ILinkable> outputModels = new List<ILinkable>();
        private static Logger logger = new Logger("Group");
        public bool IsConfigured {get; set;}
        public void AddInputModel(ILinkable linkable)
        {
            switch(linkable)
            {
            case Model model:
                inputModels.Add(model);
                break;
            case Group group:
                inputModels.AddRange(group.inputModels);
                break;
            case InPort inPort:
                inputModels.Add(inPort);
                break;
            default:
                throw logger.Error(new LigralException("Illegal type as input of a group"));
            }
        }
        public void AddOutputModel(ILinkable linkable)
        {
            switch(linkable)
            {
            case Model model:
                outputModels.Add(model);
                break;
            case Group group:
                outputModels.AddRange(group.outputModels);
                break;
            case OutPort outPort:
                outputModels.Add(outPort);
                break;
            case InPort inPort:
                break;
            default:
                throw logger.Error(new LigralException("Illegal type as output of a group"));
            }
        }
        public InPort Expose(int inPortNO)
        {
            int i = 0;
            var inPortVariableModels = inputModels.FindAll(model => model is InPortVariableModel)
                                                  .ConvertAll(model => (InPortVariableModel) model);
            if (inputModels.Count == 1 && inPortVariableModels.Count == 1)
            {
                inputModels[0].Expose(inPortNO);
            }
            else if (inputModels.Count > 1 && inPortVariableModels.Count >= 1)
            {
                throw logger.Error(new ModelException(inPortVariableModels[0], "Ambiguity due to in port variable model"));
            }
            else
            {
                foreach (ILinkable linkable in inputModels)
                {
                    int inPortCount = linkable.InPortCount();
                    if (i+inPortCount>inPortNO)
                    {
                        return linkable.Expose(inPortNO-i);
                    }
                    else
                    {
                        i += inPortCount;
                    }
                }
            }
            throw logger.Error(new LigralException("In port number exceeds limit"));
        }
        public virtual Port Expose(string portName)
        {
            return null;
        }
        public void Connect(int outPortNO, InPort inPort)
        {
            int i = 0;
            var outPortVariableModels = outputModels.FindAll(model => model is OutPortVariableModel)
                                                    .ConvertAll(model => (OutPortVariableModel) model);
            if (outputModels.Count == 1 && outPortVariableModels.Count == 1)
            {
                outputModels[0].Connect(outPortNO, inPort);
            }
            else if (outputModels.Count > 1 && outPortVariableModels.Count >= 1)
            {
                throw logger.Error(new ModelException(outPortVariableModels[0], "Ambiguity due to out port variable model"));
            }
            else // no out port variable model
            {
                foreach (ILinkable linkable in outputModels)
                {
                    int outPortCount = linkable.OutPortCount();
                    if (i+outPortCount>outPortNO)
                    {
                        linkable.Connect(outPortNO-i, inPort);
                        return;
                    }
                    else
                    {
                        i += outPortCount;
                    }
                }
                throw logger.Error(new LigralException("Out port number exceeds limit"));
            }
        }
        public int InPortCount()
        {
            return inputModels.Sum(model=>model.InPortCount());
        }
        public int OutPortCount()
        {
            return outputModels.Sum(model=>model.OutPortCount());
        }
        public void Merge(Group group)
        {
            inputModels.AddRange(group.inputModels);
            outputModels.AddRange(group.outputModels);
        }
        public virtual void Configure(Dict dictionary)
        {

        }
        public virtual string GetTypeName()
        {
            return "GROUP";
        }
        public static Group operator+(Group left, Group right)
        {
            if (left.OutPortCount()!=1||right.OutPortCount()!=1)
            {
                throw logger.Error(new LigralException("Out port number should be 1 when adding together"));
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
                throw logger.Error(new LigralException("Out port number should be 1 when adding together"));
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
                throw logger.Error(new LigralException("Out port number should be 1 when adding together"));
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
                throw logger.Error(new LigralException("Out port number should be 1 when adding together"));
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
                throw logger.Error(new LigralException("Out port number should be 1 when adding together"));
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
                throw logger.Error(new LigralException("Out port number should be 1 when adding together"));
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
                throw logger.Error(new LigralException("Out port number should be 1 when adding together"));
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
                throw logger.Error(new LigralException("Out port number should be 1 when adding together"));
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
                throw logger.Error(new LigralException("Out port number should be 1 when adding together"));
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
                throw logger.Error(new LigralException("Out port number should be 1 when adding together"));
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
                throw logger.Error(new LigralException("Out port number should be 1 when adding together"));
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
                throw logger.Error(new LigralException("Out port number should be 1 when adding together"));
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
                throw logger.Error(new LigralException("Out port number should be 1 when adding together"));
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
                throw logger.Error(new LigralException("Out port number should be 1 when adding together"));
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
                throw logger.Error(new LigralException("Out port number should be 1 when adding together"));
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
                throw logger.Error(new LigralException());
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