﻿using System;
using System.IO;
using IxMilia.Dxf;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BCad.Test.DxfTests
{
    public abstract class AbstractDxfTests
    {
        protected static DxfFile Parse(string data)
        {
            using (var ms = new MemoryStream())
            using (var writer = new StreamWriter(ms))
            {
                writer.WriteLine(data.Trim());
                writer.Flush();
                ms.Seek(0, SeekOrigin.Begin);
                return DxfFile.Load(ms);
            }
        }

        protected static DxfFile Section(string sectionName, string data)
        {
            return Parse(string.Format(@"
0
SECTION
2
{0}{1}
0
ENDSEC
0
EOF
", sectionName, string.IsNullOrWhiteSpace(data) ? null : "\r\n" + data.Trim()));
        }

        protected static void VerifyFileContents(DxfFile file, string expected, Action<string, string> predicate)
        {
            var stream = new MemoryStream();
            file.Save(stream);
            stream.Flush();
            stream.Seek(0, SeekOrigin.Begin);
            var actual = new StreamReader(stream).ReadToEnd();
            predicate(expected, actual);
        }

        protected static void VerifyFileContains(DxfFile file, string expected)
        {
            VerifyFileContents(file, expected, (ex, ac) => Assert.IsTrue(ac.Contains(ex.Trim())));
        }

        protected static void VerifyFileDoesNotContain(DxfFile file, string unexpected)
        {
            VerifyFileContents(file, unexpected, (ex, ac) => Assert.IsFalse(ac.Contains(ex.Trim())));
        }

        protected static void VerifyFileIsExactly(DxfFile file, string expected)
        {
            VerifyFileContents(file, expected, (ex, ac) => Assert.AreEqual(ex.Trim(), ac.Trim()));
        }

        protected static void AssertArrayEqual<T>(T[] expected, T[] actual)
        {
            if (expected == null)
            {
                Assert.IsNull(actual);
            }

            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.Length, actual.Length);
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], actual[i]);
            }
        }
    }
}
