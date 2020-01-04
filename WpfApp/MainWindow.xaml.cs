using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
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
using DataReceiver;
using FFT;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using Timer = System.Timers.Timer;

namespace WpfAppFramework
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public ChartValues<ObservableValue> Values { get; set; } = new ChartValues<ObservableValue>();
        private BlockingCollection<double> rawValues = new BlockingCollection<double>();
        private PortDataReceiver receiver;

        public int N { get; set; }
        public int Samples = 512;

        public double SamplingRate
        {
            get { return _samplingRate; }
            set
            {
                _samplingRate = value;
                OnPropertyChanged(nameof(SamplingRate));
            }
        }

        private double _samplingRate;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            N = 1;
            _samplingRate = 0;

            Chart.Series.Add(new ColumnSeries()
            {
                Values = Values
            });

            Chart.DisableAnimations = true;
            //Chart.DataTooltip = null;
            Chart.AnimationsSpeed = TimeSpan.FromMilliseconds(0);

            receiver = new PortDataReceiver("COM6", 115200);
            receiver.DataReceived += ReceiverOnDataReceived;
            var th = new Thread(receiver.BeginReceive);
            th.Start();
            Timer t = new Timer(500);
            t.Elapsed += (sender, args) => { DrawChart(); };
            t.Start();

        }

        private void ReceiverOnDataReceived(List<string> texts)
        {
            try
            {
                var sampleRate = 0d;
                foreach (var text in texts)
                {
                    var splittedText = text.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    Samples = int.Parse(splittedText[0], CultureInfo.InvariantCulture);
                    sampleRate = double.Parse(splittedText[1], CultureInfo.InvariantCulture);
                    var values = splittedText[2].Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse);
                    if (values.Count() < Samples)
                        return;
                    foreach (var value in values)
                    {
                        rawValues.Add(value);
                    }
                }

                if (rawValues.Count >= Samples * N)
                {
                    var spectrum = new double[Samples / 2 + 1];
                    var frequencies = new double[Samples / 2 + 1];

                    for (int n = 0; n < N; n++)
                    {
                        var data = rawValues.Skip(n * Samples).Take(Samples).ToList();
                        new FFTSpectre().GetSpectre(data, sampleRate, out var lspectr, out frequencies);
                        for (int i = 0; i < lspectr.Length; i++)
                        {
                            spectrum[i] += lspectr[i] / N;
                        }
                    }

                    Spectrum = spectrum;
                    FreqSpan = frequencies;
                    SamplingRate = sampleRate;
                    rawValues = new BlockingCollection<double>();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private double[] FreqSpan;
        public List<string> Labels { get; set; } = new List<string>();

        public double[] Spectrum;

        private void DrawChart()
        {
            if (Spectrum != null)
            {
                Values.Clear();
                Values.AddRange(Spectrum.Skip(1).Select(v => new ObservableValue(v)).ToList());
                Labels.Clear();
                Labels.AddRange(FreqSpan?.Skip(1).Select(x => x.ToString()).ToArray());
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
