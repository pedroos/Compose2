using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Immutable;

namespace Abstraction
{
    public interface INode : IEquatable<INode>
    {
        public Guid Id { get; }
        public IEnumerable<INode> Children { get; }
        public INode Parent { get; }

        // As a requirement of immutability, create a copy of the node with a specified parent. Copy the id over as 
        // reference equality won't hold.
        // Should be overriden in every non-abstract INode implementer, to instance an object of that concrete class.
        // Failure to implement this method correctly in an INode implementer will result in the object being 
        // copied as one of its supertypes when added as a children.
        public INode WithParent(INode parent, Guid id);
    }

    public class Node : INode
    {
        public Guid Id { get; protected set; }
        public object Value { get; }
        public IEnumerable<INode> Children { get; private set; }
        public INode Parent { get; protected set; }

        public Node(object value, params INode[] children)
        {
            Value = value;
            Id = Guid.NewGuid();
            var parentedChildren = children?.Select(c => c.WithParent(this, c.Id)) ?? Array.Empty<INode>();
            Children = parentedChildren;
        }

        public INode WithParent(INode parent, Guid id) => new Node(Value, Children.ToArray()) 
            { Id = id, Parent = parent };

        public override string ToString() => string.Format("Node \"{0}\"", Value ?? "null");

        public bool Equals([AllowNull] INode other) => other.Id.Equals(Id);
    }

    public static class Extensions
    {
        public static IEnumerable<(int Level, T Node)> CastPair<T>(this IEnumerable<(int Level, INode Node)> nodes)
            where T : INode => nodes.Select(n => (n.Level, (T)n.Node));

        public static IEnumerable<INode> WalkUp(this INode node)
        {
            var curr = node;
            while (true)
            {
                yield return curr;
                if (curr.Parent == default)
                    yield break;
                curr = curr.Parent;
            }
        }

        public static IEnumerable<INode> DepthFirst(this INode node)
        {
            yield return node;
            foreach (var child in node.Children)
                foreach (var n in child.DepthFirst())
                    yield return n;
        }

        public static IEnumerable<(int Level, T Res)> DepthFirstGet<T>(this INode node, Func<INode, T> func)
        {
            int level = 0;
            yield return (level, func(node));
            foreach (var child in node.Children) 
                foreach (var n in child.DepthFirst())
                    yield return (level, func(n));
        }

        // Instead of traversing the entire tree, traverses a single path, that is, chooses a single children always.
        // TODO: Path-choosing parameter
        public static IEnumerable<INode> DepthFirstPath(this INode node, INode final = null)
        {
            yield return node;
            if (final != null && node.Equals(final)) yield break;
            if (node.Children.Count() > 0)
                foreach (var n in node.Children.First().DepthFirstPath())
                {
                    yield return n;
                    if (final != null && n.Equals(final)) yield break;
                }
        }

        public static IEnumerable<(int Level, INode Node)> BreadthFirst(this INode node)
        {
            var queue = new Queue<(int Level, INode Node)>();
            queue.Enqueue((0, node));
            while (queue.Count > 0)
            {
                var curr = queue.Dequeue();
                yield return curr;
                if (curr.Node.Children.Count() > 0)
                {
                    foreach (var child in curr.Node.Children)
                        queue.Enqueue((curr.Level + 1, child));
                }
            }
        }

        public static string[] NewLineArr = new string[] { Environment.NewLine };
        
        public static IEnumerable<IEnumerable<string>> DepthFirstPrintOut(this INode node, 
            params Func<INode, IEnumerable<string>>[] funcs) => 
                node.DepthFirstGet(node => funcs.Select(f => 
                    string.Join(Environment.NewLine, f(node))
                ).Concat(NewLineArr)).Select(nfg => 
                    nfg.Res.Idents(nfg.Level));

        public static IEnumerable<T> Times<T>(this int n, T o)
        {
            for (int i = 0; i < n; ++i)
                yield return o;
        }

        public static IEnumerable<string> Idents(this IEnumerable<string> strs, int levels) =>
            strs.Select(str => string.Format("{0}{1}", string.Join("", levels.Times("    ")), str));
    }
}
