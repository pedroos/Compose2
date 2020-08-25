using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Buffers;
using System.Data;
using System.Linq;
using System.Collections.Generic;

namespace Abstraction.Tests
{
    [TestClass]
    public class AbstractionTests
    {
        [TestMethod]
        public void Sum1()
        {
            var obj = new int[] { 1, 2, 3, 4 };
            var rule = new Rule<int, int>((3, 1), (1, -1));
            var change = new SinglePositionChange<int>(Funcs.Plus.F, rule);
            var changedObj = change.Perform(obj);
            Assert.IsTrue(changedObj.SequenceEqual(new int[] { 1, 1, 3, 5 }));
        }

        [TestMethod]
        public void SumReverse1()
        {
            var obj = new int[] { 1, 2, 3, 4 };
            var rule = new Rule<int, int>((3, 1), (1, -1));
            var change = new SinglePositionChange<int>(Funcs.Plus.F, rule);
            var changedObj = change.Perform(obj);
            var changeRev = new SinglePositionChange<int>(Funcs.Plus.R, rule);
            changedObj = changeRev.Perform(changedObj);
            Assert.IsTrue(changedObj.SequenceEqual(obj));
        }

        [TestMethod]
        public void SumReverse1b()
        {
            var obj = new int[] { 1, 2, 3, 4 };
            var rule = new Rule<int, int>((3, 1), (1, -1));
            var changeF = new SinglePositionChange<int>(Funcs.Plus.F, rule);
            var changeR = new SinglePositionChange<int>(Funcs.Plus.R, rule);
            var objChanged =
                changeR.Perform(
                    changeF.Perform(
                        changeR.Perform(
                            changeF.Perform(obj)
                        )
                    )
                );
            Assert.IsTrue(objChanged.SequenceEqual(obj));
        }

        [TestMethod]
        public void NodeDepthFirst()
        {
            var root =
                new Node(0,
                    new Node(1,
                        new Node(2,
                            new Node(4)
                        ),
                        new Node(3)
                    )
                );

            var vals = root.DepthFirst().Cast<Node>().Select(n => (int)n.Value);
            Assert.IsTrue(vals.SequenceEqual(new int[] { 0, 1, 2, 4, 3 }));
        }

        [TestMethod]
        public void NodeBreadthFirst()
        {
            var root =
                new Node(0,
                    new Node(1,
                        new Node(2,
                            new Node(4)
                        ),
                        new Node(3)
                    )
                );

            var bf = root.BreadthFirst().CastPair<Node>();
            var vals = bf.Select(n => (int)n.Node.Value).ToArray();
            var levels = bf.Select(n => n.Level).ToArray();
            Assert.IsTrue(vals.SequenceEqual(new int[] { 0, 1, 2, 3, 4 }));
            Assert.IsTrue(levels.SequenceEqual(new int[] { 0, 1, 2, 2, 3 }));
        }

        [TestMethod]
        public void NodeBreadthFirst2()
        {
            var root =
                new Node(0,
                    new Node(1,
                        new Node(2),
                        new Node(3,
                            new Node(4)
                        )
                    )
                );

            var bf = root.BreadthFirst().CastPair<Node>();
            var vals = bf.Select(n => (int)n.Node.Value);
            var levels = bf.Select(n => n.Level).ToArray();
            Assert.IsTrue(vals.SequenceEqual(new int[] { 0, 1, 2, 3, 4 }));
            Assert.IsTrue(levels.SequenceEqual(new int[] { 0, 1, 2, 2, 3 }));
        }

        // PAROU AQUI

        [TestMethod]
        public void DepthFirstPath1()
        {
            var root =
                new Node(0,
                    new Node(1,
                        new Node(2,
                            new Node(4)
                        ),
                        new Node(3)
                    )
                );

            var vals = root.DepthFirstPath().Cast<Node>().Select(n => (int)n.Value).ToArray();
            Assert.IsTrue(vals.SequenceEqual(new int[] { 0, 1, 2, 4 }));
        }

        [TestMethod]
        public void DepthFirstPathFinalNode1()
        {
            var root =
                new Node(0,
                    new Node(1,
                        new Node(2,
                            new Node(4)
                        ),
                        new Node(3)
                    )
                );

            var finalNode = root.Children.First().Children.First();
            var vals = root.DepthFirstPath(finalNode).Cast<Node>().Select(n => (int)n.Value).ToArray();
            Assert.IsTrue(vals.SequenceEqual(new int[] { 0, 1, 2 }));
        }

        [TestMethod]
        public void NodeWalkUp()
        {
            var root =
                new Node(0,
                    new Node(1,
                        new Node(2,
                            new Node(4)
                        ),
                        new Node(3)
                    )
                );

            var path = root.DepthFirstPath();
            var pathVals = path.Last().WalkUp().Cast<Node>().Select(n => (int)n.Value).ToArray();
            Assert.IsTrue(pathVals.SequenceEqual(new int[] { 4, 2, 1, 0 }));
        }

