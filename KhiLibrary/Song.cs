using System.ComponentModel;
using System.Drawing;

namespace KhiLibrary
{
    /// <summary>
    /// Can be used to create an object containing an audio file's information (title, artist, album, location, duration, genres, lyrics, 
    /// art, thumbnail)
    /// </summary>
    public class Song
    {
        private string title;
        private string artist;
        private string album;
        private string path;
        private string artPath;
        private string thumbnailPath;
        private TimeSpan duration;
        private int trackNumber;
        private string genres;
        private string lyrics;
        private Image? art;
        //private Image? thumbnail;

        /// <summary>
        /// The title of the Song, returns the file name without extension if there is not title.
        /// </summary>
        public string Title
        {
            get
            {
                return title;
            }
            set
            {
                if (value is not null && value is string)
                {
                    var temp = SongInfoTools.SongModifier.SetTitle(value, path);
                    if (temp != null) { title = temp; }
                }
            }
        }

        /// <summary>
        /// The artist (singer or band name) of the song. returns an empty string if there is no artist mentioned in file.
        /// </summary>
        public string Artist
        {
            get
            {
                return artist;
            }
            set
            {
                if (value is not null && value is string)
                {
                    var temp = SongInfoTools.SongModifier.SetArtist(value, path);
                    if (temp != null) { artist = temp; }
                }
            }
        }

        /// <summary>
        /// The album that the song belongs to. returns an empty string if no album is mentioned in file.
        /// </summary>
        public string Album
        {
            get
            {
                return album;
            }
            set
            {
                if (value is not null && value is string)
                {
                    var temp = SongInfoTools.SongModifier.SetAlbum(value, path);
                    if (temp != null) { album = temp; }
                }
            }
        }

        /// <summary>
        /// The location of the song on your system. Can't be empty.
        /// </summary>
        public string Path { get => path; }

        /// <summary>
        /// The location of the cover art's thumbnail.
        /// </summary>
        public string ThumbnailPath { get => thumbnailPath; }

        /// <summary>
        /// The song's duration
        /// </summary>
        public TimeSpan Duration { get => duration; set => duration = value; }

        /// <summary>
        /// The genre or genres of the song. returns an empty string if no data is found.
        /// </summary>
        public string Genres
        {
            get
            {
                return genres;
            }
            set
            {
                if (value is not null && value is string)
                {
                    var temp = SongInfoTools.SongModifier.SetGenres(value, path);
                    if (temp != null) { genres = temp; }
                }
            }
        }

        /// <summary>
        /// The position of this song in its album.
        /// </summary>
        public int TrackNumber 
        { get => trackNumber; 
            set 
            { 
                var temp = SongInfoTools.SongModifier.SetTrackNumber(value, path);
                if (temp != null && temp != 0) { trackNumber = (int)temp; }
            } 
        }

        /// <summary>
        /// The embedded lyrics of the song. returns an empty string if none is found.
        /// </summary>
        public string Lyrics
        {
            get
            {
                if (lyrics == string.Empty || lyrics == null)
                {
                    lyrics = SongInfoTools.FetchSongInfo.GetLyrics(path);
                    return lyrics;
                }
                else
                { return lyrics; }
            }
            set
            {
                if (value is not null)
                {
                    var temp = SongInfoTools.SongModifier.SetLyrics(value, path);
                    if (temp != null) { lyrics = temp; }
                }
            }
        }

        /// <summary>
        /// The cover art of the song. returns Khi Player's logo if no image is found in the song.
        /// </summary>
        public Image Art
        {
            get 
            {
                if (!System.IO.File.Exists(artPath)) { PrepareArt(); }
                return SongInfoTools.FetchSongInfo.GetArt(artPath);
            }
            set
            {
                if (value is not null && value is Image)
                {
                    SongInfoTools.SongModifier.SetArt(value, path, thumbnailPath);
                }
            }
        }

        /// <summary>
        /// The thumbnail of the song. returns Khi Player's logo thumbnail if the song has no cover art.
        /// </summary>
        public Image Thumbnail { get => SongInfoTools.FetchSongInfo.GetThumbnail(thumbnailPath); }

        /// <summary>
        /// Returns a dictionary with the song's metadata and embedded information. Returns null in case of exception.
        /// </summary>
        public Dictionary<string, object?>? Properties 
        { 
            get 
            {
                return SongInfoTools.FetchSongInfo.GetProperties(path); 
            }
        }

        #region constructors
        /// <summary>
        /// An empty constructor to innitialize the fields
        /// </summary>
        public Song()
        {
            title = string.Empty;
            artist = string.Empty;
            album = string.Empty;
            path = string.Empty;
            artPath = string.Empty;
            thumbnailPath = string.Empty;
            duration = TimeSpan.Zero;
            genres = string.Empty;
            lyrics = string.Empty;
            trackNumber = 0;
        }

