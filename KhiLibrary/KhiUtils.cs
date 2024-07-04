using Microsoft.WindowsAPICodePack.Shell;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq.Expressions;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using TagLib;
using TagLib.Matroska;

namespace KhiLibrary
{
    /// <summary>
    /// Contains the methods needed to process audio files' data, tools for playlists and data filtering
    /// </summary>
    public class KhiUtils
    {
        private static readonly string applicationPath = Application.StartupPath;
        private static readonly string allMusicDataBase = applicationPath + "AllMusicDataBase.xml";
        private static readonly string PlaylistsRecord = applicationPath + "PlaylistsRecord.xml";
        private static readonly string albumArtsPath = applicationPath + "\\Album Arts\\";
        private static readonly string albumArtsThumbnailsPath = applicationPath + "Album Arts Thumbnails\\";
        
        /// <summary>
        /// Contains methods used by Playlist class and up for reading, writing, and editing playlists.
        /// </summary>
        public class PlaylistTools
        {
            /// <summary>
            /// Checks if a playlist with the name already exists, returns true if there is 
            /// and false if the there is no playlist with this name.  
            /// </summary>
            /// <param name="name"></param>
            /// <returns></returns>
            public static bool PlaylistAlreadyExists(string name)
            {
                bool alreadyExists = false;
                var allPlaylistsDic = ReadPlaylistsRecords();
                if (allPlaylistsDic != null)
                {
                    if (allPlaylistsDic.ContainsKey(name))
                    {
                        alreadyExists = true;
                    }
                }
                return alreadyExists;
            }

            /// <summary>
            /// Checks if the chosen playlist name follows the guidelines and that a playlist with 
            /// the name does not exist.
            /// </summary>
            /// <param name="playlistName"></param>
            /// <returns></returns>
            public static bool IsAcceptablePlaylistName(string playlistName)
            {
                bool isAcceptable = true;
                char[] unacceptableChars = { '/', '\\', ':', '*', '?', '\"', '<', '>', '|' };
                foreach (char c in unacceptableChars)
                {
                    if (playlistName.Contains(c))
                    {
                        isAcceptable = false;
                        break;
                    }
                }
                // If there already exists a database by this name, returns false
                if (PlaylistAlreadyExists(playlistName)) { isAcceptable = false; }
                return isAcceptable;
            }

            /// <summary>
            /// Writes the name and path of a playlist to an xml file, to be accessed later.
            /// </summary>
            /// <param name="playlistName"></param>
            /// <param name="playlistPath"></param>
            public static void PlaylistRecorder(string playlistName, string playlistPath)
            {
                XDocument records;
                XElement playlists;
                if (System.IO.File.Exists(PlaylistsRecord))
                {
                    records = XDocument.Load(PlaylistsRecord);
                }
                else
                {
                    records = new XDocument();
                }
                // Checking if there is any root element, creating it if there is not
                if (records.Root != null)
                {
                    playlists = records.Root;
                }
                else
                {
                    playlists = new XElement("playlists");
                    records.Add(playlists);
                }
                // First will have to check if it already exists, if it doesn't will add it, if it does, will do nothing.
                if (GetPlaylistPath(playlistName) == null)
                {
                    // Creating the elements
                    // Each element's will contain a child, the playlistPath
                    // The elements name will be the same as the playlist's name
                    XElement newPlaylist = new(playlistName, playlistPath);
                    // We then Add it to the root document
                    playlists.Add(newPlaylist);
                    // Saves the Document
                    XmlWritingTool(records, PlaylistsRecord);
                }
                
            }

            /// <summary>
            /// Returns a Dictionary<string,string>. for each element, the key is the playlistName, while the value is the playlistPath.
            /// </summary>
            /// <returns></returns>
            public static Dictionary<string, string>? ReadPlaylistsRecords()
            {
                Dictionary<string, string> playlistsDic = [];
                if (System.IO.File.Exists(PlaylistsRecord))
                {
                    XDocument records = XDocument.Load(PlaylistsRecord);
                    XElement playlists;
                    if (records.Root != null)
                    {
                        playlists = records.Root;
                        if (playlists.HasElements)
                        {
                            // Will add all of the elements to a dictionary, the element name is the playlistName, while its child is the playlistPath
                            var tempAllPlaylists = playlists.Elements();
                            foreach (XElement playlist in tempAllPlaylists)
                            {
                                try
                                {
                                    playlistsDic.Add(playlist.Name.LocalName, playlist.Value);
                                }
                                catch { }
                            }                            
                        }
                        else { playlistsDic = null; }
                    }
                    // If there is no root element, then there are no playlists, returns null
                    else { playlistsDic = null; }
                }
                else { playlistsDic = null; }
                return playlistsDic;
            }

            /// <summary>
            /// Finds and returns the path of a playlist's database using its name. returns null 
            /// string if a playlist with that name does not not exist.
            /// </summary>
            /// <param name="playlistName"></param>
            /// <returns></returns>
            public static string? GetPlaylistPath(string playlistName)
            {
                string playlistPath;
                var playlistsDic = ReadPlaylistsRecords();
                if (playlistsDic != null)
                {
                    // In case of exception, since it can happen due to incorrect name, returns null
                    try
                    {
                        var tempPath = playlistsDic.GetValueOrDefault(playlistName);
                        if (tempPath != null)
                        {
                            playlistPath = tempPath;
                            return playlistPath;
                        }
                        else { return null; }
                    }
                    catch { return null; }
                }
                else { return null; }
            }

