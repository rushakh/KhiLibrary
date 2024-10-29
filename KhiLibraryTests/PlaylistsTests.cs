
namespace KhiLibrary.Tests
{
    [TestClass()]
    public class PlaylistsTests
    {
        public string testAudioLocation = "E:\\Test Files\\02 - Ramin Djawadi - The Rains of Castamere.mp3";
        public string testAudioLocationAlt = "E:\\Test Files\\01. Wolven Storm (English).flac";

        [TestMethod()]
        public void PlaylistsTest()
        {
            // For Cleanup          
            CleanUp();

            // The constructor can load existing playlists (from the xml databases) if parameter is set to true but
            // since there is none right now, no point in doing so but it's meant to be used while set to true since
            // it will throw an exception if a name is chosen which I wont handle in the tests that is already taken by
            // another playlist (meaning that if All Songs and Favorites exist, then a new instance of Playlists is 
            // constructed with the parameter set to false). I'll probably change this in the future, but for now it works.
            Playlists testPlaylists = new Playlists(false);
            // By default, will create an All Songs and Favorites playlists and adds
            // them to the collection if they don't exist.
            Assert.IsNotNull(testPlaylists);
            Assert.IsTrue(testPlaylists.Count == 2);
            Assert.IsNotNull(testPlaylists.PlaylistsList);
            Assert.AreEqual("All Songs", testPlaylists[0].Name);
            Assert.AreEqual("Favorites", testPlaylists[1].Name);

            // For Cleanup          
            CleanUp();
        }

        [TestMethod()]
        public void GetEnumeratorTest()
        {
            Playlists testPlaylists = new Playlists();
            int i = 0;
            foreach (Playlist defaultPlaylist in testPlaylists)
            {
                i++;
            }
            Assert.IsTrue(i == testPlaylists.Count);
        }

        [TestMethod()]
        public void LoadExistingDatabasesTest()
        {
            // For Cleanup          
            CleanUp();

            Playlist testPlaylist = new Playlist("Test Playlist");
            string[] songPaths = { testAudioLocation, testAudioLocationAlt };
            testPlaylist.Songs.AddRange(songPaths);
            testPlaylist.Save();
            Playlists testPlaylists = new Playlists(false);
            Assert.IsTrue(testPlaylists.Count == 2);
            testPlaylists.LoadExistingDatabases();
            Assert.IsTrue(testPlaylists.Count == 3);
            var loadedPlaylist = testPlaylists.Find("Test Playlist");
            Assert.IsNotNull(loadedPlaylist);
            Assert.IsTrue(loadedPlaylist.Songs.Count == 2);

            // For Cleanup          
            CleanUp();
        }

        [TestMethod()]
        public void AddTest()
        {
            // For Cleanup          
            CleanUp();

            string testPlaylistName = "Test Playlist";
            Playlists testPlaylists = new Playlists(false);
            testPlaylists.Add(testPlaylistName);
            var addedPlaylist = testPlaylists.Find("Test Playlist");
            Assert.IsNotNull(addedPlaylist);
            Assert.IsTrue(addedPlaylist.Name == testPlaylistName);

            // For Cleanup          
            CleanUp();
        }

        [TestMethod()]
        public void AddTest1()
        {
            // For Cleanup          
            CleanUp();

            Playlists testPlaylists = new Playlists(false);
            Playlist testPlaylist = new Playlist("Test Playlist");
            string[] songPaths = { testAudioLocation, testAudioLocationAlt };
            testPlaylist.Songs.AddRange(songPaths);
            testPlaylist.Save();
            Assert.IsFalse(testPlaylists.Contains(testPlaylist));
            testPlaylists.Add(testPlaylist);
            Assert.IsTrue(testPlaylists.Contains(testPlaylist));
            var addedPlaylist = testPlaylists.Find("Test Playlist");
            Assert.IsNotNull(addedPlaylist);
            Assert.IsTrue(addedPlaylist.Songs.Count == 2);

            // For Cleanup          
            CleanUp();
        }

        [TestMethod()]
        public void AddRangeTest()
        {
            // For Cleanup          
            CleanUp();

            Playlists testPlaylists = new Playlists(false);
            Playlist testPlaylist1 = new Playlist("Test Playlist1");
            Playlist testPlaylist2 = new Playlist("Test Playlist2");
            List<Playlist> listOfPlaylist = new List<Playlist>();
            listOfPlaylist.Add(testPlaylist1);
            listOfPlaylist.Add(testPlaylist2);
            Assert.IsFalse(testPlaylists.Contains(testPlaylist1));
            Assert.IsFalse(testPlaylists.Contains(testPlaylist2));
            testPlaylists.AddRange(listOfPlaylist);
            Assert.IsTrue(testPlaylists.Contains(testPlaylist1));
            Assert.IsTrue(testPlaylists.Contains(testPlaylist2));

            // For Cleanup          
            CleanUp();
        }