        [TestMethod]
        public void NodeChildren1()
        {
            var root =
                new Node(0,
                    new Node(1,
                        new Node(2,
                            new Node(3)
                        )
                    )
                );

            var path = root.DepthFirstPath();
            Assert.AreEqual(4, path.Count());
            Assert.AreEqual(1, (int)((Node)path.ElementAt(0).Children.Single()).Value);
            Assert.AreEqual(2, (int)((Node)path.ElementAt(1).Children.Single()).Value);
            Assert.AreEqual(3, (int)((Node)path.ElementAt(2).Children.Single()).Value);
            Assert.AreEqual(0, path.ElementAt(3).Children.Count());
        }

        [TestMethod]
        public void NodeParents1()
        {
            var root =
                new Node(0,
                    new Node(1,
                        new Node(2,
                            new Node(3)
                        ),
                        new Node(4)
                    )
                );

            var path = root.DepthFirst();
            Assert.IsNull(path.ElementAt(0).Parent);
            Assert.AreEqual(0, (int)((Node)path.ElementAt(1).Parent).Value);
            Assert.AreEqual(1, (int)((Node)path.ElementAt(2).Parent).Value);
            Assert.AreEqual(2, (int)((Node)path.ElementAt(3).Parent).Value);
            Assert.AreEqual(1, (int)((Node)path.ElementAt(4).Parent).Value);
        }

        [TestMethod]
        public void NodeEquality1()
        {
            var node1 =
                new Node(1,
                    new Node(2,
                        new Node(3)
                    ),
                    new Node(4)
                );
            var root =
                new Node(0,
                    node1
                );
            Assert.IsTrue(node1.Equals((Node)root.Children.First()));
        }

        [TestMethod]
        public void State1()
        {
            /*
             * Versioning is an initial object and a tree of Changes.
             * Actually, unless we are to do merges/other crazy experimental stuff, we should restrain ourselves to follow 
             * paths, that is branches, in the tree (traversals are not such walks).
             * Even though we may not even want to walk paths. It's important instead to recover individual states and modify 
             * change-by-change.
             * An arbitrary state should be recoverable simply by starting for the initial object and walking the path to the 
             * state.
             * But any state in a path should be recoverable by walking the path, in either direction (as long as we store the 
             * "current" object of the current state).
             */

            var obj = new int[] { 1, 2, 3, 4 };
            var rule = new Rule<int, int>((1, 1), (3, -1));

            var changeF = new SinglePositionChange<int>(Funcs.Plus.F, rule);
            var changeR = new SinglePositionChange<int>(Funcs.Plus.R, rule);

            var root =
                new Node(changeF,
                    new Node(changeR,
                        new Node(changeF,
                            new Node(changeR)
                        )
                    )
                );

            var path = root.DepthFirstPath().Cast<Node>();
            var objChanged = path.Change(obj);

            Assert.IsTrue(objChanged.SequenceEqual(obj));
        }

        [TestMethod]
        public void StateHistory1()
        {
            var obj = new int[] { 1, 2, 3, 4 };
            var rule = new Rule<int, int>((1, 1), (3, -1));

            var changeF = new SinglePositionChange<int>(Funcs.Plus.F, rule);
            var changeR = new SinglePositionChange<int>(Funcs.Plus.R, rule);

            var root =
                new Node(changeF,
                    new Node(changeR,
                        new Node(changeF,
                            new Node(changeR)
                        )
                    )
                );

            var path = root.DepthFirstPath().Cast<Node>();
            var objsChanged = path.ChangeHistory(obj);

            Assert.AreEqual(5, objsChanged.Count());
            Assert.IsTrue(objsChanged.ElementAt(0).SequenceEqual(new int[] { 1, 2, 3, 4 }));
            Assert.IsTrue(objsChanged.ElementAt(1).SequenceEqual(new int[] { 1, 3, 3, 3 }));
            Assert.IsTrue(objsChanged.ElementAt(2).SequenceEqual(new int[] { 1, 2, 3, 4 }));
            Assert.IsTrue(objsChanged.ElementAt(3).SequenceEqual(new int[] { 1, 3, 3, 3 }));
            Assert.IsTrue(objsChanged.ElementAt(4).SequenceEqual(new int[] { 1, 2, 3, 4 }));
        }

        [TestMethod]
        public void StateNext1()
        {
            var obj = new int[] { 1, 2, 3, 4 };
            var rule = new Rule<int, int>((1, 1), (3, -1));
            var changeF = new SinglePositionChange<int>(Funcs.Plus.F, rule);

            var root = new Node(changeF);

            var objChanged = ((SinglePositionChange<int>)root.Value).Perform(obj).ToArray();

            Assert.IsTrue(objChanged.SequenceEqual(new int[] { 1, 3, 3, 3 }));
        }

