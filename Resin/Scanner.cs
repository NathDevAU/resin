﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using ProtoBuf;

namespace Resin
{
    public class Scanner
    {
        // field/fileid
        private readonly IDictionary<string, string> _fieldIndex;

        private readonly IDictionary<string, FieldReader> _fieldReaders; 

        private readonly string _directory;

        public string Dir { get { return _directory; } }

        public Scanner(string directory)
        {
            _directory = directory;
            _fieldReaders = new Dictionary<string, FieldReader>();
            _fieldIndex = new Dictionary<string, string>();

            foreach (var ixFile in Directory.GetFiles(_directory, "*.ix").Where(f => Path.GetExtension(f) != ".tmp").OrderBy(f => f))
            {
                Dictionary<string, string> ix;
                using (var fs = File.OpenRead(ixFile))
                {
                    ix = Serializer.Deserialize<Dictionary<string, string>>(fs);
                }
                foreach (var f in ix)
                {
                    _fieldIndex[f.Key] = f.Value;
                }
            }
        }

        public IEnumerable<DocumentScore> GetDocIds(Term term)
        {
            string fieldId;
            if (_fieldIndex.TryGetValue(term.Field, out fieldId))
            {
                var reader = GetReader(term.Field);
                if (reader != null)
                {
                    if (term.Prefix)
                    {
                        return GetDocIdsByPrefix(term, reader);
                    }
                    if (term.Fuzzy)
                    {
                        return GetDocIdsFuzzy(term, reader);
                    }
                    return GetDocIdsExact(term, reader);
                }
            }
            return Enumerable.Empty<DocumentScore>();
        }

        private IEnumerable<DocumentScore> GetDocIdsFuzzy(Term term, FieldReader reader)
        {
            var terms = reader.GetSimilar(term.Token, term.Edits).Select(token => new Term { Field = term.Field, Token = token }).ToList();
            return terms.SelectMany(t => GetDocIdsExact(t, reader)).GroupBy(d => d.DocId).Select(g => g.OrderByDescending(x => x.TermFrequency).First());
        }

        private IEnumerable<DocumentScore> GetDocIdsByPrefix(Term term, FieldReader reader)
        {
            var terms = reader.GetTokens(term.Token).Select(token => new Term {Field = term.Field, Token = token}).ToList();
            return terms.SelectMany(t => GetDocIdsExact(t, reader)).GroupBy(d=>d.DocId).Select(g=>g.OrderByDescending(x=>x.TermFrequency).First());
        }

        private IEnumerable<DocumentScore> GetDocIdsExact(Term term, FieldReader reader)
        {
            var postings = reader.GetPostings(term.Token);
            if (postings != null)
            {
                foreach (var doc in postings)
                {
                    yield return new DocumentScore { DocId = doc.Key, TermFrequency = doc.Value };
                }
            }
        }

        private FieldReader GetReader(string field)
        {
            string fieldId;
            if (_fieldIndex.TryGetValue(field, out fieldId))
            {
                FieldReader reader;
                if (!_fieldReaders.TryGetValue(field, out reader))
                {
                    reader = FieldReader.Load(Path.Combine(_directory, fieldId + ".fld"));
                    _fieldReaders.Add(field, reader);
                }
                return reader;
            }
            return null;
        }

        public IEnumerable<TokenInfo> GetAllTokens(string field)
        {
            var reader = GetReader(field);
            return reader == null ? Enumerable.Empty<TokenInfo>() : reader.GetAllTokens();
        }

        public int DocCount(string field)
        {
            var reader = GetReader(field);
            if (reader != null)
            {
                return reader.DocCount;
            }
            return 0;
        }
    }

    public struct TokenInfo
    {
        public string Token;
        public int Count;
    }
}