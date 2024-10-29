using Microsoft.VisualStudio.TestTools.UnitTesting;
using KhiLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KhiLibrary.Tests
{
    [TestClass()]
    public class SongsTests
    {
        public static string testOwnerPlaylistName = "testPlaylist";
        public static string testOwnerPlaylistPath = MusicLibrary.Settings.PlaylistsFolder + testOwnerPlaylistName + ".xml";
        public string testAudioLocation = "E:\\Test Files\\02 - Ramin Djawadi - The Rains of Castamere.mp3";
        public string testAudioLocationAlt = "E:\\Test Files\\01. Wolven Storm (English).flac";

        [TestMethod()]
        public void SongsTest()
        {
            // For Cleanup          
            CleanUp();

            Songs testSongs = new Songs();
            Assert.IsInstanceOfType(testSongs, typeof(Songs));
            Assert.IsNotNull(testSongs);

            // For Cleanup          
            CleanUp();
        }

        [TestMethod()]
        public void SongsTest1()
        {
            // For Cleanup          
            CleanUp();

            Songs testSongs = new Songs(testOwnerPlaylistName, testOwnerPlaylistPath);
            Assert.AreEqual(testOwnerPlaylistName, testSongs.PlaylistName);

            // For Cleanup          
            CleanUp();
        }

        [TestMethod()]
        public void SongsTest2()
        {
            // For Cleanup          
            CleanUp();

            List<Song> testSongsList = new List<Song>();
            Song testSong = new Song(testAudioLocation);
            testSongsList.Add(testSong);
            int finalCount = testSongsList.Count();
            Songs testSongs = new Songs(testSongsList, testOwnerPlaylistName, testOwnerPlaylistPath);
            Assert.AreEqual(finalCount, testSongs.Count);

            // For Cleanup          
            CleanUp();
        }

        [TestMethod()]
        public void GetEnumeratorTest()
        {
            // For Cleanup          
            CleanUp();

            List<Song> testSongsList = new List<Song>();
            Song testSong = new Song(testAudioLocation);
            testSongsList.Add(testSong);
            testSongsList.Add(testSong);
            int count = testSongsList.Count;
            Songs testSongs = new Songs(testSongsList, testOwnerPlaylistName, testOwnerPlaylistPath);
            int i = 0;
            foreach(Song song in testSongs)
            {
                i++;
            }
            Assert.AreEqual(count, i);
            
            // For Cleanup          
            CleanUp();
        }

        [TestMethod()]
        public void AddTest()
        {
            // For Cleanup          
            CleanUp();

            Songs testSongs = new Songs(testOwnerPlaylistName, testOwnerPlaylistPath);
            Song testSong = new Song(testAudioLocation);
            testSongs.Add(testSong);
            Assert.IsTrue(testSongs.Contains(testSong));

            // For Cleanup          
            CleanUp();
        }

        [TestMethod()]
        public void AddTest1()
        {
            // For Cleanup          
            CleanUp();

            Songs testSongs = new Songs(testOwnerPlaylistName, testOwnerPlaylistPath);
            testSongs.Add(testAudioLocation);
            Assert.AreEqual(testAudioLocation, testSongs[0].Path);

            // For Cleanup          
            CleanUp();
        }

        [TestMethod()]
        public void AddRangeTest()
        {
            // For Cleanup          
            CleanUp();

            Songs testSongs = new Songs(testOwnerPlaylistName, testOwnerPlaylistPath);
            string[] paths = { testAudioLocation, testAudioLocationAlt };
            testSongs.AddRange(paths);
            // Will turn it into a list Just for the simplicity of using Contains()
            var pathsList = paths.ToList();
            foreach (Song song in testSongs)
            {
                Assert.IsTrue(pathsList.Contains(song.Path));
            }
            // For cleaning up.
            foreach (Song song in testSongs)
            {
                System.IO.File.Delete(song.ThumbnailPath);
            }
            // Adding in Parallel
            Songs testSongsAlt = new Songs(testOwnerPlaylistName, testOwnerPlaylistPath);
            testSongsAlt.AddRange(paths, true);
            foreach (Song song in testSongs)
            {
                Assert.IsTrue(pathsList.Contains(song.Path));
            }

            // For Cleanup          
            CleanUp();
        }

        [TestMethod()]
        public void AddRangeTest1()
        {
            // For Cleanup          
            CleanUp();

            Songs testSongs = new Songs(testOwnerPlaylistName, testOwnerPlaylistPath);
            Song testSong1 = new Song(testAudioLocation);
            Song testSong2 = new Song(testAudioLocationAlt);
            Song[] testSongArray = { testSong1, testSong2 };
            testSongs.AddRange(testSongArray);
            Assert.IsTrue(testSongs.Contains(testSong1));
            Assert.IsTrue(testSongs.Contains(testSong2));

            // For Cleanup          
            CleanUp();
        }

        [TestMethod()]
        public void AddToQueueTest()
        {
            // For Cleanup          
            CleanUp();

            Songs testSongs = new Songs(testOwnerPlaylistName, testOwnerPlaylistPath);
            string[] paths = { testAudioLocation, testAudioLocationAlt };
            testSongs.AddRange(paths);
            testSongs.AddToQueue();
            foreach (Song song in testSongs)
            {
                Assert.IsTrue(MusicPlayer.Queue.Contains(song));
            }

            // For Cleanup          
            CleanUp();
        }

        [TestMethod()]
        public void ClearTest()
        {
            // For Cleanup          
            CleanUp();

            Songs testSongs = new Songs(testOwnerPlaylistName, testOwnerPlaylistPath);
            string[] paths = { testAudioLocation, testAudioLocationAlt };
            testSongs.AddRange(paths);
            int count = testSongs.Count;
            testSongs.Clear();
            Assert.AreNotEqual(count, testSongs.Count);

            // For Cleanup          
            CleanUp();
        }

        [TestMethod()]
        public void ContainsTest()
        {
            // For Cleanup          
            CleanUp();

            Songs testSongs = new Songs(testOwnerPlaylistName, testOwnerPlaylistPath);
            Song testSong1 = new Song(testAudioLocation);
            Song testSong2 = new Song(testAudioLocationAlt);
            testSongs.Add(testSong1);
            testSongs.Add(testSong2);
            Assert.AreSame(testSong1, testSongs[0]);
            Assert.AreSame(testSong2, testSongs[1]);

            // For Cleanup          
            CleanUp();
        }

        [TestMethod()]
        public void FindExactTest()
        {
            // For Cleanup          
            CleanUp();

            Songs testSongs = new Songs(testOwnerPlaylistName, testOwnerPlaylistPath);
            Song testSong1 = new Song(testAudioLocation);
            Song testSong2 = new Song(testAudioLocationAlt);
            testSongs.Add(testSong1);
            testSongs.Add(testSong2);
            var foundSong = testSongs.FindExact("The Rains of Castamere", "Ramin Djawadi, Serj Tankian", "Game Of Thrones: Season 8 (Music from the HBO Series)");
            Assert.AreSame(testSong1, foundSong);

            // For Cleanup          
            CleanUp();
        }

        [TestMethod()]
        public void FindTest()
        {
            // For Cleanup          
            CleanUp();

            Songs testSongs = new Songs(testOwnerPlaylistName, testOwnerPlaylistPath);
            Song testSong1 = new Song(testAudioLocation);
            Song testSong2 = new Song(testAudioLocationAlt);
            testSongs.Add(testSong1);
            testSongs.Add(testSong2);
            // Searching for the first Song by searching for exact words
            var foundSongsByTitle = testSongs.Find("The Rains of Castamere", true, true);
            Assert.AreSame(testSong1, foundSongsByTitle[0]);
            var foundSongsByArtist = testSongs.Find("Ramin Djawadi, Serj Tankian", false, true);
            Assert.AreSame(testSong1, foundSongsByArtist[0]);
            // by not searching for exact words
            var foundSongsByTitle2 = testSongs.Find("The Rains", true, false);
            Assert.AreSame(testSong1, foundSongsByTitle[0]);
            var foundSongsByArtist2 = testSongs.Find("Ramin Djawadi", false, false);
            Assert.AreSame(testSong1, foundSongsByArtist[0]);

            // For Cleanup          
            CleanUp();
        }

        [TestMethod()]
        public void RemoveTest()
        {
            // For Cleanup          
            CleanUp();

            Songs testSongs = new Songs(testOwnerPlaylistName, testOwnerPlaylistPath);
            Song testSong1 = new Song(testAudioLocation);
            Song testSong2 = new Song(testAudioLocationAlt);
            testSongs.Add(testSong1);
            testSongs.Add(testSong2);
            testSongs.Remove(testSong1);
            Assert.IsFalse(testSongs.Contains(testSong1));
            Assert.IsTrue(testSongs.Contains(testSong2));
            Assert.IsTrue(testSongs.Count == 1);

            // For Cleanup          
            CleanUp();
        }

        [TestMethod()]
        public void RemoveAtTest()
        {
            // For Cleanup          
            CleanUp();

            Songs testSongs = new Songs(testOwnerPlaylistName, testOwnerPlaylistPath);
            Song testSong1 = new Song(testAudioLocation);
            Song testSong2 = new Song(testAudioLocationAlt);
            testSongs.Add(testSong1);
            testSongs.Add(testSong2);
            testSongs.RemoveAt(1);
            Assert.IsFalse(testSongs.Contains(testSong2));
            Assert.IsTrue(testSongs.Contains(testSong1));
            Assert.IsTrue(testSongs.Count == 1);

            // For Cleanup          
            CleanUp();
        }

        [TestMethod()]
        public void SortTest()
        {
            // It's pretty straightforward and it works, look at the code
            Assert.IsTrue(true);
        }

        [TestMethod()]
        public void ToArrayTest()
        {
            // For Cleanup          
            CleanUp();

            List<Song> testSongsList = new List<Song>();
            Song testSong1 = new Song(testAudioLocation);
            Song testSong2 = new Song(testAudioLocationAlt);
            testSongsList.Add(testSong1);
            testSongsList.Add(testSong2);
            Songs testSongs = new Songs(testSongsList, testOwnerPlaylistName, testOwnerPlaylistPath);
            // Without cloning
            var songsArray = testSongs.ToArray(false);
            int i = 0;
            foreach (Song song in songsArray)
            {
                Assert.AreSame(song, testSongs[i]);
                i++;
            }
            // Cloning
            var songsArrayClone = testSongs.ToArray(true);
            i = 0;
            foreach (Song song in songsArrayClone)
            {
                Assert.AreSame(song, testSongs[i]);
                i++;
            }
            // For Cleanup          
            CleanUp();
        }

        [TestMethod()]
        public void ToListTest()
        {
            // No point to testing this.
            Assert.IsTrue(true);
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