            /// <summary>
            /// Creates or Opens an Xml Document from/in the provided path, with the mentioned playlist name included 
            /// and creates xml elements based on the selected songs' propertiest. 
            /// </summary>
            /// <param name="playlistPath"></param>
            /// <param name="nameOfPlaylist"></param>
            /// <param name="songsList"></param>
            /// <returns></returns>
            internal static XDocument DatabaseElementCreator(string playlistPath, string nameOfPlaylist, List<Song> songsList)
            {
                // The document root node
                XDocument playlistDatabase;
                XElement playlistSongs;
                if (System.IO.File.Exists(playlistPath))
                {
                    playlistDatabase = XDocument.Load(playlistPath);
                    if (playlistDatabase.Root != null)
                    {
                        playlistSongs = playlistDatabase.Root;
                        if (playlistSongs.HasAttributes)
                        {
                            playlistSongs.SetAttributeValue("lastUpdated", DateTime.Now);
                        }
                    }
                    else
                    {
                        playlistSongs = new XElement(nameOfPlaylist);
                        playlistSongs.SetAttributeValue("playlist", nameOfPlaylist);
                        playlistSongs.SetAttributeValue("creationDate", DateTime.Now);
                        playlistSongs.SetAttributeValue("lastUpdated", DateTime.Now);
                        playlistDatabase.Add(playlistSongs);
                    }
                }
                else
                {
                    playlistDatabase = new XDocument();
                    playlistSongs = new XElement(nameOfPlaylist);
                    playlistSongs.SetAttributeValue("playlist", nameOfPlaylist);
                    playlistSongs.SetAttributeValue("lastUpdated", DateTime.Now);
                    playlistDatabase.Add(playlistSongs);
                }
                if (songsList != null && songsList.Count >0)
                {
                    foreach (Song song in songsList)
                    {
                        XElement Song = new("Song");

                        XElement Title = new("Title", song.Title);
                        XElement Artist = new("Artist", song.Artist);
                        XElement Album = new("Album", song.Album);
                        XElement Path = new("Path", song.Path);
                        XElement ArtPath = new("ArtPath", song.ArtPath);
                        XElement ThumbnailPath = new("ThumbnailPath", song.ThumbnailPath);
                        XElement Duration = new("Duration", song.Duration);
                        XElement Genres = new("Genres", song.Genres);     
                        Song.Add(Title);
                        Song.Add(Artist);
                        Song.Add(Album);
                        Song.Add(Path);
                        Song.Add(ArtPath);
                        Song.Add(ThumbnailPath);
                        Song.Add(Duration);
                        Song.Add(Genres);

                        playlistSongs.Add(Song);
                    }
                }
                return playlistDatabase;
            }

            /// <summary>
            /// Saves the xml document to the specified path (Turned it into a method to avoid repetition). 
            /// </summary>
            /// <param name="playlistDatabase"></param>
            /// <param name="playlistPath"></param>
            internal static bool XmlWritingTool(XDocument playlistDatabase, string playlistPath)
            {
                bool isFinished = false;
                FileStreamOptions options = new()
                {
                    Options = FileOptions.None,
                    Access = FileAccess.ReadWrite,
                    Share = FileShare.ReadWrite,
                    Mode = FileMode.OpenOrCreate,
                    BufferSize = 4096                    
                };
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
                using (StreamWriter datastream = new(playlistPath, Encoding.UTF8, options))
                {
                    XmlWriter dataBaseWriter = XmlWriter.Create(datastream, settings);
                    playlistDatabase.Save(dataBaseWriter);
                    dataBaseWriter.Close();
                    dataBaseWriter.Dispose();
                    datastream.Dispose();
                    isFinished = true;
                }
                return isFinished;
            }

            /// <summary>
            /// Returns a Dictionary<string, string> comprised of playlist names as the keys, and playlist paths as the value. 
            /// Returns null if no playlist is found or an exception is thrown. 
            /// </summary>
            /// <returns></returns>
            internal static Dictionary<string, string>? PlaylistDatabaseFinder()
            {
                Dictionary<string, string> playlistNameAndPath = [];
                XmlDocument? musicDatabase = new();
                // The document root node
                XmlElement? playlistSongs;
                // Finds all of the xml files in the application directory
                string[] tempExistingDatabases = System.IO.Directory.GetFiles(applicationPath, "*.xml*", SearchOption.TopDirectoryOnly);

                if (tempExistingDatabases.Length > 0)
                {
                    // For each of the data bases found, gets the playlist name from the xml document, the name is the value of
                    // the "playlistName" attribute.
                    foreach (string database in tempExistingDatabases)
                    {
                        // Just to make sure
                        if (System.IO.Path.GetExtension(database).ToUpper() == ".XML")
                        {
                            musicDatabase.LoadXml(database);
                            if (musicDatabase.DocumentElement != null)
                            {
                                playlistSongs = musicDatabase.DocumentElement;
                                if (playlistSongs.HasAttributes)
                                {
                                    string playlistName = playlistSongs.GetAttribute("playlistName");
                                    if (playlistName != string.Empty)
                                    {
                                        playlistNameAndPath.Add(playlistName, database);
                                    }
                                }
                            }
                        }
                    }
                    return playlistNameAndPath;
                }
                else
                {
                    return null;
                }
            }

            /// <summary>
            /// Writes a song to the playlist's database.
            /// </summary>
            /// <param name="playlistPath"></param>
            /// <param name="nameOfPlaylist"></param>
            /// <param name="song"></param>
            public async static void SongWriter(string playlistPath, string nameOfPlaylist, Song song)
            {
                await Task.Run(() =>
                {
                    List<Song> tempSong = [song];
                    XDocument playlistDatabase = KhiUtils.PlaylistTools.DatabaseElementCreator(playlistPath, nameOfPlaylist, tempSong);
                    KhiUtils.PlaylistTools.XmlWritingTool(playlistDatabase, playlistPath);
                });
            }

            /// <summary>
            /// Writes several songs to the playlist's database.
            /// </summary>
            /// <param name="playlistPath"></param>
            /// <param name="nameOfPlaylist"></param>
            /// <param name="songsInPlaylist"></param>
            public static bool PlaylistWriter(string playlistPath, string nameOfPlaylist, List<Song> songsInPlaylist)
            {
                XDocument playlistDatabase = KhiUtils.PlaylistTools.DatabaseElementCreator(playlistPath, nameOfPlaylist, songsInPlaylist);
                bool PlaylistSaved = KhiUtils.PlaylistTools.XmlWritingTool(playlistDatabase, playlistPath);
                return PlaylistSaved;
            }

