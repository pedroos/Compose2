using Abstraction.Parser.Regexp;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Linq;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices.WindowsRuntime;

namespace Abstraction.Parser.Tree
{
    public static class Evaluator
    {
        public static IEnumerable<(int Level, Token Token)> TokenSeq(this Element elem)
        {
            return elem.BreadthFirst().CastPair<Token>();
        }

        public static IEnumerable<Token> TokenSeqDepth(this Element elem)
        {
            return elem.DepthFirst().Cast<Token>();
        }

        public static IEnumerable<Token> Evaluate(string str, Element elem)
        {
            var tokens = elem.TokenSeqDepth().ToArray();
            var tokensEn = tokens.AsEnumerable().GetEnumerator();

            var chars = str.ToCharArray();
            var charsEn = chars.AsEnumerable().GetEnumerator();

            //int pos = 0;
            if (!tokensEn.MoveNext()) return default;

            var @out = new List<Token>();

            //IEnumerable<Token> Eval() 
            //{
            bool stop = false;
            while (charsEn.MoveNext())
            {
                while (charsEn.Current == ' ')
                    if (!charsEn.MoveNext())
                    {
                        //yield break;
                        stop = true;
                        break;
                    }

                if (stop) break;

                while (tokensEn.Current.Match(charsEn.Current))
                {
                    //yield return tokensEn.Current;
                    @out.Add(tokensEn.Current);

                    if (!tokensEn.MoveNext())
                    {
                        stop = true;
                        break;//yield break;
                    }
                }

                if (stop) break;

                //++pos;
            }
            //}
            //var @out = Eval();

            return @out;
        }
    }

    public class ParseException : Exception
    {

    }
}
