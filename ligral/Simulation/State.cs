using System.Collections.Generic;

namespace Ligral.Simulation
{
    class State
    {
        private List<double> stateVariableCache = new List<double>();
        public double StateVariable {get; private set;}
        private double initialValue;
        private List<double> derivativeCache = new List<double>();
        private double derivative;
        private List<double> timeCache = new List<double>();
        private double time;
        private double errorThreshold;
        private int cacheLength;
        private bool isConfigured = false;
        public delegate void DerivativeReceivedHandler(State state);
        public event DerivativeReceivedHandler DerivativeReceived;
        public bool IsDerivativeReady {get; private set;}
        public bool IsStateVariableReady {get; private set;}
        public static List<State> StatePool = new List<State>();
        public string Name;
        public static State CreateState(double init, string name=null)
        {
            State state = new State(init);
            StatePool.Add(state);
            state.Name = name??$"State{StatePool.Count}";
            return state;
        }
        private State(double init)
        {
            initialValue = init;
            StateVariable = init;
            time = 0;
        }
        public void Config(double err, int cache)
        {
            if (isConfigured)
            {
                throw new LigralException($"Duplicated configuration of state {Name}");
            }
            errorThreshold = err;
            cacheLength = cache;
            isConfigured = true;
        }
        public void SetDerivative(double der, double t)
        {
            if (!isConfigured)
            {
                throw new LigralException($"State {Name} not configured");
            }
            derivativeCache.Add(derivative);
            timeCache.Add(time);
            if (derivativeCache.Count>cacheLength)
            {
                timeCache.RemoveAt(0);
                derivativeCache.RemoveAt(0);
            }
            derivative = der;
            time = t;
            IsDerivativeReady = true;
            IsStateVariableReady = false;
            DerivativeReceived(this);
            IsDerivativeReady = false;
        }
        public void EulerPropagate()
        {
            stateVariableCache.Add(StateVariable);
            if (stateVariableCache.Count>cacheLength)
            {
                stateVariableCache.RemoveAt(0);
            }
            StateVariable += derivative*(time-timeCache[timeCache.Count-1]);
            IsStateVariableReady = true;
        }
    }
}