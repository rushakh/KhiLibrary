using Microsoft.VisualStudio.TestTools.UnitTesting;
using KhiLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KhiLibrary.Tests
{
    [TestClass()]
    public class PlaylistTests
    {
        public string testAudioLocation = "E:\\Test Files\\02 - Ramin Djawadi - The Rains of Castamere.mp3";
        public string testAudioLocationAlt = "E:\\Test Files\\01. Wolven Storm (English).flac";

        [TestMethod()]
        public void PlaylistTest()
        {
            // For Cleanup          
            CleanUp();

            Playlist testPlaylist = new Playlist();
            Assert.IsNotNull(testPlaylist);

            // For Cleanup          
            CleanUp();
        }

        [TestMethod()]
        public void PlaylistTest1()
        {
            // For Cleanup          
            CleanUp();

            string testName = "Test Playlist Name";
            // This is where the playlist database should be after it's creation.
            string testPlaylistPath = MusicLibrary.Settings.PlaylistsFolder + testName + ".xml";
            Playlist testPlaylist = new Playlist(testName);
            Assert.IsNotNull(testPlaylist);
            Assert.AreEqual(testName, testPlaylist.Name);
            Assert.AreEqual(testPlaylistPath, testPlaylist.Path);

            // For Cleanup          
            CleanUp();
        }

        [TestMethod()]
        public void PlaylistTest2()
        {
            // For Cleanup          
            CleanUp();

            string testName = "Test Playlist Name";
            string testPlaylistPath = MusicLibrary.Settings.PlaylistsFolder + testName + ".xml";
            Songs testSongs = new Songs();
            testSongs.Add(testAudioLocation);
            // When constructing the Playlist this way, the path will be checked to see
            // if the file actually exists, but the file won't be opened to check if the
            // the playlist name and songs are actually in there or match, so an empty xml file will do.
            Playlist emptyPlaylist = new Playlist("empty");
            // Now there should be an xml file, and we can change its name
            System.IO.File.Move(emptyPlaylist.Path, testPlaylistPath);
            // Can now construct the playlist and begin testing
            Playlist testPlaylist = new Playlist(testSongs, testName, testPlaylistPath);
            Assert.IsNotNull(testPlaylist);
            Assert.AreEqual(testName, testPlaylist.Name);
            Assert.AreEqual(testPlaylistPath, testPlaylist.Path);
            Assert.IsNotNull(testPlaylist.Songs);
            Assert.AreEqual(testAudioLocation, testPlaylist.Songs[0].Path);

            // For Cleanup          
            CleanUp();
        }

        [TestMethod()]
        public void AddToQueueTest()
        {
            // For Cleanup          
            CleanUp();

            string testName = "Test Playlist Name";
            string testPlaylistPath = MusicLibrary.Settings.PlaylistsFolder + testName + ".xml";
            Playlist testPlaylist = new Playlist(testName);
            Song song1 = new Song(testAudioLocation);
            Song song2 = new Song(testAudioLocationAlt);
            testPlaylist.Songs.Add(song1);
            testPlaylist.Songs.Add(song2);
            testPlaylist.AddToQueue();
            Assert.IsTrue(MusicPlayer.Queue.Contains(song1));
            Assert.IsTrue(MusicPlayer.Queue.Contains(song2));

            // For Cleanup          
            CleanUp();
        }

        [TestMethod()]
        public void RemoveFromQueueTest()
        {
            // For Cleanup          
            CleanUp();

            string testName = "Test Playlist Name";
            string testPlaylistPath = MusicLibrary.Settings.PlaylistsFolder + testName + ".xml";
            Song song1 = new Song(testAudioLocation);
            Song song2 = new Song(testAudioLocationAlt);
            Playlist testPlaylist = new Playlist(testName);
            testPlaylist.Songs.Add(song1);
            testPlaylist.Songs.Add(song2);
            MusicPlayer.Queue.Clear();
            testPlaylist.AddToQueue();
            Assert.IsTrue(MusicPlayer.Queue.Count() == 2);
            // Now that it has been added, we can remove the playlist from the queue.
            testPlaylist.RemoveFromQueue();
            // the count should revert to zero now since both songs were from this playlist.
            Assert.IsTrue(MusicPlayer.Queue.Count() == 0);

            // Now we'll check with 2 different playlists.
            Playlist testPlaylist1 = new Playlist(testName + "1");
            Playlist testPlaylist2 = new Playlist(testName + "2");
            // Will add song1 to the first playlist, and song2 to the other.
            testPlaylist1.Songs.Add(song1);
            testPlaylist2.Songs.Add(song2);
            MusicPlayer.Queue.Clear();
            testPlaylist1.AddToQueue();
            testPlaylist2.AddToQueue();
            Assert.IsTrue(MusicPlayer.Queue.Count() == 2);
            // Now will remove the fist playlist from the queue. which means the 2nd playlist
            // should remain and the count should be 1.
            testPlaylist1.RemoveFromQueue();
            Assert.IsTrue(MusicPlayer.Queue.Count() == 1);
            Assert.IsTrue(MusicPlayer.Queue.Contains(song2));
            Assert.IsFalse(MusicPlayer.Queue.Contains(song1));

            // For Cleanup          
            CleanUp();
        }

        [TestMethod()]
        public void ExportPlaylistTest()
        {
            // For Cleanup          
            CleanUp();

            string testName = "Test Playlist Name";
            string testPlaylistPath = MusicLibrary.Settings.PlaylistsFolder + testName + ".xml";
            Song song1 = new Song(testAudioLocation);
            Song song2 = new Song(testAudioLocationAlt);
            Playlist testPlaylist = new Playlist(testName);
            testPlaylist.Songs.Add(song1);
            testPlaylist.Songs.Add(song2);
            // This method exports the playlist as an M3U8 file (basically .M3U but using UTF8) which most audio players can
            // recognize. There is also an Import method but that is not in this class, but in Playlists class (since a new
            // playlist should be constructed when importing one).
            string exportedPlaylistPath = testPlaylist.ExportPlaylist();
            Assert.IsTrue(System.IO.File.Exists(exportedPlaylistPath));
            string[] exportedPlaylist = System.IO.File.ReadAllLines(exportedPlaylistPath);
            // An .M3U/.M3U8 file basically should look like this:
            //#EXTM3U
            //#PlaylistName.m3u8
            //1stSongPath
            //2ndSongPath
            //etc

            // Based on this, the string[] should have 4 elements
            Assert.IsTrue(exportedPlaylist.Length == 4);
            // So first will check the first line
            Assert.AreEqual("#EXTM3U", exportedPlaylist[0]);
            // Then the second line
            Assert.AreEqual("#Test Playlist Name.m3u8", exportedPlaylist[1]);
            // From here on should be the locations of the playlist's songs, in this case there should be two
            Assert.AreEqual(song1.Path, exportedPlaylist[2]);
            Assert.AreEqual(song2.Path, exportedPlaylist[3]);

            // For Cleanup          
            CleanUp();
        }

        [TestMethod()]
        public void ReloadTest()
        {
            // Basically clears thei nternal list, and reads from the database again. There is no need for
            // a test.
            Assert.IsTrue(true);
        }

        [TestMethod()]
        public void PrepareSongsArtsTest()
        {
            // For Cleanup          
            CleanUp();

            string testName = "Test Playlist Name";
            string testPlaylistPath = MusicLibrary.Settings.PlaylistsFolder + testName + ".xml";
            Song song1 = new Song(testAudioLocation);
            Song song2 = new Song(testAudioLocationAlt);
            Playlist testPlaylist = new Playlist(testName);
            testPlaylist.Songs.Add(song1);
            testPlaylist.Songs.Add(song2);
            // By default the album arts of the songs are not saved when they are added, only the thumbnails. But
            // everytime the song.Art is called, the album art is extracted from the file and saved to the Temp folder
            // and is left there until deleted (either using song.UnloadArt(), playlist.UnloadSongArts() or KhiUtils.ClearTemporaryImages())
            testPlaylist.PrepareSongsArts();
            // The path should follow this pattern:
            // ...\Temp\AudioFileName.jpeg
            string song1FileName = System.IO.Path.GetFileNameWithoutExtension(testAudioLocation);
            string song2FileName = System.IO.Path.GetFileNameWithoutExtension(testAudioLocationAlt);
            string song1ArtPath = MusicLibrary.Settings.TempArtsFolder + song1FileName + ".jpeg";
            string song2ArtPath = MusicLibrary.Settings.TempArtsFolder + song2FileName + ".jpeg";
            Assert.IsTrue(System.IO.File.Exists(song1ArtPath));
            Assert.IsTrue(System.IO.File.Exists(song2ArtPath));
            Assert.IsNotNull(song1.Art);
            Assert.IsNotNull(song2.Art);

            // For Cleanup          
            CleanUp();
        }

        [TestMethod()]
        public void RemoveTest()
        {
            // For Cleanup          
            CleanUp();

            string testName = "Test Playlist Name";
            // This is where the playlist database should be after it's creation.
            string testPlaylistPath = MusicLibrary.Settings.PlaylistsFolder + testName + ".xml";
            Playlist testPlaylist = new Playlist(testName);
            // First will check if database exists
            Assert.IsTrue(System.IO.File.Exists(testPlaylistPath));
            testPlaylist.Remove();
            Assert.IsFalse(System.IO.File.Exists(testPlaylistPath));

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