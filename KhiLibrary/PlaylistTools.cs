using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace KhiLibrary
{
    /// <summary>
    /// Contains methods used by Playlist class and up for reading, writing, and editing playlists.
    /// </summary>
    internal static class PlaylistTools
    {
        /// <summary>
        /// Contains methods that can be used for reading and manipulation of playlist Records where the 
        /// name and location of the playlists are registered.
        /// </summary>
        internal static class Records
        {
            /// <summary>
            /// Checks if a playlist with the name already exists, returns true if there is
            /// and false if the there is no playlist with this name.
            /// </summary>
            /// <param name="name"></param>
            /// <returns></returns>
            internal static bool PlaylistAlreadyExists(string name)
            {
                bool alreadyExists = false;
                var allPlaylistsDic = PlaylistTools.Records.ReadPlaylistsRecords();
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
            /// Removes a playlist from the playlist records (the database that keeps tabs on playlists' names and paths).
            /// </summary>
            /// <param name="playlistName"></param>
            internal static void RemovePlaylistFromRecords(string playlistName)
            {
                XDocument records;
                XElement playlists;
                if (System.IO.File.Exists(InternalSettings.playlistsRecord))
                {
                    records = XDocument.Load(InternalSettings.playlistsRecord);
                    var tempPlaylists = records.Root;
                    if (tempPlaylists != null)
                    {
                        playlists = tempPlaylists;
                        XName playlistXName = playlistName.Replace(' ', '_');
                        var oldPlaylistRecord = playlists.Element(playlistXName);
                        if (oldPlaylistRecord != null)
                        {
                            oldPlaylistRecord.Remove();
                            PlaylistTools.DatabaseTools.XmlWritingTool(records, InternalSettings.playlistsRecord);
                        }
                    }
                }
            }

            /// <summary>
            /// Writes the name and path of a playlist to an xml file, to be accessed later.
            /// </summary>
            /// <param name="playlistName"></param>
            /// <param name="playlistPath"></param>
            internal static void PlaylistRecorder(string playlistName, string playlistPath)
            {
                XDocument records;
                XElement playlists;
                if (System.IO.File.Exists(InternalSettings.playlistsRecord))
                {
                    records = XDocument.Load(InternalSettings.playlistsRecord);
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
                    // Each element will contain a child, the playlistPath
                    // The elements name will be the same as the playlist's name
                    XElement newPlaylist = new(playlistName.Replace(' ', '_'), playlistPath);
                    // We then Add it to the root document
                    playlists.Add(newPlaylist);
                    // Saves the Document
                    PlaylistTools.DatabaseTools.XmlWritingTool(records, InternalSettings.playlistsRecord);
                }
            }

            /// <summary>
            /// Returns a Dictionary of string,string. for each element, the key is the playlistName, while the value is the playlistPath.
            /// </summary>
            /// <returns></returns>
            internal static Dictionary<string, string>? ReadPlaylistsRecords()
            {
                Dictionary<string, string>? playlistsDic = [];
                if (System.IO.File.Exists(InternalSettings.playlistsRecord))
                {
                    XDocument records = XDocument.Load(InternalSettings.playlistsRecord);
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
                                    playlistsDic.Add(playlist.Name.LocalName.Replace('_', ' '), playlist.Value);
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
            /// string if a playlist with that name does not exist.
            /// </summary>
            /// <param name="playlistName"></param>
            /// <returns></returns>
            internal static string? GetPlaylistPath(string playlistName)
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
            /// Returns a Dictionary of string, string, comprised of playlist names as the keys, and playlist paths as the value.
            /// Returns null if no playlist is found or an exception is thrown. Use in case playlistRecords is corrupted or unavailable 
            /// or in similar cases.
            /// </summary>
            /// <returns></returns>
            internal static Dictionary<string, string>? PlaylistDatabaseFinder()
            {
                Dictionary<string, string> playlistNameAndPath = [];
                XmlDocument? musicDatabase = new();
                // The document root node
                XmlElement? playlistSongs;
                // Finds all of the xml files in the application directory
                string[] tempExistingDatabases = System.IO.Directory.GetFiles(InternalSettings.applicationPath, "*.xml*", SearchOption.AllDirectories);
                if (tempExistingDatabases.Length > 0)
                {
                    // For each of the data bases found, gets the playlist name from the xml document, the name is the value of
                    // the "playlist" attribute.
                    foreach (string database in tempExistingDatabases)
                    {
                        // Just to make sure
                        var ext = Path.GetExtension(database);
                        if (DataFilteringTools.AreTheSame(ext, ".xml", true))
                        {
                            musicDatabase.LoadXml(database);
                            if (musicDatabase.DocumentElement != null)
                            {
                                playlistSongs = musicDatabase.DocumentElement;
                                if (playlistSongs.HasAttributes)
                                {
                                    string playlistName = playlistSongs.GetAttribute("playlist");
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

        }

        /// <summary>
        /// Contains methods that can be used for reading, writing and manipulation (modification or removal) of
        /// playlists.
        /// </summary>
        internal static class DatabaseTools
        {
            /// <summary>
            /// Acts as an intermediary in order to remove the specified playlist from the playlists collection, the
            /// database, and playlist records in addition to removing the thumbnail that was used for it.
            /// </summary>
            /// <param name="playlistName"></param>
            /// <param name="playlistPath"></param>
            internal static void PlaylistRemovalHandler(string playlistName, string playlistPath)
            {
                var toBeRemovedPlaylist = MusicLibrary.Playlists.Find(playlistName);
                if (toBeRemovedPlaylist != null)
                {
                    MusicLibrary.Playlists.Remove(toBeRemovedPlaylist);
                }
                string playlistThumbnailPath = InternalSettings.playlistsFolder + playlistName + ".png";
                PlaylistTools.Records.RemovePlaylistFromRecords(playlistName);
                System.IO.File.Delete(playlistPath);
                if (System.IO.File.Exists(playlistThumbnailPath))
                {
                    System.IO.File.Delete(playlistThumbnailPath);
                }
            }

            /// <summary>
            /// Finds playlists, and Removes the specified song from this playlist, and optionally
            /// from all playlists.
            /// </summary>
            /// <param name="toBeRemovedSong"></param>
            /// <param name="ownerPlaylistName"></param>
            /// <param name="fromAllPlaylists"></param>
            internal static void SongRemovalHandler(Song toBeRemovedSong, string ownerPlaylistName, bool fromAllPlaylists = false)
            {
                List<string> existingDatabases = [];
                // First should find the playlist
                var tempDatabases = PlaylistTools.Records.PlaylistDatabaseFinder();
                if (tempDatabases != null && tempDatabases.Count > 0)
                {
                    foreach (var database in tempDatabases)
                    {
                        if (fromAllPlaylists == false)
                        {
                            existingDatabases.Add(database.Value);
                        }
                        else
                        {
                            if (DataFilteringTools.AreTheSame(database.Key, ownerPlaylistName, true))
                            {
                                existingDatabases.Add(database.Value);
                                break;
                            }
                        }
                    }
                    // Now Removal can take place.
                    if (existingDatabases.Count > 0)
                    {
                        foreach (string database in existingDatabases)
                        {
                            SongInfoTools.RemoveSongInfoAndPics(toBeRemovedSong, database);
                        }
                    }
                }
            }

            /// <summary>
            ///  Creates or Opens an Xml Document from/in the provided path, updates the playlist's name in the database in 
            ///  addition to the last updated attribute.
            /// </summary>
            /// <param name="playlistPath"></param>
            /// <param name="nameOfPlaylist"></param>
            /// <returns></returns>
            internal static XDocument LoadOrCreateDatabase(string playlistPath, string nameOfPlaylist)
            {
                XDocument playlistDatabase;
                XElement playlistSongs;
                if (System.IO.File.Exists(playlistPath))
                {
                    playlistDatabase = XDocument.Load(playlistPath);
                }
                else
                {
                    playlistDatabase = new XDocument();
                }
                if (playlistDatabase.Root != null)
                {
                    playlistSongs = playlistDatabase.Root;
                }
                else
                {
                    playlistSongs = new XElement(nameOfPlaylist.Replace(' ', '_'));
                    playlistSongs.SetAttributeValue("creationDate", DateTime.Now);
                    playlistDatabase.Add(playlistSongs);
                }
                playlistSongs.SetAttributeValue("playlist", nameOfPlaylist);
                playlistSongs.SetAttributeValue("lastUpdated", DateTime.Now);
                return playlistDatabase;
            }

            /// <summary>
            /// Populates the specified database with the provided songs.
            /// </summary>
            /// <param name="playlistPath"></param>
            /// <param name="nameOfPlaylist"></param>
            /// <param name="songsList"></param>
            /// <returns></returns>
            internal static XDocument PopulateDatabase(string playlistPath, string nameOfPlaylist, Song[] songsList)
            {
                XElement playlistSongs;
                XDocument playlistDatabase = LoadOrCreateDatabase(playlistPath, nameOfPlaylist);
                var tempRoot = playlistDatabase.Root;
                if (tempRoot != null)
                {
                    playlistSongs = tempRoot;
                }
                else
                {
                    playlistSongs = new XElement(nameOfPlaylist.Replace(' ', '_'));
                    playlistSongs.SetAttributeValue("playlist", nameOfPlaylist);
                    playlistSongs.SetAttributeValue("lastUpdated", DateTime.Now);
                    playlistDatabase.Add(playlistSongs);
                }
                if (songsList != null && songsList.Length > 0)
                {
                    foreach (Song song in songsList)
                    {
                        XElement convertedSong = SongToXElement(song);
                        playlistSongs.Add(convertedSong);
                    }
                }
                return playlistDatabase;
            }

            /// <summary>
            /// Saves the xml document to the specified path (Turned it into a method to avoid repetition).
            /// </summary>
            /// <param name="playlistDatabase"></param>
            /// <param name="playlistPath"></param>
            /// <param name="overWrite"></param>
            /// <returns></returns>
            internal static bool XmlWritingTool(XDocument playlistDatabase, string playlistPath, bool overWrite = false)
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
                if (overWrite) { options.Mode = FileMode.Create; }
                else { options.Mode = FileMode.OpenOrCreate; }
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
            /// Writes a song to the playlist's database.
            /// </summary>
            /// <param name="playlistPath"></param>
            /// <param name="nameOfPlaylist"></param>
            /// <param name="song"></param>
            internal static void SongWriter(string playlistPath, string nameOfPlaylist, Song song)
            {
                Song[] tempSong = [song];
                XDocument playlistDatabase = PlaylistTools.DatabaseTools.PopulateDatabase(playlistPath, nameOfPlaylist, tempSong);
                PlaylistTools.DatabaseTools.XmlWritingTool(playlistDatabase, playlistPath);
            }

            /// <summary>
            /// Writes a playlist's songs to the playlist's database, optionally overwriting it.
            /// </summary>
            /// <param name="playlistPath"></param>
            /// <param name="nameOfPlaylist"></param>
            /// <param name="songsInPlaylist"></param>
            /// <param name="overWrite"></param>
            /// <returns></returns>
            internal static bool PlaylistWriter(string playlistPath, string nameOfPlaylist, Song[] songsInPlaylist, bool overWrite = false)
            {
                XDocument playlistDatabase = PopulateDatabase(playlistPath, nameOfPlaylist, songsInPlaylist);
                bool PlaylistSaved = XmlWritingTool(playlistDatabase, playlistPath);
                return PlaylistSaved;
            }

            /// <summary>
            /// Reads a playlist (using either its name or path), and returns a list of songs (List of type Song). 
            /// Returns null if the file doesnt exist, database is empty or faulty.
            /// </summary>
            /// <param name="playlistName"></param>
            /// <param name="playlistPath"></param>
            /// <returns></returns>
            internal static List<Song>? PlaylistReader(string playlistName, string? playlistPath = null)
            {
                List<Song> songsInPlaylist = [];
                // If no path is provided, gets the playlistPath using the name of the playlist
                if (playlistPath == null)
                { playlistPath = PlaylistTools.Records.GetPlaylistPath(playlistName); }
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
                    else { }
                }
                return songsInPlaylist;
            }

            /// <summary>
            /// Creates and returns xml elements based on the given songs' properties.
            /// </summary>
            /// <param name="song"></param>
            /// <returns></returns>
            internal static XElement SongToXElement(Song song)
            {
                XElement convertedSong = new("Song");
                XElement Title = new("Title", song.Title);
                XElement Artist = new("Artist", song.Artist);
                XElement Album = new("Album", song.Album);
                XElement Path = new("Path", song.Path);
                XElement ThumbnailPath = new("ThumbnailPath", song.ThumbnailPath);
                XElement Duration = new("Duration", song.Duration);
                XElement Genres = new("Genres", song.Genres);
                XElement TrackNumber = new("TrackNumber", song.TrackNumber.ToString());
                XElement IsFavorite = new("IsFavorite", song.IsFavorite ? "1" : "0");
                XElement PlayedCount = new("AddedOnDateTime", song.AddedOn);
                XElement AddedOnDateTime = new("PlayedCount", song.PlayedCount);

                convertedSong.Add(Title);
                convertedSong.Add(Artist);
                convertedSong.Add(Album);
                convertedSong.Add(Path);
                convertedSong.Add(ThumbnailPath);
                convertedSong.Add(Duration);
                convertedSong.Add(Genres);
                convertedSong.Add(TrackNumber);
                convertedSong.Add(IsFavorite);
                convertedSong.Add(AddedOnDateTime);
                convertedSong.Add(PlayedCount);
                return convertedSong;
            }

            /// <summary>
            /// Creates a Song object using the provided XElement (from a playlist). Returns null if there 
            /// is no Path for the audio file or if the XElement's children are less than needed.
            /// </summary>
            /// <param name="songElement"></param>
            /// <returns></returns>
            internal static Song? XElementToSong(XElement songElement)
            {
                var tempSongInfo = songElement.Elements();
                // Every Element should have 8 child elements and two attributes.
                if (tempSongInfo.Count() == 11)
                {
                    string songTitle, songArtist, songAlbum, songPath, songGenres, songThumbnailPath;
                    TimeSpan songDuration;
                    DateTime songAddedOnDateTime;
                    int songTrackNumber, songPlayedCount;
                    bool songIsFavorite;
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
                    // For Track Number
                    var tempTrackNumber = songElement.Element("TrackNumber");
                    if (tempTrackNumber != null) { songTrackNumber = int.Parse((string)tempTrackNumber); }
                    else { songTrackNumber = 0; }
                    // For IsFavorite
                    var tempIsFavorite = songElement.Element("IsFavorite");
                    if (tempIsFavorite != null) { songIsFavorite = (bool)tempIsFavorite; }
                    else { songIsFavorite = false; }
                    // For AddedOnDateTime
                    var tempAddedOnDateTime = songElement.Element("AddedOnDateTime");
                    if (tempAddedOnDateTime != null) { songAddedOnDateTime = DateTime.Parse(tempAddedOnDateTime.Value); }
                    else { songAddedOnDateTime = DateTime.Now; }
                    // For PlayedCount
                    var tempPlayeedCount = songElement.Element("PlayedCount");
                    if (tempPlayeedCount != null) { songPlayedCount = int.Parse((string)tempPlayeedCount); }
                    else { songPlayedCount = 0; }
                    // Creates a Song object using the infos
                    Song newSong = new(songTitle, songArtist, songAlbum, songPath,
                                        songThumbnailPath, songDuration, songGenres,
                                        songTrackNumber, songIsFavorite, songAddedOnDateTime, songPlayedCount);
                    return newSong;
                }
                else
                { return null; }
            }

            /// <summary>
            /// Use to Change the name of the playlist (both the database file and the mentioned name within it) and by extention its path.
            /// </summary>
            /// <param name="newPlaylistName"></param>
            /// <param name="currentPlaylistPath"></param>
            /// /// <param name="currentPlaylistName"></param>
            /// <returns></returns>
            internal static string? EditPlaylistName(string newPlaylistName, string currentPlaylistName, string currentPlaylistPath)
            {
                if (DataFilteringTools.IsAcceptablePlaylistName(newPlaylistName))
                {
                    string tempNewPlaylistPath = InternalSettings.playlistsFolder + newPlaylistName + ".xml";
                    System.IO.File.Move(currentPlaylistPath, tempNewPlaylistPath);

                    XDocument playlistDatabase = XDocument.Load(tempNewPlaylistPath);
                    XElement? rootEle;
                    rootEle = playlistDatabase.Root;
                    LoadOrCreateDatabase(tempNewPlaylistPath, newPlaylistName);
                    XDocument records;
                    XElement playlists;
                    if (System.IO.File.Exists(InternalSettings.playlistsRecord))
                    {
                        records = XDocument.Load(InternalSettings.playlistsRecord);
                        var tempPlaylists = records.Root;
                        if (tempPlaylists != null)
                        {
                            playlists = tempPlaylists;
                            XName playlistXName = currentPlaylistName.Replace(' ', '_');
                            var oldPlaylistRecord = playlists.Element(playlistXName);
                            if (oldPlaylistRecord != null)
                            {
                                oldPlaylistRecord.Name = newPlaylistName.Replace(' ', '_');
                                oldPlaylistRecord.Value = tempNewPlaylistPath;
                            }
                            {
                                XElement updatedPlaylist = new XElement(newPlaylistName.Replace(' ', '_'), tempNewPlaylistPath);
                            }
                        }
                        else
                        {
                            PlaylistTools.Records.PlaylistRecorder(newPlaylistName, tempNewPlaylistPath);
                        }
                        XmlWritingTool(records, InternalSettings.playlistsRecord);
                    }
                    else
                    {
                        PlaylistTools.Records.PlaylistRecorder(newPlaylistName, tempNewPlaylistPath);
                    }
                    return newPlaylistName;
                }
                else { return null; }
            }
        }

    }
}
