/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Linq;
using System.Collections.Generic;
using Ligral.Component;

namespace Ligral.Simulation 
{
    class Inspector
    {
        private List<Model> routine = new List<Model>();
        private List<Model> allNodes = new List<Model>();
        private Logger logger = new Logger("Inspector");
        private void Visit(Model node, bool allowGuess)
        {
            logger.Debug($"Visiting {node.ScopedName}, allow guess: {allowGuess}");
            if (!allNodes.Contains(node))
            {
                throw logger.Error(new LigralException($"Schematic Error: {node.ScopedName} is not in inspection list."));
            }
            if(node.IsReady())
            {
                if (!routine.Contains(node))
                {
                    try
                    {
                        logger.Debug($"Checking {node.ScopedName}");
                        node.Check();
                    }
                    catch (LigralException e)
                    {
                        if (!allowGuess)
                        {
                            logger.Solve();
                            logger.Debug($"Fail to check {node.ScopedName}: {e.Message}");
                            if (node is InitializeableModel iNode) 
                            {
                                iNode.ResetCheck();
                                logger.Debug($"Uncheck {iNode.ScopedName}");
                            }
                            return;
                        }
                        throw logger.Error(new LigralException("Ligral cannot guess the correct shape"));
                    }
                    if (node is InitializeableModel i && i.Guessing && !allowGuess) 
                    {
                        i.ResetCheck();
                        logger.Debug($"Uncheck {i.ScopedName}");
                        return;
                    }
                    routine.Add(node);
                    logger.Debug($"Add {node.ScopedName} to routine");
                    node.Inspect().ForEach(subNode=>Visit(subNode, allowGuess));
                }
            }
            else if (node is InitializeableModel initializeableModel)
            {
                logger.Debug($"Initialize {node.ScopedName}");
                initializeableModel.Initialize();
                Visit(initializeableModel, allowGuess);
            }
        }
        public List<Model> Inspect(List<Model> nodes)
        {
            allNodes = nodes;
            List<Model> notConnectedModels = nodes.FindAll(node=>(
                !node.IsConnected() && !(node is IFixable fixable && fixable.FixConnection())
            ));
            if (notConnectedModels.Count>0)
            {
                throw logger.Error(new LigralException("Schematic Error: Some nodes are not fully connected: "+
                string.Join(", ", notConnectedModels.ConvertAll(node=>node.ScopedName))));
                // string.Join(", ", nodes.FindAll(node=>routine.FindAll(n=>n.Name==node.Name).Count==0).ConvertAll(node=>node.Name)));
            }
            // System.Console.WriteLine(string.Join(", ", allNodes.ConvertAll(node=>node.Name)));
            foreach(Model node in nodes.FindAll(m => m.InPortCount() == 0))
            {
                Visit(node, false);
            }
            var leftNodes = nodes.Except(routine);
            int leftNodesCount = leftNodes.Count();
            while (true)
            {
                foreach(Model node in leftNodes)
                {
                    // if (routine.Contains(node)) continue;
                    Visit(node, false);
                }
                leftNodes = nodes.Except(routine);
                if (leftNodes.Count() == leftNodesCount) break;
                leftNodesCount = leftNodes.Count();
                if (leftNodesCount == 0) break;
                logger.Warn($"Ligral cannot determine all shape this time, {leftNodesCount} nodes ({string.Join(", ", leftNodes.Select(node=>node.ScopedName))}) was left, will try again.");
            }
            foreach(Model node in leftNodes)
            {
                // if (routine.Contains(node)) continue;
                if (node is InitializeableModel i)
                {
                    i.ResetCheck();
                }
            }
            if (leftNodesCount > 0)
            {
                logger.Warn($"Try to guess shape of {leftNodesCount} nodes: {string.Join(", ", leftNodes.Select(node=>node.ScopedName))}");
                foreach(Model node in leftNodes)
                {
                    Visit(node, true);
                }
            }
            // System.Console.WriteLine(string.Join(", ", routine.ConvertAll(node=>node.Name)));
            if (routine.Count != nodes.Count)
            {
                List<Model> algebraicLoop = AlgebraicLoopDetect(allNodes.FindAll(node=>!routine.Contains(node)));
                throw logger.Error(new LigralException("Algebraic Loop:" + string.Join(" -> ", algebraicLoop.ConvertAll(node=>node.ScopedName))+"\n"+
                                            "schematic Error: Algebraic loop exists."));
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