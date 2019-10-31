using System;
using System.Collections.Generic;
using System.Numerics;
using DSPLib;

namespace FFT
{
    public class FFTSpectre
    {
        public void GetSpectre(List<double> values, double samplingRate, out double[] spectrum, out double[] freqSpan)
        {
            // Instantiate a new DFT
            DFT dft = new DFT();

            // Initialize the DFT
            // You only need to do this once or if you change any of the DFT parameters.
            dft.Initialize((uint)values.Count);

            // Call the DFT and get the scaled spectrum back
            Complex[] cSpectrum = dft.Execute(values.ToArray());

            // Convert the complex spectrum to magnitude
            spectrum = DSP.ConvertComplex.ToMagnitude(cSpectrum);
            freqSpan = dft.FrequencySpan(samplingRate);
        }
    }
}
