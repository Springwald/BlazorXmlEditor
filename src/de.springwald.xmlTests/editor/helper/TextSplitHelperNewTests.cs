using Microsoft.VisualStudio.TestTools.UnitTesting;
using de.springwald.xml.editor.helper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace de.springwald.xml.editor.helper.Tests
{
    [TestClass()]
    public class TextSplitHelperNewTests
    {
        private int LineSpaceY = 14;
        private int FontHeight = 20;
        private int FontWidth = 10;

        private PaintContext PaintContext
        {
            get
            {
                return new PaintContext
                {
                    LimitLeft = 0,
                    LimitRight = 100,
                    PaintPosX = 100,
                    PaintPosY = 0,
                };
            }
        }


        [TestMethod()]
        public void SplitInvertedTest()
        {
            var result = TextSplitHelperNew.SplitText("1234567890ABCDEFG", 5, 5, PaintContext, LineSpaceY, FontHeight, FontWidth).ToArray();
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Length);
            Assert.AreEqual("12345",result[0].Text);
            Assert.IsFalse(result[0].Inverted);
            Assert.AreEqual("67890", result[1].Text);
            Assert.IsTrue(result[0].Inverted);
            Assert.AreEqual("ABCDE", result[2].Text);
            Assert.IsFalse(result[0].Inverted);
        }


        [TestMethod()]
        public void SplitEmptyTest()
        {
            var result = TextSplitHelperNew.SplitText(string.Empty, -1, 0, PaintContext, LineSpaceY, FontHeight, FontWidth).ToArray();
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Length);
        }

        [TestMethod()]
        public void SplitOneCharTest()
        {
            var result = TextSplitHelperNew.SplitText("A", -1, 0, PaintContext, LineSpaceY, FontHeight, FontWidth).ToArray();
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual("A", result[0].Text);
            Assert.IsFalse(result[0].Inverted);
        }

        [TestMethod()]
        public void SplitOneCharTestInverted()
        {
            var result = TextSplitHelperNew.SplitText("A", 0, 1, PaintContext, LineSpaceY, FontHeight, FontWidth).ToArray();
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual("A", result[0].Text);
            Assert.IsTrue(result[0].Inverted);
        }


    }
}