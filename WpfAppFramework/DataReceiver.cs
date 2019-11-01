using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace WpfAppFramework
{
    public class PortDataReceiver
    {
        private object lockObj = new Object();
        private readonly SerialPort _port;

        public delegate void PortDataReceiverHandler(List<string> data);
        public event PortDataReceiverHandler DataReceived;
        private Regex regexp = new Regex(@"[\r\n]*^block[\r\n]+([\d]*[\r\n]*[\d]*.[\d]*[\r\n]*[-\d;]*)[\r\n]+end[\r\n]*$", RegexOptions.Multiline);

        public PortDataReceiver(string name, int baudrate)
        {
            _port = new SerialPort(name, baudrate);
        }

        public void BeginReceive()
        {
            _port.DataReceived += PortOnDataReceived;
            _port.Open();
        }

        public void StopReceive()
        {
            _port.Close();
            _port.DataReceived -= PortOnDataReceived;
        }

        private string buffer = "";
        
        private void PortOnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            lock (lockObj)
            {
                var sp = (SerialPort) sender;

                buffer += sp.ReadExisting();
                var matches = regexp.Matches(buffer);
                if (matches.Count > 0)
                {
                    var data = new List<string>();
                    foreach (Match match in matches)
                    {
                        data.Add(match.Groups[1].Value);
                    }
                    DataReceived?.Invoke(data);
                    buffer = "";
                }
            }

        }
    }
}
