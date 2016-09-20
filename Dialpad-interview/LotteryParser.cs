using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DialpadTest
{
    public class LotteryNumber
    {
        private readonly int _number;
        private readonly List<char> _rawNumber;

        private static bool IsDigit(char digit)
        {
            return (digit >= '0' && digit <= '9');
        }

        public LotteryNumber(params char[] digits)
        {
            if(!digits.All(c => IsDigit(c)))
                throw new ArgumentOutOfRangeException(nameof(digits));
            this._rawNumber = new List<char>(digits);
            this._number = ConvertToDecimal();
            if(!this.Validate())
                throw new ArgumentOutOfRangeException(nameof(digits));
        }

        public string ToString()
        {
            return new string(_rawNumber.ToArray());
        }

        /*
        public LotteryNumber(char digit)
        {
            if(!IsDigit(digit))
                throw new ArgumentOutOfRangeException(nameof(digit));

            this.ConvertToDecimal();

        }

        public LotteryNumber(char tensDigit, char onesDigit)
        {
            if (!IsDigit(onesDigit))
                throw new ArgumentOutOfRangeException(nameof(onesDigit));

            if (!IsDigit(tensDigit))
                throw new ArgumentOutOfRangeException(nameof(tensDigit));

            if (tensDigit > '5')
                throw new ArgumentOutOfRangeException(nameof(tensDigit));

            this._rawNumber = new List<char> { tensDigit, onesDigit};
            this._number = this.ConvertToDecimal();
        }
        */

        private int ConvertToDecimal()
        {
            int num = 0;
            foreach (var digit in this._rawNumber)
            {
                var d = digit - '0';
                num = num*10 + d;
            }

            return num;
        }


        private bool Validate()
        {
            return _number >= 1 && _number <= 59;
        }
    }

    internal struct ParserState
    {
        public int NumbersSeen;
        public int NextIndex;
    }

    public class LotteryParser
    {
        private const int ExpectedLength = 7;
        private const int MaxDigits = 2;

        public IEnumerable<LotteryNumber> GetLotteryTicket(string potentialNumber)
        {
            // split into 7 lottery numbers
            // the numbers should be between 1 and 59
            // numbers should be unique

            if (potentialNumber.Length < ExpectedLength || potentialNumber.Length > MaxDigits * ExpectedLength)
            {
                // too long or too short
                return null;
            }

            foreach (var digit in potentialNumber)
            {
                if (digit < '0' || digit > '9')
                    return null;
            }

            InitParsing(potentialNumber);
            bool res = ParseLotteryTicket();
            if (!res) return null;

            return this._numberListSoFar;
        }

        /* STATE */
        private ParserState _parserState;
        private string _input;
        private List<LotteryNumber> _numberListSoFar;
        private char _lastToken;

        private void InitParsing(string rawNumber)
        {
            _parserState = new ParserState
            {
                NextIndex = 0,
                NumbersSeen = 0
            };

            _input = rawNumber;
            _numberListSoFar = new List<LotteryNumber>();
        }

        private void restoreSavedState(ParserState savedState)
        {
            _parserState = savedState;
            // restore list size to savedState.NumbersSeen
            _numberListSoFar.RemoveRange(savedState.NumbersSeen, _numberListSoFar.Count - savedState.NumbersSeen);
        }

        private void MoveNext()
        {
            this._lastToken = GetNextToken();
            _parserState.NextIndex += 1;
        }

        private int GetRemainingLength()
        {
            return this._input.Length - this._parserState.NextIndex;
        }

        private char GetNextToken()
        {
            return _input[_parserState.NextIndex];
        }

        private bool ValidateFinalState()
        {
            return _numberListSoFar.Count == ExpectedLength && GetRemainingLength() == 0;
        }

        private char NextToken
        {
            get { return this.GetNextToken(); }
        }

        public bool ParseLotteryTicket()
        {
            if (_numberListSoFar.Count == LotteryParser.ExpectedLength ||
                this.GetRemainingLength() == 0)
            {
                // termination condition: we have reached max length or there are more characters to process
                return ValidateFinalState();
            }

            var savedState = _parserState; // to help with backtracking
            bool res;

            //try one digit and matching the rest of it
            res = ParseOneDigit();
            if (res) return true;

            // if first option doesn't work, clear the state from that attempt and try with 2 digits
            restoreSavedState(savedState);
            res = ParseTwoDigits();
            //if (res) return true;

            return false;
        }

        private bool ParseOneDigit()
        {
            bool res = MatchDigit();

            if (!res) return false;

            AddLotteryNumber(new LotteryNumber(this._lastToken));

            return ParseLotteryTicket();
        }

        private void AddLotteryNumber(LotteryNumber num)
        {
            //do unique check
            this._numberListSoFar.Add(num);
            _parserState.NumbersSeen += 1;
        }

        private bool ParseTwoDigits()
        {
            bool res;
            
            res = MatchDigitUnder5();
            if (!res) return false;
            var tensdigit = this._lastToken;

            res = MatchDigit();
            if (!res) return false;
            var onesdigit = this._lastToken;

            AddLotteryNumber(new LotteryNumber(tensdigit, onesdigit));

            return ParseLotteryTicket();
        }

        private bool MatchDigitUnder5()
        {
            var res = MatchDigit();
            return this._lastToken <= '5';
        }

        private bool MatchDigit()
        {
            if (GetRemainingLength() == 0)
                return false;

            char tok = GetNextToken();
            if (tok < '0' || tok > '9')
            {
                return false;
            }

            MoveNext();
            return true;
        }
    }
}
