/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using Ligral.Component.Models;
using Ligral.Syntax;

namespace Ligral.Component
{
    public struct ScopedModelType
    {
        public string ScopeName;
        public string ModelName;
    }
    public static class ModelManager
    {
        private static Dictionary<string,int> modelCount = new Dictionary<string, int>();
        private static Dictionary<string,Dictionary<string,int>> extendedModelCount = new Dictionary<string,Dictionary<string,int>>();
        public static List<Model> ModelPool = new List<Model>();
        private static Logger logger = new Logger("ModelManager");
        public static Dictionary<string,System.Func<Model>> ModelTypePool = new Dictionary<string, System.Func<Model>>()
        {
            {"Node", ()=>new Node()},
            {"Gain", ()=>new Gain()},
            {"SineWave", ()=>new SineWave()},
            {"Step", ()=>new Step()},
            {"Playback", ()=>new Playback()},
            {"Integrator", ()=>new Integrator()},
            {"1/s", ()=>new Integrator()},
            {"BoundedIntegrator", ()=>new BoundedIntegrator()},
            {"Scope", ()=>new Scope()},
            {"PhaseDiagram", ()=>new PhaseDiagram()},
            {"Print", ()=>new Print()},
            {"Constant", ()=>new Constant()},
            {"Add", ()=>new Add()},
            {"Sub", ()=>new Sub()},
            {"Mul", ()=>new Mul()},
            {"Div", ()=>new Div()},
            {"RDiv", ()=>new RDiv()},
            {"DotMul", ()=>new DotMul()},
            {"DotDiv", ()=>new DotDiv()},
            {"DotPow", ()=>new DotPow()},
            {"Abs", ()=>new Abs()},
            {"<Input>", ()=>new Input()},
            {"<Output>", ()=>new Output()},
            {"Memory", ()=>new Memory()},
            {"Clock", ()=>new Clock()},
            {"Deadzone", ()=>new Deadzone()},
            {"Saturation", ()=>new Saturation()},
            {"Sin", ()=>new Sin()},
            {"Cos", ()=>new Cos()},
            {"Tan", ()=>new Tan()},
            {"Sinh", ()=>new Sinh()},
            {"Cosh", ()=>new Cosh()},
            {"Tanh", ()=>new Tanh()},
            {"Asin", ()=>new Asin()},
            {"Acos", ()=>new Acos()},
            {"Atan", ()=>new Atan()},
            {"Atan2", ()=>new Atan2()},
            {"Asinh", ()=>new Asinh()},
            {"Acosh", ()=>new Acosh()},
            {"Atanh", ()=>new Atanh()},
            {"Exp", ()=>new Exp()},
            {"Pow", ()=>new Pow()},
            {"Pow2", ()=>new Pow2()},
            {"Sqrt", ()=>new Sqrt()},
            {"Sign", ()=>new Sign()},
            {"Log", ()=>new Log()},
            {"Log2", ()=>new Log2()},
            {"ThresholdSwitch", ()=>new Switch()},
            {"Switch", ()=>new Switch()},
            {"Max", ()=>new Max()},
            {"Min", ()=>new Min()},
            {"Rand", ()=>new Rand()},
            {"Terminal", ()=>new Terminal()},
            {"_", ()=>new Terminal()},
            {"VStack", ()=>new VStack()},
            {"HStack", ()=>new HStack()},
            {"Split", ()=>new Split()},
            {"VSplit", ()=>new VSplit()},
            {"HSplit", ()=>new HSplit()},
            {"InputMarker", ()=>new InputMarker()},
            {"OutputSink", ()=>new OutputSink()},
            {"Sweep", ()=>new Sweep()},
            {"Inv", ()=>new Inverse()},
            {"Inverse", ()=>new Inverse()},
            {"Equal", ()=>new Equal()},
            {"EqualToZero", ()=>new EqualToZero()},
            {"=0", ()=>new EqualToZero()},
            {"Variable", ()=>new Variable()},
            {"Interpolation", ()=>new Interpolation()},
            {"UDPListener", ()=>new UDPListener()},
            {"UDPSender", ()=>new UDPSender()},
            {"Cross", ()=>new Cross()},
            {"Transpose", ()=>new Transpose()},
            {"'", ()=>new Transpose()},
            {"Sec", ()=>new Sec()},
            {"Csc", ()=>new Csc()},
            {"Cot", ()=>new Cot()},
            {"Interpolation2D", ()=>new Interpolation2D()},
            {"InterpolationHD", ()=>new InterpolationHD()},
            {"TransferFunction", ()=>new TransferFunction()},
            {"TF", ()=>new TransferFunction()},
            {"Slice", ()=>new Slice()},
            {"Fill", ()=>new Fill()},
            {"Linspace", ()=>new Linspace()},
            {"Diagonal", ()=>new Diagonal()}
        };
        public static Dictionary<string, Dictionary<string,System.Func<Model>>> ExtendedModelTypePool = new Dictionary<string, Dictionary<string, System.Func<Model>>>();
        public static Model Create(string modelType, Token token)
        {
            Model model;
            if (ModelTypePool.ContainsKey(modelType))
            {
                model = ModelTypePool[modelType]();
                model.ModelToken = token;
            }
            else
            {
                throw logger.Error(new LigralException("No model named "+modelType));
            }
            if (modelCount.ContainsKey(modelType))
            {
                modelCount[modelType]++;
            }
            else
            {
                modelCount[modelType] = 1;
            }
            model.DefaultName = modelType+modelCount[modelType].ToString();
            model.Scope = Interpreter.ScopeName;
            ModelPool.Add(model);
            return model;
        }
        public static Model Create(ScopedModelType scopedModelType, Token token)
        {
            string scopeName = scopedModelType.ScopeName;
            if (!ExtendedModelTypePool.ContainsKey(scopeName))
            {
                throw logger.Error(new NotFoundException($"Plugin {scopeName}"));
            }
            var mainModelTypePool = ModelTypePool;
            ModelTypePool = ExtendedModelTypePool[scopeName];
            var mainModelCount = modelCount;
            if (extendedModelCount.ContainsKey(scopeName))
            {
                modelCount = extendedModelCount[scopeName];
            }
            else
            {
                modelCount = new Dictionary<string, int>();
                extendedModelCount[scopeName] = modelCount;
            }
            Model model = Create(scopedModelType.ModelName, token);
            ModelTypePool = mainModelTypePool;
            modelCount = mainModelCount;
            return model;
        }
    }
}