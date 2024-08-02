using NAudio.Wave;

namespace KhiLibrary
{
    /// <summary>
    /// A 10 band equalizer to be used for MusicPlayer.
    /// </summary>
    public class Equalizer
    {
        private string eqName;
        private readonly float freq = (float)31.5;
        private readonly float bandwidth = (float)0.8;

        private NAudio.Extras.EqualizerBand bandZero = new NAudio.Extras.EqualizerBand();
        private NAudio.Extras.EqualizerBand bandOne = new NAudio.Extras.EqualizerBand();
        private NAudio.Extras.EqualizerBand bandTwo = new NAudio.Extras.EqualizerBand();
        private NAudio.Extras.EqualizerBand bandThree = new NAudio.Extras.EqualizerBand();
        private NAudio.Extras.EqualizerBand bandFour = new NAudio.Extras.EqualizerBand();
        private NAudio.Extras.EqualizerBand bandFive = new NAudio.Extras.EqualizerBand();
        private NAudio.Extras.EqualizerBand bandSix = new NAudio.Extras.EqualizerBand();
        private NAudio.Extras.EqualizerBand bandSeven = new NAudio.Extras.EqualizerBand();
        private NAudio.Extras.EqualizerBand bandEight = new NAudio.Extras.EqualizerBand();
        private NAudio.Extras.EqualizerBand bandNine = new NAudio.Extras.EqualizerBand();
        private NAudio.Extras.EqualizerBand[] bands;

        /// <summary>
        /// The Name of this Equalizer profile.
        /// </summary>
        public string EqualizerName { get => eqName; set => eqName = value; }

        /// <summary>
        /// The gain (positive or negatice) in 31.5 Hz frequency.
        /// </summary>
        public float BandZeroGain { get => bandZero.Gain; set => bandZero.Gain = (float)value; }

        /// <summary>
        /// The gain (positive or negatice) in 63 Hz frequency.
        /// </summary>
        public float BandOneGain { get => bandOne.Gain; set => bandOne.Gain = (float)value; }

        /// <summary>
        /// The gain (positive or negatice) in 125 Hz frequency.
        /// </summary>
        public float BandTwoGain { get => bandTwo.Gain; set => bandTwo.Gain = (float)value; }

        /// <summary>
        /// The gain (positive or negatice) in 250 Hz frequency.
        /// </summary>
        public float BandThreeGain { get => bandThree.Gain; set => bandThree.Gain = (float)value; }

        /// <summary>
        /// The gain (positive or negatice) in 500 Hz frequency.
        /// </summary>
        public float BandFourGain { get => bandFour.Gain; set => bandFour.Gain = (float)value; }

        /// <summary>
        /// The gain (positive or negatice) in 1 kHz frequency.
        /// </summary>
        public float BandFiveGain { get => bandFive.Gain; set => bandFive.Gain = (float)value; }

        /// <summary>
        /// The gain (positive or negatice) in 2 kHz frequency.
        /// </summary>
        public float BandSixGain { get => bandSix.Gain; set => bandSix.Gain = (float)value; }

        /// <summary>
        /// The gain (positive or negatice) in 4 kHz frequency.
        /// </summary>
        public float BandSevenGain { get => bandSeven.Gain; set => bandSeven.Gain = (float)value; }

        /// <summary>
        /// The gain (positive or negatice) in 8 kHz frequency.
        /// </summary>
        public float BandEightGain { get => bandEight.Gain; set => bandEight.Gain = (float)value; }

        /// <summary>
        /// The gain (positive or negatice) in 16 kHz frequency.
        /// </summary>
        public float BandNineGain { get => bandNine.Gain; set => bandNine.Gain = (float)value; }

        /// <summary>
        /// Creates an equalizer profile that can be added to Equalizers collection and used for MusicPlayer. 
        /// Band Gains are by default set to 0.
        /// </summary>
        /// <param name="equalizerName"></param>
        public Equalizer(string equalizerName)
        {
            eqName = equalizerName;
            // these are numbers that i prefer, will set to them to zero as default later.
            bandZero.Gain = (float)0;
            bandZero.Frequency = freq;
            bandZero.Bandwidth = bandwidth;
            bandOne.Frequency = freq * 2;
            bandOne.Gain = (float)0;
            bandOne.Bandwidth = bandwidth;
            bandTwo.Frequency = freq * 4;
            bandTwo.Bandwidth = bandwidth;
            bandTwo.Gain = (float)0;
            bandThree.Frequency = freq * 8;
            bandThree.Bandwidth = bandwidth;
            bandThree.Gain = (float)0;
            bandFour.Frequency = freq * 16;
            bandFour.Bandwidth = bandwidth;
            bandFour.Gain = (float)0;
            bandFive.Frequency = freq * 32;
            bandFive.Bandwidth = bandwidth;
            bandFive.Gain = (float)0;
            bandSix.Frequency = freq * 64;
            bandSix.Bandwidth = bandwidth;
            bandSix.Gain = (float)0;
            bandSeven.Frequency = freq * 128;
            bandSeven.Bandwidth = bandwidth;
            bandSeven.Gain = (float)0;
            bandEight.Frequency = freq * 256;
            bandEight.Bandwidth = bandwidth;
            bandEight.Gain = (float)0;
            bandNine.Frequency = freq * 512;
            bandNine.Bandwidth = bandwidth;
            bandNine.Gain = (float)0;
            bands = [ bandZero, bandOne, bandTwo, bandThree, bandFour,
                      bandFive, bandSix, bandEight, bandNine];
        }

        /// <summary>
        /// Updates the equalizer Bands.
        /// </summary>
        public void UpdateBands()
        {
            
            bands = [ bandZero, bandOne, bandTwo, bandThree, bandFour,
                      bandFive, bandSix, bandEight, bandNine];
        }

        /// <summary>
        /// Attaches the Equalizer to the Audio Reader.
        /// </summary>
        /// <param name="sourceAudioReader"></param>
        /// <returns></returns>
        public ISampleProvider EqualizeSong (ISampleProvider sourceAudioReader)
        {
            NAudio.Extras.Equalizer equalizer = new NAudio.Extras.Equalizer(sourceAudioReader, bands);
            return equalizer;
        }
    }
}
