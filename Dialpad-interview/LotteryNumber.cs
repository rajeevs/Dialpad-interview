using System;
using System.Collections.Generic;
using System.Linq;

namespace DialpadTest
{
    /// <summary>
    /// Stores a lottery number
    /// </summary>
    public class LotteryNumber
    {
        private readonly int _number;
        private readonly List<char> _rawNumber;

        private LotteryNumber(params char[] digits)
        {
            if (!digits.All(IsDigit))
                throw new ArgumentOutOfRangeException(nameof(digits));
            _rawNumber = new List<char>(digits);
            _number = ConvertToDecimal();
            if (!Validate())
                throw new ArgumentOutOfRangeException(nameof(digits));
        }

        /// <summary>
        /// Creates a lottery number
        /// </summary>
        /// <param name="num">out parameter with created object</param>
        /// <param name="digits">digits to use for creation</param>
        /// <returns>true if success</returns>
        public static bool Create(out LotteryNumber num, params char[] digits)
        {
            try
            {
                num = new LotteryNumber(digits);
                return true;
            }
            catch (ArgumentException)
            {
                num = null;
                return false;
            }
        }

        /// <summary>
        /// Convert to String
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return new string(_rawNumber.ToArray());
        }

        /// <summary>
        /// Equality is based on decimal value so that 01 are 1 equal 
        /// </summary>
        /// <param name="other"></param>
        /// <returns>true if equals</returns>
        public bool Equals(LotteryNumber other)
        {
            return _number == other?._number;
        }

        public static bool IsDigit(char digit)
        {
            return (digit >= '0' && digit <= '9');
        }

        /// <summary>
        /// Convert the raw string to decimal
        /// </summary>
        /// <returns>decimal value</returns>
        private int ConvertToDecimal()
        {
            int num = 0;
            foreach (var digit in _rawNumber)
            {
                var d = digit - '0';
                num = num * 10 + d;
            }

            return num;
        }

        /// <summary>
        /// Validates that the number is between min and max specified
        /// </summary>
        /// <returns></returns>
        private bool Validate()
        {
            return _number >= LotteryNumberConstants.MinValue && _number <= LotteryNumberConstants.MaxValue;
        }
    }
}
