using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace WEditor
{
    public static class ClassExtensions
    {
        /// <summary>
        /// Orders a given list by Natural sort. Natural sort is what is most commonly expected (as compared to Alphabetical Sort) when sorting, especially
        /// when dealing with things with numbers on the end. Natural sort will put it as 1, 2,... 9, 10 while Alphabetical sort puts it as 1, 10, 2, ... 9, 99.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items">List of items to sort</param>
        /// <param name="selector">Which parameter of the item to sort by</param>
        /// <param name="stringComparer">Override the string comparerator if needed.</param>
        /// <returns></returns>
        public static IEnumerable<T> OrderByNatural<T>(this IEnumerable<T> items, Func<T, string> selector, StringComparer stringComparer = null)
        {
            var regex = new Regex(@"\d+", RegexOptions.Compiled);

            int maxDigits = items
                          .SelectMany(i => regex.Matches(selector(i)).Cast<Match>().Select(digitChunk => (int?)digitChunk.Value.Length))
                          .Max() ?? 0;

            return items.OrderBy(i => regex.Replace(selector(i), match => match.Value.PadLeft(maxDigits, '0')), stringComparer ?? StringComparer.CurrentCulture);
        }
    }
}
