using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Buffers;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.Serialization;
using System.ComponentModel.Design;

namespace Abstraction.Tests
{
    using Abstraction.Parser.Tree;
    using NuGet.Frameworks;

    [TestClass]
    public class ParserTests
    {
        Tk tk;

        [TestInitialize]
        public void Initialize()
        {
            tk = new Tk();
        }

        [TestMethod]
        public void TokenSeqDepthTypes1()
        {
            var rule = tk.Rule;
            var tokens = tk.Rule.TokenSeqDepth();

            var typesTest = new Type[]
            {
                typeof(Element),
                typeof(Element),
                typeof(Element),
                typeof(TokenElement),
                typeof(Operator),
                typeof(Operator),
                typeof(Element),
                typeof(TokenElement),
                typeof(Operator),
                typeof(Operator)
            };

            Assert.AreEqual(typesTest.Length, tokens.Count());
            for (int i = 0; i < typesTest.Length; ++i)
                Assert.IsTrue(typesTest[i].IsAssignableFrom(tokens.ElementAt(i).GetType()));
        }

        [TestMethod]
        public void TokenSeqDepthTypes2()
        {
            var tokens = tk.Function.TokenSeqDepth();
            var types = tokens.Select(t => t.GetType());

            Assert.IsTrue(types.SequenceEqual(new Type[]
            {
                typeof(Element),
                typeof(Element),
                typeof(TokenElement),
                typeof(Operator),
                typeof(Operator),
                typeof(Element),
                typeof(Element),
                typeof(TokenElement),
                typeof(Operator),
                typeof(Operator),
                typeof(Element),
                typeof(TokenElement),
                typeof(Operator)
            }));
        }

        [TestMethod]
        public void TokenSeqDepthElemTypes1()
        {
            var tokens = tk.Rule.TokenSeqDepth();

            Assert.IsTrue(tokens.OfType<Element>().Select(e => e.Type).SequenceEqual(new ElemType[]
            {
                tk.Rule.Type,
                ElemType.KeyValue,
                ElemType.Position1,
                tk.KeyValueItemToken.Type,
                ElemType.Position2,
                tk.KeyValueItemToken.Type
            }));
        }

        [TestMethod]
        public void TokenSeqDepthElemTypes2()
        {
            var tokens = tk.Function.TokenSeqDepth();

            Assert.IsTrue(tokens.OfType<Element>().Select(e => e.Type).SequenceEqual(new ElemType[]
            {
                tk.Function.Type,
                tk.Expr.Type,
                tk.ExprToken.Type,
                ElemType.Pair,
                tk.Expr.Type,
                tk.ExprToken.Type, 
                tk.Expr.Type, 
                tk.ExprToken.Type
            }));
        }

        [TestMethod]
        public void TokenSeqDepthOperatorNames1()
        {
            var tokens = tk.Rule.TokenSeqDepth();

            Assert.IsTrue(tokens.OfType<Operator>().Select(o => o.Name).SequenceEqual(new string[]
            {
                tk.Array.Name, 
                tk.And.Name, 
                tk.Array.Name, 
                tk.Array.Name
            }));
        }

        [TestMethod]
        public void TokenSeqDepthOperatorNames2()
        {
            var tokens = tk.Function.TokenSeqDepth();

            Assert.IsTrue(tokens.OfType<Operator>().Select(o => o.Name).SequenceEqual(new string[]
            {
                tk.Array.Name, 
                tk.Or.Name, 
                tk.Array.Name, 
                tk.And.Name, 
                tk.Array.Name
            }));
        }

        [TestMethod]
        public void TokenSeqLevels1()
        {
            var tokens = tk.Rule.TokenSeq();

            Assert.IsTrue(tokens.Select(t => t.Level).SequenceEqual(new int[]
            {
                0, 1, 1, 2, 2, 2, 3, 3, 3, 3
            }));
        }

        [TestMethod]
        public void TokenSeqLevels2()
        {
            var tokens = tk.Function.TokenSeq();

            Assert.IsTrue(tokens.Select(t => t.Level).SequenceEqual(new int[]
            {
                0, 1, 1, 1, 2, 2, 2, 2, 2, 3, 3, 3, 3
            }));
        }

        [TestMethod]
        public void TokenSeqTypes1()
        {
            var tokens = tk.Rule.TokenSeq().ToArray();
            var types = tokens.Select(t => t.Token.GetType()).ToArray();

            Assert.IsTrue(types.SequenceEqual(new Type[]
            {
                typeof(Element),
                typeof(Element),
                typeof(Operator),
                typeof(Element),
                typeof(Operator),
                typeof(Element),
                typeof(TokenElement),
                typeof(Operator),
                typeof(TokenElement),
                typeof(Operator)
            }));
        }

