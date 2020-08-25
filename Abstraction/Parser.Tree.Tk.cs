using Abstraction.Parser.Regexp;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Linq;
using System.Diagnostics.CodeAnalysis;
using System.ComponentModel;

namespace Abstraction.Parser.Tree
{
    public class Tk
    {
        public readonly Operator And;
        //public readonly Operator Array;

        public readonly Atom Letter;
        public readonly Atom Numeral;

        public readonly TokenElement KeyValueItemToken;
        public readonly Element Rule;
        public readonly TokenElement ExprToken;
        public readonly Element Expr;
        public readonly Element Function;

        public Tk()
        {
            And = new Operator(this, "And", OperatorType.Infix);
            //Array = new Operator(this, "Array", OperatorType.Suffix);

            Element EitherEager(params Token[] tokens) => new Element(this, ElemType.EitherEager, tokens);

            Letter = new Atom(this, "Letter", @"[a-zA-Z]");
            Numeral = new Atom(this, "Numeral", @"[0-9]");

            //Element Array(Token typeParameter) => new Element(this, typeParameter, ElemType.Array);
            Element Array(Token token) => new Element(this, ElemType.Array, token);

            //KeyValueItemToken = new TokenElement(this, ElemType.KeyValueItemToken, Numeral);

            TokenElement KeyValueItemToken(Atom atom) => 
                new TokenElement(this, ElemType.KeyValueItemToken, atom);

            //PositionElement Position1(params Token[] tokens) =>
            //    new PositionElement(this, 1, ElemType.Position, tokens);

            //PositionElement Position2(params Token[] tokens) =>
            //    new PositionElement(this, 2, ElemType.Position, tokens);

            //Element KeyValue(Token keyTypeParameter, Token valueTypeParameter) =>
            //    new Element(this, new Token[] { keyTypeParameter, valueTypeParameter }, ElemType.KeyValue,
            //        Position1(KeyValueItemToken, Array), And, Position2(KeyValueItemToken, Array)
            //    );

            //Element KeyValue(Token keyTypeParameter, Token valueTypeParameter) =>
            //    new Element(this, new Token[] { keyTypeParameter, valueTypeParameter }, ElemType.KeyValue,
            //        Position1(keyTypeParameter), And, Position2(valueTypeParameter)
            //    );

            Element KeyValue(Token keyTypeParameter, Token valueTypeParameter) =>
                new Element(this, ElemType.KeyValue, 
                    keyTypeParameter, And, valueTypeParameter);

            //Rule = new Element(this, ElemType.Rule,
            //    KeyValue(Numeral, Numeral), Array
            //);

            Rule = new Element(this, ElemType.Rule,
                Array(KeyValue(
                    Array(KeyValueItemToken(Numeral)), Array(KeyValueItemToken(Numeral)))));

            ExprToken = new TokenElement(this, ElemType.ExprToken, Letter);

            //Expr = new Element(this, ElemType.Expr,
            //    ExprToken, Array
            //);

            Expr = new Element(this, ElemType.Expr,
                Array(ExprToken));

            //Element Pair(Token position1TypeParameter, Token position2TypeParameter) =>
            //    new Element(this, new Token[] { position1TypeParameter, position2TypeParameter },
            //        ElemType.Pair, position1TypeParameter, And, position2TypeParameter);

            Element Pair(Token typeParameter1, Token typeParameter2) =>
                new Element(this, ElemType.Pair, 
                    typeParameter1, And, typeParameter2);

            Function = new Element(this, ElemType.Function,
                EitherEager(Pair(Expr, Expr), Expr));
        }
    }
}
