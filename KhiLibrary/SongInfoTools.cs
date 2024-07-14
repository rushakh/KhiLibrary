using NAudio.CoreAudioApi;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Xml;
using ATL;

namespace KhiLibrary
{
    /// <summary>
    /// An assortment of tools for extraction and manipulation of an Audio file's 
    /// info or Image/Thumbnail.
    /// </summary>
    internal static class SongInfoTools
    {
        /// <summary>
        /// Contains methods for changing the audio file's embedded data.
        /// </summary>
        internal static class SongModifier
        {
            /// <summary>
            /// Use to embedd lyrics into the song. WILL NOT WORK in case the file is in use. Returns null in case of exception.
            /// </summary>
            /// <param name="newTitle"></param>
            /// <param name="audioPath"></param>
            /// <returns></returns>
            internal static string? SetTitle(string newTitle, string audioPath)
            {
                try
                {
                    ATL.Track? track = new ATL.Track(audioPath);
                    track.Title = newTitle;
                    track.Save();
                    track = null;
                    return newTitle;
                }
                catch
                {
                    return null;
                }
            }

            /// <summary>
            /// Use to embedd (overwrite) Artists into the song. WILL NOT WORK in case the file is in use. Returns null in case of exception.
            /// </summary>
            /// <param name="newArtist"></param>
            /// <param name="audioPath"></param>
            /// <returns></returns>
            internal static string? SetArtist(string newArtist, string audioPath)
            {
                try
                {
                    ATL.Track? track = new ATL.Track(audioPath);
                    track.Artist = newArtist;
                    track.Save();
                    track = null;
                    return newArtist;
                }
                catch
                { return null; }
            }

            /// <summary>
            /// Use to embedd (overwrite) Album into the song. WILL NOT WORK in case the file is in use. Returns null in case of exception.
            /// </summary>
            /// <param name="newAlbum"></param>
            /// <param name="audioPath"></param>
            /// <returns></returns>
            internal static string? SetAlbum(string newAlbum, string audioPath)
            {
                try
                {
                    ATL.Track? track = new ATL.Track(audioPath);
                    track.Album = newAlbum;
                    track.Save();
                    track = null;
                    return newAlbum;
                }
                catch
                {
                    return null;
                }
            }

            /// <summary>
            /// Use to embedd genres into the son. WILL NOT WORK in case the file is in use. Returns null in case of exception.
            /// </summary>
            /// <param name="newGenres"></param>
            /// <param name="audioPath"></param>
            /// <returns></returns>
            internal static string? SetGenres(string newGenres, string audioPath)
            {
                try
                {
                    ATL.Track? track = new ATL.Track(audioPath);
                    track.Genre = newGenres;
                    track.Save();
                    track = null;
                    return newGenres;
                }
                catch { return null; }
            }

            /// <summary>
            /// Use to embedd (overwrite) Track Number into the song. Returns null in case of exception.
            /// </summary>
            /// <param name="newTrackNumber"></param>
            /// <param name="audioPath"></param>
            /// <returns></returns>
            internal static int? SetTrackNumber(int newTrackNumber, string audioPath)
            {
                try
                {
                    ATL.Track? track = new ATL.Track(audioPath);
                    track.TrackNumber = newTrackNumber;
                    track.Save();
                    track = null;
                    return newTrackNumber;
                }
                catch { return null; }
            }

            /// <summary>
            /// Use to embedd lyrics into the song. WILL NOT WORK in case the file is in use. Returns the song's lyrics in case of success,
            /// returns empty string otherwise.
            /// </summary>
            /// <param name="newLyrics"></param>
            /// <param name="audioPath"></param>
            /// <returns></returns>
            internal static string SetLyrics(string newLyrics, string audioPath)
            {
                try
                {
                    ATL.Track? track = new ATL.Track(audioPath);
                    LyricsInfo lyrcInfo = new LyricsInfo();
                    track.Lyrics.Clear();
                    track.Lyrics.UnsynchronizedLyrics = newLyrics;
                    track.Save();
                    track = null;
                    return newLyrics;
                }
                catch
                {
                    return string.Empty;
                }
            }

            /// <summary>
            /// For Changing the Song's picture/cover art embedded within the file. Also changes the arts 
            /// saved in Album art and thumbnail folders.
            /// </summary>
            /// <param name="newImage"></param>
            /// <param name="audioPath"></param>
            /// <param name="thumbnailPath"></param>
            internal static void SetArt(Image newImage, string audioPath, string thumbnailPath)
            {
                try
                {
                    ATL.Track? track = new ATL.Track(audioPath);
                    ImageConverter converter = new ImageConverter();
                    byte[] imageData = (byte[])converter.ConvertTo(newImage, typeof(byte[]));
                    track.EmbeddedPictures.Clear();
                    var picInfo = ATL.PictureInfo.fromBinaryData(imageData);
                    track.EmbeddedPictures.Add(picInfo);
                    track.Save();
                    Image? tempThumbnail = newImage.GetThumbnailImage(60, 60, () => false, nint.Zero);
                    tempThumbnail.Save(thumbnailPath, System.Drawing.Imaging.ImageFormat.Png);
                    tempThumbnail.Dispose();
                    tempThumbnail = null;
                    track = null;
                    newImage.Dispose();
                }
                catch { }
            }
        }

