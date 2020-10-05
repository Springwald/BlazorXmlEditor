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
        public void SplitEmptyText()
        {
            var result = TextSplitHelperNew.SplitText(string.Empty, -1, 0, PaintContext, LineSpaceY, FontHeight, FontWidth).ToArray();
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Length);
        }

        [TestMethod()]
        public void SplitOneCharText()
        {
            var result = TextSplitHelperNew.SplitText("A", -1, 0, PaintContext, LineSpaceY, FontHeight, FontWidth).ToArray();
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual("A", result[0].Text);
            Assert.IsFalse(result[0].Inverted);
        }

        [TestMethod()]
        public void SplitOneCharTextInverted()
        {
            var result = TextSplitHelperNew.SplitText("A", 0, 1, PaintContext, LineSpaceY, FontHeight, FontWidth).ToArray();
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual("A", result[0].Text);
            Assert.IsTrue(result[0].Inverted);
        }
    }
}