        /// <summary>
        /// Creates an instance of the Song class, using an audio file's path. set willBePlayedImmediatly to true 
        /// If this song is going to be played immediatly after its construction.
        /// </summary>
        /// <param name="audioFilePath"></param>
        /// <param name="willBePlayedImmediatly"></param>
        /// <exception cref="FileNotFoundException"></exception>
        public Song(string audioFilePath, bool willBePlayedImmediatly = false)
        {
            if (System.IO.File.Exists(audioFilePath))
            {
                string[]? tempInfo = SongInfoTools.GetInfo(audioFilePath, willBePlayedImmediatly);
                string[]? songInfo = tempInfo.ToArray();
                tempInfo = null;
                title = songInfo[0];
                artist = songInfo[1];
                album = songInfo[2];
                path = songInfo[3];
                string fileName = System.IO.Path.GetFileNameWithoutExtension(path);
                artPath = InternalSettings.tempArtsFolder + fileName + ".jpeg";
                thumbnailPath = songInfo[4];
                genres = songInfo[5];
                lyrics = songInfo[6];
                string temp = songInfo[7];
                if (temp != string.Empty)
                {
                    TimeSpanConverter timeConverter = new TimeSpanConverter();
                    var tempDur = timeConverter.ConvertFromString(temp);
                    if (tempDur != null) { duration = (TimeSpan)tempDur; }
                    else { duration = TimeSpan.Zero; }
                }
                else { duration = TimeSpan.Zero; }
                var tempNum = songInfo[8];
                if (tempNum != null)
                {
                    if (tempNum != string.Empty)
                    {
                        try
                        {
                            trackNumber = int.Parse(tempNum);
                        }
                        catch
                        { trackNumber = 0; }
                    }
                    else { trackNumber = 0; }
                }
                else { trackNumber = 0; }
                songInfo = null;
            }
            else
            { throw new FileNotFoundException("File at " + audioFilePath + "either could not be found"); }
        }

        /// <summary>
        /// Creates an instance of the Song class, using the provided information. Mainly used 
        /// to create Song objects based on database's info.
        /// </summary>
        /// <param name="songTitle"></param>
        /// <param name="songArtist"></param>
        /// <param name="songAlbum"></param>
        /// <param name="songPath"></param>
        /// <param name="songThumbnailPath"></param>
        /// <param name="songDuration"></param>
        /// <param name="songGenres"></param>
        /// <param name="songTrackNumber"></param>
        public Song(string songTitle, string songArtist, string songAlbum, string songPath,
                    string songThumbnailPath, TimeSpan songDuration, string songGenres, int songTrackNumber)
        {
            title = songTitle;
            artist = songArtist;
            album = songAlbum;
            path = songPath;
            string fileName = System.IO.Path.GetFileNameWithoutExtension(path);
            artPath = InternalSettings.tempArtsFolder + fileName + ".jpeg";
            thumbnailPath = songThumbnailPath;
            duration = songDuration;
            genres = songGenres;
            // Lyrics will be loaded from file
            lyrics = string.Empty;
            trackNumber = songTrackNumber;
        }
        #endregion

        /// <summary>
        /// Get or Set the song's properties
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public object? this[int index]
        {
            get
            {
                try
                {
                    if (index <= 10)
                    {
                        switch (index)
                        {
                            case 0:
                                return Title;
                            case 1:
                                return Artist;
                            case 2:
                                return Album;
                            case 3:
                                return Path;
                            case 4:
                                return ThumbnailPath;
                            case 5:
                                return duration;
                            case 6:
                                return Genres;
                            case 7:
                                return TrackNumber;
                            case 8:
                                return Lyrics;
                            case 9:
                                return Art;
                            case 10:
                                return Thumbnail;
                            default:
                                return null;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
                catch
                {
                    return null;
                }
            }
            set
            {
                if (value is not null && (value is string || value is TimeSpan || value is Image))
                {
                    if (index <= 10 && index >= 0)
                    {
                        switch (index)
                        {
                            case 0:
                                Title = (string)value;
                                break;
                            case 1:
                                Artist = (string)value;
                                break;
                            case 2:
                                Album = (string)value;
                                break;
                            case 6:
                                Genres = (string)value;
                                break;
                            case 7:
                                TrackNumber = (int)value;
                                break;
                            case 8:
                                Lyrics = (string)value;
                                break;
                            case 9:
                                Art = (Image)value;
                                break;
                        }
                    }
                    else
                    { throw new IndexOutOfRangeException(); }
                }
                else
                {
                    throw new ArgumentException("Value must be an instance of the Song class and not null.");
                }
            }
        }

        /// <summary>
        /// Adds this song to the playback queue.
        /// </summary>
        public void AddToQueue ()
        {
            MusicPlayer.Queue.AddToQueue(this);
        }

        /// <summary>
        /// Removes this song from the playback queue.
        /// </summary>
        public void RemoveFromQueue()
        {
            MusicPlayer.Queue.RemoveFromQueue(this);
        }

        /// <summary>
        /// Extracts and saves the embedded album art of the song to the temp folder. if it does not contain any or in case of exceptions, 
        /// the art that will be loaded at the time of use will be this application's default image.
        /// </summary>
        public void PrepareArt()
        {
            var tempArtPath = SongInfoTools.PrepareArt(path);
            if (tempArtPath != null) { artPath = tempArtPath; }
            else { artPath = string.Empty; }
        }

        /// <summary>
        /// Disposes of the loaded album art and optionally deletes it if it exists.
        /// </summary>
        /// <param name="deleteTempImage"></param>
        public void UnloadArt(bool deleteTempImage = false)
        {
            try
            {
                if (art != null)
                {
                    art.Dispose();
                    art = null;
                }
                if (deleteTempImage)
                {
                    if (System.IO.File.Exists(artPath))
                    {
                        System.IO.File.Delete(artPath);
                    }
                }
            }
            catch { }
        }
    }
}