        /// <summary>
        /// Contains methods for extracting embedded data and properties from an audio file.
        /// </summary>
        internal static class FetchSongInfo
        {
            /// <summary>
            /// Gets the prepared album art cover from the temp folder, extracts it if it's not there, or returns the default 
            /// image if there is not embedded image or in case of exception.
            /// </summary>
            /// <param name="artPath"></param>
            /// <returns></returns>
            internal static Image GetArt(string artPath)
            {
                try
                {
                    if (System.IO.File.Exists(artPath))
                    {
                        Image tempArt = Image.FromFile(artPath);
                        Bitmap art = new Bitmap(tempArt, tempArt.Size);
                        tempArt.Dispose();
                        return art;
                    }
                    else
                    {
                        return KhiLibrary.Resources.Khi_Player;
                    }
                }
                catch 
                { 
                    return KhiLibrary.Resources.Khi_Player;
                }
            }

            /// <summary>
            /// Use to get the embedded lyrics in the song.
            /// </summary>
            /// <param name="audioPath"></param>
            /// <returns></returns>
            internal static string GetLyrics(string audioPath)
            {
                try
                {
                    string? tempLyrics;
                    ATL.Track? track = new ATL.Track(audioPath);
                    tempLyrics = track.Lyrics.UnsynchronizedLyrics;
                    if (tempLyrics != string.Empty)
                    {
                        tempLyrics = tempLyrics.ReplaceLineEndings();
                    }
                    track = null;
                    return tempLyrics;
                }
                catch
                {
                    return string.Empty;
                }
            }

            /// <summary>
            /// Loads and returns the thumbnail. If it doesn't exist, or an exception is  thrown, returns the default thumbnail. 
            /// </summary>
            /// <param name="thumbnailPath"></param>
            /// <returns></returns>
            internal static Image GetThumbnail(string thumbnailPath)
            {
                try
                {
                    Image? tempThumbnail = Image.FromFile(thumbnailPath);
                    return tempThumbnail;
                }
                catch
                {
                    return Resources.Khi_Player_Thumbnail;
                }
            }

            /// <summary>
            /// Returns a dictionary with the song's metadata and embedded information. Returns null in case of exception.
            /// </summary>
            /// <returns></returns>
            internal static Dictionary<string, object?>? GetProperties(string audioPath)
            {
                try
                {
                    Dictionary<string, object?> properties = new Dictionary<string, object?>();
                    Track track = new Track(audioPath);
                    var format = track.AudioFormat.Name;
                    var bitRate = track.Bitrate;
                    var bitDepth = track.BitDepth;
                    var sampleRate = track.SampleRate;
                    var originalReleaseDate = track.OriginalReleaseDate;
                    var isVBR = track.IsVBR;
                    var encodedBy = track.EncodedBy;
                    var encoder = track.Encoder;
                    var productID = track.ProductId;
                    var Publisher = track.Publisher;
                    var audioSource = track.AudioSourceUrl;
                    var lyricist = track.Lyricist;
                    var copyRight = track.Copyright;
                    var description = track.Description;
                    var comment = track.Comment;
                    properties.Add("AudioSourceUri", audioSource);
                    properties.Add("Bitrate", bitRate);
                    properties.Add("BitDepth", bitDepth);
                    properties.Add("Comment", comment);
                    properties.Add("Copyright", copyRight);
                    properties.Add("Description", description);
                    properties.Add("Encoder", encoder);
                    properties.Add("EncodedBy", encodedBy);
                    properties.Add("Format", format);
                    properties.Add("IsVBR", isVBR);
                    properties.Add("Lyricist", lyricist);
                    properties.Add("OriginalReleaseDate", originalReleaseDate);
                    properties.Add("ProductId", productID);
                    properties.Add("Publisher", Publisher);
                    properties.Add("SampleRate", sampleRate);
                    return properties;
                }
                catch
                { return null; }
            }
        }

