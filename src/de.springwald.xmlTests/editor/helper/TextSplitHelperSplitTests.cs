using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using static de.springwald.xml.editor.helper.TextSplitHelper;

namespace de.springwald.xml.editor.helper.Tests
{
    [TestClass()]
    public class TextSplitHelperSplitTests
    {
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

        [TestMethod()]
        public void SplitMultipleLines4()
        {
            var result = TextSplitHelper.SplitText("12345 67890 ABCDE FGHI 1234567890ABCDEFGHIJK 134567 89 0123456", invertStart: -1, invertLength: 0, maxLength: 11, maxLengthFirstLine: 11).ToArray();
            Assert.IsNotNull(result);
            Assert.AreEqual(5, result.Length);

            Assert.AreEqual("12345 67890", result[0].Text);
            Assert.IsFalse(result[0].Inverted);
            Assert.AreEqual(0, result[0].LineNo);

            Assert.AreEqual(" ABCDE FGHI", result[1].Text);
            Assert.IsFalse(result[1].Inverted);
            Assert.AreEqual(1, result[1].LineNo);

            Assert.AreEqual(" 1234567890ABCDEFGHIJK", result[2].Text);
            Assert.IsFalse(result[2].Inverted);
            Assert.AreEqual(2, result[2].LineNo);

            Assert.AreEqual(" 134567 89", result[3].Text);
            Assert.IsFalse(result[3].Inverted);
            Assert.AreEqual(3, result[3].LineNo);

            Assert.AreEqual(" 0123456", result[4].Text);
            Assert.IsFalse(result[4].Inverted);
            Assert.AreEqual(4, result[4].LineNo);
        }

    }
}
