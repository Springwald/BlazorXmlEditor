using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using static de.springwald.xml.editor.helper.TextSplitHelper;

namespace de.springwald.xml.editor.helper.Tests
{
    [TestClass()]
    public class TextSplitHelperNewTests
    {
        [TestMethod()]
        public void SplitInvertedAll()
        {
            var result = TextSplitHelper.SplitText("1234567890ABCDE", 0, 15, 1000, 1000).ToArray();
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual("1234567890ABCDE", result[0].Text);
            Assert.IsTrue(result[0].Inverted);
            Assert.AreEqual(0, result[0].LineNo);
        }

        [TestMethod()]
        public void SplitInvertedAll1()
        {
            var result = TextSplitHelper.SplitText("12345 67890 ABCDE", 0, 17, 1000, 1000).ToArray();
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual("12345 67890 ABCDE", result[0].Text);
            Assert.IsTrue(result[0].Inverted);
            Assert.AreEqual(0, result[0].LineNo);
        }

        [TestMethod()]
        public void SplitInvertedNothing()
        {
            var result = TextSplitHelper.SplitText("1234567890ABCDE", -1, 0, 1000, 1000).ToArray();
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual("1234567890ABCDE", result[0].Text);
            Assert.IsFalse(result[0].Inverted);
            Assert.AreEqual(0, result[0].LineNo);
        }

        [TestMethod()]
        public void SplitInvertedNothing1()
        {
            var result = TextSplitHelper.SplitText("12345 67890 ABCDE", 0, 17, 1000, 1000).ToArray();
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual("12345 67890 ABCDE", result[0].Text);
            Assert.IsTrue(result[0].Inverted);
            Assert.AreEqual(0, result[0].LineNo);
        }


        [TestMethod()]
        public void SplitInvertedTest()
        {
            var result = TextSplitHelper.SplitText("1234567890ABCDE", 5, 5, 1000, 1000).ToArray();
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Length);
            Assert.AreEqual("12345", result[0].Text);
            Assert.IsFalse(result[0].Inverted);
            Assert.AreEqual(0, result[0].LineNo);
            Assert.AreEqual("67890", result[1].Text);
            Assert.IsTrue(result[1].Inverted);
            Assert.AreEqual(0, result[1].LineNo);
            Assert.AreEqual("ABCDE", result[2].Text);
            Assert.IsFalse(result[2].Inverted);
            Assert.AreEqual(0, result[2].LineNo);
        }


        [TestMethod()]
        public void SplitInvertedTest1()
        {
            var result = TextSplitHelper.SplitText("1234567890ABCDE", 10, 5, 1000, 1000).ToArray();
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Length);

            Assert.AreEqual("1234567890", result[0].Text);
            Assert.IsFalse(result[0].Inverted);
            Assert.AreEqual(0, result[0].LineNo);

