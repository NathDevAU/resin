﻿using System.IO;
using System.Linq;
using NUnit.Framework;
using Resin;

namespace Tests
{
    [TestFixture]
    public class FieldWriterTests
    {
        [Test]
        public void Can_read_write()
        {
            var fileName = Setup.Dir + "\\Can_read_write\\0.fld";
            using (var writer = new FieldWriter(fileName))
            {
                writer.Write(0, "Hello", 0);
                writer.Write(0, "World!", 1);
            }

            Assert.IsTrue(File.Exists(fileName));

            var reader = FieldReader.Load(fileName);
            var terms = reader.GetAllTokens().Select(t => t.Token).ToList();

            Assert.IsTrue(terms.Contains("Hello"));
            Assert.IsTrue(terms.Contains("World!"));
        }

        [Test]
        public void Can_append()
        {
            var fileName0 = "c:\\temp\\resin_tests\\FieldWriterTests\\Can_append\\0.fld";
            var fileName1 = "c:\\temp\\resin_tests\\FieldWriterTests\\Can_append\\1.fld";

            using (var writer = new FieldWriter(fileName0))
            {
                writer.Write(0, "hello", 1);
            }
            var terms = FieldReader.Load(fileName0).GetAllTokens().Select(t => t.Token).ToList();

            Assert.AreEqual(1, terms.Count);
            Assert.IsTrue(terms.Contains("hello"));
            Assert.IsFalse(terms.Contains("world"));

            using (var writer = new FieldWriter(fileName1))
            {
                writer.Write(0, "world", 1);
            }
            terms = FieldReader.Load(fileName0).GetAllTokens().Select(t => t.Token).ToList();

            Assert.AreEqual(1, terms.Count);
            Assert.IsTrue(terms.Contains("hello"));
            Assert.IsFalse(terms.Contains("world"));

            terms = FieldReader.Load(fileName1).GetAllTokens().Select(t => t.Token).ToList();

            Assert.AreEqual(1, terms.Count);
            Assert.IsFalse(terms.Contains("hello"));
            Assert.IsTrue(terms.Contains("world"));
        }
    }
}
