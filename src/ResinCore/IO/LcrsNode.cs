using StreamIndex;
using System;
using System.Diagnostics;

namespace Resin.IO
{
    [DebuggerDisplay("{Value} {EndOfWord}")]
    public struct LcrsNode : IEquatable<LcrsNode>
    {
        public readonly char Value;
        public readonly bool HaveSibling;
        public readonly bool HaveChild;
        public readonly bool EndOfWord;
        public readonly short Depth;
        public readonly int Weight;
        public readonly BlockInfo? PostingsAddress;

        public LcrsNode(LcrsTrie trie, short depth, int weight, BlockInfo? postingsAddress)
        {
            Value = trie.Value;
            HaveSibling = trie.RightSibling != null;
            HaveChild = trie.LeftChild != null;
            EndOfWord = trie.EndOfWord;
            Depth = depth;
            Weight = weight;
            PostingsAddress = postingsAddress;
        }

        public LcrsNode(char value, bool haveSibling, bool haveChild, bool endOfWord, short depth, int weight, BlockInfo? postingsAddress)
        {
            Value = value;
            HaveSibling = haveSibling;
            HaveChild = haveChild;
            EndOfWord = endOfWord;
            Depth = depth;
            Weight = weight;
            PostingsAddress = postingsAddress;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Value.GetHashCode();
                hashCode = (hashCode * 397) ^ HaveSibling.GetHashCode();
                hashCode = (hashCode * 397) ^ HaveChild.GetHashCode();
                hashCode = (hashCode * 397) ^ EndOfWord.GetHashCode();
                hashCode = (hashCode * 397) ^ Depth;
                hashCode = (hashCode * 397) ^ Weight;
                return hashCode;
            }
        }

        public static bool operator ==(LcrsNode left, LcrsNode right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(LcrsNode left, LcrsNode right)
        {
            return !left.Equals(right);
        }

        public static LcrsNode MinValue
        {
            get { return new LcrsNode(new LcrsTrie(), 0, 0, null); }
        }

        public bool Equals(LcrsNode other)
        {
            return Value == other.Value
                && HaveSibling.Equals(other.HaveSibling) 
                && HaveChild.Equals(other.HaveChild) 
                && EndOfWord.Equals(other.EndOfWord) 
                && Depth == other.Depth
                && Weight == other.Weight;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is LcrsNode && Equals((LcrsNode)obj);
        }
    }
}