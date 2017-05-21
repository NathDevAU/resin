using System;
using System.Collections.Generic;

namespace Resin.IO
{
    [System.Diagnostics.DebuggerDisplay("{Value} {Count}")]
    public struct Word
    {
        public readonly string Value;
        public readonly int Count;
        public readonly BlockInfo? PostingsAddress;
        public readonly IList<DocumentPosting> Postings;

        public Word(string value, int count = 1, BlockInfo? postingsAddress = null, IList<DocumentPosting> postings = null)
        {
            if (postingsAddress.HasValue && postingsAddress.Equals(BlockInfo.MinValue))
            {
                throw new ArgumentOutOfRangeException("postingsAddress");
            }

            Value = value;
            Count = count;
            PostingsAddress = postingsAddress;
            Postings = postings;
        }

        public static Word MinValue { get { return new Word(string.Empty);} }

        public override string ToString()
        {
            return Value;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}