        /// <summary>
        /// Extracts the album art cover and if it exists, saves it to the temp folder. returns null if audio file does not contain 
        /// embedded image or in case of encountering exceptions.
        /// </summary>
        /// <param name="audioPath"></param>
        /// <returns></returns>
        internal static string? PrepareArt(string audioPath)
        {
            try
            {
                string fileName = System.IO.Path.GetFileNameWithoutExtension(audioPath);
                string artPath = InternalSettings.tempArtsFolder + fileName + ".jpeg";
                if (!System.IO.File.Exists(artPath))
                {
                    ATL.Settings.FileBufferSize = 10 * 1024;
                    ATL.Settings.UseFileNameWhenNoTitle = true;
                    ATL.Settings.ReadAllMetaFrames = false;
                    ATL.Settings.OutputStacktracesToConsole = false;

                    ATL.Track? track = new ATL.Track(audioPath);
                    if (track.EmbeddedPictures.Any() && track.EmbeddedPictures[0] is not null)
                    {
                        var pic = track.EmbeddedPictures[0];
                        if (pic.PictureData != null && pic.NativeFormat != Commons.ImageFormat.Unsupported)
                        {
                            MemoryStream memory = new MemoryStream(pic.PictureData, true);
                            Image? tempArt = Image.FromStream(memory);
                            tempArt.Save(artPath, System.Drawing.Imaging.ImageFormat.Jpeg);
                            tempArt.Dispose();
                            tempArt = null;
                            memory.Dispose();
                            track = null;
                            return artPath;
                        }
                        else 
                        {
                            track = null;
                            return null; 
                        }
                    }
                    else 
                    {
                        track = null;
                        return null;
                    }
                   
                }
                else { return artPath; }
            }
            catch { return null; }
        }

        /// <summary>
        /// Gets and returns an aido file's Info; title, artist, album, etc. Saves the art in case the application 
        /// is not in virtual mode or if this song is going to played immediatly after the creation of the Song object.
        /// </summary>
        /// <param name="audioFilePath"></param>
        /// <param name="willBePlayedImmediatly"></param>
        /// <returns></returns>
        internal static string[] GetInfo(string audioFilePath, bool willBePlayedImmediatly = false)
        {
            try
            {
                string title, artist, album, path, artPath, thumbnailPath, genres, duration, trackNumber;
                InternalSettings.CreateDirectories();
                // Settings
                ATL.Settings.FileBufferSize = 10 * 1024;
                //ATL.Settings.ID3v2_forceAPICEncodingToLatin1 = false;
                ATL.Settings.UseFileNameWhenNoTitle = true;
                ATL.Settings.ReadAllMetaFrames = false;
                ATL.Settings.OutputStacktracesToConsole = false;
                string fileNameWithoutExtention = Path.GetFileNameWithoutExtension(audioFilePath);
                ATL.Track track = new ATL.Track(audioFilePath);
                title = track.Title;
                artist = track.Artist;
                album = track.Album;
                path = track.Path;
                genres = track.Genre;
                var time = TimeSpan.FromMilliseconds(track.DurationMs);
                var tempTimeString = new TimeSpanConverter().ConvertToString(time);
                if (tempTimeString != null) { duration = tempTimeString; }
                else { duration = TimeSpan.Zero.ToString(); }
                var tempNum = track.TrackNumber;
                if (tempNum != null) { trackNumber = tempNum.ToString(); }
                else { trackNumber = string.Empty; }
                thumbnailPath = MakeThumbnailPaths(fileNameWithoutExtention, ".png");
                if (track.EmbeddedPictures.Any() && track.EmbeddedPictures[0] is not null)
                {
                    var pic = track.EmbeddedPictures[0];
                    if (pic.PictureData != null && pic.NativeFormat != Commons.ImageFormat.Unsupported)
                    {
                        MemoryStream? memory = new MemoryStream(pic.PictureData, true);
                        Image? art = Image.FromStream(memory);
                        Image? thumbnail = art.GetThumbnailImage(60, 60, () => false, nint.Zero);
                        thumbnail.Save(thumbnailPath, ImageFormat.Png);
                        thumbnail.Dispose();
                        thumbnail = null;
                        // Should save the album art itself as well if the application is not going to be used in
                        // virtual mode or if this song was chosen to be played immediatly from outside the application
                        // (e.g., Open With context menu). If it's going to be played immediatly, the album art should
                        // be ready so as not open the file again and create the image.
                        if (!InternalSettings.prepareForVirtualMode || willBePlayedImmediatly)
                        {
                            string fileName = System.IO.Path.GetFileNameWithoutExtension(path);
                            artPath = InternalSettings.tempArtsFolder + fileName + ".jpeg";
                            art.Save(artPath, ImageFormat.Jpeg);
                            art.Dispose();
                            art = null;
                        }                                                                   
                        memory.Dispose();
                        memory = null;
                    }
                }
                string[] info = [title, artist, album, path, thumbnailPath, genres, string.Empty, duration, trackNumber];
                return info;
            }
            catch
            {
                string[] info = new string[9];
                return info;
            }
        }

