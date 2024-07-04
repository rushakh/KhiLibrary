using System.Drawing;
using TagLib;

namespace KhiLibrary
{
    /// <summary>
    /// An object containing an audio file's information (title, artist, album, location, duration, genres, lyrics, 
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
        private string genres;
        private string lyrics;
        //private Image? art;
        //private Image? thumbnail;
        // Should later add a method for changing playlist that employs the static addsong and remove song methods
        private int index;
        private static int count;

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
                    title = SetTitle(value);
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
                    artist = SetArtist(value);
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
                    album = SetAlbum(value);
                }
            }
        }

        /// <summary>
        /// The location of the song on your system. Can't be empty.
        /// </summary>
        public string Path { get => path; }

        /// <summary>
        /// The location of the cover art of the song.
        /// </summary>
        public string ArtPath { get => artPath; }

        /// <summary>
        /// The location of the cover art's thumbnail.
        /// </summary>
        public string ThumbnailPath { get => thumbnailPath; }

        /// <summary>
        /// The string representation of the song's duration
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
                    genres = SetGenres(value);
                }
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
                    lyrics = GetLyrics();
                    return lyrics;
                }
                else
                { return lyrics; }
            }
            set
            {
                if (value is not null)
                {
                    lyrics = SetLyrics(value);
                }
            }
        }

        /// <summary>
        /// The cover art of the song. returns Khi Player's logo if no image is found in the song.
        /// </summary>
        public Image Art
        {
            get { return GetImage(artPath); }
            set
            {
                if (value is not null && value is Image)
                { SetArt(value); }
            }
        }

        /// <summary>
        /// The thumbnail of the song. returns Khi Player's logo thumbnail if the song has no cover art.
        /// </summary>
        public Image Thumbnail { get => GetImage(thumbnailPath); }

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
            //art = null;
            //thumbnail = null;
            Indexer();
        }

        /// <summary>
        /// Creates an instance of the Song class, using an audio file's path.
        /// </summary>
        /// <param name="audioFilePath"></param>
        public Song(string audioFilePath)
        {
            //string[] songInfo;
            //ReadOnlyCollection<string> songInfo;
            (var songInfo, duration) = KhiUtils.SongInfoTools.GetInfo(audioFilePath);
            title = songInfo[0];
            artist = songInfo[1];
            album = songInfo[2];
            path = songInfo[3];
            artPath = songInfo[4];
            thumbnailPath = songInfo[5];
            // duration already got the value
            genres = songInfo[6];
            lyrics = songInfo[7];
            //Indexer();
        }

        /// <summary>
        /// Creates an instance of the Song class, using the provided information. Mainly used 
        /// to create Song objects based on database's info.
        /// </summary>
        /// <param name="songTitle"></param>
        /// <param name="songArtist"></param>
        /// <param name="songAlbum"></param>
        /// <param name="songPath"></param>
        /// <param name="songArtPath"></param>
        /// <param name="songThumbnailPath"></param>
        /// <param name="songDuration"></param>
        /// <param name="songGenres"></param>
        public Song(string songTitle, string songArtist, string songAlbum, string songPath,
                    string songArtPath, string songThumbnailPath, TimeSpan songDuration, string songGenres)
        {
            title = songTitle;
            artist = songArtist;
            album = songAlbum;
            path = songPath;
            artPath = songArtPath;
            thumbnailPath = songThumbnailPath;
            duration = songDuration;
            genres = songGenres;
            // Lyrics will be loaded from file
            lyrics = string.Empty;
        }
        #endregion

        /// <summary>
        /// Get or Set the song's properties
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public object this[int index]
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
                                return ArtPath;
                            case 5:
                                return ThumbnailPath;
                            case 6:
                                return duration;
                            case 7:
                                return Genres;
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
                            case 7:
                                Genres = (string)value;
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
        #region InnerWorkings
        private void Indexer(bool removed = false)
        {
            if (removed == false)
            {
                index = count;
                count++;
            }
            else
            {
                count--;
            }
        }

        /// <summary>
        /// Returns an Image that is loaded from the file in the provided location. 
        /// If the file doesn't exist, is in use, corrupted, or cannot be accessed, returns the default KhiPlayer logo.
        /// </summary>
        /// <param name="imagePath"></param>
        /// <returns></returns>
        private static Image GetImage(string imagePath)
        {
            try
            {
                Image artOrThumbnail;
                using (var tempbmp = new Bitmap(imagePath))
                {
                    artOrThumbnail = new Bitmap(tempbmp);
                }
                return artOrThumbnail;
            }
            catch
            {
                return Resources.Khi_Player;
            }
        }

        /// <summary>
        /// For Changing the Song's picture/cover art embedded within the file. Also changes the arts 
        /// saved in Album art and thumbnail folders.
        /// </summary>
        /// <param name="newImage"></param>
        private void SetArt(Image newImage)
        {
            try
            {
                using (TagLib.File musicTags = TagLib.File.Create(path))
                {
                    TagLib.File.IFileAbstraction imageAbs = (TagLib.File.IFileAbstraction)newImage;
                    var newByteVec = ByteVector.FromFile(imageAbs);
                    musicTags.Tag.Pictures[0].Data = newByteVec;
                    musicTags.Save();
                    System.IO.FileInfo fileInfo = new(path);
                    string fileName = fileInfo.Name.Split('.')[0];
                    // For reReading the File data and getting the new images (to test if it has changed and get the images at the same time).
                    var tempPics = musicTags.Tag.Pictures;
                    KhiUtils.SongInfoTools.ImageHandler(tempPics, fileName);
                    musicTags.Dispose();
                }
            }
            catch { }
        }

        /// <summary>
        /// Use to get the embedded lyrics in the song. 
        /// </summary>
        /// <returns></returns>
        private string GetLyrics()
        {
            string? tempLyrics;
            try
            {
                using (TagLib.File lyrictag = TagLib.File.Create(path, TagLib.ReadStyle.None))
                {
                    tempLyrics = lyrictag.Tag.Lyrics;                   
                    if (tempLyrics != null && tempLyrics != string.Empty)
                    { 
                        String copiedLyrics = new string(tempLyrics.ReplaceLineEndings());
                        return copiedLyrics;
                    }
                    else { return string.Empty; }
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Use to embedd lyrics into the song. WILL NOT WORK in case the file is in use. Returns the song's lyrics in case of success,
        /// returns empty string otherwise. 
        /// </summary>
        /// <param name="newLyrics"></param>
        /// <returns></returns>
        private string SetLyrics(string newLyrics)
        {
            try
            {
                using (TagLib.File lyrictag = TagLib.File.Create(path, TagLib.ReadStyle.None))
                {
                    lyrictag.Tag.Lyrics = newLyrics;
                    lyrictag.Save();
                    lyrictag.Dispose();
                    return newLyrics;
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Use to embedd lyrics into the song. WILL NOT WORK in case the file is in use.
        /// </summary>
        /// <param name="newTitle"></param>
        /// <returns></returns>
        private string SetTitle(string newTitle)
        {
            string titleBackup = title;
            try
            {
                using (TagLib.File titleTag = TagLib.File.Create(path, TagLib.ReadStyle.None))
                {
                    titleTag.Tag.Title = newTitle;
                    titleTag.Save();
                    titleTag.Dispose();
                    return newTitle;
                }
            }
            catch
            {
                return titleBackup;
            }
        }

        /// <summary>
        /// Use to embedd (overwrite) Artists into the song. WILL NOT WORK in case the file is in use.
        /// </summary>
        /// <param name="newArtist"></param>
        /// <returns></returns>
        private string SetArtist(string newArtist)
        {
            string[] tempArtist = [newArtist];
            string tempConcatedArtists = SetArtist(tempArtist);
            return tempConcatedArtists;
        }

        /// <summary>
        /// Use to embedd (overwrite) Artists into the song. WILL NOT WORK in case the file is in use.
        /// </summary>
        /// <param name="newArtists"></param>
        /// <returns></returns>
        private string SetArtist(string[] newArtists)
        {
            string artistsBackup = artist;
            if (newArtists != null && newArtists.Length > 0)
            {               
                string tempConcatedArtists;
                try
                {
                    using (TagLib.File artistsTag = TagLib.File.Create(path, TagLib.ReadStyle.None))
                    {
                        artistsTag.Tag.Performers = newArtists;
                        artistsTag.Save();
                        //tempConcatedArtists = KhiUtils.SongInfoTools.GetArtist(artistsTag).Result;
                        //return tempConcatedArtists;

                        if (newArtists.Length == 0) { tempConcatedArtists = string.Empty; }
                        // If only one artist is mentioned, returns that
                        else if (newArtists.Length == 1)
                        {
                            tempConcatedArtists = (string)artistsTag.Tag.FirstPerformer.Clone();
                        }
                        // If several artists are mentioned, concats them all with a space in between
                        else
                        {
                            // Just to initilize the string, since it's easier this way
                            tempConcatedArtists = string.Empty;
                            foreach (var oneArtist in newArtists)
                            {
                                if (tempConcatedArtists != string.Empty)
                                {
                                    tempConcatedArtists += " " + (string)oneArtist.Clone();
                                }
                                else
                                {
                                    tempConcatedArtists = (string)oneArtist.Clone();
                                }
                            }
                        }
                        String copiedArtists = new string (tempConcatedArtists);
                        artistsTag.Dispose();
                        return copiedArtists;
                    }
                }
                catch
                {
                    return artistsBackup;
                }
            }
            else
            { return artistsBackup; }
        }

        /// <summary>
        /// Use to embedd (overwrite) Album into the song. WILL NOT WORK in case the file is in use.
        /// </summary>
        /// <param name="newAlbum"></param>
        /// <returns></returns>
        private string SetAlbum(string newAlbum)
        {
            string albumBackup = album;
            try
            {
                using (TagLib.File albumTag = TagLib.File.Create(path, TagLib.ReadStyle.None))
                {
                    albumTag.Tag.Title = newAlbum;
                    albumTag.Save(); 
                    albumTag.Dispose();
                    return newAlbum;
                }
            }
            catch
            {
                return albumBackup;
            }
        }

        /// <summary>
        /// Use to embedd genres into the son. WILL NOT WORK in case the file is in use.
        /// </summary>
        /// <param name="newGenres"></param>
        /// <returns></returns>
        private string SetGenres(string newGenres)
        {
            string[] tempGenres = { newGenres };
            string tempConcatedGenres = SetGenres(tempGenres);
            return tempConcatedGenres;
        }

        /// <summary>
        /// Use to embedd genres into the son. WILL NOT WORK in case the file is in use.
        /// </summary>
        /// <param name="newGenres"></param>
        /// <returns></returns>
        private string SetGenres(string[] newGenres)
        {
            if (newGenres != null && newGenres.Length > 0)
            {
                string tempConcatedGenres;
                try
                {
                    using (TagLib.File genresTag = TagLib.File.Create(path, TagLib.ReadStyle.None))
                    {
                        genresTag.Tag.Genres = newGenres;
                        genresTag.Save();

                        if (newGenres.Length == 0) { tempConcatedGenres = string.Empty; }
                        // If only one artist is mentioned, returns that
                        else if (newGenres.Length == 1)
                        {
                            tempConcatedGenres = (string)genresTag.Tag.FirstPerformer.Clone();
                        }
                        // If several artists are mentioned, concats them all with a space in between
                        else
                        {
                            // Just to initilize the string, since it's easier this way
                            tempConcatedGenres = string.Empty;
                            foreach (var oneArtist in newGenres)
                            {
                                if (tempConcatedGenres != string.Empty)
                                {
                                    tempConcatedGenres += " " + (string)oneArtist.Clone();
                                }
                                else
                                {
                                    tempConcatedGenres = (string)oneArtist.Clone();
                                }
                            }
                        }
                        String copiedGenres = new string(tempConcatedGenres);
                        genresTag.Dispose();
                        return copiedGenres;
                    }
                }
                catch
                {
                    return string.Empty;
                }
            }
            else
            { return string.Empty; }
        }
        #endregion
    }
}
