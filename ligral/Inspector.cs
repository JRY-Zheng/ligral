using System.Collections.Generic;

namespace Ligral 
{
    class Inspector
    {
        private List<Model> routine = new List<Model>();
        private List<Model> allNodes = new List<Model>();
        private void Visit(Model node)
        {
            if (!allNodes.Contains(node))
            {
                throw new LigralException($"Schematic Error: {node.Name} is not in inspection list.");
            }
            if(node.IsReady())
            {
                if (!routine.Contains(node))
                {
                    routine.Add(node);
                    node.Inspect().ForEach(subNode=>Visit(subNode));
                }
            }
            else if (node.Initializeable)
            {
                node.Initialize();
                Visit(node);
            }
        }
        public List<Model> Inspect(List<Model> nodes)
        {
            allNodes = nodes;
            // System.Console.WriteLine(string.Join(", ", allNodes.ConvertAll(node=>node.Name)));
            foreach(Model node in nodes)
            {
                Visit(node);
            }
            // System.Console.WriteLine(string.Join(", ", routine.ConvertAll(node=>node.Name)));
            if (routine.Count != nodes.Count)
            {
                List<Model> notConnectedModels = nodes.FindAll(node=>!node.IsConnected());
                if (notConnectedModels.Count>0)
                {
                    throw new LigralException("Schematic Error: Some nodes are not fully connected: "+
                    string.Join(", ", notConnectedModels.ConvertAll(node=>node.Name)));
                    // string.Join(", ", nodes.FindAll(node=>routine.FindAll(n=>n.Name==node.Name).Count==0).ConvertAll(node=>node.Name)));
                }
                else
                {
                    throw new LigralException("Schematic Error: Algebraic loop exists.");
                }
            }
            return routine;
        }
    }
}