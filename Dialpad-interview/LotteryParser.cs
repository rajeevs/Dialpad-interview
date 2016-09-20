using System;
using System.Collections.Generic;
using System.Linq;

namespace DialpadTest
{
    /// <summary>
    /// Parses the lottery number
    /// </summary>
    public class LotteryParser
    {
        /// <summary>
        /// Parsing state
        /// </summary>
        internal struct ParserState
        {
            /// <summary>
            /// Numbers seen: If we've seen 49 25 45, this would be == 3
            /// </summary>
            public int NumbersSeen;

            /// <summary>
            /// Index of next character in input array
            /// </summary>
            public int NextIndex;
        }

        // constants

        /// <summary>
        /// Returns possible lottery splits for a list of numeric strings
        /// </summary>
        /// <param name="rawNumbers">list of potential numeric strings</param>
        /// <returns>list of valid lottery numbers</returns>
        public IEnumerable<IEnumerable<LotteryNumber>> GetLotteryTickets(string[] rawNumbers)
        {
            if(rawNumbers == null)
                throw new ArgumentNullException(nameof(rawNumbers));

            var outputs = rawNumbers.Select(GetLotteryTicket);
            return outputs;
        }

        /// <summary>
        /// Returns the split for a single lottery number
        /// </summary>
        /// <param name="rawNumber">raw input number</param>
        /// <returns>a list of 7 lottery numbers or null</returns>
        public IEnumerable<LotteryNumber> GetLotteryTicket(string rawNumber)
        {
            if (String.IsNullOrWhiteSpace(rawNumber))
            {
                throw new ArgumentOutOfRangeException(nameof(rawNumber), rawNumber, "Argument is null or whitespaces");
            }

            if (rawNumber.Length < LotteryNumberConstants.TicketLength || rawNumber.Length > LotteryNumberConstants.MaxDigits * LotteryNumberConstants.TicketLength)
            {
                // too long or too short
                return null;
            }

            if (!rawNumber.All(LotteryNumber.IsDigit))
            {
                throw new ArgumentOutOfRangeException(nameof(rawNumber), rawNumber,"Expected all digits");
            }

            // Initialize parsing state
            InitParsing(rawNumber);
            // Attempt to parse
            bool res = ParseLotteryTicket();
            return res? _numberListSoFar : null;
        }

        #region State
        private ParserState _parserState;
        private string _input;
        private List<LotteryNumber> _numberListSoFar;
        private char _lastToken;
        #endregion

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
            _lastToken = GetNextToken();
            _parserState.NextIndex += 1;
        }

        /// <summary>
        /// Returns the number of characters left in input array that have not been processed yet
        /// </summary>
        /// <returns></returns>
        private int GetRemainingLength()
        {
            return _input.Length - _parserState.NextIndex;
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
            return _numberListSoFar.Count == LotteryNumberConstants.TicketLength && GetRemainingLength() == 0;
        }

        /// <summary>
        /// Mainly used for debugging state. Ignore not used comment
        /// </summary>
        // ReSharper disable once UnusedMember.Local
        private char NextToken => GetNextToken();

        #region Parsing functions

        // The parsing methodology is inspired by a Top-down left recursive descent parser design with
        // backtracking
        // The BNF grammar is 
        // LOTTERY_TICKET   -> {DIGIT}1,2 LOTTERY_TICKET                
        //                  -> {}  
        // Basic idea is we try as many digits as MaxDigits and then parse rest of LOTTERY_TICKET 
        // If parsing rest of string fails, we backtrack, restore state and try out a different sized digit
        //
        // In between parsing, we have yacc styled actions
        // ex:
        // LOTTERY_TICKET ->  DIGIT  <1,2>
        //                   { // build number using last 1-2 digits, check if it has been seen already
        //                      // add to the list }
        //                  LOTTERY_TICKET
        //                
        // We save parsing state before work starts while trying the various digit possibilities and restore state when a particular
        // recursive call doesn't pan out

        /// <summary>
        /// Main parsing function. Uses backtracking and recursion to parse for a valid lottery ticket
        /// split. This function is not thread-safe
        /// </summary>
        /// <returns>true if it is a valid lottery number (this._numbersSoFar has the actual final split)</returns>
        private bool ParseLotteryTicket()
        {
            if (_numberListSoFar.Count == LotteryNumberConstants.TicketLength ||
                GetRemainingLength() == 0)
            {
                // termination condition: we have reached max length or there are more characters to process
                return ValidateFinalState();
            }

            var initialState = _parserState; // to help with backtracking

            for (uint dCtr = 1; dCtr <= LotteryNumberConstants.MaxDigits; dCtr++)
            {
                // restore to beginning of this function. Redundant 1st time but keeping it for simplicity
                RestoreSavedState(initialState);

                char[] digits;
                var res = MatchDigits(dCtr, out digits);
                if (!res) return false;

                res = ValidateAndAddLotteryNumber(digits);
                if(!res) continue;

                res = ParseLotteryTicket();
                if (res) return true;
            }
            return false;
        }

        /// <summary>
        /// Matches a single digit, and moves token forwards if it is a digit
        /// </summary>
        /// <returns>true if next char is digit, else false</returns>
        private bool MatchDigits(uint times, out char[] digits)
        {
            digits = null;
            var matchedDigits = new List<char>();
            for (int dCtr = 0; dCtr < times; dCtr++)
            {
                var res = MatchDigit();
                if (!res) return false;

                matchedDigits.Add(_lastToken);
            }

            digits = matchedDigits.ToArray();
            return true;
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

        /// <summary>
        /// Validate and add number created using digits
        /// </summary>
        /// <param name="digits">digits to use to form number</param>
        /// <returns>true if success, false otherwise</returns>
        private bool ValidateAndAddLotteryNumber(char[] digits)
        {
            LotteryNumber num;
            var res = LotteryNumber.Create(out num, digits);
            if (!res) return false;
            return AddLotteryNumber(num);
        }

        /// <summary>
        /// Adds a lottery number to the _numbersSoFar list if it is unique
        /// </summary>
        /// <param name="num">number to add</param>
        /// <returns>true if unique, false otherwise</returns>
        private bool AddLotteryNumber(LotteryNumber num)
        {
            if(num == null)
                throw new ArgumentNullException(nameof(num));

            //do unique check
            if (_numberListSoFar.Find(n => n.Equals(num)) != null) return false;
            _numberListSoFar.Add(num);
            _parserState.NumbersSeen += 1;
            return true;
        }
    }
}