        /// <summary>
        /// Removes a song from the database and its associated pictures from the art and thumbnail folders.
        /// </summary>
        /// <param name="toBeRemovedSong"></param>
        /// <param name="playlistPath"></param>
        internal static async void RemoveSongInfoAndPics(Song toBeRemovedSong, string playlistPath)
        {
            await Task.Run(() =>
            {
                List<int> similarItemsIndices = [];
                XmlElement? AllSongs;  //the document root node
                XmlDocument? musicDatabase = new();

                musicDatabase.Load(playlistPath);
                if (musicDatabase.HasChildNodes)
                {
                    if (musicDatabase.DocumentElement?.HasChildNodes == true)
                    {
                        AllSongs = musicDatabase.DocumentElement;
                        XmlNodeList AllSongsElements = AllSongs.ChildNodes;
                        XmlNode? toBeRemovedSongElement;
                        for (int i = 0; i < AllSongs.ChildNodes.Count; i++)
                        {
                            XmlNode? removalCandidate = AllSongsElements[i];
                            if (removalCandidate != null && removalCandidate.HasChildNodes)
                            {
                                var childNode = removalCandidate.ChildNodes[3];
                                if (childNode != null && childNode.InnerText == toBeRemovedSong.Path)
                                    similarItemsIndices.Add(i);
                                toBeRemovedSongElement = AllSongs.ChildNodes[i];
                                // Just to get rid of the null reference warning
                                if (toBeRemovedSongElement != null)
                                {
                                    AllSongs.RemoveChild(toBeRemovedSongElement);
                                }
                                i--; // since the nodes will rearrange themselves, if the counter continues normally
                                     // it will skip an item, hence the need for this
                            }
                        }
                        // To remove the pics
                        try
                        {
                            //if (System.IO.File.Exists(toBeRemovedSong.ArtPath)) { System.IO.File.Delete(toBeRemovedSong.ArtPath); }
                            if (System.IO.File.Exists(toBeRemovedSong.ThumbnailPath)) { System.IO.File.Delete(toBeRemovedSong.ThumbnailPath); }
                        }
                        catch
                        {
                            // In Case it wasn't properly disposed of 
                            Task.Delay(3000);
                            GC.WaitForPendingFinalizers();
                            int gen = GC.MaxGeneration;
                            GC.Collect(gen, GCCollectionMode.Aggressive);
                            // Will try once again, and in case of exception, simply ignores the pictures, since they are loaded on demand
                            // and using the path included in the database. Since the song is removed from the database, removing the pictures
                            // is not a priority. 
                            try
                            {
                                //if (System.IO.File.Exists(toBeRemovedSong.ArtPath)) { System.IO.File.Delete(toBeRemovedSong.ArtPath); }
                                if (System.IO.File.Exists(toBeRemovedSong.ThumbnailPath)) { System.IO.File.Delete(toBeRemovedSong.ThumbnailPath); }
                            }
                            catch { }
                        }
                        musicDatabase.Save(playlistPath);
                    }
                }
            });
        }

        /// <summary>
        /// Creates Art and Thumbnail Paths with the specified image format and inclusion in the database. Returns the 
        /// Art Path first, then the Thumbnail Path.
        /// </summary>
        /// <param name="audioFileNameWithoutExtension"></param>
        /// <param name="formatWithDot"></param>
        /// <returns></returns>
        internal static string MakeThumbnailPaths(string audioFileNameWithoutExtension, string formatWithDot)
        {
            //string artPath = GlobalVariables.AlbumArtsPath + audioFileNameWithoutExtension + formatWithDot;
            string thumbnailPath = InternalSettings.albumArtsThumbnailsPath + audioFileNameWithoutExtension + formatWithDot;
            return thumbnailPath;
        }

