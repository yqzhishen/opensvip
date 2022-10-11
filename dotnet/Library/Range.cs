using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenSvip.Library
{
    public abstract class Range
    {
        public abstract bool IsEmpty();
        
        public abstract Tuple<int, int>[] SubRanges();
        
        public abstract bool Includes(int value);

        public bool CoveredBy(Range range)
        {
            return Except(range).IsEmpty();
        }

        public bool Covers(Range range)
        {
            return range.CoveredBy(this);
        }
        
        public abstract Tuple<int, int> SubRangeIncluding(int value);
        
        public abstract Range Intersect(Range range);
        public abstract Range Union(Range range);
        
        public abstract Range Except(Range range);

        public Range Complete(Range completeRange)
        {
            return completeRange.Except(this);
        }

        public Range Expand(int radius)
        {
            return Expand(radius, radius);
        }

        public abstract Range Expand(int leftRadius, int rightRadius);

        public Range Shrink(int radius)
        {
            return Expand(-radius, -radius);
        }

        public Range Shrink(int leftRadius, int rightRadius)
        {
            return Expand(-leftRadius, -rightRadius);
        }

        public abstract Range Shift(int offset);

        public static Range Create(params Tuple<int, int>[] ranges)
        {
            var range = new CompoundRange(ranges);
            if (range.IsEmpty())
            {
                return EmptyRange.Instance;
            }
            var subRanges = range.SubRanges();
            return subRanges.Length == 1
                ? (Range) new SingleRange(subRanges[0].Item1, subRanges[0].Item2)
                : range;
        }

        public static Range operator &(Range range1, Range range2)
        {
            return range1.Intersect(range2);
        }

        public static Range operator |(Range range1, Range range2)
        {
            return range1.Union(range2);
        }

        public static Range operator -(Range range1, Range range2)
        {
            return range1.Except(range2);
        }

        public static Range operator ^(Range range1, Range range2)
        {
            return (range1 | range2) - (range1 & range2);
        }
        
        public static Range operator >>(Range range, int offset)
        {
            return range.Shift(offset);
        }

        public static Range operator <<(Range range, int offset)
        {
            return range.Shift(-offset);
        }
    }

    public class EmptyRange : Range
    {
        public static Range Instance { get; } = new EmptyRange();
        
        private EmptyRange() {}
        
        public override bool IsEmpty()
        {
            return true;
        }

        public override Tuple<int, int>[] SubRanges()
        {
            return Array.Empty<Tuple<int, int>>();
        }

        public override bool Includes(int value)
        {
            return false;
        }

        public override Tuple<int, int> SubRangeIncluding(int value)
        {
            return null;
        }

        public override Range Intersect(Range range)
        {
            return this;
        }

        public override Range Union(Range range)
        {
            return range;
        }

        public override Range Except(Range range)
        {
            return this;
        }

        public override Range Expand(int leftRadius, int rightRadius)
        {
            return this;
        }

        public override Range Shift(int offset)
        {
            return this;
        }
    }

    public class SingleRange : Range
    {
        public int Start { get; set; }
        
        public int End { get; set; }
        
        public SingleRange() {}

        public SingleRange(int start, int end)
        {
            Start = start;
            End = end;
        }

        public override bool IsEmpty()
        {
            return Start > End;
        }
        
        public override Tuple<int, int>[] SubRanges()
        {
            return IsEmpty() ? Array.Empty<Tuple<int, int>>() : new[] {new Tuple<int, int>(Start, End)};
        }

        public override bool Includes(int value)
        {
            return value >= Start && value <= End;
        }

        public override Tuple<int, int> SubRangeIncluding(int value)
        {
            return Includes(value) ? SubRanges()[0] : null;
        }

        public override Range Intersect(Range range)
        {
            if (IsEmpty() || range.IsEmpty())
            {
                return EmptyRange.Instance;
            }
            var ranges = range.SubRanges().ToList();
            if (ranges.All(e => End < e.Item1) || ranges.All(e => Start > e.Item2))
            {
                return EmptyRange.Instance;
            }
            var startIndex = ranges.FindIndex(e => Start <= e.Item2);
            var endIndex = ranges.FindLastIndex(e => End >= e.Item1);
            if (startIndex > endIndex)
            {
                return EmptyRange.Instance;
            }
            if (startIndex == endIndex)
            {
                return new SingleRange(Math.Max(Start, ranges[startIndex].Item1), Math.Min(End, ranges[endIndex].Item2));
            }
            var intersectRanges = new List<Tuple<int, int>>
            {
                new Tuple<int, int>(Math.Max(Start, ranges[startIndex].Item1), ranges[startIndex].Item2)
            };
            intersectRanges.AddRange(ranges.GetRange(startIndex + 1, endIndex - startIndex - 1));
            intersectRanges.Add(new Tuple<int, int>(ranges[endIndex].Item1, Math.Min(End, ranges[endIndex].Item2)));

            return intersectRanges.Count == 1
                ? (Range) new SingleRange(intersectRanges[0].Item1, intersectRanges[0].Item2)
                : new CompoundRange(intersectRanges);
        }

        public override Range Union(Range range)
        {
            if (IsEmpty())
            {
                return range;
            }
            if (range.IsEmpty())
            {
                return this;
            }
            var unionRange = new CompoundRange(range.SubRanges().Append(new Tuple<int, int>(Start, End)));
            var unionSubRanges = unionRange.SubRanges();
            return unionSubRanges.Length == 1
                ? (Range) new SingleRange(unionSubRanges[0].Item1, unionSubRanges[0].Item2)
                : unionRange;
        }

        public override Range Except(Range range)
        {
            if (IsEmpty())
            {
                return EmptyRange.Instance;
            }
            var intersection = Intersect(range);
            if (intersection.IsEmpty())
            {
                return this;
            }
            var intersectRanges = intersection.SubRanges();
            var exceptSubRanges = new List<Tuple<int, int>>();
            var start = Start;
            foreach (var (s, e) in intersectRanges)
            {
                exceptSubRanges.Add(new Tuple<int, int>(start, s));
                start = e;
            }
            exceptSubRanges.Add(new Tuple<int, int>(start, End));
            var exceptRange = new CompoundRange(exceptSubRanges);
            if (exceptRange.IsEmpty())
            {
                return EmptyRange.Instance;
            }
            var ranges = exceptRange.SubRanges();
            return ranges.Length == 1
                ? (Range) new SingleRange(ranges[0].Item1, ranges[0].Item2)
                : exceptRange;
        }

        public override Range Expand(int leftRadius, int rightRadius)
        {
            if (IsEmpty())
            {
                return EmptyRange.Instance;
            }
            var range = new SingleRange(Start - leftRadius, End + rightRadius);
            return range.IsEmpty() ? EmptyRange.Instance : range;
        }

        public override Range Shift(int offset)
        {
            return IsEmpty() ? EmptyRange.Instance : new SingleRange(Start + offset, End + offset);
        }
    }

    public class CompoundRange : Range
    {
        private readonly List<SingleRange> Ranges;

        public CompoundRange(IEnumerable<Tuple<int, int>> subRanges)
        {
            Ranges = subRanges
                .Where(range => range.Item1 <= range.Item2)
                .OrderBy(range => range.Item1)
                .Select(range => new SingleRange(range.Item1, range.Item2))
                .ToList();
            var i = 0;
            while (i < Ranges.Count - 1)
            {
                if (Ranges[i].End >= Ranges[i + 1].Start)
                {
                    if (Ranges[i].End < Ranges[i + 1].End)
                    {
                        Ranges[i].End = Ranges[i + 1].End;
                    }
                    Ranges.RemoveAt(i + 1);
                }
                else
                {
                    ++i;
                }
            }
        }
        public override bool IsEmpty()
        {
            return Ranges.All(range => range.IsEmpty());
        }

        public override Tuple<int, int>[] SubRanges()
        {
            return Ranges.ConvertAll(range => new Tuple<int, int>(range.Start, range.End)).ToArray();
        }

        public override bool Includes(int value)
        {
            return Ranges.Any(range => range.Includes(value));
        }

        public override Tuple<int, int> SubRangeIncluding(int value)
        {
            var range = Ranges.Find(e => e.Includes(value));
            return range == null ? null : new Tuple<int, int>(range.Start, range.End);
        }

        public override Range Intersect(Range range)
        {
            if (IsEmpty() || range.IsEmpty())
            {
                return EmptyRange.Instance;
            }
            var intersectRange = Ranges.Aggregate(
                EmptyRange.Instance,
                (current, e) => current.Union(e.Intersect(range)));
            if (intersectRange.IsEmpty())
            {
                return EmptyRange.Instance;
            }
            var intersectSubRanges = intersectRange.SubRanges();
            return intersectSubRanges.Length == 1
                ? new SingleRange(intersectSubRanges[0].Item1, intersectSubRanges[0].Item2)
                : intersectRange;
        }

        public override Range Union(Range range)
        {
            if (IsEmpty())
            {
                return range;
            }
            if (range.IsEmpty())
            {
                return this;
            }
            var unionRange = new CompoundRange(range.SubRanges().Concat(SubRanges()));
            var unionSubRanges = unionRange.SubRanges();
            return unionSubRanges.Length == 1
                ? (Range) new SingleRange(unionSubRanges[0].Item1, unionSubRanges[0].Item2)
                : unionRange;
        }

        public override Range Except(Range range)
        {
            if (IsEmpty())
            {
                return EmptyRange.Instance;
            }
            if (range.IsEmpty())
            {
                return this;
            }
            var exceptRange = Ranges.Aggregate(
                EmptyRange.Instance,
                (current, e) => current.Union(e.Except(range)));
            if (exceptRange.IsEmpty())
            {
                return EmptyRange.Instance;
            }
            var intersectSubRanges = exceptRange.SubRanges();
            return intersectSubRanges.Length == 1
                ? new SingleRange(intersectSubRanges[0].Item1, intersectSubRanges[0].Item2)
                : exceptRange;
        }

        public override Range Expand(int leftRadius, int rightRadius)
        {
            if (IsEmpty())
            {
                return EmptyRange.Instance;
            }
            var expandRange = Ranges.Aggregate(
                EmptyRange.Instance,
                (current, e) => current.Union(e.Expand(leftRadius, rightRadius)));
            var expandSubRanges = expandRange.SubRanges();
            return expandSubRanges.Length == 1
                ? new SingleRange(expandSubRanges[0].Item1, expandSubRanges[0].Item2)
                : expandRange;
        }

        public override Range Shift(int offset)
        {
            return new CompoundRange(SubRanges()
                .Select(e => new Tuple<int, int>(e.Item1 + offset, e.Item2 + offset)));
        }
    }
}
