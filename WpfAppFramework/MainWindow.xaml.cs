using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DataReceiverStandart;
using FFTStandart;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using Timer = System.Timers.Timer;

namespace WpfAppFramework
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ChartValues<ObservableValue> Values { get; set; } = new ChartValues<ObservableValue>();
        private BlockingCollection<double> rawValues = new BlockingCollection<double>();
        private PortDataReceiver receiver;

        public int N { get; set; }
        public int Samples = 1024;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            N = 1;
            Chart.Series.Add(new ColumnSeries()
            {
                Values = Values
            });
            Chart.AxisY.Add(
                new Axis
                {
                    //MaxValue = 50
                });
            Chart.DisableAnimations = true;
            Chart.DataTooltip = null;
            Chart.AnimationsSpeed = TimeSpan.FromMilliseconds(0);

            receiver = new PortDataReceiver("COM5", 115200);
            receiver.DataReceived += ReceiverOnDataReceived;
            var th = new Thread(receiver.BeginReceive);
            th.Start();
            Timer t = new Timer(500);
            t.Elapsed += (sender, args) => { DrawChart(); };
            t.Start();

        }

        private void ReceiverOnDataReceived(string text)
        {
            try
            {
                var lastRNPos = text.LastIndexOf("\r\n", StringComparison.InvariantCulture);
                if (lastRNPos != -1)
                    text = text.Remove(lastRNPos);
                var values = text.Trim().Split(new[] {"\r\n", "\r", "\n"}, StringSplitOptions.RemoveEmptyEntries);
                foreach (var value in values)
                {
                    rawValues.Add(int.Parse(value));
                }

                if (rawValues.Count >= Samples * N)
                {
                    var spectrum = new double[Samples / 2 + 1];
                    for (int n = 0; n < N; n++)
                    {
                        var data = rawValues.Skip(n * Samples).Take(Samples).ToList();
                        new FFTSpectre().GetSpectre(data, 1420, out var lspectr, out FreqSpan);
                        for (int i = 0; i < lspectr.Length; i++)
                        {
                            spectrum[i] += lspectr[i] / N;
                        }
                    }

                    Spectrum = spectrum;
                    rawValues = new BlockingCollection<double>();
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private double[] FreqSpan;
        public string[] Labels => FreqSpan?.Skip(1).Select(x=>x.ToString()).ToArray();

        public double[] Spectrum;

        private void DrawChart()
        {
            if (Spectrum != null)
            {
                Values.Clear();
                Values.AddRange(Spectrum.Skip(1).Select(v => new ObservableValue(v)).ToList());
            }
        }
    }
}
