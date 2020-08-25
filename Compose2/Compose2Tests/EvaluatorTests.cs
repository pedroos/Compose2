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

    [TestClass]
    public class EvaluatorTests
    {
        Tk tk;

        [TestInitialize]
        public void Initialize()
        {
            tk = new Tk();
        }

        [TestMethod]
        public void EvaluateRule1()
        {
            string str = "(1→45, 23→ −1)";

            string printOut = 
                string.Join("", 
                    tk.Rule.DepthFirstPrintOut(Extensions.ElementPrintOut1, Extensions.TokenPrintOut1)
                        .Select(po => string.Join(Environment.NewLine, po))
                );

            var @out = Evaluator.Evaluate(str, tk.Rule).ToArray();
        }

        [TestMethod]
        public void EvaluateFun1()
        {
            string str = "(a + b , a− b)";

            string printOut =
                string.Join("",
                    tk.Function.DepthFirstPrintOut(Extensions.ElementPrintOut1, Extensions.TokenPrintOut1)
                        .Select(po => string.Join(Environment.NewLine, po))
                );

            var @out = Evaluator.Evaluate(str, tk.Function).ToArray();
        }
    }
}