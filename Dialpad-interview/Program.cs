using System;
using System.IO;
using System.Linq;
using System.Xml;
using DialpadTest;

namespace DialPadTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Program.Usage();
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

        private static void Usage()
        {
            Console.WriteLine("USAGE: {0}.exe fileName",
                System.Diagnostics.Process.GetCurrentProcess().ProcessName);
            Console.WriteLine("\nFile should contain possible lottery numbers with one number per line");
            Console.WriteLine("EXAMPLE input:\n1\r\n42\r\n100848\r\n4938532894754\r\n");
            Console.WriteLine("EXAMPLE Output:\n4938532894754 -> 49 38 53 28 9 47 54\r\n1234567 -> 1 2 3 4 5 6 7");
        }
    }
}