            /// <summary>
            /// Reads a playlist (using either its name or path), and returns a list<Song>. 
            /// Returns null if the file doesnt exist, database is empty or faulty.
            /// </summary>
            /// <param name="playlistName"></param>
            /// <param name="playlistPath"></param>
            /// <returns></returns>
            public static List<Song>? PlaylistReader(string playlistName, string? playlistPath = null)
            {
                List<Song> songsInPlaylist = [];
                // If no path is provided, gets the playlistPath using the name of the playlist
                if (playlistPath == null)
                { playlistPath = GetPlaylistPath(playlistName); }
                // And If the path is null now, then the playlist does not exist.
                if (playlistPath == null)
                {
                    return null;
                }
                // Begins going through the playlist
                if (System.IO.File.Exists(playlistPath))
                {
                    XDocument playlistDatabase = XDocument.Load(playlistPath);
                    XElement? playlistSongs = playlistDatabase.Root;
                    // Checks if there is a root element and that it has Children, if not returns null.
                    if (playlistSongs != null && playlistSongs.HasElements)
                    {
                        var songs = playlistSongs.Elements();
                        foreach (XElement tempSong in songs)
                        {
                            // Checks if the song element has children, if not, returns null.
                            if (tempSong.HasElements)
                            {
                                var loadedSong = XElementToSong(tempSong);
                                if (loadedSong != null)
                                {
                                    songsInPlaylist.Add(loadedSong);
                                }
                                // If there was a problem, skips this element.
                                else { continue; }
                            }
                            else { continue; }
                        }
                        // All the elements have been read, and Song objects have been turned into a list.
                        return songsInPlaylist;
                    }
                    else { }//songsInPlaylist = null; }
                }
                return songsInPlaylist;
            }

            /// <summary>
            /// Creates a Song object using the provided XElement (from a playlist). Returns null if there 
            /// is no Path for the audio file or if the XElement's children are less than needed.
            /// </summary>
            /// <param name="songElement"></param>
            /// <returns></returns>
            public static Song? XElementToSong(XElement songElement)
            {
                var tempSongInfo = songElement.Elements();
                if (tempSongInfo.Count() == 8)
                {
                    string songTitle, songArtist, songAlbum, songPath, songGenres, songArtPath, songThumbnailPath;
                    TimeSpan songDuration;

                    // For Path
                    var tempPathElement = songElement.Element("Path");
                    if (tempPathElement != null) { songPath = (string)tempPathElement; }
                    // If there is no Path, then there was a problem with this item, will go to the next item
                    else { return null; }
                    // For Title
                    var tempTitleElement = songElement.Element("Title");
                    if (tempTitleElement != null) { songTitle = (string)tempTitleElement; }
                    else { songTitle = string.Empty; }
                    // For Artist
                    var tempArtistElement = songElement.Element("Artist");
                    if (tempArtistElement != null) { songArtist = (string)tempArtistElement; }
                    else { songArtist = string.Empty; }
                    // For Album
                    var tempAlbumElement = songElement.Element("Album");
                    if (tempAlbumElement != null) { songAlbum = (string)tempAlbumElement; }
                    else { songAlbum = string.Empty; }
                    // For ArtPath
                    var tempArtPathElement = songElement.Element("ArtPath");
                    if (tempArtPathElement != null) { songArtPath = (string)tempArtPathElement; }
                    else { songArtPath = string.Empty; }
                    // For ThumbnailPath
                    var tempThumbnailPathElement = songElement.Element("ThumbnailPath");
                    if (tempThumbnailPathElement != null) { songThumbnailPath = (string)tempThumbnailPathElement; }
                    else { songThumbnailPath = string.Empty; }
                    // For Duration
                    var tempDurationElement = songElement.Element("Duration");
                    if (tempDurationElement != null) { songDuration = (TimeSpan)tempDurationElement; }
                    else { songDuration = TimeSpan.Zero; }
                    // For Genres
                    var tempGenresElement = songElement.Element("Genres");
                    if (tempGenresElement != null) { songGenres = (string)tempGenresElement; }
                    else { songGenres = string.Empty; }
                    // Creates a Song object using the infos
                    Song newSong = new(songTitle, songArtist, songAlbum,
                        songPath, songArtPath, songThumbnailPath, songDuration, songGenres);
                    return newSong;
                }
                else
                { return null; }
            }
        }

        /// <summary>
        /// An assortment of tools for checking if data fits the application's criteria and 
        /// can be used, or whether they are duplicates and already exist within the database.
        /// </summary>
        public class DataFilteringTools
        {
            /// <summary>
            /// Checks to see if the audio files' extentions are compatible with the application, and 
            /// returns the suitable ones. If none of them are suitable, returns an empty array
            /// </summary>
            /// <param name="ChosenAudioFilePaths"></param>
            /// <returns></returns>
            public static string[] FilterFilesBasedOnExtention(string[] ChosenAudioFilePaths)
            {
                List<string> filteredFilePaths = [];
                foreach (string filePath in ChosenAudioFilePaths)
                {
                    if (IsAcceptableFormat(filePath)) { filteredFilePaths.Add(filePath); }
                }
                return filteredFilePaths.ToArray();
            }

            /// <summary>
            /// Checks if the chosen audio file that is to be processed is of an acceptable extension or not.
            /// </summary>
            /// <param name="audioFilePath"></param>
            /// <returns></returns>
            public static bool IsAcceptableFormat(string audioFilePath)
            {
                try
                {
                    bool isAcceptable = false;
                    var temp = System.IO.Path.GetExtension(audioFilePath).Trim().ToLower();
                    if (temp == ".mp3" || temp == ".wav" || temp == ".flac" || temp == ".aiff" || temp == ".wma" ||
                        temp == ".pcm" || temp == ".aac" || temp == ".oog" || temp == ".alac" || temp == ".m4a")
                    { isAcceptable = true; }
                    return isAcceptable;
                }
                catch { return false; }
            }

