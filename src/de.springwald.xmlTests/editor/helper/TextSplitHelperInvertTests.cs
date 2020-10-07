using de.springwald.xml.editor.helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace de.springwald.xmlTests.editor.helper
{
    [TestClass()]

    public class TextSplitHelperInvertTests
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
        public void SplitInvertedTest2()
        {
            var result = TextSplitHelper.SplitText("1234567890 ABCDE", 3, 7, 10, 10).ToArray();
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Length);

            Assert.AreEqual("123", result[0].Text);
            Assert.IsFalse(result[0].Inverted);
            Assert.AreEqual(0, result[0].LineNo);

            Assert.AreEqual("4567890", result[1].Text);
            Assert.IsTrue(result[1].Inverted);
            Assert.AreEqual(0, result[1].LineNo);

            Assert.AreEqual(" ABCDE", result[2].Text);
            Assert.IsFalse(result[2].Inverted);
            Assert.AreEqual(1, result[2].LineNo);
        }

        [TestMethod()]
        public void SplitMixInvertedAndNewLine()
        {
            var result = TextSplitHelper.SplitText("OOOII IIOOO OOOOO OOOOO", invertStart: 3, invertLength: 5, maxLength: 5, maxLengthFirstLine: 5).ToArray();
            Assert.IsNotNull(result);
            Assert.AreEqual(6, result.Length);

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

            Assert.AreEqual(" OOOOO", result[4].Text);
            Assert.IsFalse(result[4].Inverted);
            Assert.AreEqual(2, result[4].LineNo);

            Assert.AreEqual(" OOOOO", result[5].Text);
            Assert.IsFalse(result[5].Inverted);
            Assert.AreEqual(3, result[5].LineNo);
        }


        [TestMethod()]
        public void SplitMultipleInvertedLines4()
        {
            var result = TextSplitHelper.SplitText("OOOOO OOOOO OOIII IIII IIIIIIIIIIIIIIII IIIOO OO OOOOOOO", invertStart: 14, invertLength: 29, maxLength: 11, maxLengthFirstLine: 11).ToArray();
            Assert.IsNotNull(result);
            Assert.AreEqual(7, result.Length);

            Assert.AreEqual("OOOOO OOOOO", result[0].Text);
            Assert.IsFalse(result[0].Inverted);
            Assert.AreEqual(0, result[0].LineNo);

            Assert.AreEqual(" OO", result[1].Text);
            Assert.IsFalse(result[1].Inverted);
            Assert.AreEqual(1, result[1].LineNo);

            Assert.AreEqual("III IIII", result[2].Text);
            Assert.IsTrue(result[2].Inverted);
            Assert.AreEqual(1, result[2].LineNo);

            Assert.AreEqual(" IIIIIIIIIIIIIIII", result[3].Text);
            Assert.IsTrue(result[3].Inverted);
            Assert.AreEqual(2, result[3].LineNo);

            Assert.AreEqual(" III", result[4].Text);
            Assert.IsTrue(result[4].Inverted);
            Assert.AreEqual(3, result[4].LineNo);

            Assert.AreEqual("OO OO", result[5].Text);
            Assert.IsFalse(result[5].Inverted);
            Assert.AreEqual(3, result[5].LineNo);

            Assert.AreEqual(" OOOOOOO", result[6].Text);
            Assert.IsFalse(result[6].Inverted);
            Assert.AreEqual(4, result[6].LineNo);
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
    }
}
