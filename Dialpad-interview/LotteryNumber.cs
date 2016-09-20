using System;
using System.Collections.Generic;
using System.Linq;

namespace DialpadTest
{
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

        public static bool Create(out LotteryNumber num, params char[] digits)
        {
            try
            {
                num = new LotteryNumber(digits);
                return true;
            }
            catch (Exception)
            {
                num = null;
                return false;
            }
        }

        public override string ToString()
        {
            return new string(_rawNumber.ToArray());
        }

        public bool Equals(LotteryNumber other)
        {
            return _number == other?._number;
        }

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

        private bool Validate()
        {
            return _number >= LotteryNumberConstants.MinValue && _number <= LotteryNumberConstants.MaxValue;
        }

        public static bool IsDigit(char digit)
        {
            return (digit >= '0' && digit <= '9');
        }
    }
}