            /// <summary>
            /// Checks if the song is a duplicate. In case the xml database doesnt exist, returns false, 
            /// and returns true in case the new song is null.
            /// </summary>
            /// <param name="newSong"></param>
            /// <param name="playlistPath"></param>
            /// <returns></returns>
            internal static bool FilterDuplicates(Song newSong, string playlistPath)
            {
                if (System.IO.File.Exists(playlistPath) && newSong != null)
                {
                    XDocument playlistDataBase = XDocument.Load(playlistPath);
                    XElement? playlistSongs = playlistDataBase.Root; //the document root node
                    bool isDuplicate = KhiUtils.DataFilteringTools.CheckForDuplicates(playlistSongs, newSong);
                    return isDuplicate;
                }
                else if (newSong == null) { return true; }
                else { return false; }
            }

            /// <summary>
            /// Checks if the songs already exist in the playlist's database, removes the duplicates and 
            /// returns the rest; returns an empty List<Song> if all are duplicates or if the List of songs 
            /// is null.
            /// </summary>
            /// <param name="newSongs"></param>
            /// <param name="playlistPath"></param>
            /// <returns></returns>
            internal static List<Song> FilterDuplicates(List<Song> newSongs, string playlistPath)
            {
                List<Song> filesList = new List<Song>();

                if (System.IO.File.Exists(playlistPath) && newSongs != null)
                {
                    XDocument playlistDataBase = XDocument.Load(playlistPath);
                    XElement? playlistSongs = playlistDataBase.Root; //the document root node
                    foreach (var music in newSongs)
                    {
                        bool isDuplicate = KhiUtils.DataFilteringTools.CheckForDuplicates(playlistSongs, music);
                        if (isDuplicate == false) { filesList.Add(music); }
                    }
                    return filesList;
                }
                else if (newSongs == null)
                {
                    return filesList;
                }
                else
                {
                    return newSongs;
                }
            }

            /// <summary>
            /// Checks if the songs at the specified locations already exist in the playlist's database. 
            /// Returns the paths that are not duplicates as a  <see langword="string"/>[].
            /// </summary>
            /// <param name="audioPaths"></param>
            /// <param name="playlistPath"></param>
            /// <returns></returns>
            internal static string[] FilterDuplicates(string[] audioPaths, string playlistPath)
            {
                List<string> checkedPaths = new List<string>();
                XDocument playlistDataBase = XDocument.Load(playlistPath);
                XElement? playlistSongs = playlistDataBase.Root; //the document root node
                if (playlistSongs != null && playlistSongs.HasElements)
                {
                    int i = 0;
                    foreach (XElement playlistSong in playlistSongs.Elements())
                    {
                        XElement? path = playlistSong.Element("Path");
                        if (path != null && audioPaths[i] != path.Value)
                        {
                            checkedPaths.Add(audioPaths[i]);
                        }
                        if (i < audioPaths.Length) { i++; }
                    }
                    return checkedPaths.ToArray();
                }
                else
                { return audioPaths; }
            }

            /// <summary>
            /// ***HAVE TO CHANGE IT Checks if the song already exists within the playlist database.
            /// </summary>
            /// <param name="playlistSongs"></param>
            /// <param name="music"></param>
            /// <returns></returns>
            internal static bool CheckForDuplicates(XElement? playlistSongs, Song music)
            {
                bool isDuplicate = false;                
                try
                {
                    if (playlistSongs != null)
                    {
                        foreach (XElement playlistSong in playlistSongs.Elements())
                        {
                            XElement? path = playlistSong.Element("Path");
                            if (path != null)
                            {
                                if (music.Path == path.Value) { isDuplicate = true; }
                            }
                            // If the path is not similar then at least it's not the same file, but it might still be a duplicate
                            if (isDuplicate == false)
                            {
                                XElement? title = playlistSong.Element("Title");
                                XElement? artist = playlistSong.Element("Artist");
                                XElement? album = playlistSong.Element("Album");

                                if (title != null && title.Value != null &&
                                    artist != null && artist.Value != null &&
                                    album != null && album.Value != null)
                                {
                                    if (title.Value == music.Title &&
                                    artist.Value == music.Artist &&
                                    album.Value == music.Album)
                                    {
                                        isDuplicate = true;
                                    }
                                }

                            }
                        }
                    }
                    return isDuplicate;
                }
                catch
                { return isDuplicate; }
            }
        }

        /// <summary>
        /// An assortment of tools for extraction and manipulation of an Audio file's 
        /// info or Image/Thumbnail.
        /// </summary>
        public class SongInfoTools
        {
            /// <summary>
            /// Async Method that Populates this instance with the song's info, comprised of title, artist, album, path, artPath, 
            /// thumbnailPath, duration and genres. The art, Thumbnail and lyrics will be loaded on demand to 
            /// cut back on unnecessary memory usage.
            /// </summary>
            /// <param name="audioFilePath"></param>
            /// <returns></returns>
            /// <exception cref="FileNotFoundException"></exception>
            public static async Task<(string[], TimeSpan)> GetInfoAsync(string audioFilePath)
            {
                string songTitle, songArtist, songAlbum, songPath, songGenres, songLyrics, songArtPath, songThumbnailPath;
                TimeSpan songDuration;
                string[] songInfo = new string[8];
                if (System.IO.File.Exists(audioFilePath))
                {
                    try
                    {
                        // Getting errors, should try making them syncronous for now and see if it gets fixed
                        using (TagLib.File musicTags = TagLib.File.Create(audioFilePath, ReadStyle.Average))
                        {
                            //var tempTags = musicTags.Tag;
                            songPath = audioFilePath;
                            // For title
                            songTitle =  await GetTitle(musicTags, audioFilePath);
                            // For artists
                            songArtist = await GetArtist(musicTags);
                            // For Album, if it is mentioned, returns it, otherwise, an empty string is returned.
                            if (musicTags.Tag.Album == null) { songAlbum = string.Empty; }
                            else { songAlbum = (string)musicTags.Tag.Album.Clone(); }
                            // For duration
                            var time = musicTags.Properties.Duration.Duration();
                            songDuration = time;
                            // For genres
                            songGenres = await GetGenres(musicTags);
                            // *** For lyrics: For now Commented Out --> they should be extracted as needed to cut back on memory usage
                            // var temp = musicTags.Tag.Lyrics;
                            //if (temp != null) { songLyrics = musicTags.Tag.Lyrics.ReplaceLineEndings(); }
                            //else { songLyrics = string.Empty; }
                            // Getting the file name without extention, for the ImageHandler method
                            string fileName = System.IO.Path.GetFileNameWithoutExtension(audioFilePath);
                            // For saving the cover art and the thumbnail, and getting their paths
                            IPicture[] embeddedImages = musicTags.Tag.Pictures;
                            ImageHandler(embeddedImages, fileName);
                            songArtPath = KhiUtils.albumArtsPath + fileName + ".bmp";
                            songThumbnailPath = KhiUtils.albumArtsThumbnailsPath + fileName + ".bmp";

                            songInfo[0] = songTitle;
                            songInfo[1] = songArtist;
                            songInfo[2] = songAlbum;
                            songInfo[3] = songPath;
                            songInfo[4] = songArtPath;
                            songInfo[5] = songThumbnailPath;
                            songInfo[6] = songGenres;
                            songInfo[7] = string.Empty; //songLyrics;
                            musicTags.Dispose();
                            //return (songInfo, songDuration);
                        }
                        return (songInfo, songDuration);
                    }
                    catch
                    {
                        (songInfo, songDuration) = GetInfoAlt(audioFilePath);
                        return (songInfo, songDuration);
                    }
                }
                // In case the file doesn't exist, the path was not correct etc, throws an exception
                else
                {
                    throw new FileNotFoundException();
                }
            }