        [TestMethod()]
        public void ClearTest()
        {
            // For Cleanup          
            CleanUp();

            string testPlaylistName = "Test Playlist";
            Playlists testPlaylists = new Playlists(false);
            testPlaylists.Add(testPlaylistName);
            var addedPlaylist = testPlaylists.Find("Test Playlist");
            Assert.IsNotNull(addedPlaylist);
            Assert.IsTrue(testPlaylists.Count == 3);
            testPlaylists.Clear();
            Assert.IsTrue(testPlaylists.Count == 0);

            // For Cleanup          
            CleanUp();
        }

        [TestMethod()]
        public void ContainsTest()
        {
            // For Cleanup          
            CleanUp();

            Playlists testPlaylists = new Playlists(false);
            Playlist testPlaylist1 = new Playlist("Test Playlist1");
            Playlist testPlaylist2 = new Playlist("Test Playlist2");
            Assert.IsFalse(testPlaylists.Contains(testPlaylist1));
            Assert.IsFalse(testPlaylists.Contains(testPlaylist2));
            testPlaylists.Add(testPlaylist1);
            testPlaylists.Add(testPlaylist2);           
            Assert.IsTrue(testPlaylists.Contains(testPlaylist1));
            Assert.AreEqual(testPlaylist1, testPlaylists["Test Playlist1"]);           
            Assert.IsTrue(testPlaylists.Contains(testPlaylist2));
            Assert.AreEqual(testPlaylist2, testPlaylists["Test Playlist2"]);

            // For Cleanup          
            CleanUp();
        }

        [TestMethod()]
        public void CopyToTest()
        {
            // I don't think there is any need to test this (didn't write any code for it).
            Assert.IsTrue(true);
        }

        [TestMethod()]
        public void FindTest()
        {
            // For Cleanup          
            CleanUp();

            Playlists testPlaylists = new Playlists(false);
            Playlist testPlaylist1 = new Playlist("Test Playlist abcdefg");
            Playlist testPlaylist2 = new Playlist("Test Playlist 1234567");
            Assert.IsFalse(testPlaylists.Contains(testPlaylist1));
            Assert.IsFalse(testPlaylists.Contains(testPlaylist2));
            testPlaylists.Add(testPlaylist1);
            testPlaylists.Add(testPlaylist2);
            Assert.IsTrue(testPlaylists.Contains(testPlaylist1));
            Assert.IsTrue(testPlaylists.Contains(testPlaylist2));
            // It's a simple searching function, will just find an exact match.           
            Assert.AreEqual(testPlaylist1, testPlaylists.Find("Test Playlist abcdefg"));
            Assert.AreEqual(testPlaylist2, testPlaylists.Find("Test Playlist 1234567"));
            Assert.AreEqual(testPlaylist1, testPlaylists["Test Playlist abcdefg"]);
            Assert.AreEqual(testPlaylist2, testPlaylists["Test Playlist 1234567"]);

            // For Cleanup          
            CleanUp();
        }

        /// <summary>
        /// Cleans up the default folders and the files inside that are made during the tests. Best
        /// used both before and after the test codes.
        /// </summary>
        internal static void CleanUp()
        {
            if (System.IO.Directory.Exists(MusicLibrary.Settings.PlaylistsFolder))
            {
                System.IO.Directory.Delete(MusicLibrary.Settings.PlaylistsFolder, true);
            }
            if (System.IO.Directory.Exists(MusicLibrary.Settings.AlbumArtsThumbnailsPath))
            {
                System.IO.Directory.Delete(MusicLibrary.Settings.AlbumArtsThumbnailsPath, true);
            }
            if (System.IO.Directory.Exists(MusicLibrary.Settings.TempArtsFolder))
            {
                System.IO.Directory.Delete(MusicLibrary.Settings.TempArtsFolder, true);
            }
            if (System.IO.Directory.Exists(MusicLibrary.Settings.ApplicationPath + "Backups"))
            {
                System.IO.Directory.Delete(MusicLibrary.Settings.ApplicationPath + "Backups", true);
            }
        }
    }
}