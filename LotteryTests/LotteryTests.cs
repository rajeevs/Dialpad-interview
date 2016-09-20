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

        private void CheckParseResult(IEnumerable<LotteryNumber> num, string[] expected)
        {
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
