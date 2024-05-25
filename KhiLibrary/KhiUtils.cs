using Microsoft.WindowsAPICodePack.Shell;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using TagLib;

namespace KhiLibrary
{
    public class KhiUtils
    {
        private static readonly string applicationPath = Application.StartupPath;
        private static readonly string allMusicDataBase = applicationPath + "AllMusicDataBase.xml";
        private static readonly string PlaylistsRecord = applicationPath + "PlaylistsRecord.xml";
        private static readonly string albumArtsPath = applicationPath + "\\Album Arts\\";
        private static readonly string albumArtsThumbnailsPath = applicationPath + "Album Arts Thumbnails\\";

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
                // Creating the elements
                // Each element's will contain a child, the playlistPath
                // The elements name will be the same as the playlist's name
                XElement newPlaylist = new(playlistName, playlistPath);
                // We then Add it to the root document
                playlists.Add(newPlaylist);
                // Saves the Document
                XmlWritingTool(records, PlaylistsRecord);
            }

            /// <summary>
            /// Returns a Dictionary<string, string>. for each element, the key is the playlistName, while the value is the playlistPath.
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
                            var tempAllPlaylists = playlists.Descendants();
                            foreach (XElement playlist in tempAllPlaylists)
                            {
                                try
                                {
                                    playlistsDic.Add(playlist.Name.ToString(), playlist.Elements().First().Value);
                                }
                                catch { }
                            }
                            if (playlistsDic.Count > 0)
                            {
                                return playlistsDic;
                            }
                            else { return null; }
                        }
                        else { return null; }
                    }
                    // If there is no root element, then there are no playlists, returns null
                    else { return null; }
                }
                else { return null; }
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
            /// <param name="song"></param>
            /// <returns></returns>
            internal static XDocument DatabaseElementCreator(string playlistPath, string nameOfPlaylist, List<Song> songsList)
            {
                //XmlDocument playlistDatabase = new XmlDocument();
                //XmlElement playlistSongs;  //the document root node

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
                        /*
                        playlistSongs = playlistDatabase.Add().CreateElement("ArrayOfArrayOfObject");
                        playlistSongs.SetAttribute("playlist", nameOfPlaylist);
                        playlistSongs.SetAttribute("lastUpdated", DateTime.Now.ToString());
                        */
                    }
                }
                else
                {
                    playlistDatabase = new XDocument();
                    playlistSongs = new XElement(nameOfPlaylist);
                    playlistSongs.SetAttributeValue("playlist", nameOfPlaylist);
                    playlistSongs.SetAttributeValue("lastUpdated", DateTime.Now);
                    playlistDatabase.Add(playlistSongs);
                    /*
                    playlistSongs = playlistDatabase.CreateElement("ArrayOfArrayOfObject");
                    playlistSongs.SetAttribute("playlist", nameOfPlaylist);
                    playlistSongs.SetAttribute("lastUpdated", DateTime.Now.ToString());
                    playlistDatabase.AppendChild(playlistSongs);
                    */
                }
                /*
                XmlElement Song = playlistDatabase.CreateElement("ArrayOfObject");
                if (playlistSongs.HasChildNodes)
                {
                    playlistSongs.InsertAfter(Song, playlistSongs.LastChild);
                }
                else { playlistSongs.AppendChild(Song); }
                */
                foreach (Song song in songsList)
                {
                    XElement Song = new("Title");

                    XElement Title = new("Title", song.Title);
                    XElement Artist = new("Artist", song.Title);
                    XElement Album = new("Album", song.Title);
                    XElement Path = new("Path", song.Title);
                    XElement ArtPath = new("ArtPath", song.Title);
                    XElement ThumbnailPath = new("ThumbnailPath", song.Title);
                    XElement Duration = new("Duration", song.Title);
                    XElement Genres = new("Genres", song.Title);

                    Song.Add(Title);
                    Song.Add(Artist);
                    Song.Add(Album);
                    Song.Add(Path);
                    Song.Add(ArtPath);
                    Song.Add(ThumbnailPath);
                    Song.Add(Duration);
                    Song.Add(Genres);

                    playlistSongs.Add(Song);

                    /*
                    //XmlElement Title = playlistDatabase.CreateElement("string");
                    XmlElement Artist = playlistDatabase.CreateElement("string");
                    XmlElement Album = playlistDatabase.CreateElement("string");
                    XmlElement Path = playlistDatabase.CreateElement("string");
                    XmlElement ArtPath = playlistDatabase.CreateElement("string");
                    XmlElement ThumbnailPath = playlistDatabase.CreateElement("string");
                    XmlElement Duration = playlistDatabase.CreateElement("TimeSpan");
                    XmlElement Genres = playlistDatabase.CreateElement("string");

                    Title.InnerText = song.Title;
                    Artist.InnerText = song.Artist;
                    Album.InnerText = song.Album;
                    Path.InnerText = song.Path;
                    ArtPath.InnerText = song.ArtPath;
                    ThumbnailPath.InnerText = song.ThumbnailPath;
                    Duration.InnerText = song.Duration.ToString();
                    Genres.InnerText = song.Genres;

                    Song.AppendChild(Title);
                    Song.AppendChild(Artist);
                    Song.AppendChild(Album);
                    Song.AppendChild(Path);
                    Song.AppendChild(ArtPath);
                    Song.AppendChild(ThumbnailPath);
                    Song.AppendChild(Duration);
                    Song.AppendChild(Genres);
                    */
                }
                return playlistDatabase;
            }

            /// <summary>
            /// Saves the xml document to the specified path (Turned it into a method to avoid repetition). 
            /// </summary>
            /// <param name="playlistDatabase"></param>
            /// <param name="playlistPath"></param>
            internal static async void XmlWritingTool(XDocument playlistDatabase, string playlistPath)
            {
                await Task.Run(() =>
                {
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
                        Encoding = Encoding.UTF8,
                        Indent = true,
                        NewLineChars = System.Environment.NewLine
                    };

                    using (StreamWriter datastream = new(playlistPath, Encoding.UTF8, options))
                    {
                        XmlWriter dataBaseWriter = XmlWriter.Create(datastream, settings);
                        playlistDatabase.Save(dataBaseWriter);
                        dataBaseWriter.Dispose();
                    }
                });
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
            public async static void PlaylistWriter(string playlistPath, string nameOfPlaylist, List<Song> songsInPlaylist)
            {
                await Task.Run(() =>
                {
                    XDocument playlistDatabase = KhiUtils.PlaylistTools.DatabaseElementCreator(playlistPath, nameOfPlaylist, songsInPlaylist);
                    KhiUtils.PlaylistTools.XmlWritingTool(playlistDatabase, playlistPath);
                });
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
                playlistPath = GetPlaylistPath(playlistName);
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
                        var songs = playlistSongs.Descendants();
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
                            else { return null; }
                        }
                        // All the elements have been read, and Song objects have been turned into a list.
                        return songsInPlaylist;
                    }
                    else { return null; }
                }
                return null;
            }

            /// <summary>
            /// Creates a Song object using the provided XElement (from a playlist). Returns null if there 
            /// is no Path for the audio file or if the XElement's children are less than needed.
            /// </summary>
            /// <param name="songElement"></param>
            /// <returns></returns>
            public static Song? XElementToSong(XElement songElement)
            {
                var tempSongInfo = songElement.Descendants();
                if (tempSongInfo.Count() >= 8)
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
            /// <returns></returns>
            public static bool IsAcceptableFormat(string audioFilePath)
            {
                bool isAcceptable = false;
                var temp = System.IO.Path.GetExtension(audioFilePath).Trim().ToLower();
                if (temp == ".mp3" || temp == ".wav" || temp == ".flac" || temp == ".aiff" || temp == ".wma" || temp == ".pcm" || temp == ".aac" || temp == ".oog" || temp == ".alac")
                { isAcceptable = true; }
                return isAcceptable;
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
                    XmlDocument playlistDataBase = new();
                    XmlElement? playlistSongs;  //the document root node
                    playlistDataBase.Load(playlistPath);
                    playlistSongs = playlistDataBase.DocumentElement;
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
                List<Song> filesList = [];

                if (System.IO.File.Exists(playlistPath) && newSongs != null)
                {
                    XmlDocument playlistDataBase = new();
                    XmlElement? playlistSongs;  //the document root node
                    playlistDataBase.Load(playlistPath);
                    playlistSongs = playlistDataBase.DocumentElement;
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
                    return newSongs.ToList();
                }
            }

            /// <summary>
            /// ***HAVE TO CHANGE IT Checks if the song already exists within the playlist database.
            /// </summary>
            /// <param name="playlistSongs"></param>
            /// <param name="music"></param>
            /// <returns></returns>
            internal static bool CheckForDuplicates(XmlElement? playlistSongs, Song music)
            {
                bool isDuplicate = false;
                try
                {
                    if (playlistSongs != null && !playlistSongs.IsEmpty && playlistSongs.HasChildNodes)
                    {
                        XmlNodeList songElements = playlistSongs.ChildNodes;
                        for (int i = 0; i < songElements.Count; i++)
                        {
                            isDuplicate = false;
                            XmlNodeList songProperties = songElements[i].ChildNodes;
                            string title, artist, album, path;
                            if (songProperties[i] != null && songProperties[i].HasChildNodes)
                            {
                                if (songProperties[i].ChildNodes[0] != null &&
                                    songProperties[i].ChildNodes[1] != null &&
                                    songProperties[i].ChildNodes[2] != null &&
                                    songProperties[i].ChildNodes[3] != null)
                                {
                                    title = songProperties[i].ChildNodes[0].InnerText;
                                    artist = songProperties[i].ChildNodes[1].InnerText;
                                    album = songProperties[i].ChildNodes[2].InnerText;
                                    path = songProperties[i].ChildNodes[3].InnerText;
                                    // If the paths are the same then logically they are the same file, that's it is checked first.
                                    if (path == music.Path)
                                    {
                                        isDuplicate = true;
                                        break;
                                    }
                                    else if (title == music.Title && artist == music.Artist && album == music.Album)
                                    {
                                        isDuplicate = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    return isDuplicate;
                }
                catch
                { return false; }
            }
        }

        /// <summary>
        /// An assortment of tools for extraction and manipulation of an Audio file's 
        /// info or Image/Thumbnail.
        /// </summary>
        public class SongInfoTools
        {
            /// <summary>
            /// Populates this instance with the song's info, comprised of title, artist, album, path, artPath, 
            /// thumbnailPath, duration and genres. The art, Thumbnail and lyrics will be loaded on demand to 
            /// cut back on unnecessary memory usage.
            /// </summary>
            /// <param name="audioFilePath"></param>
            /// <returns></returns>
            public static async Task<(string[], TimeSpan)> GetInfo(string audioFilePath)
            {
                string songTitle, songArtist, songAlbum, songPath, songGenres, songLyrics, songArtPath, songThumbnailPath;
                TimeSpan songDuration;
                string[] songInfo = new string[8];
                if (System.IO.File.Exists(audioFilePath))
                {
                    try
                    {
                        using (TagLib.File musicTags = TagLib.File.Create(audioFilePath, ReadStyle.PictureLazy))
                        {
                            var tempTags = musicTags.Tag;
                            songPath = audioFilePath;
                            // For title
                            songTitle = await GetTitle(tempTags, audioFilePath);
                            // For artists
                            songArtist = await GetArtist(tempTags);
                            // For Album, if it is mentioned, returns it, otherwise, an empty string is returned.
                            if (musicTags.Tag.Album == null) { songAlbum = string.Empty; }
                            else { songAlbum = (string)musicTags.Tag.Album.Clone(); }
                            // For duration
                            var time = musicTags.Properties.Duration;
                            songDuration = time;
                            // For genres
                            songGenres = await GetGenres(tempTags);
                            // For lyrics
                            var temp = musicTags.Tag.Lyrics;
                            if (temp != null) { songLyrics = musicTags.Tag.Lyrics.ReplaceLineEndings(); }
                            else { songLyrics = string.Empty; }
                            // Getting the file name without extention, for the ImageHandler method
                            System.IO.FileInfo fileInfo = new(songPath);
                            string fileName = fileInfo.Name;
                            fileName = fileName.Split('.')[0];
                            // For saving the cover art and the thumbnail, and getting their paths
                            KhiUtils.SongInfoTools.ImageHandler(tempTags, fileName);
                            songArtPath = KhiUtils.albumArtsPath + fileName + ".bmp";
                            songThumbnailPath = KhiUtils.albumArtsThumbnailsPath + fileName + ".bmp";
                            songInfo[0] = songTitle;
                            songInfo[1] = songArtist;
                            songInfo[2] = songAlbum;
                            songInfo[3] = songPath;
                            songInfo[4] = songArtPath;
                            songInfo[5] = songThumbnailPath;
                            songInfo[6] = songGenres;
                            songInfo[7] = songLyrics;
                            musicTags.Dispose();
                            return (songInfo, songDuration);
                        }
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
            /// Gets and retuns the audio file's info, using an alternative method (using ShellFile). 
            /// </summary>
            /// <param name="audioFilePath"></param>
            /// <returns></returns>
            /// <exception cref="FileLoadException"></exception>
            private static (string[], TimeSpan) GetInfoAlt(string audioFilePath)
            {
                string songTitle, songArtist, songAlbum, songPath, songGenres, songLyrics, songArtPath, songThumbnailPath;
                TimeSpan songDuration;
                string[] songInfo = new string[8];
                songPath = audioFilePath;
                ShellFile songShell = ShellFile.FromFilePath(audioFilePath);
                if (songShell != null && songShell.Properties != null)
                {
                    // For Title
                    var tempTitle = songShell.Properties.GetProperty("Title").ToString();
                    if (tempTitle != null) { songTitle = tempTitle; }
                    else { songTitle = string.Empty; }
                    // For Artist
                    var tempArtist = songShell.Properties.GetProperty("Album artist").ToString();
                    if (tempArtist != null) { songArtist = tempArtist; }
                    else { songArtist = string.Empty; }
                    // For Album
                    var tempAlbum = songShell.Properties.GetProperty("Album").ToString();
                    if (tempAlbum != null) { songAlbum = tempAlbum; }
                    else { songAlbum = string.Empty; }
                    // For Duration
                    var tempDur = songShell.Properties.GetProperty("Length").ValueAsObject;
                    if (tempDur != null) { songDuration = (TimeSpan)tempDur; }
                    else { songDuration = TimeSpan.Zero; }
                    // For Genres
                    var tempGenres = songShell.Properties.GetProperty("Genre").ToString();
                    if (tempGenres != null) { songGenres = tempGenres; }
                    else { songGenres = string.Empty; }
                    // Getting the file name without extention, for the ImageHandler method
                    System.IO.FileInfo fileInfo = new(songPath);
                    string fileName = fileInfo.Name;
                    fileName = fileName.Split('.')[0];
                    // For extracting and Saving the Images
                    KhiUtils.SongInfoTools.ImageHandler(songShell, fileName);
                    songArtPath = KhiUtils.albumArtsPath + fileName + ".bmp";
                    songThumbnailPath = KhiUtils.albumArtsThumbnailsPath + fileName + ".bmp";
                    songShell.Dispose();
                    songLyrics = string.Empty;

                    songInfo[0] = songTitle;
                    songInfo[1] = songArtist;
                    songInfo[2] = songAlbum;
                    songInfo[3] = songPath;
                    songInfo[4] = songArtPath;
                    songInfo[5] = songThumbnailPath;
                    songInfo[6] = songGenres;
                    songInfo[7] = songLyrics;
                    return (songInfo, songDuration);
                }
                else
                { throw new FileLoadException(); }
            }

            /// <summary>
            /// Asynchronously extracts thr title of the song.
            /// </summary>
            /// <param name="tags"></param>
            /// <param name="audiFilePath"></param>
            /// <returns></returns>
            private static async Task<string> GetTitle(TagLib.Tag tags, string audiFilePath)
            {
                string songTitle = await Task<string>.Run(() =>
                {
                    string tempTitle;
                    if (tags.Title == null)
                    {
                        System.IO.FileInfo sth = new(audiFilePath);
                        tempTitle = (string)sth.Name.Clone();
                    }
                    else
                    {
                        tempTitle = (string)tags.Title.Clone();
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
            public static async Task<string> GetArtist(TagLib.Tag tags)
            {
                var allArtists = tags.Performers;
                string songArtist = await Task<string>.Run(() =>
                {
                    // If no artist is mentioned, returns an emoty string
                    if (allArtists.Length == 0) { songArtist = string.Empty; }
                    // If only one artist is mentioned, returns that
                    else if (allArtists.Length == 1)
                    {
                        songArtist = (string)tags.FirstPerformer.Clone();
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
            public static async Task<string> GetGenres(TagLib.Tag tags)
            {
                var allGenres = tags.Genres;
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
                await Task.Run(() =>
                {
                    // Using local variables because there is no need to have the images in memory. It's better and less memory expensive
                    // to load them when they are needed
                    Image art, thumbnail;
                    static bool ThumbnailCallback() { return false; }

                    MemoryStream picConverter = new();
                    if (embeddedImages.Length > 0)
                    {
                        picConverter = new MemoryStream(embeddedImages[0].Data.Data);
                        if (!picConverter.CanRead || !picConverter.CanWrite || embeddedImages[0].Type == TagLib.PictureType.NotAPicture)
                        {
                            art = Resources.Khi_Player;
                            thumbnail = Resources.Khi_Player.GetThumbnailImage(60, 60, ThumbnailCallback, 0);
                        }
                        else
                        {
                            art = (Image)Image.FromStream(picConverter).Clone();
                            thumbnail = art.GetThumbnailImage(60, 60, ThumbnailCallback, 0);
                        }
                    }
                    else
                    {
                        art = Resources.Khi_Player;
                        thumbnail = Resources.Khi_Player_Thumbnail;
                    }
                    // To populate the object's artPath and ThumbnailPath that is used for loading the images
                    string tempArtPath = KhiUtils.albumArtsPath + audioFileNameWithoutExtension + ".bmp";
                    string tempThumbnailPath = KhiUtils.albumArtsThumbnailsPath + audioFileNameWithoutExtension + ".bmp";
                    // For saving cover art
                    ImageSaver(art, tempArtPath);
                    ImageSaver(thumbnail, tempThumbnailPath);
                });
            }

            /// <summary>
            /// Extracts and saves the song's cover art and its thumbnail to the Album art and thumbnail folders.
            /// </summary>
            /// <param name="tags"></param>
            /// <param name="audioFileNameWithoutExtension"></param>
            public static async void ImageHandler(TagLib.Tag tags, string audioFileNameWithoutExtension)
            {
                IPicture[] embeddedImages = tags.Pictures;
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
                static bool ThumbnailCallback() { return false; }
                await Task.Run(() =>
                {
                    Image art, thumbnail;
                    var tempArt = songShell.Thumbnail;
                    // For getting the Image and thumbnail
                    if (tempArt != null)
                    {
                        art = (Image)tempArt.ExtraLargeBitmap.Clone();
                        thumbnail = (Image)art.GetThumbnailImage(60, 60, ThumbnailCallback, 0);
                    }
                    // If the ShellFile is null, uses the default KhiPlayer image nad thumbnail.
                    else
                    {
                        art = Resources.Khi_Player;
                        thumbnail = Resources.Khi_Player_Thumbnail;
                    }
                    string artPath = KhiUtils.albumArtsPath + audioFileNameWithoutExtension + ".bmp";
                    string thumbnailPath = KhiUtils.albumArtsThumbnailsPath + audioFileNameWithoutExtension + ".bmp";
                    // For saving the images
                    ImageSaver(art, artPath);
                    ImageSaver(thumbnail, thumbnailPath);
                });
            }

            /// <summary>
            /// Saves the Image to the the provided path (the path is used to simply know if the image is a cover art or its thumbnail).
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
                    image.Save(artSaver, image.RawFormat);
                    artSaver.Dispose();
                }
            }
        }

    }
}
