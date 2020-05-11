using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OneSTools.TechLog;
using System.Diagnostics;

namespace OneSTools.TechLogConsoleApp
{
    class Program
    {
        static object locker = new object();
        static int i = 0;

        static async Task Main(string[] args)
        {
            var parser = new TechLogParser(@"C:\Users\akpaev.e.ENTERPRISE\Desktop\ExpertTools\tl", EventHandler);

            var watch = new Stopwatch();
            watch.Start();

            await parser.Parse();

            watch.Stop();

            Console.WriteLine($"Считано событий: {i}");
            Console.WriteLine($"Время выполнения: {watch.Elapsed}");
            Console.ReadKey();
        }

        private static void EventHandler(Dictionary<string, string> eventData)
        {
            lock(locker)
            {
                i++;
            }
        }
    }
}
