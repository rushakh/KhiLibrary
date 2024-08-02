using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace KhiLibrary
{
    /// <summary>
    /// For containing and managing a collection of Equalizers (of type Equalizer).
    /// </summary>
    public class Equalizers
    {
        private List<Equalizer> equalizersList = new List<Equalizer>();

        /// <summary>
        /// The existing Equalizer profiles currently in the List.
        /// </summary>
        public List<Equalizer> EqualizerProfiles { get => equalizersList; set => equalizersList = value; }

        /// <summary>
        /// Creates a new Instance of the Equalizers class (Not Recommended to create more than 1 instance at a time). 
        /// Can Optionally directly load the profiles saved to the Database.
        /// </summary>
        /// <param name="LoadExistingProfiles"></param>
        public Equalizers(bool LoadExistingProfiles = false)
        {
            if (LoadExistingProfiles)
            {
                LoadExisting();
            }
            else
            {
                equalizersList = new List<Equalizer>();
            }
        }

        /// <summary>
        /// Gets or sets the Equalizer at the provided index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public Equalizer this[int index]
        {
            get
            {
                if (EqualizerProfiles != null && EqualizerProfiles.Count >= index)
                {
                    return EqualizerProfiles[index];
                }
                else if (EqualizerProfiles == null)
                {
                    throw new ArgumentNullException();
                }
                else
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
            set
            {
                if (value is not null && value is Equalizer)
                {
                    if (EqualizerProfiles != null && EqualizerProfiles.Count >= index)
                    {
                        EqualizerProfiles[index] = value;
                    }
                    else if (EqualizerProfiles == null)
                    {
                        throw new ArgumentNullException();
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException();
                    }
                }
                else
                {
                    throw new ArgumentException("Value must be an instance of the Equalizer class and not null.");
                }
            }
        }

        /// <summary>
        /// Returns an Enumerator that iterates through the equalizers in this equalizer collection.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Equalizer> GetEnumerator()
        {
            return EqualizerProfiles.GetEnumerator();
        }

        /// <summary>
        /// Loads the existing profiles from the database, all unsaved changes will be gone.
        /// </summary>
        public void LoadExisting ()
        {
            List<Equalizer> loadedEqualizers = ReadEqualizersProfiles();
            if (loadedEqualizers.Count != 0)
            {
                equalizersList = loadedEqualizers;
            }
            else
            {
                // This is my the EQ tuned for myself, Should remove the values before Release.
                Equalizer personalEq = new Equalizer("Personal Eq");
                personalEq.BandZeroGain = (float)-5.5;
                personalEq.BandOneGain = (float)-5.2;
                personalEq.BandTwoGain = (float)-5.0;
                personalEq.BandThreeGain = (float)0.5;
                personalEq.BandFourGain = (float)-2.0;
                personalEq.BandFiveGain = (float)-2.4;
                personalEq.BandSixGain = (float)-2.4;
                personalEq.BandSevenGain = (float)1.1;
                personalEq.BandEightGain = (float)3.0;
                personalEq.BandNineGain = (float)3.0;
                personalEq.UpdateBands();
                loadedEqualizers.Add(personalEq);
                equalizersList = loadedEqualizers;
                SaveEqualizers();
            }
        }

        /// <summary>
        /// Adds an Equalizer profile containing default values to the list. Will not be added to the database unless Saved.
        /// </summary>
        /// <param name="profileName"></param>
        public void Add(string profileName)
        {
            Equalizer newEq = new Equalizer(profileName);
            equalizersList.Add(newEq);
        }

        /// <summary>
        /// Adds an Equalizer profile to the list. Will not be added to the database unless Saved.
        /// </summary>
        /// <param name="newEqualizer"></param>
        public void Add(Equalizer newEqualizer)
        {
            equalizersList.Add(newEqualizer);
        }

        /// <summary>
        /// Removes a saved profile from the database.
        /// </summary>
        /// <param name="toBeRemovedProfile"></param>
        public void Remove(Equalizer toBeRemovedProfile)
        {
            string EqName = toBeRemovedProfile.EqualizerName;

            foreach (var equalizerProfile in equalizersList)
            {
                if (equalizerProfile.EqualizerName == EqName)
                {
                    equalizersList.Remove(equalizerProfile);
                    SaveEqualizers();
                    break;
                }
            }
        }

        /// <summary>
        /// Saves the profiles to xml database.
        /// </summary>
        public void SaveEqualizers()
        {
            XDocument equalizerProfiles = EqualizersToXmlElements();
            SaveEqualizersToDatabase(equalizerProfiles);
        }

        /// <summary>
        /// Reads the Equalizer profiles from the database and returns a list of type Equalizer. Returns an empty 
        /// list if the Equalizer Profile xml Document doesn't exist or is empty.
        /// </summary>
        /// <returns></returns>
        private List<Equalizer> ReadEqualizersProfiles()
        {
            XDocument equalizerProfiles;
            XElement equalizers;
            List<Equalizer> tempEqualizersList = new List<Equalizer>();
            if (System.IO.File.Exists(InternalSettings.equalizersProfilesPath))
            {
                equalizerProfiles = XDocument.Load (InternalSettings.equalizersProfilesPath);
                var tempEqualizers = equalizerProfiles.Root;
                if (tempEqualizers != null)
                {
                    equalizers = tempEqualizers;
                    if (equalizers.HasElements)
                    {
                        var equalizzerProfiles = equalizers.Elements();
                        foreach (var equalizer in equalizzerProfiles)
                        {
                            Equalizer newEq = XmlElementToEqualizer(equalizer);
                            tempEqualizersList.Add(newEq);
                        }
                    }
                }
            }
            return tempEqualizersList;
        }

        /// <summary>
        /// Saves the Equalizer profiles in the list to database. Overwrites the old document by default as it is easier
        /// and since the document never contains alot of info.
        /// </summary>
        /// <param name="equalizerProfiles"></param>
        /// <param name="overWrite"></param>
        private void SaveEqualizersToDatabase(XDocument equalizerProfiles, bool overWrite = true)
        {
            // For saving to Xml document.
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
                equalizerProfiles.Save(dataBaseWriter);
                dataBaseWriter.Close();
                dataBaseWriter.Dispose();
                datastream.Dispose();
            }
        }

        /// <summary>
        /// Loads the xml document that contains the Equalizer profiles, or creates a new one if it doesn't exist and 
        /// writes the profiles as xml elements. Returns the xmlDocument.
        /// </summary>
        /// <returns></returns>
        private XDocument EqualizersToXmlElements()
        {
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
                equalizerProfiles = XDocument.Load(InternalSettings.equalizersProfilesPath);
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
                newEQ.SetAttributeValue("EqualizerName", eq.EqualizerName);
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
            return equalizerProfiles;
        }

        /// <summary>
        /// Extracts the information from the XElement and then creates and returns an Equalizer object based on that 
        /// info. Throwns exception if the element does not contain the information or they are not in the expected positions or etc.
        /// </summary>
        /// <param name="equalizer"></param>
        /// <returns></returns>
        private Equalizer XmlElementToEqualizer(XElement equalizer)
        {
            // The exception that needs to be thrown  to provide information regarding the cause of the problem
            // and to provide extra information that might be needed for fixing it.
            ArgumentException equalizerProfilesDocumentException = new ArgumentException
                    ("The xml Document does not contain required information, is corrupted," +
                    " or tampered.\r\n Every Element of the Document's Root element should contain" +
                    " one attribute with the XName of \"EqualizerName\" with its value (obviously) corresponding to " +
                    "the Equalizer's Name property. The said element should also contain 10 child elements (Number of" +
                    " bands in the Equalizer) that should be parsed as float and they are the values of the Equalizer's" +
                    " band gains.");

            if (equalizer.HasAttributes)
            {
                var tempName = equalizer.Attribute("EqualizerName");
                if (tempName != null)
                {
                    string name = tempName.Value;
                    float zero, first, second, third, fourth, fifth, sixth, seventh, eighth, nineth;
                    Equalizer newEq = new Equalizer(name);
                    var bands = equalizer.Elements().ToList();
                    // Can later create Equalizers with higher or lower number of bands
                    // and the condition based on count can effectively make the creation process easier
                    // and act as a filter (a weak one but better than nothing) for corrupted or incorrect info.
                    if (bands.Count == 10)
                    {
                        zero = float.Parse(bands[0].Value);
                        first = float.Parse(bands[1].Value);
                        second = float.Parse(bands[2].Value);
                        third = float.Parse(bands[3].Value);
                        fourth = float.Parse(bands[4].Value);
                        fifth = float.Parse(bands[5].Value);
                        sixth = float.Parse(bands[6].Value);
                        seventh = float.Parse(bands[7].Value);
                        eighth = float.Parse(bands[8].Value);
                        nineth = float.Parse(bands[9].Value);
                        newEq.BandZeroGain = zero;
                        newEq.BandOneGain = first;
                        newEq.BandTwoGain = second;
                        newEq.BandThreeGain = third;
                        newEq.BandFourGain = fourth;
                        newEq.BandFiveGain = fifth;
                        newEq.BandSixGain = sixth;
                        newEq.BandSevenGain = seventh;
                        newEq.BandEightGain = eighth;
                        newEq.BandNineGain = nineth;
                        newEq.UpdateBands();
                        return newEq;
                    }
                    else
                    {
                        throw equalizerProfilesDocumentException;
                    }
                }
                else 
                {
                    throw equalizerProfilesDocumentException;
                }
            }
            else
            {
                throw equalizerProfilesDocumentException;
            }
        }
    }
}
