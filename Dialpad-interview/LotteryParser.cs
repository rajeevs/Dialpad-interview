using System;
using System.Collections.Generic;
using System.Linq;

namespace DialpadTest
{
    public class LotteryNumber
    {
        private readonly int _number;
        private readonly List<char> _rawNumber;

        public static bool IsDigit(char digit)
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

        public override string ToString()
        {
            return new string(_rawNumber.ToArray());
        }

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

        public bool Equals(LotteryNumber other)
        {
            return this._number == other._number;
        }
    }

    public class LotteryParser
    {
        /// <summary>
        /// Parsing state
        /// </summary>
        internal struct ParserState
        {
            public int NumbersSeen;
            public int NextIndex;
        }

        // constants

        /// <summary>
        ///  Expected length of lottery ticket
        /// </summary>
        private const int ExpectedLength = 7;

        /// <summary>
        /// Maximum digits in a lottery number
        /// </summary>
        private const int MaxDigits = 2;

        /// <summary>
        /// Returns possible lottery splits for a list of numeric strings
        /// </summary>
        /// <param name="rawNumbers">list of potential numeric strings</param>
        /// <returns>list of valid lottery numbers</returns>
        public IEnumerable<IEnumerable<LotteryNumber>> GetLotteryTicket(string[] rawNumbers)
        {
            if(rawNumbers == null)
                throw new ArgumentNullException(nameof(rawNumbers));

            var outputs = rawNumbers.Select(num => this.GetLotteryTicket(num));
            return outputs;
        }

        /// <summary>
        /// Returns the split for a single lottery number
        /// </summary>
        /// <param name="rawNumber">raw input number</param>
        /// <returns>a list of 7 lottery numbers or null</returns>
        public IEnumerable<LotteryNumber> GetLotteryTicket(string rawNumber)
        {
            // split into 7 lottery numbers
            // the numbers should be between 1 and 59
            // numbers should be unique

            if (rawNumber.Length < ExpectedLength || rawNumber.Length > MaxDigits * ExpectedLength)
            {
                // too long or too short
                return null;
            }

            if (!rawNumber.All(d => LotteryNumber.IsDigit(d)))
            {
                return null;
            }

            /* Initialize parsing state */
            InitParsing(rawNumber);
            /* Attempt to parse */
            bool res = ParseLotteryTicket();
            /* return result */
            return res? this._numberListSoFar : null;
        }

        /* STATE */
        private ParserState _parserState;
        private string _input;
        private List<LotteryNumber> _numberListSoFar;
        private char _lastToken;


        /// <summary>
        /// Initialize the parsing state. Should be called before calling ParseLotteryTicket()
        /// </summary>
        /// <param name="rawNumber"></param>
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

        /// <summary>
        /// Restores the state for backtracking. Resets next Token and index into the numbersSeen list
        /// </summary>
        /// <param name="savedState">state to restore to</param>
        private void RestoreSavedState(ParserState savedState)
        {
            _parserState = savedState;
            // restore list size to savedState.NumbersSeen
            if(savedState.NumbersSeen != _numberListSoFar.Count)
                _numberListSoFar.RemoveRange(savedState.NumbersSeen, _numberListSoFar.Count - savedState.NumbersSeen);
        }

        /// <summary>
        /// Skip to the next token
        /// </summary>
        private void MoveNext()
        {
            this._lastToken = GetNextToken();
            _parserState.NextIndex += 1;
        }

        /// <summary>
        /// Returns the number of characters left in input array that have not been processed yet
        /// </summary>
        /// <returns></returns>
        private int GetRemainingLength()
        {
            return this._input.Length - this._parserState.NextIndex;
        }

        /// <summary>
        /// Returns the next character pointed to. Will raise an exception if at end of input
        /// </summary>
        /// <returns></returns>
        private char GetNextToken()
        {
            return _input[_parserState.NextIndex];
        }

        /// <summary>
        /// Checks if output is valid : the lottery number list has 7 entries and no input remains
        /// </summary>
        /// <returns>true if both conditions hold, else false</returns>
        private bool ValidateFinalState()
        {
            return _numberListSoFar.Count == ExpectedLength && GetRemainingLength() == 0;
        }

