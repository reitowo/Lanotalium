using System;
using System.Collections.Generic;

namespace Schwarzer.Chart.Bms.Bemusilization.Utils
{
    public static class BinarySearch
    {
        private const string LowerBoundOutOfRangeMsg = "Lower bound index must be greater or equals to " +
            "0 but smaller than input length ({0}).";
        private const string UpperBoundOutOfRangeMsg = "Upper bound index must be greater or equals to " +
            "lower bound index ({0}) but smaller than input length ({1}).";
        private const string AlterReadOnlyCollectionMsg = "Cannot alter a read-only collection.";

        public static int BinarySearchIndex<T>(
            this IList<T> sortedList, T key,
            BinarySearchMethod method = BinarySearchMethod.Exact,
            int lowerBoundIndex = 0,
            int upperBoundIndex = -1,
            IComparer<T> comparer = null)
        {
            if (sortedList == null)
                throw new ArgumentNullException("sortedList");
            if (comparer == null)
                comparer = Comparer<T>.Default;
            int count = sortedList.Count;
            if (lowerBoundIndex < 0 || lowerBoundIndex > count || (count > 0 && lowerBoundIndex == count))
                throw new ArgumentOutOfRangeException(
                    "lowerBoundIndex",
                    lowerBoundIndex,
                    string.Format(LowerBoundOutOfRangeMsg, count)
                );
            if (upperBoundIndex < 0) upperBoundIndex = count + upperBoundIndex;
            if ((count > 0 && (upperBoundIndex == count || upperBoundIndex < lowerBoundIndex)) || upperBoundIndex > count)
                throw new ArgumentOutOfRangeException(
                    "upperBoundIndex",
                    upperBoundIndex,
                    string.Format(UpperBoundOutOfRangeMsg, lowerBoundIndex, count)
                );
            if (count == 0) return -1;
            bool isFirst = (method & BinarySearchMethod.FirstExact) == BinarySearchMethod.FirstExact;
            bool isLast = (method & BinarySearchMethod.LastExact) == BinarySearchMethod.LastExact;
            while (lowerBoundIndex <= upperBoundIndex)
            {
                int middleIndex = lowerBoundIndex + (upperBoundIndex - lowerBoundIndex) / 2;
                int comparison = comparer.Compare(sortedList[middleIndex], key);
                if (comparison < 0)
                    lowerBoundIndex = middleIndex + 1;
                else if (comparison > 0)
                    upperBoundIndex = middleIndex - 1;
                else if (upperBoundIndex - lowerBoundIndex < 2)
                {
                    if (isFirst && comparer.Compare(sortedList[lowerBoundIndex], key) == 0)
                        return lowerBoundIndex;
                    if (isLast && comparer.Compare(key, sortedList[upperBoundIndex]) == 0)
                        return upperBoundIndex;
                    return middleIndex;
                }
                else if (isLast)
                    lowerBoundIndex = middleIndex;
                else if (isFirst)
                    upperBoundIndex = middleIndex;
                else
                    return middleIndex;
            }
            if ((method & BinarySearchMethod.FloorClosest) == BinarySearchMethod.FloorClosest)
                return upperBoundIndex;
            if ((method & BinarySearchMethod.CeilClosest) == BinarySearchMethod.CeilClosest)
                return lowerBoundIndex;
            return -1;
        }

        public static bool BinarySerachRange<T>(IList<T> sortedList, T entry,
            out int firstIndex, out int lastIndex,
            IComparer<T> comparer = null,
            int fromIndex = 0, int toIndex = -1)
        {
            if (comparer == null)
                comparer = Comparer<T>.Default;
            firstIndex = BinarySearchIndex(
                sortedList, entry,
                BinarySearchMethod.FirstExact,
                fromIndex, toIndex, comparer
            );
            lastIndex = firstIndex < 0 ? -1 : BinarySearchIndex(
                sortedList, entry,
                BinarySearchMethod.LastExact,
                firstIndex, toIndex, comparer
            );
            return firstIndex >= 0;
        }

        public static int BinarySearchExacts<T>(this IList<T> sortedList, T entry,
            IComparer<T> comparer = null,
            IEqualityComparer<T> equalityComparer = null,
            int fromIndex = 0, int toIndex = -1)
        {
            if (sortedList == null)
                throw new ArgumentNullException("sortedList");
            if (equalityComparer == null)
                equalityComparer = EqualityComparer<T>.Default;
            int firstIndex, lastIndex;
            if (BinarySerachRange(sortedList, entry, out firstIndex, out lastIndex, comparer, fromIndex, toIndex))
                for (int i = firstIndex; i <= lastIndex; i++)
                    if (equalityComparer.Equals(entry, sortedList[i]))
                        return i;
            return -1;
        }

