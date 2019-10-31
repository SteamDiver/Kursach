using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;
using System.Windows.Markup;
using DataReceiverStandart;

namespace ConsoleApp
{
    class Program
    {
        private static List<string> values = new List<string>();
        private static Stopwatch stopwatch;
        static void Main(string[] args)
        {   
            stopwatch = new Stopwatch();
            var receiver = new PortDataReceiver("COM5", 115200);
            receiver.DataReceived+= ReceiverOnDataReceived;             
            receiver.BeginReceive();
            stopwatch.Start();
            var timer = new Timer(500);
            timer.Elapsed += PrintFreq;
            timer.Start();
            Console.ReadKey();
        }

        private static void PrintFreq(object sender, ElapsedEventArgs e)
        {
            Console.Clear();
            var value = (decimal)values.Count / (decimal)stopwatch.ElapsedMilliseconds * 1000;
            Console.WriteLine(value);
        }

        private static void ReceiverOnDataReceived(string text)
        {
            values.AddRange(text.Split(new []{"\r\n", "\r", "\n"}, StringSplitOptions.RemoveEmptyEntries));
        }
    }
}