            /// <summary>
            /// Populates this instance with the song's info, comprised of title, artist, album, path, artPath, 
            /// thumbnailPath, duration and genres. The art, Thumbnail and lyrics will be loaded on demand to 
            /// cut back on unnecessary memory usage.
            /// </summary>
            /// <param name="audioFilePath"></param>
            /// <returns></returns>
            /// <exception cref="FileNotFoundException"></exception>
            public static (ReadOnlyCollection<string>, TimeSpan) GetInfo(string audioFilePath)
            {
                string songTitle, songArtist, songAlbum, songPath, songGenres, songLyrics, songArtPath, songThumbnailPath;
                TimeSpan songDuration;
                string[] songInfo = new string[8];
                ReadOnlyCollection<string> songInfos;
                if (System.IO.File.Exists(audioFilePath))
                {
                    try
                    {
                        // Getting errors, should try making them syncronous for now and see if it gets fixed
                        using (TagLib.File musicTags = TagLib.File.Create(audioFilePath, ReadStyle.Average))
                        {
                            //var tempTags = musicTags.Tag;
                            songPath = audioFilePath;
                            // Getting the file name without extention, for the ImageHandler method
                            string fileName = System.IO.Path.GetFileNameWithoutExtension(audioFilePath);
                            songArtPath = KhiUtils.albumArtsPath + fileName + ".bmp";
                            songThumbnailPath = KhiUtils.albumArtsThumbnailsPath + fileName + ".bmp";
                            // For saving the cover art and the thumbnail, and getting their paths
                            IPicture[] embeddedImages = musicTags.Tag.Pictures;
                            //ImageHandler(embeddedImages, fileName);
                            Image albumArt;
                            bool useDefaultPic = false;
                            if (embeddedImages != null && embeddedImages.Length > 0)
                            {
                                if (embeddedImages[0].Data != null && embeddedImages[0].Data.Data != null)
                                {
                                    var tempAlbumArt = new ImageConverter().ConvertFrom(embeddedImages[0].Data.Data);
                                    if (tempAlbumArt != null)
                                    {
                                        albumArt = (Image)tempAlbumArt;
                                        ImageSaver(albumArt, songArtPath);
                                        ThumbnailSaver(albumArt, songThumbnailPath);
                                        albumArt.Dispose();
                                    }
                                    else
                                    { useDefaultPic = true; }
                                }
                                else 
                                    { useDefaultPic = true; }                                
                            }
                            else
                            { useDefaultPic = true; }
                            if (useDefaultPic)
                            {
                                albumArt = KhiLibrary.Resources.kh;
                                var thumbnail = KhiLibrary.Resources.Khi_Player_Thumbnail;
                                // Checks if the AlbumArt and thumbnails directories exists, if not creates them.
                                if (!System.IO.Directory.Exists(KhiUtils.albumArtsPath)) { System.IO.Directory.CreateDirectory(KhiUtils.albumArtsPath); }
                                if (!System.IO.Directory.Exists(KhiUtils.albumArtsThumbnailsPath)) { System.IO.Directory.CreateDirectory(KhiUtils.albumArtsThumbnailsPath); }
                                albumArt.Save(albumArtsPath);
                                thumbnail.Save(albumArtsThumbnailsPath);
                                thumbnail.Dispose();
                                albumArt.Dispose();
                            }
                            // For title
                            string tempTitle;
                            if (musicTags.Tag.Title == null)
                            {
                                System.IO.FileInfo sth = new(audioFilePath);
                                tempTitle = (string)sth.Name.Clone();
                            }
                            else
                            {
                                tempTitle = (string)musicTags.Tag.Title.Clone();
                            }
                            songTitle = tempTitle;
                            // For artists
                            var allArtists = musicTags.Tag.Performers;
                            // If no artist is mentioned, returns an empty string
                            if (allArtists.Length == 0) { songArtist = string.Empty; }
                            // If only one artist is mentioned, returns that
                            else if (allArtists.Length == 1)
                            {
                                songArtist = (string)musicTags.Tag.FirstPerformer.Clone();
                            }
                            // If several artists are mentioned, concats them all with a space in between
                            else
                            {
                                // Just to initilize the string, since it's easier this way
                                songArtist = string.Empty;
                                foreach (var oneArtist in allArtists)
                                {
                                    if (songArtist != string.Empty)
                                    {
                                        songArtist += " " + (string)oneArtist.Clone();
                                    }
                                    else
                                    {
                                        songArtist = (string)oneArtist.Clone();
                                    }
                                }
                            }
                            // For Album, if it is mentioned, returns it, otherwise, an empty string is returned.
                            if (musicTags.Tag.Album == null) { songAlbum = string.Empty; }
                            else { songAlbum = (string)musicTags.Tag.Album.Clone(); }
                            // For duration
                            var time = musicTags.Properties.Duration.Duration();
                            songDuration = time;
                            // For genres
                            var allGenres = musicTags.Tag.Genres;
                            // If no Genre is mentioned in the file returns an empty string.
                            if (allGenres.Length == 0) { songGenres = string.Empty; }
                            // If only one genre is mentioned, returns that.
                            else if (allGenres.Length == 1)
                            {
                                songGenres = allGenres[0];
                            }
                            // If several genres are mentioned, concats them all with a space in between.
                            else
                            {
                                // just to initilize the string, easier this way.
                                songGenres = string.Empty;
                                foreach (string oneGenre in allGenres)
                                {
                                    // So that the first loop does not perform this
                                    if (songGenres != string.Empty)
                                    {
                                        songGenres += " " + (string)oneGenre.Clone();
                                    }
                                    else
                                    {
                                        songGenres = (string)oneGenre.Clone();
                                    }
                                }
                            }
                            // *** For lyrics: For now Commented Out --> they should be extracted as needed to cut back on memory usage
                            // var temp = musicTags.Tag.Lyrics;
                            //if (temp != null) { songLyrics = musicTags.Tag.Lyrics.ReplaceLineEndings(); }
                            //else { songLyrics = string.Empty; }                            
                            

                            songInfo[0] = songTitle;
                            songInfo[1] = songArtist;
                            songInfo[2] = songAlbum;
                            songInfo[3] = songPath;
                            songInfo[4] = songArtPath;
                            songInfo[5] = songThumbnailPath;
                            songInfo[6] = songGenres;
                            songInfo[7] = string.Empty; //songLyrics;
                            songInfos = songInfo.AsReadOnly();
                            musicTags.Dispose();
                            return (songInfos, songDuration);
                        }
                        //return (songInfo, songDuration);
                    }
                    catch
                    {
                        (songInfo, songDuration) = GetInfoAlt(audioFilePath);
                        songInfos = songInfo.AsReadOnly();
                        return (songInfos, songDuration);
                    }
                }
                // In case the file doesn't exist, the path was not correct etc, throws an exception
                else
                {
                    throw new FileNotFoundException();
                }
            }

