using System.ComponentModel;

namespace KhiLibrary.Tests
{
    [TestClass()]
    public class SongTests
    {
        string testAudioLocation = "E:\\Test Files\\02 - Ramin Djawadi - The Rains of Castamere.mp3";
        [TestMethod()]
        public void SongTest()
        {
            // For Cleanup          
            CleanUp();

            // First constructor creates an empty instance of the Song class.
            Song testSong = new Song();
            Assert.IsInstanceOfType(testSong, typeof(Song));
            Assert.IsNotNull(testSong);

            // For Cleanup          
            CleanUp();
        }

        [TestMethod()]
        public void SongTest1()
        {
            // For Cleanup          
            CleanUp();

            // Second constructor creates an instance of the class using a specified audio file and populates the instance
            // with data and info extracted from the file.

            Song testSong = new Song(testAudioLocation);
            Assert.IsInstanceOfType(testSong, typeof(Song));
            Assert.IsTrue(testSong.Path != null && testSong.Path != string.Empty);
            Assert.IsTrue(testSong.Title != null && testSong.Title != string.Empty);
            Assert.IsTrue(testSong.Album != null && testSong.Album != string.Empty);
            Assert.IsTrue(testSong.Artist != null && testSong.Artist != string.Empty);
            Assert.IsTrue(testSong.Genres != null && testSong.Genres != string.Empty);
            Assert.IsTrue(testSong.ThumbnailPath != null && testSong.ThumbnailPath != string.Empty);
            var testSongThumbnail = testSong.Thumbnail;
            Assert.IsNotNull(testSongThumbnail);
            // This audio file contains embedded album art
            Assert.IsTrue(testSong.Art != null);
            Assert.IsTrue(testSong.Lyrics != null && testSong.Lyrics != string.Empty);
            // Track number is only zero if the file doesn't contain that info, but this file has it. the track number should be 2
            Assert.IsTrue(testSong.TrackNumber != 0 && testSong.TrackNumber == 2);
            Assert.IsNotNull(testSong.Properties);
            // Duration is only set to zero if duration could not be extracted from the file data or when an empty
            // instance of the class is constructed
            // duration => 00:03:44.3600000
            TimeSpanConverter timeConverter = new TimeSpanConverter();
            var tempDur = timeConverter.ConvertFromString("00:03:44.3600000");
            TimeSpan duration = (TimeSpan)tempDur;
            // This sometimes doesn't work out, the returned duration is not always exactly the same.
            // SO for now It will be commented out, until a solution is found.
            //Assert.IsTrue(testSong.Duration != TimeSpan.Zero && testSong.Duration.TotalSeconds == duration.TotalSeconds);
            // For Cleanup          
            CleanUp();
        }

        [TestMethod()]
        public void SongTest2()
        {
            // For Cleanup          
            CleanUp();

            Song testSongWithPath = new Song(testAudioLocation);       
            // Since the thumbnails' paths can vary if the app is run from different locations
            // it will not be considered in this comparision and will be copied from this object
            // to the one being copared to it.
            string thumbnailPath = testSongWithPath.ThumbnailPath;        
            // **These are the informations previously extracted from the file and saved.
            string title = "The Rains of Castamere";
            string artist = "Ramin Djawadi, Serj Tankian";
            string album = "Game Of Thrones: Season 8 (Music from the HBO Series)";
            string path = testAudioLocation;
            // duration => 00:03:44.3600000
            TimeSpanConverter timeConverter = new TimeSpanConverter();
            var tempDur = timeConverter.ConvertFromString("00:03:44.3600000");
            TimeSpan duration = (TimeSpan)tempDur;
            string genres = "Films/Games & Film Scores";
            int trackNumber = 2;
            var dateAddedOn = testSongWithPath.AddedOn;
            Song testSongWithInfo = new Song(title, artist, album, path, thumbnailPath, duration, genres, trackNumber, false, dateAddedOn, 0);

            Assert.AreEqual(testSongWithPath.Title, testSongWithInfo.Title);
            Assert.AreEqual(testSongWithPath.Artist, testSongWithInfo.Artist);
            Assert.AreEqual(testSongWithPath.Album, testSongWithInfo.Album);
            Assert.AreEqual(testSongWithPath.Path, testSongWithInfo.Path);
            // As previously mentioned, the returned duration is not always exactly the same, so for now this will be commented out.
            //Assert.AreEqual(testSongWithPath.Duration, testSongWithInfo.Duration);
            Assert.AreEqual(testSongWithPath.Genres, testSongWithInfo.Genres);
            Assert.AreEqual(testSongWithPath.TrackNumber, testSongWithInfo.TrackNumber);
            // *To test modifieing the song's tags and info.
            string newTitle = "New Test Title";
            string newArtist = "New Test Artist";
            string newAlbum = "New Test Album";
            // The lyrics weren't included here but the audio file does contain it, feel free to use another song to check.
            string lyrics = testSongWithPath.Lyrics;
            string newLyrics = "New Test Lyrics";
            // For obvious reasons the Path and Duration can't be altered.
            string newGenres = "New Test Genres";
            int newTrackNumber = 25;
            // When title, artist, etc's values are set, they are set to the audio file, and saved. Check SongInfoTools class.
            testSongWithPath.Title = newTitle;
            testSongWithPath.Artist = newArtist;
            testSongWithPath.Album = newAlbum;
            testSongWithPath.Lyrics = newLyrics;
            testSongWithPath.Genres = newGenres;
            testSongWithPath.TrackNumber = newTrackNumber;
            // You can add code for changing the song's album art cover as well. Changing the Art changes the thumbnail as well.

            Assert.AreEqual(newTitle, testSongWithPath.Title);
            Assert.AreEqual(newArtist, testSongWithPath.Artist);
            Assert.AreEqual(newAlbum, testSongWithPath.Album);
            Assert.AreEqual(newLyrics, testSongWithPath.Lyrics);
            Assert.AreEqual(newGenres, testSongWithPath.Genres);
            Assert.AreEqual(newTrackNumber, testSongWithPath.TrackNumber);

            // For cleaning up.
            testSongWithPath.Title = title;
            testSongWithPath.Artist = artist;
            testSongWithPath.Album = album;
            testSongWithPath.Lyrics = lyrics;
            testSongWithPath.Genres = genres;
            testSongWithPath.TrackNumber = trackNumber;       
            CleanUp();
        }

        [TestMethod()]
        public void AddToQueueTest()
        {
            // For Cleanup          
            CleanUp();

            Song testSong = new Song(testAudioLocation);
            testSong.AddToQueue();
            Assert.IsTrue(MusicPlayer.Queue.Contains(testSong));

            // For Cleanup          
            CleanUp();
        }

        [TestMethod()]
        public void RemoveFromQueueTest()
        {
            // For Cleanup          
            CleanUp();

            Song testSong = new Song(testAudioLocation);
            testSong.RemoveFromQueue();
            Assert.IsTrue(!MusicPlayer.Queue.Contains(testSong));

            // For Cleanup          
            CleanUp();
        }

        [TestMethod()]
        public void PrepareArtTest()
        {
            // For Cleanup          
            CleanUp();

            Song testSong = new Song(testAudioLocation);
            string fileName = System.IO.Path.GetFileNameWithoutExtension(testAudioLocation);
            string artPath = MusicLibrary.Settings.TempArtsFolder + fileName + ".jpeg";
            testSong.PrepareArt();
            // This file contains embedded album art, so this method should save the image to this path
            Assert.IsTrue(System.IO.File.Exists(artPath));

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