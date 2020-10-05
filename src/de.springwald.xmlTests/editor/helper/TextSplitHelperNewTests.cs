﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
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
        public void SplitInvertedAll()
        {
            var result = TextSplitHelperNew.SplitText("1234567890ABCDE", 0, 15, PaintContext, LineSpaceY, FontHeight, FontWidth).ToArray();
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual("1234567890ABCDE", result[0].Text);
            Assert.IsTrue(result[0].Inverted);
        }

        [TestMethod()]
        public void SplitInvertedAll1()
        {
            var result = TextSplitHelperNew.SplitText("12345 67890 ABCDE", 0, 17, PaintContext, LineSpaceY, FontHeight, FontWidth).ToArray();
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual("12345 67890 ABCDE", result[0].Text);
            Assert.IsTrue(result[0].Inverted);
        }

        [TestMethod()]
        public void SplitInvertedNothing()
        {
            var result = TextSplitHelperNew.SplitText("1234567890ABCDE", -1, 0, PaintContext, LineSpaceY, FontHeight, FontWidth).ToArray();
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual("1234567890ABCDE", result[0].Text);
            Assert.IsFalse(result[0].Inverted);
        }

        [TestMethod()]
        public void SplitInvertedNothing1()
        {
            var result = TextSplitHelperNew.SplitText("12345 67890 ABCDE", 0, 17, PaintContext, LineSpaceY, FontHeight, FontWidth).ToArray();
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual("12345 67890 ABCDE", result[0].Text);
            Assert.IsTrue(result[0].Inverted);
        }


        [TestMethod()]
        public void SplitInvertedTest()
        {
            var result = TextSplitHelperNew.SplitText("1234567890ABCDE", 5, 5, PaintContext, LineSpaceY, FontHeight, FontWidth).ToArray();
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Length);
            Assert.AreEqual("12345",result[0].Text);
            Assert.IsFalse(result[0].Inverted);
            Assert.AreEqual("67890", result[1].Text);
            Assert.IsTrue(result[1].Inverted);
            Assert.AreEqual("ABCDE", result[2].Text);
            Assert.IsFalse(result[2].Inverted);
        }


        [TestMethod()]
        public void SplitInvertedTest1()
        {
            var result = TextSplitHelperNew.SplitText("1234567890ABCDE", 10, 5, PaintContext, LineSpaceY, FontHeight, FontWidth).ToArray();
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual("1234567890", result[0].Text);
            Assert.IsFalse(result[0].Inverted);
            Assert.AreEqual("ABCDE", result[1].Text);
            Assert.IsTrue(result[1].Inverted);
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