            /// <summary>
            /// Gets and retuns the audio file's info, using an alternative method (using ShellFile). 
            /// </summary>
            /// <param name="audioFilePath"></param>
            /// <returns></returns>
            /// <exception cref="FileLoadException"></exception>
            public static (string[], TimeSpan) GetInfoAlt(string audioFilePath)
            {
                string songTitle, songArtist, songAlbum, songPath, songGenres, songLyrics, songArtPath, songThumbnailPath;
                TimeSpan songDuration;
                string[] songInfo = new string[8];
                songPath = audioFilePath;
                using (ShellFile songShell = ShellFile.FromFilePath(audioFilePath))
                {
                    if (songShell != null && songShell.Properties != null && songShell.Properties.System != null)
                    {
                        // For Title
                        var tempTitle = songShell.Properties.System.Title.Value;
                        if (tempTitle != null) { songTitle = tempTitle; }
                        else { songTitle = string.Empty; }
                        // Getting the file name without extention, for the ImageHandler method
                        string fileName = System.IO.Path.GetFileNameWithoutExtension(songPath);
                        songArtPath = KhiUtils.albumArtsPath + fileName + ".bmp";
                        songThumbnailPath = KhiUtils.albumArtsThumbnailsPath + fileName + ".bmp";
                        // For extracting and Saving the Images
                        Image art;
                        var tempArt = songShell.Thumbnail;
                        // For getting the Image and thumbnail
                            if (tempArt != null)
                            {
                                using (Bitmap tempPic = new Bitmap(tempArt.LargeBitmap))
                                {
                                    art = new Bitmap(tempPic);
                                }
                                //art = (Image)tempArt.LargeBitmap.Clone();
                            }
                            // If the ShellFile is null, uses the default KhiPlayer image nad thumbnail.
                            else
                            {
                                art = Resources.Khi_Player;
                            }
                        // For saving the images
                        
                            ImageSaver(art, songArtPath);
                            ThumbnailSaver(art, songThumbnailPath);
                            art.Dispose();
                        // For Artist
                        var tempArtist = songShell.Properties.System.Music.Artist.Value;
                        if (tempArtist != null) 
                        {
                            // If no artist is mentioned, returns an emoty string
                            if (tempArtist.Length == 0) { songArtist = string.Empty; }
                            // If only one artist is mentioned, returns that
                            else if (tempArtist.Length == 1)
                            {
                                songArtist = tempArtist[0];
                            }
                            // If several artists are mentioned, concats them all with a space in between
                            else
                            {
                                // Just to initilize the string, since it's easier this way
                                songArtist = string.Empty;
                                foreach (var oneArtist in tempArtist)
                                {
                                    if (songArtist != string.Empty)
                                    {
                                        songArtist += " " + (string)oneArtist.Clone();
                                    }
                                    else
                                    {
                                        songArtist = (string)oneArtist.Clone();
                                    }
                                }
                            }
                        }
                        else { songArtist = string.Empty; }
                        // For Album
                        var tempAlbum = songShell.Properties.System.Music.AlbumTitle.Value;
                        if (tempAlbum != null) { songAlbum = tempAlbum; }
                        else { songAlbum = string.Empty; }
                        // For Duration
                        var tempDur = (ulong)songShell.Properties.System.Media.Duration.ValueAsObject;
                        if (tempDur != null) { songDuration = TimeSpan.FromTicks((long)tempDur); }
                        else { songDuration = TimeSpan.Zero; }
                        // For Genres
                        var tempGenres = songShell.Properties.System.Music.Genre.Value;
                        if (tempGenres != null) 
                        { 
                            // If no Genre is mentioned in the file returns an empty string.
                            if (tempGenres.Length == 0) { songGenres = string.Empty; }
                            // If only one genre is mentioned, returns that.
                            else if (tempGenres.Length == 1)
                            {
                                songGenres = tempGenres[0];
                            }
                            // If several genres are mentioned, concats them all with a space in between.
                            else
                            {
                                // just to initilize the string, easier this way.
                                songGenres = string.Empty;
                                foreach (string oneGenre in tempGenres)
                                {
                                    // So that the first loop does not perform this
                                    if (songGenres != string.Empty)
                                    {
                                        songGenres += " " + (string)oneGenre.Clone();
                                    }
                                    else
                                    {
                                        songGenres = (string)oneGenre.Clone();
                                    }
                                }
                            }
                        }
                        else { songGenres = string.Empty; }
                        // *** For lyrics: For now Commented Out --> they should be extracted as needed to cut back on memory usage
                        //var tempLyrics = songShell.Properties.System.Music.Lyrics.Value;
                        //if (tempLyrics != null) { songLyrics = tempLyrics.ReplaceLineEndings(); }
                        //else { songLyrics = string.Empty; }                        

                        songInfo[0] = songTitle;
                        songInfo[1] = songArtist;
                        songInfo[2] = songAlbum;
                        songInfo[3] = songPath;
                        songInfo[4] = songArtPath;
                        songInfo[5] = songThumbnailPath;
                        songInfo[6] = songGenres;
                        songInfo[7] = string.Empty; //songLyrics;
                      
                        songShell.Dispose();
                        //return (songInfo, songDuration);
                    }
                    else
                    { throw new FileLoadException(); }
                    return (songInfo, songDuration);
                }
            }

