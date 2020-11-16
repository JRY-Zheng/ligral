using System.Collections.Generic;
using Ligral.Component;

namespace Ligral.Simulation 
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
            else if (node is InitializeableModel initializeableModel)
            {
                initializeableModel.Initialize();
                Visit(initializeableModel);
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
                    List<Model> algebraicLoop = AlgebraicLoopDetect(allNodes.FindAll(node=>!routine.Contains(node)));
                    System.Console.WriteLine("Algebraic Loop:" + string.Join(" -> ", algebraicLoop.ConvertAll(node=>node.ScopedName)));
                    throw new LigralException("Schematic Error: Algebraic loop exists.");
                }
            }
            return routine;
        }
        private void DirectFeedThroughAdvance(Model root, Model node, List<Model> loop)
        {
            loop.Add(node);
            // System.Console.WriteLine(string.Join(".", loop.ConvertAll(node=>"")) + node.Name);
            if (root == node)
            {
                throw new AlgebraicLoopException();
            }
            else if (!(node is InitializeableModel))
            {
                node.Inspect().ForEach(subNode=>DirectFeedThroughAdvance(root, subNode, loop));
            }
            loop.Remove(node);
        }
        private List<Model> AlgebraicLoopDetect(List<Model> unsolvedNodes)
        {
            List<Model> algebraicLoop = new List<Model>();
            foreach(Model node in unsolvedNodes)
            {
                algebraicLoop.Add(node);
                // System.Console.WriteLine(node.Name);
                try
                {
                    node.Inspect().ForEach(subNode=>DirectFeedThroughAdvance(node, subNode, algebraicLoop));
                }
                catch (AlgebraicLoopException)
                {
                    return algebraicLoop;
                }
                algebraicLoop.Remove(node);
            }
            return algebraicLoop;
        }
    }
    class AlgebraicLoopException : System.Exception
    {}
}