        [TestMethod]
        public void StatePreviousManual1()
        {
            var obj = new int[] { 1, 2, 3, 4 };
            var rule = new Rule<int, int>((1, 1), (3, -1));

            var changeF = new SinglePositionChange<int>(Funcs.Plus.F, rule);
            var changeR = new SinglePositionChange<int>(Funcs.Plus.R, rule);

            var root =
                new Node(changeF,
                    new Node(changeR,
                        new Node(changeF,
                            new Node(changeR)
                        )
                    )
                );

            // "Manually" revert changes by taking the original object and redoing all changes up to a previous point.
            var destNode = root.Children.First().Children.First();
            var path = root.DepthFirstPath(destNode).Cast<Node>();
            var objChanged = path.Change(obj);

            Assert.IsTrue(objChanged.SequenceEqual(new int[] { 1, 3, 3, 3 }));
        }

        [TestMethod]
        public void StateUnchange1()
        {
            var obj = new int[] { 1, 2, 3, 4 };
            var rule = new Rule<int, int>((1, 1), (3, -1));

            var change = new SinglePositionChange<int>(Funcs.Plus, null, rule);

            var root =
                new Node(change,
                    new Node(change)
                );

            // Undo changes by using the reverse function.
            var path = root.DepthFirstPath().Cast<Node>();
            var objChanged = path.Change(obj);

            Assert.IsTrue(objChanged.SequenceEqual(new int[] { 1, 4, 3, 2 }));

            objChanged = path.Change(objChanged, true);

            Assert.IsTrue(objChanged.SequenceEqual(obj));
        }

        [TestMethod]
        public void StatePreviousUnchange1()
        {
            var obj = new int[] { 1, 2, 3, 4 };
            var rule = new Rule<int, int>((1, 1), (3, -1));

            var change = new SinglePositionChange<int>(Funcs.Plus, null, rule);

            var root =
                new Node(change,
                    new Node(change,
                        new Node(change,
                            new Node(change,
                                new Node(change)
                            )
                        )
                    )
                );

            // Reverse-undo from a specific node.
            // First, change up to the specific node.
            var destNode = root.Children.First().Children.First().Children.First();
            var path = root.DepthFirstPath(destNode).Cast<Node>();
            var objChanged = path.Change(obj);

            Assert.IsTrue(objChanged.SequenceEqual(new int[] { 1, 6, 3, 0 }));

            // Then, undo only some changes by selecting a subpath.
            // Path from node (0-based) 3 (the current node) to node 2, by skipping two nodes in the beginning.
            // Since there are now two nodes, two transformations are undone.
            path = path.Skip(2);
            objChanged = path.Change(objChanged, true);

            Assert.IsTrue(objChanged.SequenceEqual(new int[] { 1, 4, 3, 2 }));
        }

        [TestMethod]
        public void StatePreviousUnchangeHistory1()
        {
            var obj = new int[] { 1, 2, 3, 4 };
            var rule = new Rule<int, int>((1, 1), (3, -1));

            var change = new SinglePositionChange<int>(Funcs.Plus, null, rule);

            var root =
                new Node(change,
                    new Node(change,
                        new Node(change,
                            new Node(change,
                                new Node(change)
                            )
                        )
                    )
                );

            var destNode = root.Children.First().Children.First().Children.First();
            var path = root.DepthFirstPath(destNode).Cast<Node>();
            var objsChanged = path.ChangeHistory(obj);

            Assert.AreEqual(5, objsChanged.Count());
            Assert.IsTrue(objsChanged.ElementAt(0).SequenceEqual(new int[] { 1, 2, 3, 4 }));
            Assert.IsTrue(objsChanged.ElementAt(1).SequenceEqual(new int[] { 1, 3, 3, 3 }));
            Assert.IsTrue(objsChanged.ElementAt(2).SequenceEqual(new int[] { 1, 4, 3, 2 }));
            Assert.IsTrue(objsChanged.ElementAt(3).SequenceEqual(new int[] { 1, 5, 3, 1 }));
            Assert.IsTrue(objsChanged.ElementAt(4).SequenceEqual(new int[] { 1, 6, 3, 0 }));

            path = path.Skip(2);
            objsChanged = path.ChangeHistory(objsChanged.Last(), true);

            Assert.AreEqual(3, objsChanged.Count());
            Assert.IsTrue(objsChanged.ElementAt(0).SequenceEqual(new int[] { 1, 6, 3, 0 }));
            Assert.IsTrue(objsChanged.ElementAt(1).SequenceEqual(new int[] { 1, 5, 3, 1 }));
            Assert.IsTrue(objsChanged.ElementAt(2).SequenceEqual(new int[] { 1, 4, 3, 2 }));
        }
    }
}