using System.IO.Ports;

namespace DataReceiverStandart
{
    public class PortDataReceiver
    {
        private readonly SerialPort _port;

        public delegate void PortDataReceiverHandler(string text);
        public event PortDataReceiverHandler DataReceived;


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

        private void PortOnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var sp = (SerialPort)sender;
            DataReceived?.Invoke(sp.ReadExisting());
        }
    }
}