        ///// <summary>
        ///// Contains methods for getting audio file info using alternative methods, TagLibSharp and ShellFile.
        ///// </summary>
        //internal class AlternativeInfos
        //{
        //    /// <summary>
        //    /// Gets and retuns the audio file's info, using an alternative method, Taglib
        //    /// </summary>
        //    /// <param name="audioFilePath"></param>
        //    /// <returns></returns>
        //    /// <exception cref="FileNotFoundException"></exception>
        //    internal static string[] GetInfo(string audioFilePath)
        //    {
        //        List<string> songInfo = new List<string>();
        //        string[] songInfoArray;
        //        if (System.IO.File.Exists(audioFilePath))
        //        {
        //            try
        //            {
        //                // Getting errors, should try making them syncronous for now and see if it gets fixed
        //                using (TagLib.File musicTags = TagLib.File.Create(audioFilePath, ReadStyle.Average))
        //                {
        //                    string songTitle, songArtist, songAlbum, songPath, songGenres, songLyrics, songArtPath, songThumbnailPath, songDurationString;
        //                    //TimeSpan songDuration;
        //                    songPath = audioFilePath;
        //                    // Getting the file name without extention, for the ImageHandler method
        //                    string fileName = System.IO.Path.GetFileNameWithoutExtension(audioFilePath);
        //                    songArtPath = Settings.AlbumArtsPath + fileName + ".bmp";
        //                    songThumbnailPath = Settings.AlbumArtsThumbnailsPath + fileName + ".bmp";
        //                    // For saving the cover art and the thumbnail, and getting their paths
        //                    ImageHandler(musicTags, fileName);
        //                    // For title
        //                    songTitle = GetTitle(musicTags, audioFilePath);
        //                    // For artists
        //                    songArtist = GetArtist(musicTags);
        //                    // For Album, if it is mentioned, returns it, otherwise, an empty string is returned.
        //                    if (musicTags.Tag.Album == null) { songAlbum = string.Empty; }
        //                    else { songAlbum = (string)musicTags.Tag.Album.Clone(); }
        //                    // For duration
        //                    TimeSpan time = musicTags.Properties.Duration.Duration();
        //                    TimeSpanConverter durationConverter = new TimeSpanConverter();
        //                    var tempDurString = durationConverter.ConvertToString(time);
        //                    if (tempDurString != null)
        //                    {
        //                        songDurationString = tempDurString.ToString();
        //                    }
        //                    else { songDurationString = string.Empty; }
        //                    // For genres
        //                    songGenres = GetGenres(musicTags);
        //                    // *** For lyrics: For now Commented Out --> they should be extracted as needed to cut back on memory usage
        //                    // var temp = musicTags.Tag.Lyrics;
        //                    //if (temp != null) { songLyrics = musicTags.Tag.Lyrics.ReplaceLineEndings(); }
        //                    //else { songLyrics = string.Empty; }                            

        //                    songInfo.Add(songTitle);
        //                    songInfo.Add(songArtist);
        //                    songInfo.Add(songAlbum);
        //                    songInfo.Add(songPath);
        //                    songInfo.Add(songArtPath);
        //                    songInfo.Add(songThumbnailPath);
        //                    songInfo.Add(songGenres);
        //                    songInfo.Add(string.Empty);
        //                    songInfo.Add(songDurationString);
        //                    songInfoArray = songInfo.ToArray();

        //                    musicTags.Dispose();
        //                }
        //                return songInfoArray;
        //            }
        //            catch
        //            {
        //                songInfoArray = GetInfoAlt(audioFilePath);
        //                return songInfoArray;
        //            }
        //        }
        //        // In case the file doesn't exist, the path was not correct etc, throws an exception
        //        else
        //        {
        //            throw new FileNotFoundException();
        //        }
        //    }

