using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Xml.Linq;
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
                        Bitmap art;
                        using (FileStream imageStream = new FileStream(artPath, FileMode.Open, FileAccess.Read))
                        {
                            Image tempArt = Image.FromStream(imageStream);
                            art = new Bitmap(tempArt);
                            tempArt.Dispose();
                            imageStream.Dispose();
                        }
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
                    Bitmap thumbnailBitmap;
                    using (FileStream imageStream = new FileStream(thumbnailPath, FileMode.Open, FileAccess.Read))
                    {
                        Image? tempThumbnail = Image.FromStream(imageStream);
                        thumbnailBitmap = new Bitmap(tempThumbnail);
                        tempThumbnail.Dispose();
                        imageStream.Dispose();
                    }
                    return thumbnailBitmap;
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
            // For Getting the duration
            var time = TimeSpan.FromMilliseconds(track.DurationMs);
            var tempTimeString = new TimeSpanConverter().ConvertToString(time);
            if (tempTimeString != null) { duration = tempTimeString; }
            else { duration = TimeSpan.Zero.ToString(); }
            // For getting the track number
            var tempNum = track.TrackNumber;
            if (tempNum != null) 
            {
                int num = (int)tempNum;
                trackNumber = num.ToString();
            }
            else { trackNumber = string.Empty; }
            // For getting art and thumbnail
            thumbnailPath = MakeThumbnailPaths(fileNameWithoutExtention, ".png");
            if (track.EmbeddedPictures.Any() && track.EmbeddedPictures[0] is not null)
            {
                var pic = track.EmbeddedPictures[0];
                if (pic.PictureData != null && pic.NativeFormat != Commons.ImageFormat.Unsupported)
                {
                    MemoryStream? memory = new MemoryStream(pic.PictureData, true);
                    Image? art = Image.FromStream(memory);
                    Image? thumbnail = art.GetThumbnailImage(60, 60, () => false, nint.Zero);
                    // To check if the thumbnail already exists (e.g., if the song is a duplicate)
                    // If it exists, should check if it's in use. If it's in use, will ignore it otherwise it will be replaced.
                    if (System.IO.File.Exists(thumbnailPath))
                    {
                        if (!KhiUtils.IsFileLocked(thumbnailPath))
                        {
                            thumbnail.Save(thumbnailPath, ImageFormat.Png);
                        }
                    }
                    else
                    {
                        thumbnail.Save(thumbnailPath, ImageFormat.Png);
                    }
                    thumbnail.Dispose();
                    thumbnail = null;
                    // Should save the album art itself as well if the application is not going to be used in
                    // virtual mode or if this song was chosen to be played immediatly from outside the application
                    // (e.g., Open With context menu). If it's going to be played immediatly, the album art should
                    // be ready so as not open the file again and create the image.
                    if (!InternalSettings.prepareForVirtualMode || willBePlayedImmediatly)
                    {
                        artPath = InternalSettings.tempArtsFolder + fileNameWithoutExtention + ".jpeg";
                        if (System.IO.File.Exists(artPath))
                        {
                            if (!KhiUtils.IsFileLocked(artPath))
                            {
                                art.Save(artPath, ImageFormat.Jpeg);
                            }
                        }
                        else
                        {
                            art.Save(artPath, ImageFormat.Jpeg);
                        }
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

        /// <summary>
        /// Removes a song from the database and its associated pictures from the art and thumbnail folders.
        /// </summary>
        /// <param name="toBeRemovedSong"></param>
        /// <param name="playlistPath"></param>
        internal static async void RemoveSongInfoAndPics(Song toBeRemovedSong, string playlistPath)
        {
            await Task.Run(() =>
            {
                string fileName = System.IO.Path.GetFileNameWithoutExtension(toBeRemovedSong.Path);
                string artPath = InternalSettings.tempArtsFolder + fileName + ".jpeg";
                string songTitle = toBeRemovedSong.Title;
                string songArtist = toBeRemovedSong.Artist;
                string songAlbum = toBeRemovedSong.Album;
                string songPath = toBeRemovedSong.Path;

                List<XElement> removalCandidates = new List<XElement>();
                XDocument playlistDatabase = XDocument.Load(playlistPath);
                if (playlistDatabase.Root != null)
                {
                    XElement playlistSongs = playlistDatabase.Root;
                    // Since duplicates might have been added to the database at some point
                    // if the settings were changed, more than one entry for this song might exist.
                    foreach (XElement songElement in playlistSongs.Elements())
                    {
                        if (songElement.Element("Title")?.Value == songTitle &&
                            songElement.Element("Artist")?.Value == songTitle &&
                            songElement.Element("Album")?.Value == songTitle &&
                            songElement.Element("Path")?.Value == songTitle)
                        {
                            removalCandidates.Add(songElement);
                        }
                    }
                    foreach (XElement toRemovedElement in removalCandidates)
                    {
                        toRemovedElement.Remove();
                    }
                }
                PlaylistTools.DatabaseTools.XmlWritingTool(playlistDatabase, playlistPath);
                // To remove the pics
                try
                {
                    if (System.IO.File.Exists(artPath)) { System.IO.File.Delete(artPath); }
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
                        if (System.IO.File.Exists(artPath)) { System.IO.File.Delete(artPath); }
                        if (System.IO.File.Exists(toBeRemovedSong.ThumbnailPath)) { System.IO.File.Delete(toBeRemovedSong.ThumbnailPath); }
                    }
                    catch { }
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
            string thumbnailPath = InternalSettings.albumArtsThumbnailsPath + audioFileNameWithoutExtension + formatWithDot;
            return thumbnailPath;
        }
    }
}
