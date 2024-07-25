using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace KhiLibrary
{
    internal class Equalizers
    {
        private List<Equalizer> equalizersList = new List<Equalizer>();

        public Equalizers()
        {
            equalizersList = new List<Equalizer> ();
        }

        public void LoadExisting ()
        {

        }

        public void Add()
        {

        }

        public void Remove ()
        {

        }

        private void ReadEqualizersProfiles()
        {

        }

        private void SaveEqualizersToDatabase()
        {
            bool overWrite = true;
            XDocument equalizerProfiles;
            XElement equalizers;
            if (!System.IO.File.Exists(InternalSettings.equalizersProfilesPath))
            {
                equalizerProfiles = new XDocument();
                equalizers = new XElement("Equalizers");
                equalizerProfiles.Add(equalizers);
            }
            else
            {
                equalizerProfiles = new XDocument(InternalSettings.equalizersProfilesPath);
                var tempEqualizers = equalizerProfiles.Root;
                if (tempEqualizers == null)
                {
                    equalizers = new XElement("Equalizers");
                    equalizerProfiles.Add(equalizers);
                }
                else
                {
                    equalizers = tempEqualizers;
                }
            }
            foreach (Equalizer eq in equalizersList)
            {
                XElement newEQ = new XElement("EqualizerProfile");
                newEQ.SetAttributeValue ("EqualizerName", eq.EqualizerName);
                XElement EqBandZeroGain = new XElement("bandZeroGain", eq.BandZeroGain);
                XElement EqBandOneGain = new XElement("bandOneGain", eq.BandOneGain);
                XElement EqBandTwoGain = new XElement("bandTwoGain", eq.BandTwoGain);
                XElement EqBandThreeGain = new XElement("bandThreeGain", eq.BandThreeGain);
                XElement EqBandFourGain = new XElement("bandFourGain", eq.BandFourGain);
                XElement EqBandFiveGain = new XElement("bandFiveGain", eq.BandFiveGain);
                XElement EqBandSixGain = new XElement("bandSixGain", eq.BandSixGain);
                XElement EqBandSevenGain = new XElement("bandSevenGain", eq.BandSevenGain);
                XElement EqBandEightGain = new XElement("bandEightGain", eq.BandEightGain);
                XElement EqBandNineGain = new XElement("bandNineGain", eq.BandNineGain);
                newEQ.Add(EqBandZeroGain);
                newEQ.Add(EqBandOneGain);
                newEQ.Add(EqBandTwoGain);
                newEQ.Add(EqBandThreeGain);
                newEQ.Add(EqBandFourGain);
                newEQ.Add(EqBandFiveGain);
                newEQ.Add(EqBandSixGain);
                newEQ.Add(EqBandSevenGain);
                newEQ.Add(EqBandEightGain);
                newEQ.Add(EqBandNineGain);
                equalizers.Add(newEQ);
            }

            FileStreamOptions options = new()
            {
                Options = FileOptions.None,
                Access = FileAccess.ReadWrite,
                Share = FileShare.ReadWrite,
                Mode = FileMode.OpenOrCreate,
                BufferSize = 4096
            };
            if (overWrite) { options.Mode = FileMode.Create; }
            else { options.Mode = FileMode.OpenOrCreate; }
            XmlWriterSettings settings = new()
            {
                ConformanceLevel = ConformanceLevel.Auto,
                CloseOutput = true,
                Encoding = Encoding.UTF8,
                Indent = true,
                NewLineChars = System.Environment.NewLine,
                NewLineHandling = NewLineHandling.None,
                WriteEndDocumentOnClose = true,

            };
            using (StreamWriter datastream = new(InternalSettings.equalizersProfilesPath, Encoding.UTF8, options))
            {
                XmlWriter dataBaseWriter = XmlWriter.Create(datastream, settings);
                playlistDatabase.Save(dataBaseWriter);
                dataBaseWriter.Close();
                dataBaseWriter.Dispose();
                datastream.Dispose();
                isFinished = true;
            }
        }
    }
}
