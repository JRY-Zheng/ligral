using System.Collections.Generic;

namespace Ligral.Simulation
{
    class State
    {
        public double StateVariable;
        public double InitialValue;
        public double Derivative;
        private List<double> timeCache = new List<double>();
        private double errorThreshold;
        private int cacheLength;
        private bool isConfigured = false;
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
            InitialValue = init;
            StateVariable = init;
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
        public void EulerPropagate()
        {
            StateVariable += Derivative*(Wanderer.Time-timeCache[timeCache.Count-1]);
        }
    }
}