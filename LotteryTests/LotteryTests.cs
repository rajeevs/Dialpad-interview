using System;
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
            Assert.AreNotEqual(null, res);
        }


    }
}