        //    /// <summary>
        //    /// Gets and retuns the audio file's info, using an alternative method (using ShellFile). 
        //    /// </summary>
        //    /// <param name="audioFilePath"></param>
        //    /// <returns></returns>
        //    /// <exception cref="FileLoadException"></exception>
        //    internal static string[] GetInfoAlt(string audioFilePath)
        //    {
        //        string songTitle, songArtist, songAlbum, songPath, songGenres, songLyrics, songArtPath, songThumbnailPath, songDurationString;
        //        List<string> songInfo = new List<string>();
        //        string[] songInfoArray;
        //        songPath = audioFilePath;
        //        using (ShellFile songShell = ShellFile.FromFilePath(audioFilePath))
        //        {
        //            if (songShell != null && songShell.Properties != null && songShell.Properties.System != null)
        //            {
        //                // Getting the file name without extention, for the ImageHandler
        //                // method or Title if no title is included in audio's properties
        //                string fileName = System.IO.Path.GetFileNameWithoutExtension(songPath);
        //                // For Title
        //                var tempT = songShell.Properties.System.Title;
        //                if (tempT != null)
        //                {
        //                    var tempTitle = tempT.Value;
        //                    if (tempTitle != null) { songTitle = tempTitle; }
        //                    else { songTitle = fileName; }
        //                }
        //                else { songTitle = fileName; }
        //                // For album art and thumbnail paths
        //                (songArtPath, songThumbnailPath) = MakeArtAndThumbnailPaths(fileName, ".bmp");
        //                // For extracting and Saving the Images
        //                Image art;
        //                var tempArt = songShell.Thumbnail;
        //                // For getting the Image and thumbnail
        //                if (tempArt != null)
        //                {
        //                    using (Image tempPic = tempArt.ExtraLargeBitmap)
        //                    {
        //                        art = new Bitmap(tempPic);
        //                    }
        //                }
        //                // If the ShellFile is null, uses the default KhiPlayer image and thumbnail.
        //                else
        //                {
        //                    art = Resources.Khi_Player;
        //                }
        //                // For saving the images                      
        //                ImageSaver(art, songArtPath);
        //                ThumbnailSaver(art, songThumbnailPath);
        //                art.Dispose();
        //                // For Artist
        //                var tempAr = songShell.Properties.System.Music.Artist;
        //                if (tempAr != null)
        //                {
        //                    var tempArtist = tempAr.Value;
        //                    if (tempArtist != null)
        //                    {
        //                        // If no artist is mentioned, returns an emoty string
        //                        if (tempArtist.Length == 0) { songArtist = string.Empty; }
        //                        // If only one artist is mentioned, returns that
        //                        else if (tempArtist.Length == 1)
        //                        {
        //                            songArtist = tempArtist[0];
        //                        }
        //                        // If several artists are mentioned, concats them all with a space in between
        //                        else
        //                        {
        //                            // Just to initilize the string, since it's easier this way
        //                            songArtist = string.Empty;
        //                            foreach (var oneArtist in tempArtist)
        //                            {
        //                                if (songArtist != string.Empty)
        //                                {
        //                                    songArtist += " " + (string)oneArtist.Clone();
        //                                }
        //                                else
        //                                {
        //                                    songArtist = (string)oneArtist.Clone();
        //                                }
        //                            }
        //                        }
        //                    }
        //                    else { songArtist = string.Empty; }
        //                }
        //                else { songArtist = string.Empty; }
        //                // For Album
        //                var TempA = songShell.Properties.System.Music.AlbumTitle;
        //                if (TempA != null)
        //                {
        //                    var tempAlbum = TempA.Value;
        //                    if (tempAlbum != null) { songAlbum = tempAlbum; }
        //                    else { songAlbum = string.Empty; }
        //                }
        //                else { songAlbum = string.Empty; }
        //                // For Duration
        //                var tempD = songShell.Properties.System.Media.Duration;
        //                if (tempD != null)
        //                {
        //                    var tempDObj = tempD.ValueAsObject;
        //                    if (tempDObj != null)
        //                    {
        //                        var tempDur = (ulong)tempDObj;
        //                        if (tempDur != 0)
        //                        {
        //                            TimeSpan time = TimeSpan.FromTicks((long)tempDur);
        //                            TimeSpanConverter durationConverter = new TimeSpanConverter();
        //                            var tempDurString = durationConverter.ConvertToString(time);
        //                            if (tempDurString != null)
        //                            {
        //                                songDurationString = tempDurString.ToString();
        //                            }
        //                            else
        //                            { songDurationString = string.Empty; }
        //                        }
        //                        else { songDurationString = string.Empty; }
        //                    }
        //                    else { songDurationString = string.Empty; }
        //                }
        //                else { songDurationString = string.Empty; }
        //                // For Genres
        //                var tempG = songShell.Properties.System.Music.Genre;
        //                if (tempG != null)
        //                {
        //                    var tempGenres = tempG.Value;
        //                    if (tempGenres != null)
        //                    {
        //                        // If no Genre is mentioned in the file returns an empty string.
        //                        if (tempGenres.Length == 0) { songGenres = string.Empty; }
        //                        // If only one genre is mentioned, returns that.
        //                        else if (tempGenres.Length == 1)
        //                        {
        //                            songGenres = tempGenres[0];
        //                        }
        //                        // If several genres are mentioned, concats them all with a space in between.
        //                        else
        //                        {
        //                            // just to initilize the string, easier this way.
        //                            songGenres = string.Empty;
        //                            foreach (string oneGenre in tempGenres)
        //                            {
        //                                // So that the first loop does not perform this
        //                                if (songGenres != string.Empty)
        //                                {
        //                                    songGenres += " " + (string)oneGenre.Clone();
        //                                }
        //                                else
        //                                {
        //                                    songGenres = (string)oneGenre.Clone();
        //                                }
        //                            }
        //                        }
        //                    }
        //                    else { songGenres = string.Empty; }
        //                }
        //                else { songGenres = string.Empty; }
        //                // *** For lyrics: For now Commented Out --> they should be extracted as needed to cut back on memory usage
        //                //var tempLyrics = songShell.Properties.System.Music.Lyrics.Value;
        //                //if (tempLyrics != null) { songLyrics = tempLyrics.ReplaceLineEndings(); }
        //                //else { songLyrics = string.Empty; }                        