            Assert.AreEqual("ABCDE", result[1].Text);
            Assert.IsTrue(result[1].Inverted);
            Assert.AreEqual(0, result[1].LineNo);
        }

        [TestMethod()]
        public void SplitMixInvertedAndNewLine()
        {
            var result = TextSplitHelper.SplitText("OOOII IIOOO", invertStart: 3, invertLength: 5, maxLength: 5, maxLengthFirstLine: 5).ToArray();
            Assert.IsNotNull(result);
            Assert.AreEqual(4, result.Length);

            Assert.AreEqual("OOO", result[0].Text);
            Assert.IsFalse(result[0].Inverted);
            Assert.AreEqual(0, result[0].LineNo);

            Assert.AreEqual("II", result[1].Text);
            Assert.IsTrue(result[1].Inverted);
            Assert.AreEqual(0, result[1].LineNo);

            Assert.AreEqual(" II", result[2].Text);
            Assert.IsTrue(result[2].Inverted);
            Assert.AreEqual(1, result[2].LineNo);

            Assert.AreEqual("OOO", result[3].Text);
            Assert.IsFalse(result[3].Inverted);
            Assert.AreEqual(1, result[3].LineNo);
        }

        [TestMethod()]
        public void SplitEmptyTest()
        {
            var result = TextSplitHelper.SplitText(string.Empty, -1, 0, 1000, 1000).ToArray();
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Length);
        }

        [TestMethod()]
        public void SplitOneCharTest()
        {
            var result = TextSplitHelper.SplitText("A", -1, 0, 1000, 1000).ToArray();
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual("A", result[0].Text);
            Assert.IsFalse(result[0].Inverted);
            Assert.AreEqual(0, result[0].LineNo);
        }

        [TestMethod()]
        public void SplitOneCharTestInverted()
        {
            var result = TextSplitHelper.SplitText("A", 0, 1, 1000, 1000).ToArray();
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual("A", result[0].Text);
            Assert.IsTrue(result[0].Inverted);
            Assert.AreEqual(0, result[0].LineNo);
        }

        [TestMethod()]
        public void SplitMultipleLinesShortLines()
        {
            var result = TextSplitHelper.SplitText("12345 67890 ABCDE", -1, 0, 2, 2).ToArray();
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Length);
            Assert.AreEqual("12345", result[0].Text);
            Assert.AreEqual(" 67890", result[1].Text);
            Assert.AreEqual(" ABCDE", result[2].Text);
            Assert.IsFalse(result[0].Inverted);
            Assert.IsFalse(result[1].Inverted);
            Assert.IsFalse(result[2].Inverted);
            Assert.AreEqual(0, result[0].LineNo);
            Assert.AreEqual(1, result[1].LineNo);
            Assert.AreEqual(2, result[2].LineNo);
        }

        [TestMethod()]
        public void SplitMultipleLines()
        {
            var result = TextSplitHelper.SplitText("12345 67890 ABCDE", -1, 0, 5, 5).ToArray();
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Length);
            Assert.AreEqual("12345", result[0].Text);
            Assert.AreEqual(" 67890", result[1].Text);
            Assert.AreEqual(" ABCDE", result[2].Text);
            Assert.IsFalse(result[0].Inverted);
            Assert.IsFalse(result[1].Inverted);
            Assert.IsFalse(result[2].Inverted);
            Assert.AreEqual(0, result[0].LineNo);
            Assert.AreEqual(1, result[1].LineNo);
            Assert.AreEqual(2, result[2].LineNo);
        }

        [TestMethod()]
        public void SplitMultipleLines2()
        {
            var result = TextSplitHelper.SplitText("12345 67890 ABCDE", -1, 0, 15, 15).ToArray();
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual("12345 67890", result[0].Text);
            Assert.AreEqual(" ABCDE", result[1].Text);
            Assert.IsFalse(result[0].Inverted);
            Assert.IsFalse(result[1].Inverted);
            Assert.AreEqual(0, result[0].LineNo);
            Assert.AreEqual(1, result[1].LineNo);
        }

        [TestMethod()]
        public void SplitMultipleLines3()
        {
            var result = TextSplitHelper.SplitText("12345 67890 ABCDE", -1, 0, 5, 5).ToArray();
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Length);
            Assert.AreEqual("12345", result[0].Text);
            Assert.AreEqual(" 67890", result[1].Text);
            Assert.AreEqual(" ABCDE", result[2].Text);
            Assert.IsFalse(result[0].Inverted);
            Assert.IsFalse(result[1].Inverted);
            Assert.IsFalse(result[2].Inverted);
            Assert.AreEqual(0, result[0].LineNo);
            Assert.AreEqual(1, result[1].LineNo);
            Assert.AreEqual(2, result[2].LineNo);
        }


        private void CheckPart(TextPart part, string text, int lineNo, bool inverted, TextPart[] allParts)
        {
            Assert.AreEqual(text, part.Text, "lineNo:" + lineNo);
            Assert.AreEqual(lineNo, part.LineNo, "lineNo");
            Assert.AreEqual(inverted, part.Inverted, "inverted");
        }
    }
}
