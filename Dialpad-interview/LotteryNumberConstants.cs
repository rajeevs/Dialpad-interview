using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DialpadTest
{
    /// <summary>
    /// Constants that drive the parsing and validity of lottery numbers
    /// </summary>
    public class LotteryNumberConstants
    {
        /// <summary>
        ///  Required Length of lottery ticket (in numbers)
        /// </summary>
        public const int ExpectedLength = 7;

        /// <summary>
        /// Maximum digits in a lottery number
        /// </summary>
        public const int MaxDigits = 2;

        /// <summary>
        /// min value of a lottery number
        /// </summary>
        public const int MinValue = 1;

        /// <summary>
        /// max value of a lottery number
        /// </summary>
        public const int MaxValue = 59;
    }

}
