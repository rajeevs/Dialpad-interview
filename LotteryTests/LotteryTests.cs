using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using DialpadTest;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LotteryTests
{
    [TestClass]
    public class LotteryTests
    {
        [TestMethod]
        public void TestInvalidChars()
        {
            var lp = new LotteryParser();
            var res = lp.GetLotteryTicket("abcdefgh");

            Assert.AreEqual(null, res);
        }

        [TestMethod]
        public void TestLength()
        {
            var lp = new LotteryParser();
            var res = lp.GetLotteryTicket("01234");
            Assert.AreEqual(null, res);

            res = lp.GetLotteryTicket("0123456789012345");
            Assert.AreEqual(null, res);
        }


        [TestMethod]
        public void TestSimple()
        {
            var lp = new LotteryParser();
            var res = lp.GetLotteryTicket("1234567");
            var expected = new string[] { "1", "2", "3", "4", "5", "6", "7" };
            CheckParseResult(res, expected);
        }

        [TestMethod]
        public void TestDoubleDigits()
        {
            var lp = new LotteryParser();
            var res = lp.GetLotteryTicket("12345678");
            var expected = new string[] { "1", "2", "3", "4", "56", "7", "8" };
            CheckParseResult(res, expected);
        }


        [TestMethod]
        public void TestDoubleDigitsUnique()
        {
            var lp = new LotteryParser();
            var res = lp.GetLotteryTicket("1123456");
            string[] expected = null;
            CheckParseResult(res, expected);

            res = lp.GetLotteryTicket("11234567");
            expected = new string[] { "1", "12", "3", "4", "5", "6", "7"};
            CheckParseResult(res, expected);
        }

        [TestMethod]
        public void TestQuestionInput()
        {
            string[] inputs = {"1", "42", "100848", "4938532894754", "1234567", "472844278465445"};
            string[][] expecteds =
            {
                null,
                null,
                null,
                new string[] { "49", "38", "53", "28", "9", "47", "54"},
                new string[] { "1", "2", "3", "4", "5", "6", "7"},
                null
            };
        }

        private void CheckParseResult(IEnumerable<LotteryNumber> num, string[] expected)
        {
            if (expected == null)
            {
                Assert.AreEqual(null, num);
                return;
            }

            Assert.AreNotEqual(null, num);

            var actual = num.Select(x => x.ToString()).ToArray();

            Assert.AreEqual(actual.Length, expected.Length);

            for (int i = 0; i < actual.Length; i++)
            {
                Assert.AreEqual(actual[i], expected[i]);
            }
        }
    }
}
