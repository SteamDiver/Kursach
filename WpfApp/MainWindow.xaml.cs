using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DataReceiver;
using FFT;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;

namespace WpfApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<ObservableValue> chartValues = new List<ObservableValue>();
        private List<double> rawValues = new List<double>();
        public SeriesCollection ChartData { get; set; }
        private PortDataReceiver receiver;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            ChartData = new SeriesCollection()
            {
                new LineSeries() {  Values = new ChartValues<ObservableValue>(chartValues) }
            };
            Chart.DataContext = this;
            Chart.Series = ChartData;

            receiver = new PortDataReceiverStandart("COM5", 115200);
            receiver.DataReceived += ReceiverOnDataReceived;
            var th = new Thread(receiver.BeginReceive);
            th.Start();
           
        }

        private void ReceiverOnDataReceived(string text)
        {
            try
            {
                rawValues.AddRange(text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(double.Parse));
                foreach (var value in rawValues)
                {
                    chartValues.Add(new ObservableValue(value));
                }
                
                //if (rawValues.Count >= 4096)
                //{
                //    var mValues = rawValues.Take(4096).ToList();
                //    new FFTSpectre().GetSpectre(mValues, 710, out double[] spectrum, out double[] freqSpan);
                //    chartValues = mValues.Select(v => new ObservableValue(v)).ToList();
                    
                //    rawValues.Clear();
                //    //receiver.StopReceive();
                //}
            }
            catch
            {

            }
        }
    }
}
