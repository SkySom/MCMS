using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mcms.IO.Core.Extensions
{
    public static class EnumerableExtensions
    {

        public static async Task ForEachWithProgressCallbackAsync<TElement>(this IEnumerable<TElement> enumerable,
            Func<TElement, Task> callback, Action<int, int, int> progressCallback)
        {
            var elements = enumerable.ToList();
            var totalLineCount = elements.Count;
            var currentlyProcessed = 0d;
            var currentPercentage = -1d;

            foreach (var element in elements)
            {
                currentlyProcessed += 1;
                var newPercentage = Math.Floor(currentlyProcessed / totalLineCount * 100);
                if (newPercentage > currentPercentage)
                {
                    currentPercentage = newPercentage;
                    progressCallback(totalLineCount, (int) currentlyProcessed, (int) currentPercentage);
                }

                await callback(element);
            }
        }

        public static void ForEachWithProgressCallback<TElement>(this IEnumerable<TElement> enumerable,
            Action<TElement> callback, Action<int, int, int> progressCallback)
        {
            var elements = enumerable.ToList();
            var totalLineCount = elements.Count;
            var currentlyProcessed = 0d;
            var currentPercentage = -1d;

            foreach (var element in elements)
            {
                currentlyProcessed += 1;
                var newPercentage = Math.Floor(currentlyProcessed / totalLineCount * 100);
                if (newPercentage > currentPercentage)
                {
                    currentPercentage = newPercentage;
                    progressCallback(totalLineCount, (int) currentlyProcessed, (int) currentPercentage);
                }

                callback(element);
            }
        }
    }
}
