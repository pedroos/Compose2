using Abstraction.Parser.Regexp;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Linq;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Abstraction.Parser.Tree
{
    /* Tokens are characters with a token type.
     * An expression is a sequence of tokens of the same type.
     * An expression forms a path from an specific token.
     * The cursor moves from character to character.
     */

     /* Rule:
      * (1→11, 231→ −1)
      * (             obj_start: rule<int,int> → elem_start: keyvalue<int,int> → key_start: int
      * 1             keyvalue_token
      * →             keyvalue_sep → key_end → value_start: int
      * 1             keyvalue_token
      *     1         keyvalue_token
      * ,             elem_end → elem_start: keyvalue<int,int> → key_start: int
      *               space
      * 2             keyvalue_token
      *     3         keyvalue_token
      *         1     keyvalue_token
      * →             keyvalue_sep → key_end → value_start: int
      * −1            keyvalue_token
      * )             obj_end → elem_end
      * 
      * Function:
      * (a + b, a−b)
      * (             obj_start: fun<int,int,int> → elem_start: expr // Expression is not typed and not parsed.
      * a + b         expr_token
      * ,             elem_end → elem_start: expr
      *  a-b          expr_token
      * )             obj_end → elem_end
      * 
      * Mandatory parent-children relationships:
      *  - obj_start[rule]        > elem_start[keyvalue]
      *  - elem_start[keyvalue]   > key_start
      *  - key_start|value_start  > keyvalue_token
      *  - elem_start[keyvalue]   > keyvalue_sep
      *  - elem_start[keyvalue]   > key_end
      *  - elem_start[keyvalue]   > value_start
      *  - elem_start[keyvalue]   > value_end
      *  - obj_start              > elem_end
      *  - obj_start[function]    > elem_start[expr]
      *  - elem_start[expr]       > expr_token
      */

     /*
     * Possibilities tree (open/close model):
     * 
     * rule = keyvalue[]
     * keyvalue = keyvalue_item + keyvalue_item
     * keyvalue_item = keyvalueitem_token[]
     * keyvalueitem_token = numeral
     * 
     * function = expr | pair<expr>
     * expr = expr_token[]
     * expr_token = char
     * pair<t> = t + t
     * 
     * Operators and properties:
     * +
     *     separator: char
     * []
     *     separator: char
     */

    public abstract class Token : INode
    {
        public Guid Id { get; protected set; }
        protected Tk Tk { get; }

        public Token(Tk tk, params INode[] tokens) 
        {
            Tk = tk;
            Id = Guid.NewGuid();
            var parentedChildren = tokens?.Select(c => c.WithParent(this, c.Id)) ?? Array.Empty<INode>();
            Children = parentedChildren;
        }

        public abstract INode WithParent(INode parent, Guid id);// => new Token(Tk, Children.ToArray()) { Id = id, 
            // Parent = parent };

        public override string ToString() => GetType().Name;

        public IEnumerable<INode> Children { get; private set; }

        public INode Parent { get; protected set; }

        public bool Equals([AllowNull] INode other) => other.Id.Equals(Id);

        public abstract bool Match(char chr);
    }

    public enum OperatorType
    {
        Infix, // separator
        Suffix
    }

    public class Operator : Token
    {
        public string Name { get; }
        public OperatorType OperatorType { get; }
        public Operator(Tk tk, string name, OperatorType operatorType) : base(tk)
        {
            Name = name;
            OperatorType = operatorType;
        }
        public override INode WithParent(INode parent, Guid id) => new Operator(Tk, Name, OperatorType) { 
            Id = id, Parent = parent };
        public override string ToString() => string.Format("{0} op", Name);
        public override bool Match(char chr) =>
            Parent is Element el && 
                (Equals(Tk.And) && el.Type == ElemType.KeyValue && chr == '→');
    }

    public enum ElemType
    {
        EitherEager, 
        Array, 
        KeyValueItemToken, 
        //Position, 
        Pair, 
        KeyValue, 
        Rule, 
        ExprToken, 
        Expr, 
        Function
    }

    /*
     * Node types without additional properties can be Element instances.
     * Element should be instanced only for node types with additional properties.
     */
    public class Element : Token
    {
        public ElemType Type { get; }
        //public IEnumerable<Token> TypeParameters { get; }
        public Element(Tk tk, ElemType type, params INode[] tokens) : base(tk, tokens)
        {
            Type = type;
        }
        //public Element(Tk tk, Token typeParameter, ElemType type, params INode[] tokens) : this(tk, type,
        //    tokens)
        //{
        //    TypeParameters = new Token[] { typeParameter };
        //}
        //public Element(Tk tk, IEnumerable<Token> typeParameters, ElemType type, params INode[] tokens) : this(tk, type, 
        //    tokens)
        //{
        //    TypeParameters = typeParameters;
        //}
        public override INode WithParent(INode parent, Guid id) => new Element(Tk, /*TypeParameters, */Type, 
            Children.ToArray()) { Id = id, Parent = parent };
        public override string ToString() => string.Format("{0} el", Type);

        public override bool Match(char chr) =>
            (Type == ElemType.Rule && chr == '(') ||
            (Type == ElemType.Function && chr == '(') ||
            (Parent is Element pel && 
                ((Type == ElemType.Array && 
                    (pel.Type == ElemType.Rule || 
                    pel.Type == ElemType.KeyValue || 
                    pel.Type == ElemType.Expr)) || 
                (Type == ElemType.KeyValue && pel.Parent is Element ppel && ppel.Type == ElemType.Rule) || 
                (Type == ElemType.Expr && pel.Type == ElemType.Function) || 
                (Type == ElemType.Pair && pel.Type == ElemType.Function && chr == ',')));
    }

    //public class PositionElement : Element
    //{
    //    public int Position { get; }
    //    public PositionElement(Tk tk, int position, ElemType type, params INode[] tokens) : base(tk, type, tokens)
    //    {
    //        Position = position;
    //    }
    //    public override INode WithParent(INode parent, Guid id) =>
    //        new PositionElement(Tk, Position, Type, Children.ToArray()) { Id = id, Parent = parent };
    //}

    // Childless.
    public class TokenElement : Element
    {
        public Atom Atom { get; }
        public TokenElement(Tk tk, ElemType type, Atom atom) : base(tk, type)
        {
            Atom = atom;
        }
        public override INode WithParent(INode parent, Guid id) =>
            new TokenElement(Tk, Type, Atom) { Id = id, Parent = parent };
        public override bool Match(char chr) => Atom.Match(chr);
        public override string ToString() => string.Format("{0}: {1}", base.ToString(), Atom);
    }

    public class Atom : Token
    {
        //public char? Char { get; }
        public string Name { get; }
        public string Regexp { get; }
        public Atom(Tk tk, /*char? @char*/ string name, string regex) : base(tk)
        {
            //Char = @char;
            Name = name;
            Regexp = regex;
        }
        public override INode WithParent(INode parent, Guid id)
        {
            throw new InvalidOperationException("Atom can not be a child");
        }
        public override bool Match(char chr) => Regex.IsMatch(chr.ToString(), Regexp);
        public override string ToString() => Name ?? base.ToString();
    }

    public static class Extensions
    {
        public static Func<INode, IEnumerable<string>> TokenPrintOut1 =
            (INode node) => 
                !(node is Token token) ? new string[] { "(not a token)" } :
                new string[] {
                    ".NET type: " + token.GetType().Name, 
                    "Parent: " + (token.Parent != null ? (token.Parent is Element elem ? elem.Type.ToString() : 
                        token.Parent.GetType().Name) + " (" + token.Parent.Id.ToString("N").Substring(0, 6) + ")" : 
                        "none"), 
                    "Children: " + token.Children.Count().ToString()
                };

        public static Func<INode, IEnumerable<string>> ElementPrintOut1 =
            (INode node) =>
                //new string[] { ((Token)node).Id.ToString() }.Concat(
                !(node is Element el) ? new string[] { "(not an element)" } : new string[] {
                    "Elem type: " + el.Type.ToString() + " (" + el.Id.ToString("N").Substring(0, 6) + ")"//, 
                    //"Type parameters: " + (el.TypeParameters?.Count() ?? 0).ToString()
                        //(el.TypeParameters != default ? Environment.NewLine + 
                        //    string.Join(Environment.NewLine, el.TypeParameters.Select(tp => 
                        //        string.Join(Environment.NewLine, 
                        //            TokenPrintOut1(tp)
                        //            .Concat(Abstraction.Extensions.NewLineArr) 
                        //            .Concat(ElementPrintOut1(tp))
                        //            .Concat(Abstraction.Extensions.NewLineArr)
                        //            .Idents(1)
                        //        )
                        //    ).Idents(1)) 
                        //: "none")
                };
    }
}
