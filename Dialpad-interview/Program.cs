using System;
using System.IO;
using System.Linq;
using DialpadTest;

namespace DialPadTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                return;
            }

            var fileName = args[0];
            
            if (!File.Exists(fileName))
            {
                Console.WriteLine("Cannot locate file : {0}", fileName);
                return;
            }

            string[] allNumbers = File.ReadAllLines(fileName);
            var lp = new LotteryParser();
            var allTickets = lp.GetLotteryTickets(allNumbers).
                                Select(t => t != null? String.Join(" ", t.Select(n => n.ToString())): null
                                );

            var results = allNumbers.Zip(allTickets, (n, t) => new Tuple<string, string>(n, t));

            foreach (var res in results.Where(t => t.Item2 != null))
            {
                Console.WriteLine("{0} -> {1}", res.Item1, res.Item2);
            }
        }
    }
}