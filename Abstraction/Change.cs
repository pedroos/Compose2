using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Metadata.Ecma335;
using System.Text.RegularExpressions;

namespace Abstraction
{
    /*
     * The following materializations are made:
     * - Functions are Func<T,...>s
     * - Positions are ints
     * - Positionals are IEnumerables
     * - Positional rule is Rule<int,...>
     */

    public static class Chars
    {
        public static readonly char Rarrow = '\u2192';
        public static readonly char Plus = '\u002b';
        public static readonly char Minus = '\u2212';
    }

    /*
     * A rule is an argument to a change function. It will denote 'what' has changed; 'how' the change is applied to a 
     * type of object is defined by a change function.
     */

    public class Rule<TFrom,TTo>
    {
        public List<(TFrom From, TTo To)> Items { get; }
        public Rule()
        {
            Items = new List<(TFrom From, TTo To)>();
        }
        internal Rule(IEnumerable<(TFrom From, TTo To)> items) : base()
        {
            Items = items.ToList();
        }
        public Rule(params (TFrom From, TTo To)[] items) : base()
        {
            Items = items.ToList();
        }
        public bool OfFrom(TFrom from, out TTo to)
        {
            var fr = Items.SingleOrDefault(i => i.From.Equals(from));
            to = default;
            if (fr.Equals(default))
                return false;
            to = fr.To;
            return true;
        }
        public override string ToString()
        {
            return string.Join(", ", Items.Select(i => string.Format("{0}{2}{1}", i.From.ToString().Replace('-', 
                Chars.Minus), i.To.ToString().Replace('-', Chars.Minus), Chars.Rarrow)));
        }
        
        static readonly Regex parseRegex = new Regex(string.Format(@"^( *[0-9]*{0}[{1}-]?[0-9]*,?)*$", Chars.Rarrow, 
            Chars.Minus));
        public static Rule<int,int> ParseInt(string str)
        {
            if (!parseRegex.IsMatch(str)) return default;
            var vals = str.Split(",").Select(s => s.Trim()).Where(s => s.Length > 0)
                .Select(s => s.Split(Chars.Rarrow).Select(s => s.Trim()).Select(s => s == "-" || 
                s == Chars.Minus.ToString() || s.Length == 0 ? "0" : s.Replace(Chars.Minus, '-'))
                .Where(s => s != Chars.Rarrow.ToString()).ToArray());
            return new Rule<int,int>(vals.Select(v => (int.Parse(v.First()), int.Parse(v.Skip(1).First()))));
        }
    }

    /*
     * The Change class describes a change in a object of a specific type for versioning.
     * It contains the change function and a specific rule argument.
     * A Change instance is also the mechanism to perform the change on a specific object at runtime.
     * The Change's Perform() method knows how to apply its function to the specific object type using a rule of the 
     * specific rule type as an argument.
     */

    public abstract class Change<TOperand, TRuleFrom, TRuleTo>
    {
        protected readonly Func<TRuleTo, TRuleTo, TRuleTo> function;
        protected readonly Func<TRuleTo, TRuleTo, TRuleTo> reverseFunction;
        protected string functionsText;
        protected readonly Rule<TRuleFrom, TRuleTo> rule;
        public Change(Func<TRuleTo, TRuleTo, TRuleTo> function, Rule<TRuleFrom, TRuleTo> rule)
        {
            this.function = function;
            functionsText = function.ToString();
            this.rule = rule;
        }
        public Change((Func<TRuleTo, TRuleTo, TRuleTo> F, Func<TRuleTo, TRuleTo, TRuleTo> R) function, 
            string functionsText, Rule<TRuleFrom, TRuleTo> rule) : this(function.F, rule)
        {
            reverseFunction = function.R;
            this.functionsText = functionsText ?? function.ToString();
        }
        protected abstract TOperand Perform(TOperand operand, Func<TRuleTo, TRuleTo, TRuleTo> function);
        public TOperand Perform(TOperand operand) => Perform(operand, function);
        public TOperand Unperform(TOperand operand)
        {
            if (reverseFunction == null) throw new InvalidOperationException("This change is not reversible.");
            return Perform(operand, reverseFunction);
        }
    }

    /*
     * A change in a positional object.
     * Iterate over all elements listed in the rule and apply the function together with the rule.
     * Because there are issues in C# generic inheritance, this is the only change type implemented, to not proliferate 
     * checks over Node Value types. All Changes throughout the system are considered this kind of change only.
     * All Changes will be over IEnumerables, but we accept it for now.
     * The Node value is kept untyped to allow using it for objects other than Changes.
     * The base Change class is still useful so kept.
     */

    public class SinglePositionChange<T> : Change<IEnumerable<T>, int, T>
    {
        public SinglePositionChange(Func<T, T, T> function, Rule<int, T> rule) : base(function, rule) { }
        public SinglePositionChange((Func<T, T, T> F, Func<T, T, T> R) function, string functionsText, 
            Rule<int, T> rule) : base(function, functionsText, rule) { }

        protected override IEnumerable<T> Perform(IEnumerable<T> operand, Func<T, T, T> function) =>
            operand
                .Select((e, idx) =>
                    rule.OfFrom(idx, out T to) ?
                        function(e, to) :
                        e);

        public override string ToString() =>
            string.Format("fun = ({0}); arg = ({1})", functionsText, rule);
    }

    // For example, create a "Window" Change, which walks elements two-by-two...

    /*
     * Functions library.
     * Reversible Functions are declared as pairs of original (F) and its reverse (R).
     */

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class TextAttribute : Attribute
    {
        public string Text { get; }
        public TextAttribute(string text)
        {
            Text = text;
        }
    }

    public static class Funcs
    {
        [Text("a \u002b b, a \u2212 b")]
        public static (Func<int,int,int> F, Func<int,int,int> R) Plus = ((a, b) => a + b, (a, b) => a - b);
    }
}