        //                songInfo.Add(songTitle);
        //                songInfo.Add(songArtist);
        //                songInfo.Add(songAlbum);
        //                songInfo.Add(songPath);
        //                songInfo.Add(songArtPath);
        //                songInfo.Add(songThumbnailPath);
        //                songInfo.Add(songGenres);
        //                songInfo.Add(string.Empty);
        //                songInfo.Add(songDurationString);
        //                songInfoArray = songInfo.ToArray();
        //                songShell.Dispose();
        //            }
        //            else
        //            { throw new FileLoadException(); }
        //            return (songInfoArray);
        //        }
        //    }

        //    /// <summary>
        //    /// Extracts the title of the song.
        //    /// </summary>
        //    /// <param name="tags"></param>
        //    /// <param name="audioFilePath"></param>
        //    /// <returns></returns>
        //    private static string GetTitle(TagLib.File tags, string audioFilePath)
        //    {
        //        string tempTitle;
        //        if (tags.Tag.Title == null)
        //        {
        //            System.IO.FileInfo sth = new(audioFilePath);
        //            tempTitle = (string)sth.Name.Clone();
        //        }
        //        else
        //        {
        //            tempTitle = (string)tags.Tag.Title.Clone();
        //        }
        //        return tempTitle;
        //    }

        //    /// <summary>
        //    /// Extracts the Artist/Artists of the song.
        //    /// </summary>
        //    /// <param name="tags"></param>
        //    /// <returns></returns>
        //    internal static string GetArtist(TagLib.File tags)
        //    {
        //        string songArtist;
        //        var allArtists = tags.Tag.Performers;
        //        // If no artist is mentioned, returns an emoty string
        //        if (allArtists.Length == 0) { songArtist = string.Empty; }
        //        // If only one artist is mentioned, returns that
        //        else if (allArtists.Length == 1)
        //        {
        //            songArtist = (string)tags.Tag.FirstPerformer.Clone();
        //        }
        //        // If several artists are mentioned, concats them all with a space in between
        //        else
        //        {
        //            // Just to initilize the string, since it's easier this way
        //            songArtist = string.Empty;
        //            foreach (var oneArtist in allArtists)
        //            {
        //                if (songArtist != string.Empty)
        //                {
        //                    songArtist += " " + (string)oneArtist.Clone();
        //                }
        //                else
        //                {
        //                    songArtist = (string)oneArtist.Clone();
        //                }
        //            }
        //        }
        //        return songArtist;
        //    }

        //    /// <summary>
        //    /// Extracts the Genres of the song.
        //    /// </summary>
        //    /// <param name="tags"></param>
        //    /// <returns></returns>
        //    internal static string GetGenres(TagLib.File tags)
        //    {
        //        var allGenres = tags.Tag.Genres;
        //        string songGenres;
        //        // If no Genre is mentioned in the file returns an empty string.
        //        if (allGenres.Length == 0) { songGenres = string.Empty; }
        //        // If only one genre is mentioned, returns that.
        //        else if (allGenres.Length == 1)
        //        {
        //            songGenres = allGenres[0];
        //        }
        //        // If several genres are mentioned, concats them all with a space in between.
        //        else
        //        {
        //            // just to initilize the string, easier this way.
        //            songGenres = string.Empty;
        //            foreach (string oneGenre in allGenres)
        //            {
        //                // So that the first loop does not perform this
        //                if (songGenres != string.Empty)
        //                {
        //                    songGenres += " " + (string)oneGenre.Clone();
        //                }
        //                else
        //                {
        //                    songGenres = (string)oneGenre.Clone();
        //                }
        //            }
        //        }
        //        return songGenres;
        //    }

        //    /// <summary>
        //    /// Extracts and saves the song's cover art and its thumbnail to the Album art and thumbnail folders.
        //    /// </summary>
        //    /// <param name="embeddedImages"></param>
        //    /// <param name="audioFileNameWithoutExtension"></param>
        //    internal static void ImageHandler(IPicture[] embeddedImages, string audioFileNameWithoutExtension)
        //    {
        //        // To populate the object's artPath and ThumbnailPath that is used for loading the images
        //        string tempArtPath = Settings.AlbumArtsPath + audioFileNameWithoutExtension + ".bmp";
        //        string tempThumbnailPath = Settings.AlbumArtsThumbnailsPath + audioFileNameWithoutExtension + ".bmp";
        //        bool useDefaultImage = false;
        //        // Using local variables because there is no need to have the images in memory. It's better and less memory expensive
        //        // to load them when they are needed
        //        Image art;
        //        if (embeddedImages.Length > 0 && embeddedImages[0].Type != TagLib.PictureType.NotAPicture)
        //        {
        //            using (MemoryStream picConverter = new MemoryStream(embeddedImages[0].Data.Data))
        //            {
        //                if (picConverter != null && picConverter.CanRead)
        //                {
        //                    var tempArt = Image.FromStream(picConverter);
        //                    art = (Image)tempArt.Clone();
        //                    ImageSaver(art, tempArtPath);
        //                    ThumbnailSaver(art, tempThumbnailPath);
        //                    art.Dispose();
        //                    tempArt.Dispose();
        //                    picConverter.Dispose();
        //                }
        //                else
        //                {
        //                    useDefaultImage = true;
        //                }
        //            }
        //        }
        //        else
        //        {
        //            useDefaultImage = true;
        //        }
        //        if (useDefaultImage)
        //        {
        //            art = Resources.Khi_Player;
        //            Image thumbnail = Resources.Khi_Player_Thumbnail;
        //            ImageSaver(art, tempArtPath);
        //            ImageSaver(thumbnail, tempThumbnailPath);
        //            art.Dispose();
        //            thumbnail.Dispose();
        //        }
        //    }

