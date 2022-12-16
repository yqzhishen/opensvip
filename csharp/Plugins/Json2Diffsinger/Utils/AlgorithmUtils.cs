using System;
using System.Collections.Generic;
using System.Linq;

namespace Json2DiffSinger.Utils
{
    public static class AlgorithmUtils
    {
        public static T BinaryFindFirst<T>(this List<T> list, Predicate<T> match)
        {
            if (!list.Any())
            {
                return default;
            }
            
            var left = 0;
            var right = list.Count - 1;
            while (left < right)
            {
                var mid = (left + right) / 2;
                if (match(list[mid]))
                {
                    right = mid;
                }
                else
                {
                    left = mid + 1;
                }
            }
            return match(list[right]) ? list[right] : default;
        }

        public static T BinaryFindLast<T>(this List<T> list, Predicate<T> match)
        {
            if (!list.Any())
            {
                return default;
            }
            var left = 0;
            var right = list.Count - 1;
            while (left < right)
            {
                var mid = (left + right + 1) / 2;
                if (match(list[mid]))
                {
                    left = mid;
                }
                else
                {
                    right = mid - 1;
                }
            }
            return match(list[left]) ? list[left] : default;
        }
    }
}