            /// <summary>
            /// Asynchronously extracts the title of the song.
            /// </summary>
            /// <param name="tags"></param>
            /// <param name="audiFilePath"></param>
            /// <returns></returns>
            private static async Task<string> GetTitle(TagLib.File tags, string audiFilePath)
            {

                string songTitle = await Task<string>.Run(() =>
                {
                    string tempTitle;
                    if (tags.Tag.Title == null)
                    {
                        System.IO.FileInfo sth = new(audiFilePath);
                        tempTitle = (string)sth.Name.Clone();
                    }
                    else
                    {
                        tempTitle = (string)tags.Tag.Title.Clone();
                    }
                    return tempTitle;
                });
                return songTitle;
            }

            /// <summary>
            /// Asynchronously extracts the Artist/Artists of the song.
            /// </summary>
            /// <param name="tags"></param>
            /// <returns></returns>
            public static async Task<string> GetArtist(TagLib.File tags)
            {
                var allArtists = tags.Tag.Performers;
                string songArtist = await Task<string>.Run(() =>
                {
                    // If no artist is mentioned, returns an emoty string
                    if (allArtists.Length == 0) { songArtist = string.Empty; }
                    // If only one artist is mentioned, returns that
                    else if (allArtists.Length == 1)
                    {
                        songArtist = (string)tags.Tag.FirstPerformer.Clone();
                    }
                    // If several artists are mentioned, concats them all with a space in between
                    else
                    {
                        // Just to initilize the string, since it's easier this way
                        songArtist = string.Empty;
                        foreach (var oneArtist in allArtists)
                        {
                            if (songArtist != string.Empty)
                            {
                                songArtist += " " + (string)oneArtist.Clone();
                            }
                            else
                            {
                                songArtist = (string)oneArtist.Clone();
                            }
                        }
                    }
                    return songArtist;
                });
                return songArtist;
            }

            /// <summary>
            /// Asynchronously extracts the Genres of the song.
            /// </summary>
            /// <param name="tags"></param>
            /// <returns></returns>
            public static async Task<string> GetGenres(TagLib.File tags)
            {
                var allGenres = tags.Tag.Genres;
                string songGenres = await Task.Run(() =>
                {
                    // If no Genre is mentioned in the file returns an empty string.
                    if (allGenres.Length == 0) { songGenres = string.Empty; }
                    // If only one genre is mentioned, returns that.
                    else if (allGenres.Length == 1)
                    {
                        songGenres = allGenres[0];
                    }
                    // If several genres are mentioned, concats them all with a space in between.
                    else
                    {
                        // just to initilize the string, easier this way.
                        songGenres = string.Empty;
                        foreach (string oneGenre in allGenres)
                        {
                            // So that the first loop does not perform this
                            if (songGenres != string.Empty)
                            {
                                songGenres += " " + (string)oneGenre.Clone();
                            }
                            else
                            {
                                songGenres = (string)oneGenre.Clone();
                            }
                        }
                    }
                    return songGenres;
                });
                return songGenres;
            }

