using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Immutable;

namespace Abstraction
{
    /*
     * Show this in https://observablehq.com/@d3/collapsible-tree.
     * https://cefsharp.github.io/
     */

    public interface INode<out T>
    {
        public Guid Id { get; }
        public T Value { get; }
        public IEnumerable<INode<T>> Children { get; }
        public INode<T> Parent { get; }

        public IEnumerable<INode<T>> WalkUp();
        public IEnumerable<INode<T>> DepthFirst();
        public IEnumerable<INode<T>> DepthFirstPath(INode<T> finalNode = null);
    }

    public class Node : INode<object>, IEquatable<Node>
    {
        public Guid Id { get; }
        public object Value { get; }
        public IEnumerable<INode<object>> Children { get; }
        public INode<object> Parent { get; }

        public Node(object value, params INode<object>[] children)
        {
            Id = Guid.NewGuid();
            Value = value;
            var parentedChildren = children.Select(c => new Node(c, this, c.Id));
            Children = parentedChildren;
        }

        // To preserve immutability, creates a copy of the node with a specified parent.
        // Reference equality won't hold anymore, so copy the id over.
        private Node(INode<object> original, INode<object> parent, Guid id) : 
            this(original.Value, original.Children.ToArray())
        {
            Id = id;
            Parent = parent;
        }

        public IEnumerable<INode<object>> WalkUp()
        {
            var curr = (INode<object>)this;
            while (true)
            {
                yield return curr;
                if (curr.Parent == default)
                    yield break;
                curr = curr.Parent;
            }
        }

        public IEnumerable<INode<object>> DepthFirst()
        {
            yield return this;
            foreach (var child in Children) 
                foreach (var node in child.DepthFirst()) 
                    yield return node;
        }

        // Instead of traversing the entire tree, traverses a single path, that is, chooses a single children always.
        // TODO: Path-choosing parameter
        public IEnumerable<INode<object>> DepthFirstPath(INode<object> finalNode = null)
        {
            yield return this;
            if (finalNode != null && Equals(finalNode)) yield break;
            if (Children.Count() > 0)
                foreach (var node in Children.First().DepthFirstPath())
                {
                    yield return node;
                    if (finalNode != null && node.Equals(finalNode)) yield break;
                }
        }

        public IEnumerable<(int Level, INode<object> Node)> BreadthFirst()
        {
            var queue = new Queue<(int Level, INode<object> Node)>();
            int level = 0;
            queue.Enqueue((level, this));
            while (queue.Count > 0)
            {
                var curr = queue.Dequeue();
                yield return curr;
                if (curr.Node.Children.Count() > 0)
                {
                    level++;
                    foreach (var child in curr.Node.Children)
                        queue.Enqueue((level, child));
                }
            }
        }

        public override string ToString()
        {
            return string.Format("Node \"{0}\"", Value);
        }

        public bool Equals([AllowNull] Node other)
        {
            return other.Id.Equals(Id);
        }
    }
}
