using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Abstraction
{
    public static class State
    {
        /// <param name="unchange">Reverse the supplied path and apply the reverse function.</param>
        public static IEnumerable<T> Change<T>(this IEnumerable<Node> path, IEnumerable<T> initialObj, 
            bool unchange = false)
        {
            var newPath = !unchange ? path : path.Reverse();
            var pathEn = newPath.GetEnumerator();

            IEnumerable<T> objCurr = initialObj;

            while (pathEn.MoveNext())
            {
                if (!(pathEn.Current.Value is SinglePositionChange<T> change))
                    throw new ArgumentException(nameof(pathEn.Current.Value));
                objCurr = !unchange ? change.Perform(objCurr) : change.Unperform(objCurr);
            }

            return objCurr;
        }

        /// <param name="unchange">Reverse the supplied path and apply the reverse function.</param>
        public static IEnumerable<IEnumerable<T>> ChangeHistory<T>(this IEnumerable<Node> path, 
            IEnumerable<T> initialObj, bool unchange = false)
        {
            var newPath = !unchange ? path : path.Reverse();

            var pathEn = newPath.GetEnumerator();

            var objCurr = initialObj;
            var objs = new List<IEnumerable<T>>() { objCurr };

            while (pathEn.MoveNext())
            {
                if (!(pathEn.Current.Value is SinglePositionChange<T> change))
                    throw new ArgumentException(nameof(pathEn.Current.Value));
                objCurr = !unchange ? change.Perform(objs.Last()) : change.Unperform(objCurr);
                objs.Add(objCurr);
            }

            return objs;
        }
    }
}