        [TestMethod]
        public void TokenSeqTypes2()
        {
            var tokens = tk.Function.TokenSeq();
            var types = tokens.Select(t => t.Token.GetType()).ToArray();

            Assert.IsTrue(types.SequenceEqual(new Type[]
            {
                typeof(Element),
                typeof(Element),
                typeof(Operator),
                typeof(Element),
                typeof(TokenElement),
                typeof(Operator),
                typeof(Element),
                typeof(Operator),
                typeof(Element),
                typeof(TokenElement),
                typeof(Operator),
                typeof(TokenElement),
                typeof(Operator)
            }));
        }

        [TestMethod]
        public void TokenSeqElemTypes1()
        {
            var tokens = tk.Rule.TokenSeq();
            var names = tokens.Select(t => t.Token).OfType<Element>().Select(e => e.Type);

            Assert.IsTrue(names.SequenceEqual(new ElemType[]
            {
                tk.Rule.Type,
                ElemType.KeyValue,
                ElemType.Position1, 
                ElemType.Position2, 
                tk.KeyValueItemToken.Type,
                tk.KeyValueItemToken.Type
            }));
        }

        [TestMethod]
        public void TokenSeqElemTypes2()
        {
            var tokens = tk.Function.TokenSeq();
            var names = tokens.Select(t => t.Token).OfType<Element>().Select(e => e.Type);

            Assert.IsTrue(names.SequenceEqual(new ElemType[]
            {
                tk.Function.Type,
                tk.Expr.Type, 
                ElemType.Pair, 
                tk.ExprToken.Type, 
                tk.Expr.Type, 
                tk.Expr.Type, 
                tk.ExprToken.Type, 
                tk.ExprToken.Type
            }));
        }

        [TestMethod]
        public void TokenSeqOperatorNames1()
        {
            var tokens = tk.Rule.TokenSeq();
            var names = tokens.Select(t => t.Token).OfType<Operator>().Select(o => o.Name);

            Assert.IsTrue(names.SequenceEqual(new string[]
            {
                tk.Array.Name,
                tk.And.Name,
                tk.Array.Name,
                tk.Array.Name
            }));
        }

        [TestMethod]
        public void TokenSeqOperatorNames2()
        {
            var tokens = tk.Function.TokenSeq();
            var names = tokens.Select(t => t.Token).OfType<Operator>().Select(o => o.Name);

            Assert.IsTrue(names.SequenceEqual(new string[]
            {
                tk.Or.Name,
                tk.Array.Name,
                tk.And.Name,
                tk.Array.Name,
                tk.Array.Name
            }));
        }

        [TestMethod]
        public void Unicode1()
        {
            var ue = new UnicodeEnumerator("a:1");
            Assert.IsTrue(ue.Next(out char chr, out char? unicode2));
            Assert.AreEqual('a', chr);
            Assert.IsFalse(unicode2.HasValue);
            Assert.IsTrue(ue.Next(out chr, out unicode2));
            Assert.AreEqual(':', chr);
            Assert.IsFalse(unicode2.HasValue);
            Assert.IsTrue(ue.Next(out chr, out unicode2));
            Assert.AreEqual('1', chr);
            Assert.IsFalse(unicode2.HasValue);
            Assert.IsFalse(ue.Next(out chr, out unicode2));
            Assert.AreEqual(default, chr);
            Assert.IsFalse(unicode2.HasValue);
        }

        [TestMethod]
        public void Unicode2()
        {
            var ue = new UnicodeEnumerator("a→→1");
            Assert.IsTrue(ue.Next(out char chr, out char? unicode2));
            Assert.AreEqual('a', chr);
            Assert.IsFalse(unicode2.HasValue);
            Assert.IsTrue(ue.Next(out chr, out unicode2));
            Assert.AreEqual(8594, chr);
            Assert.IsTrue(unicode2.HasValue);
            Assert.AreEqual(4, unicode2);
            Assert.IsTrue(ue.Next(out chr, out unicode2));
            Assert.AreEqual(8594, chr);
            Assert.IsTrue(unicode2.HasValue);
            Assert.AreEqual(4, unicode2);
            Assert.IsTrue(ue.Next(out chr, out unicode2));
            Assert.AreEqual('1', chr);
            Assert.IsFalse(unicode2.HasValue);
            Assert.IsFalse(ue.Next(out chr, out unicode2));
            Assert.AreEqual(default, chr);
            Assert.IsFalse(unicode2.HasValue);
        }
    }
}