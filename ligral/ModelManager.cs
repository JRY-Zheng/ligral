using System.Collections.Generic;
using Ligral.Models;

namespace Ligral
{
    static class ModelManager
    {
        private static Dictionary<string,int> modelCount = new Dictionary<string, int>();
        public static List<Model> ModelPool = new List<Model>();
        public static Dictionary<string,System.Func<Model>> ModelTypePool = new Dictionary<string, System.Func<Model>>()
        {
            {"Node", ()=>new Node()},
            {"Gain", ()=>new Gain()},
            {"SineWave", ()=>new SineWave()},
            {"Step", ()=>new Step()},
            {"Playback", ()=>new Playback()},
            {"Integrator", ()=>new Integrator()},
            {"BoundedIntegrator", ()=>new BoundedIntegrator()},
            {"Scope", ()=>new Scope()},
            {"PhaseDiagram", ()=>new PhaseDiagram()},
            {"Print", ()=>new Print()},
            {"Constant", ()=>new Constant()},
            {"Calculator", ()=>new Calculator()},
            {"Abs", ()=>new Abs()},
            {"Input", ()=>new Input()},
            {"Output", ()=>new Output()},
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
            {"LogicSwitch", ()=>new LogicSwitch()},
            {"ThresholdSwitch", ()=>new ThresholdSwitch()},
            {"Max", ()=>new Max()},
            {"Min", ()=>new Min()},
            {"Rand", ()=>new Rand()},
            {"_", ()=>new Terminal()},
            {"VStack", ()=>new VStack()},
            {"HStack", ()=>new HStack()},
            {"HSplit", ()=>new HSplit()}
        };

        public static Model Create(string modelType)
        {
            Model model;
            if (ModelTypePool.ContainsKey(modelType))
            {
                model = ModelTypePool[modelType]();
            }
            else
            {
                throw new LigralException("No model named "+modelType);
            }
            if (modelCount.ContainsKey(modelType))
            {
                modelCount[modelType]++;
            }
            else
            {
                modelCount[modelType] = 1;
            }
            model.Name = modelType+modelCount[modelType].ToString();
            ModelPool.Add(model);
            return model;
        }
    }
}