            /// <summary>
            /// Removes a song from the database and its associated pictures from the art and thumbnail folders.
            /// </summary>
            /// <param name="toBeRemovedSong"></param>
            /// <param name="playlistPath"></param>
            public static async void RemoveSongInfoAndPics(Song toBeRemovedSong, string playlistPath)
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
                                if (removalCandidate != null && removalCandidate.HasChildNodes &&
                                    removalCandidate.ChildNodes[3] != null && removalCandidate.ChildNodes[3].InnerText == toBeRemovedSong.Path)
                                {
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
                                if (System.IO.File.Exists(toBeRemovedSong.ArtPath)) { System.IO.File.Delete(toBeRemovedSong.ArtPath); }
                                if (System.IO.File.Exists(toBeRemovedSong.ThumbnailPath)) { System.IO.File.Delete(toBeRemovedSong.ThumbnailPath); }
                            }
                            catch
                            {
                                // In Case it wasn't properly disposed of 
                                Task.Delay(3000);
                                GC.WaitForPendingFinalizers();
                                GC.Collect();

                                // Will try once again, and in case of exception, simply ignores the pictures, since they are loaded on demand
                                // and using the path included in the database. Since the song is removed from the database, removing the pictures
                                // is not a priority. 
                                try
                                {
                                    if (System.IO.File.Exists(toBeRemovedSong.ArtPath)) { System.IO.File.Delete(toBeRemovedSong.ArtPath); }
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
            /// Extracts and saves the song's cover art and its thumbnail to the Album art and thumbnail folders.
            /// </summary>
            /// <param name="embeddedImages"></param>
            /// <param name="audioFileNameWithoutExtension"></param>
            public static async void ImageHandler(IPicture[] embeddedImages, string audioFileNameWithoutExtension)
            {
                // To populate the object's artPath and ThumbnailPath that is used for loading the images
                string tempArtPath = KhiUtils.albumArtsPath + audioFileNameWithoutExtension + ".bmp";
                string tempThumbnailPath = KhiUtils.albumArtsThumbnailsPath + audioFileNameWithoutExtension + ".bmp";
                await Task.Run(() =>
                {
                    //MemoryStream picConverter;
                    bool useDefaultImage = false;
                    // Using local variables because there is no need to have the images in memory. It's better and less memory expensive
                    // to load them when they are needed
                    Image art;                  
                    if (embeddedImages.Length > 0 && embeddedImages[0].Type != TagLib.PictureType.NotAPicture)
                    {
                        using (MemoryStream picConverter = new MemoryStream(embeddedImages[0].Data.Data))
                        {
                            if (picConverter != null && picConverter.CanRead)
                            {
                                var tempArt = Image.FromStream(picConverter);
                                art = (Image)tempArt.Clone();
                                ImageSaver(art, tempArtPath);
                                ThumbnailSaver(art, tempThumbnailPath);
                                art.Dispose();
                                tempArt.Dispose();
                                picConverter.Dispose();
                            }
                            else
                            {
                                useDefaultImage = true;
                            }
                        }
                    }
                    else
                    {
                        useDefaultImage = true; 
                    }
                    if (useDefaultImage)
                    {
                        art = Resources.Khi_Player;
                        Image thumbnail = Resources.Khi_Player_Thumbnail;
                        ImageSaver(art, tempArtPath);
                        ImageSaver(thumbnail, tempThumbnailPath);
                        art.Dispose();
                        thumbnail.Dispose();
                    }
                });
            }

            /// <summary>
            /// Extracts and saves the song's cover art and its thumbnail to the Album art and thumbnail folders.
            /// </summary>
            /// <param name="tags"></param>
            /// <param name="audioFileNameWithoutExtension"></param>
            public static async void ImageHandler(TagLib.File tags, string audioFileNameWithoutExtension)
            {
                IPicture[] embeddedImages = tags.Tag.Pictures;
                await Task.Run(() =>
                {
                    ImageHandler(embeddedImages, audioFileNameWithoutExtension);
                });
            }

            /// <summary>
            /// Extracts and saves the song's cover art and its thumbnail to the Album art and thumbnail folders
            /// </summary>
            /// <param name="songShell"></param>
            /// <param name="audioFileNameWithoutExtension"></param>
            public static async void ImageHandler(ShellFile songShell, string audioFileNameWithoutExtension)
            {               
                await Task.Run(() =>
                {
                    Image art;
                    var tempArt = songShell.Thumbnail;
                    // For getting the Image and thumbnail
                    if (tempArt != null)
                    {
                        using (var tempbmp = new Bitmap(tempArt.Bitmap))
                        {
                            art = new Bitmap(tempbmp);
                        }
                    }
                    // If the ShellFile is null, uses the default KhiPlayer image nad thumbnail.
                    else
                    {
                        art = Resources.Khi_Player;
                    }
                    string artPath = KhiUtils.albumArtsPath + audioFileNameWithoutExtension + ".bmp";
                    string thumbnailPath = KhiUtils.albumArtsThumbnailsPath + audioFileNameWithoutExtension + ".bmp";
                    // For saving the images
                    ImageSaver(art, artPath);
                    ThumbnailSaver(art, thumbnailPath);
                    art.Dispose();
                });
            }

            /// <summary>
            /// Saves the Image to the the provided path.
            /// </summary>
            /// <param name="image"></param>
            /// <param name="imagePath"></param>
            private static void ImageSaver(Image image, string imagePath)
            {
                // Checks if the AlbumArt and thumbnails directories exists, if not creates them.
                if (!System.IO.Directory.Exists(KhiUtils.albumArtsPath)) { System.IO.Directory.CreateDirectory(KhiUtils.albumArtsPath); }
                if (!System.IO.Directory.Exists(KhiUtils.albumArtsThumbnailsPath)) { System.IO.Directory.CreateDirectory(KhiUtils.albumArtsThumbnailsPath); }

                // Saving the Image to the provided location.
                using (FileStream artSaver = new(imagePath, FileMode.Create, FileAccess.ReadWrite))
                {
                    try
                    {
                        image.Save(artSaver, image.RawFormat);
                        artSaver.Dispose();
                    }
                    catch
                    {
                        image.Save(artSaver, System.Drawing.Imaging.ImageFormat.Bmp);
                        artSaver.Dispose();
                    }
                }
            }

            /// <summary>
            /// Creates a thumbnail of the image (60, 60) and saves it to the provided location.
            /// </summary>
            /// <param name="image"></param>
            /// <param name="thumbnailPath"></param>
            /// <param name="checkDirectory"></param>
            private static void ThumbnailSaver(Image image, string thumbnailPath, bool checkDirectory = false)
            {
                // Checks if the AlbumArt and thumbnails directories exists, if not creates them.
                if (checkDirectory) 
                {
                    if (!System.IO.Directory.Exists(KhiUtils.albumArtsPath)) { System.IO.Directory.CreateDirectory(KhiUtils.albumArtsPath); }
                    if (!System.IO.Directory.Exists(KhiUtils.albumArtsThumbnailsPath)) { System.IO.Directory.CreateDirectory(KhiUtils.albumArtsThumbnailsPath); }
                }
                // Creates the Thumbnail
                Image thumbnail = image.GetThumbnailImage(60, 60, () => false, 0);
                // Saving the thumbnail to the provided location.
                using (FileStream thumbnailSaver = new(thumbnailPath, FileMode.Create, FileAccess.Write))
                {
                    try
                    {
                        thumbnail.Save(thumbnailSaver, image.RawFormat);
                        thumbnailSaver.Dispose();
                        thumbnail.Dispose();
                    }
                    catch
                    {
                        thumbnail.Save(thumbnailSaver, System.Drawing.Imaging.ImageFormat.Bmp);
                        thumbnailSaver.Dispose();
                        thumbnail.Dispose();
                    }
                }
            }
        }

    }
}
