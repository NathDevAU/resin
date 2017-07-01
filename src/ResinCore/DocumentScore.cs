﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Resin
{
    /// <summary>
    /// Scored posting. To combine inside a index, use doc ID. To combine between indices, use doc hash.
    /// </summary>
    public class DocumentScore
    {
        public int DocumentId { get; private set; }
        public double Score { get; private set; }
        public IxInfo Ix { get; private set; }
        public UInt64 DocHash { get; private set; }

        public DocumentScore(int documentId, UInt64 docHash, double score, IxInfo ix)
        {
            DocumentId = documentId;
            Score = score;
            Ix = ix;
            DocHash = docHash;
        }

        public void Add(DocumentScore score)
        {
            if (!score.DocumentId.Equals(DocumentId)) throw new ArgumentException("Document IDs differ. Cannot combine.", "score");

            Score = (Score + score.Score);
        }

        public static IEnumerable<DocumentScore> Not(IEnumerable<DocumentScore> source, IEnumerable<DocumentScore> exclude)
        {
            var dic = exclude.ToDictionary(x => x.DocumentId);

            foreach (var score in source)
            {
                DocumentScore exists;
                if (!dic.TryGetValue(score.DocumentId, out exists))
                {
                    yield return score;
                }
            }
        }

        public static IEnumerable<DocumentScore> CombineOr(IEnumerable<DocumentScore> first, IEnumerable<DocumentScore> other)
        {
            if (first == null) return other;

            return first.Concat(other).GroupBy(x => x.DocumentId).Select(group =>
            {
                var list = group.ToList();
                
                var top = list.First();
                foreach (var score in list.Skip(1))
                {
                    top.Add(score);
                }
                return top;
            });
        }

        public static IEnumerable<DocumentScore> CombineAnd(IEnumerable<DocumentScore> first, IEnumerable<DocumentScore> other)
        {
            var dic = other.ToDictionary(x => x.DocumentId);

            foreach (var score in first)
            {
                DocumentScore exists;
                if (dic.TryGetValue(score.DocumentId, out exists))
                {
                    score.Add(exists);
                    yield return score;
                }
            }
        }

        public override string ToString()
        {
            return string.Format("docid:{0} score:{1}", DocumentId, Score);
        }
    }

    public static class DocumentScoreExtensions
    {
        public static IEnumerable<DocumentScore> CombineTakingLatestVersion(this IList<IList<DocumentScore>> source)
        {
            if (source.Count == 0) return new List<DocumentScore>();

            if (source.Count == 1) return source[0];

            var first = source[0];

            foreach (var list in source.Skip(1))
            {
                first = CombineTakingLatestVersion(first, list).ToList();
            }
            return first;
        }

        public static IEnumerable<DocumentScore> CombineTakingLatestVersion(IEnumerable<DocumentScore> first, IEnumerable<DocumentScore> second)
        {
            var unique = new Dictionary<UInt64, DocumentScore>();

            foreach (var score in first.Concat(second))
            {
                DocumentScore exists;

                if (unique.TryGetValue(score.DocHash, out exists))
                {
                    exists = TakeLatestVersion(exists, score);
                }
                else
                {
                    unique.Add(score.DocHash, score);
                }
            }
            foreach(var score in unique.Values)
            {
                yield return score;
            }
        }

        public static DocumentScore TakeLatestVersion(DocumentScore first, DocumentScore second)
        {
            if (!first.DocHash.Equals(second.DocHash)) throw new ArgumentException("Document hashes differ. Cannot take latest version.", "score");

            if (first.Ix.VersionId > second.Ix.VersionId)
            {
                return first;
            }
            return second;
        }
    }
}