using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Abstraction.Parser.Regexp
{
    public enum ObjTypes
    {
        Rule, 
        Function, 
        KeyValue, 
        Expr
    }

    public abstract class Token
    {
        /// <summary>
        /// Object type for token types appliable to multiple object types
        /// </summary>
        public ObjTypes? ObjType { get; }

        /// <summary>
        /// Obligatory parent token type for this token to be valid
        /// </summary>
        public IEnumerable<Type> OblParent { get; }

        /// <summary>
        /// A token type to open automatically when this token is opened
        /// </summary>
        public Type OpenToken { get; }

        public Token(ObjTypes? objType, IEnumerable<Type> oblParent)
        {
            ObjType = objType;
            OblParent = oblParent;
        }
        public Token(ObjTypes? objType, IEnumerable<Type> oblParent, Type openToken) : this(objType, oblParent)
        {
            OpenToken = openToken;
        }
        public override string ToString() =>
            string.Format("{0}{1}", 
                "",//TokenName, 
                ObjType.HasValue ? string.Format("[{0}]", ObjType.Value) : "");

        readonly static Dictionary<Type, IEnumerable<Type>> tokenTypeArrays = new Dictionary<Type, IEnumerable<Type>>();

        protected static IEnumerable<Type> Tta<T>()
        {
            if (!tokenTypeArrays.ContainsKey(typeof(T)))
                tokenTypeArrays.Add(typeof(T), new Type[] { typeof(T) });
            return tokenTypeArrays[typeof(T)];
        }

        static readonly Dictionary<Type, Token> tokenCache = new Dictionary<Type, Token>();

        public static T Get<T>()
            where T : Token, new()
        {
            if (!tokenCache.ContainsKey(typeof(T))) 
                tokenCache.Add(typeof(T), new T());
            return (T)tokenCache[typeof(T)];
        }

        public static Token Get(Type tokenType)
        {
            if (!tokenCache.ContainsKey(tokenType))
            {
                try
                {
                    tokenCache.Add(tokenType, (Token)Activator.CreateInstance(tokenType));
                }
                catch (MissingMethodException ex)
                {
                    throw new EvaluationException(string.Format("Token '{0}' could not be instanced. A parameterless " +
                        "constructor could not be found.",
                        tokenType.GetCustomAttribute<TokenAttribute>().TokenName), ex);
                }
            }
            return tokenCache[tokenType];
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class TokenAttribute : Attribute
    {
        public string TokenName { get; set; }
        public string Regex { get; set; }

        /// <summary>
        /// Whether the token contains any per-token data.
        /// </summary>
        public bool IsDataToken { get; set; } = false;
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class TokensAttribute : Attribute 
    {
        public IEnumerable<Token> Tokens { get; set; }
    }

    /// <summary>
    /// A semantic element defined by a sequence of tokens.
    /// </summary>
    public class Element
    {
        public Regex Regex { get; }
        public IEnumerable<Type> TokenTypes { get; }
        public Element(Regex regex, IEnumerable<Type> tokenTypes)
        {
            Regex = regex;
            TokenTypes = tokenTypes;
        }

        public static Element Rule = new Element(new Regex(@"\(→\)"), new Type[] { 
            typeof(ObjStart), 
            typeof(KeyValueToken), 
            typeof(KeyValueSep), 
            typeof(KeyValueToken), 
            typeof(ElemEnd), 
            typeof(KeyValueToken), 
            typeof(KeyValueSep), 
            typeof(KeyValueToken), 
            typeof(ObjEnd)
        });
        public static Element Function = new Element(new Regex(@"\(\)"), new Type[] { 
            typeof(ObjStart), 
            typeof(ExprToken), 
            typeof(ElemEnd), 
            typeof(ExprToken), 
            typeof(ObjEnd)
        });

        public static IEnumerable<Element> Elements = new Element[] { Rule, Function };
    }

    public static class Evaluator
    {
        /// <summary>
        /// Find elements in the string, parse them into tokens, and ignore other content.
        /// </summary>
        public static IEnumerable<IEnumerable<Token>> Evaluate(string str)
        {
            // Elements without regexes are ignored
            foreach (var el in Element.Elements)
            {
                if (el.Regex == null) continue;
                var matches = el.Regex.Matches(str);
                foreach (Match match in matches)
                {
                    yield return Evaluate(match.Value, el);
                }
            }
        }

        /// <summary>
        /// Parse the string into tokens assuming it contains a single element of the specified type.
        /// </summary>
        public static IEnumerable<Token> Evaluate(string str, Element element)
        {
            string currStr = str;
            var tokenTypesEn = element.TokenTypes.GetEnumerator();

            while (tokenTypesEn.MoveNext())
            {
                var tokenAttr = tokenTypesEn.Current.GetType().GetCustomAttribute<TokenAttribute>();
                if (tokenAttr == null)
                    throw new InvalidOperationException(string.Format("'{0}': missing TokenAttribute", 
                        tokenTypesEn.Current.Name));
                string regex = tokenAttr.Regex.StartsWith("$") ? tokenAttr.Regex : "$" + tokenAttr.Regex;
               var match = Regex.Match(currStr, regex);
                if (!match.Success)
                    throw new EvaluationException(string.Format("Unmatched token: '{0}'", tokenAttr.TokenName));

                if (!tokenAttr.IsDataToken)
                {
                    yield return Token.Get(tokenTypesEn.Current);
                }
                else
                {
                    if (tokenTypesEn.Current == typeof(KeyValueToken))
                        yield return new KeyValueToken(default);
                    else if (tokenTypesEn.Current == typeof(ExprToken))
                        yield return new ExprToken(default);
                    else
                        throw new NotImplementedException();
                }

                currStr = currStr.Remove(0, match.Length);
            }
        }
    }

    public class EvaluationException : Exception
    {
        public EvaluationException(string message) : base(message) { }
        public EvaluationException(string message, Exception innerException) : base(message, innerException) { }
    }

    /*
     * Tokens are sequences of characters with a token type.
     * Tokens are evaluated from a regular expression matching from the beginning of a string. Matched tokens are removed.
     * An expression is a sequence of tokens.
     * The cursor moves from token to token.
     * Spaces are ignored.
     */

    /* 
     * Rule:
     * (1→11, 23→ −1)
     * (             obj_start: rule<int,int> → elem_start: keyvalue<int,int> → key_start: int
     * 1             keyvalue_token
     * →             keyvalue_sep → key_end → value_start: int
     * 11            keyvalue_token
     * ,             elem_end → elem_start: keyvalue<int,int> → key_start: int
     *  23           keyvalue_token
     * →             keyvalue_sep → key_end → value_start: int
     * −1            keyvalue_token
     * )             obj_end → elem_end
     * 
     * Or char-by-char:
     * (1→11, 231→ −1)
     * A path is a sequence of characters which belong to the same token.
     * Otherwise, it's just a sequence.
     * Spaces don't create paths (are ignored).
     * When a different type of char is found, the path is closed and the sequence resumed.
     * As long as we keep finding chars of the same type, we keep appending to the path.
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
     * Parameterless-constructor token type instances should be obtained through Token.Get<T>() or Token.Get().
     * Instances with constructor parameters should be instanced directly.
     */

    [Token(TokenName = null, Regex = @"")]
    public abstract class ObjStart : Token
    {
        public ObjStart(ObjTypes objType, IEnumerable<Type> oblParent, Type openToken) : base(objType, oblParent,
            openToken)
        { }
    }

    [Token(TokenName = null, Regex = @"")]
    public abstract class RuleObjStart : ObjStart
    {
        public RuleObjStart() : base(ObjTypes.Rule, null, typeof(KeyValueElemStart)) { }
    }

    [Token(TokenName = "elem_start", Regex = @"")]
    public abstract class ElemStart : Token
    {
        public ElemStart(ObjTypes objType, IEnumerable<Type> oblParent, Type openToken) : base(objType, oblParent,
            openToken)
        { }
    }

    [Token(TokenName = null, Regex = @"")]
    public class KeyValueElemStart : ElemStart
    {
        public KeyValueElemStart() : base(ObjTypes.KeyValue, Tta<RuleObjStart>(), typeof(KeyStart)) { }
    }

    [Token(TokenName = "key_start", Regex = @"")]
    public class KeyStart : Token
    {
        public KeyStart() : base(null, Tta<KeyValueElemStart>()) { }
    }

    [Token(TokenName = "value_start", Regex = @"")]
    public class ValueStart : Token
    {
        public ValueStart() : base(null, Tta<KeyValueElemStart>()) { }
    }

    [Token(IsDataToken = true, TokenName = "keyvalue_token", Regex = @"")]
    public class KeyValueToken : Token
    {
        public int Value { get; }
        public KeyValueToken(int value) : base(null, Tta<KeyStart>(), typeof(ValueStart))
        {
            Value = value;
        }
    }

    [Token(TokenName = "keyvalue_sep", Regex = @"")]
    public class KeyValueSep : Token
    {
        public KeyValueSep() : base(null, Tta<KeyValueElemStart>(), typeof(KeyEnd)) { }
    }

    [Token(TokenName = "key_end", Regex = @"")]
    public class KeyEnd : Token
    {
        public KeyEnd() : base(null, Tta<KeyValueElemStart>(), typeof(ValueStart)) { }
    }

    [Token(TokenName = "value_end", Regex = @"")]
    public class ValueEnd : Token
    {
        public ValueEnd() : base(null, Tta<KeyValueElemStart>()) { }
    }

    [Token(TokenName = "elem_end", Regex = @"")]
    public class ElemEnd : Token
    {
        public ElemEnd() : base(null, Tta<ObjStart>(), typeof(ElemStart)) { }
    }

    [Token(TokenName = null, Regex = @"")]
    public class FunctionObjStart : ObjStart
    {
        public FunctionObjStart() : base(ObjTypes.Function, null, typeof(ExprElemStart)) { }
    }

    [Token(TokenName = null, Regex = @"")]
    public class ExprElemStart : ElemStart
    {
        public ExprElemStart() : base(ObjTypes.Expr, Tta<FunctionObjStart>(), null) { }
    }

    [Token(IsDataToken = true, TokenName = "expr_token", Regex = @"")]
    public class ExprToken : Token
    {
        public string Expr { get; }
        public ExprToken(string expr) : base(null, Tta<ExprElemStart>(), null)
        {
            Expr = expr;
        }
    }

    [Token(TokenName = "obj_end", Regex = @"")]
    public class ObjEnd : Token
    {
        public ObjEnd() : base(null, null, typeof(ElemEnd)) { }
    }
}
