/* Copyright 2019-2020 Junruoyu Zheng. All rights reserved.

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using Ligral.Component;

namespace Ligral.Simulation 
{
    class Inspector
    {
        private List<Model> routine = new List<Model>();
        private List<Model> allNodes = new List<Model>();
        private Logger logger = new Logger("Inspector");
        private void Visit(Model node)
        {
            if (!allNodes.Contains(node))
            {
                throw logger.Error(new LigralException($"Schematic Error: {node.Name} is not in inspection list."));
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
                    throw new LigralException("Algebraic Loop:" + string.Join(" -> ", algebraicLoop.ConvertAll(node=>node.ScopedName))+"\n"+
                                              "schematic Error: Algebraic loop exists.");
                }
            }
            logger.Info("Inspection passed.");
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