        /// <summary>
        /// Mainly used for debugging state. Ignore not used comment
        /// </summary>
        private char NextToken
        {
            get { return this.GetNextToken(); }
        }

        #region Parsing functions

        // The parsing methodology is inspired by a Top-down left recursive descent parser design with
        // backtracking
        // The BNF grammar is 
        // LOTTERY_TICKET   -> DIGIT LOTTERY_TICKET                
        //                  -> DIGIT_UNDER_5  DIGIT LOTTERY_TICKET
        //                  -> {}  
        // Basic idea is we have a function for each Rule and LOTTERY_TICKET has 2 rules for evaluation
        // and it attempts them in order. If the 1st rule fails, then we try the second one
        //
        // Also, we have yacc style embedded actions that are performed along with parsing
        // ex:
        // LOTTERY_TICKET -> DIGIT_UNDER_5  DIGIT   
        //                   { // build number using last 2 digits, check if it has been seen already
        //                      // add to the list }
        //                  LOTTERY_TICKET
        //                
        // We save parsing state before the rule while trying the 2 rule evaluations and restore state when a particular
        // search doesn't pan out

        /// <summary>
        /// Main parsing function. Uses backtracking and recursion to parse for a valid lottery ticket
        /// split
        /// </summary>
        /// <returns>true if it is a valid lottery number (this._numbersSoFar has the actual final split)</returns>
        private bool ParseLotteryTicket()
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
            RestoreSavedState(savedState); // don't need to do this but doing so for symmetry
            res = ParseOneDigit();
            if (res) return true;

            // if first option doesn't work, clear the state from that attempt and try with 2 digits
            RestoreSavedState(savedState);
            res = ParseTwoDigits();
            if (res) return true;

            return false;
        }

        /// <summary>
        /// Attempt to parse one digit and then the rest of the string
        /// </summary>
        /// <returns></returns>
        private bool ParseOneDigit()
        {
            // MatchDigit   { Do unique checks }
            // ParseRest

            // Match any digit
            bool res = MatchDigit();
            if (!res) return false;

            // ACTION associated with digit
            var num = new LotteryNumber(this._lastToken);
            if (!AddLotteryNumber(num)) return false;

            // parse rest of the rule
            // Technically this is a tail-recursion split across 2 functions and can be converted to iteration
            return ParseLotteryTicket();
        }

        /// <summary>
        /// Attempt to parse 2 digits (between 1 and 59)and then the rest of the string
        /// </summary>
        /// <returns>true if can parse 2 digits immediately and then rest of the ticket succesfully</returns>
        private bool ParseTwoDigits()
        {
            // Match digit under 5
            // Match any digit
            // ParseRestOfString

            bool res;
            
            res = MatchDigitUnder5();
            if (!res) return false;
            var tensdigit = this._lastToken;

            res = MatchDigit();
            if (!res) return false;
            var onesdigit = this._lastToken;

            var num = new LotteryNumber(tensdigit, onesdigit);
            if (!AddLotteryNumber(num)) return false;

            // parse rest of the rule
            // Technically this is a tail-recursion split across 2 functions and can be converted to iteration
            return ParseLotteryTicket();
        }

        /// <summary>
        /// Check if next char is a digit under 5 and moves forward in input
        /// </summary>
        /// <returns>true if next character is digit under 5, else false</returns>
        private bool MatchDigitUnder5()
        {
            var res = MatchDigit();
            return this._lastToken <= '5';
        }

        /// <summary>
        /// Matches a single digit, and moves token forwards if it is a digit
        /// </summary>
        /// <returns>true if next char is digit, else false</returns>
        private bool MatchDigit()
        {
            if (GetRemainingLength() == 0)
                return false;

            char tok = GetNextToken();
            if(!LotteryNumber.IsDigit(tok))
                return false;
            MoveNext();
            return true;
        }

        #endregion 

        private bool AddLotteryNumber(LotteryNumber num)
        {
            //do unique check
            if (_numberListSoFar.Find(n => n.Equals(num)) != null) return false;
            this._numberListSoFar.Add(num);
            _parserState.NumbersSeen += 1;
            return true;
        }

    }
}