        //    /// <summary>
        //    /// Extracts and saves the song's cover art and its thumbnail to the Album art and thumbnail folders.
        //    /// </summary>
        //    /// <param name="tags"></param>
        //    /// <param name="audioFileNameWithoutExtension"></param>
        //    internal static void ImageHandler(TagLib.File tags, string audioFileNameWithoutExtension)
        //    {
        //        IPicture[] embeddedImages = tags.Tag.Pictures;
        //        ImageHandler(embeddedImages, audioFileNameWithoutExtension);
        //    }

        //    /// <summary>
        //    /// Extracts and saves the song's cover art and its thumbnail to the Album art and thumbnail folders
        //    /// </summary>
        //    /// <param name="songShell"></param>
        //    /// <param name="audioFileNameWithoutExtension"></param>
        //    internal static void ImageHandler(ShellFile songShell, string audioFileNameWithoutExtension)
        //    {
        //        Image art;
        //        var tempArt = songShell.Thumbnail;
        //        // For getting the Image and thumbnail
        //        if (tempArt != null)
        //        {
        //            using (var tempbmp = new Bitmap(tempArt.Bitmap))
        //            {
        //                art = new Bitmap(tempbmp);
        //            }
        //        }
        //        // If the ShellFile is null, uses the default KhiPlayer image nad thumbnail.
        //        else
        //        {
        //            art = Resources.Khi_Player;
        //        }
        //        (string artPath, string thumbnailPath) = MakeArtAndThumbnailPaths(audioFileNameWithoutExtension, ".bmp");
        //        // For saving the images
        //        ImageSaver(art, artPath);
        //        ThumbnailSaver(art, thumbnailPath);
        //        art.Dispose();
        //    }

        //    /// <summary>
        //    /// Saves the Image to the the provided path.
        //    /// </summary>
        //    /// <param name="image"></param>
        //    /// <param name="imagePath"></param>
        //    internal static void ImageSaver(Image image, string imagePath)
        //    {
        //        // Checks if the AlbumArt and thumbnails directories exists, if not creates them.
        //        Settings.CreateDirectories();
        //        // Saving the Image to the provided location.
        //        using (FileStream artSaver = new(imagePath, FileMode.Create, FileAccess.Write))
        //        {
        //            try
        //            {
        //                image.Save(artSaver, image.RawFormat);
        //                artSaver.Dispose();
        //            }
        //            catch
        //            {
        //                image.Save(artSaver, System.Drawing.Imaging.ImageFormat.Bmp);
        //                artSaver.Dispose();
        //            }
        //        }
        //    }

        //    /// <summary>
        //    /// Creates a thumbnail of the image (60, 60) and saves it to the provided location.
        //    /// </summary>
        //    /// <param name="image"></param>
        //    /// <param name="thumbnailPath"></param>
        //    /// <param name="checkDirectory"></param>
        //    private static void ThumbnailSaver(Image image, string thumbnailPath, bool checkDirectory = false)
        //    {
        //        // Checks if the AlbumArt and thumbnails directories exists, if not creates them.
        //        if (checkDirectory)
        //        {
        //            Settings.CreateDirectories();
        //        }
        //        // Creates the Thumbnail
        //        Image thumbnail = image.GetThumbnailImage(60, 60, () => false, 0);
        //        // Saving the thumbnail to the provided location.
        //        using (FileStream thumbnailSaver = new(thumbnailPath, FileMode.Create, FileAccess.Write))
        //        {
        //            try
        //            {
        //                thumbnail.Save(thumbnailSaver, image.RawFormat);
        //                thumbnailSaver.Dispose();
        //                thumbnail.Dispose();
        //            }
        //            catch
        //            {
        //                thumbnail.Save(thumbnailSaver, System.Drawing.Imaging.ImageFormat.Bmp);
        //                thumbnailSaver.Dispose();
        //                thumbnail.Dispose();
        //            }
        //        }
        //    }
        //}
    }
}
