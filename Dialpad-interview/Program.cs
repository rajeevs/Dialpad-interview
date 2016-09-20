using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DialPadTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                return;
            }

            var fileName = args[1];
            if (File.Exists(fileName))
            {
                Console.WriteLine("Processing {0}", fileName);
            }
        }
    }
}