        public static int BinarySearchLastExacts<T>(this IList<T> sortedList, T entry,
            IComparer<T> comparer = null,
            IEqualityComparer<T> equalityComparer = null,
            int fromIndex = 0, int toIndex = -1)
        {
            if (sortedList == null)
                throw new ArgumentNullException("sortedList");
            if (equalityComparer == null)
                equalityComparer = EqualityComparer<T>.Default;
            int firstIndex, lastIndex;
            if (BinarySerachRange(sortedList, entry, out firstIndex, out lastIndex, comparer, fromIndex, toIndex))
                for (int i = lastIndex; i >= firstIndex; i--)
                    if (equalityComparer.Equals(entry, sortedList[i]))
                        return i;
            return -1;
        }

        public static T FindClosestValue<T>(
            this IList<T> sortedList, T key,
            bool findLarger,
            int lowerBoundIndex = 0,
            int upperBoundIndex = -1,
            IComparer<T> comparer = null,
            T defaultValue = default(T))
        {
            int resultIdx = BinarySearchIndex(
                sortedList, key,
                findLarger ? BinarySearchMethod.CeilClosest : BinarySearchMethod.FloorClosest,
                lowerBoundIndex,
                upperBoundIndex,
                comparer
            );
            return resultIdx < 0 || sortedList.Count < 1 ? defaultValue : sortedList[resultIdx];
        }

        public static int InsertInOrdered<T>(
            this IList<T> sortedList, T item,
            IComparer<T> comparer = null,
            int fromIndex = 0, int toIndex = -1, bool insertFirst = true)
        {
            if (sortedList == null)
                throw new ArgumentNullException("sortedList");
            if (sortedList.IsReadOnly)
                throw new ArgumentException(AlterReadOnlyCollectionMsg);
            if (comparer == null)
                comparer = Comparer<T>.Default;
            int index = BinarySearchIndex(
                sortedList, item,
                BinarySearchMethod.CeilClosest | (insertFirst ? BinarySearchMethod.FirstExact : BinarySearchMethod.LastExact),
                fromIndex, toIndex, comparer
            );
            if (index >= sortedList.Count)
                sortedList.Add(item);
            else if (insertFirst)
                sortedList.Insert(index < 0 ? 0 : index, item);
            else if (index >= 0)
            {
                if (comparer.Compare(item, sortedList[index]) < 0)
                {
                    sortedList.Insert(index, item);
                }
                else
                {
                    sortedList.Insert(index + 1, item);
                }
            }
            else
                sortedList.Insert(0, item);
            return index;
        }

        public static void InsertInOrdered<T>(
            this IList<T> sortedList, IEnumerable<T> items,
            IComparer<T> comparer = null,
            int fromIndex = 0, int toIndex = -1)
        {
            if (sortedList == null)
                throw new ArgumentNullException("sortedList");
            if (items == null)
                throw new ArgumentNullException("items");
            if (sortedList.IsReadOnly)
                throw new ArgumentException(AlterReadOnlyCollectionMsg);
            if (comparer == null)
                comparer = Comparer<T>.Default;
            const BinarySearchMethod method = BinarySearchMethod.CeilClosest | BinarySearchMethod.FirstExact;
            int index = -1;
            bool hasPreviousItem = false;
            T previousItem = default(T);
            foreach (T item in items)
            {
                if (!hasPreviousItem)
                    index = BinarySearchIndex(sortedList, item, method, fromIndex, toIndex, comparer);
                else
                {
                    int comparison = comparer.Compare(previousItem, item);
                    if (comparison < 0)
                        index = BinarySearchIndex(sortedList, item, method, index, toIndex, comparer);
                    else if (comparison > 0)
                        index = BinarySearchIndex(sortedList, item, method, fromIndex, index, comparer);
                }
                previousItem = item;
                hasPreviousItem = true;
                if (index >= sortedList.Count)
                    sortedList.Add(item);
                else if (index < 0)
                {
                    index = 0;
                    sortedList.Insert(0, item);
                }
                else
                    sortedList.Insert(index, item);
                if (toIndex >= 0) toIndex++;
            }
        }
    }

    [Flags]
    public enum BinarySearchMethod
    {
        Exact = 0x00, // 00 0000
        FloorClosest = 0x05, // 00 0101
        CeilClosest = 0x06, // 00 0110
        FirstExact = 0x28, // 10 1000
        LastExact = 0x30  // 11 0000
    }
}