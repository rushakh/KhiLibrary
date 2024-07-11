using System.Xml;
using System.Xml.Linq;
using Application = System.Windows.Forms.Application;

namespace KhiLibrary
{
    /// <summary>
    /// An Object containing a playlist and its songs.
    /// </summary>
    public class Playlist
    {
        #region fields
        private string playlistName;
        private string playlistPath;
        private DateTime creationDate;
        private DateTime lastUpdated;
        private Songs songsList;
        #endregion

        #region properties
        // Acessors
        /// <summary>
        /// Returns the Name of the playlist.
        /// </summary>
        public string Name
        {
            get
            {
                return playlistName;
            }
            set
            {
                if (value is not null && value is string)
                {
                    var temp = SetPlaylistName(value);
                    if (temp != null) { playlistName = temp; }
                }
            }
        }

        /// <summary>
        /// Returns the location of the playlist's xml database.
        /// </summary>
        public string Path { get => playlistPath; set => playlistPath = value; }

        /// <summary>
        /// Returns the sum of the songs' duration 
        /// </summary>
        public TimeSpan TotalPlayTime { get => songsList.TotalPlayTime; }

        /// <summary>
        /// The date that the playlist was created on.
        /// </summary>
        public DateTime CreationDate { get => creationDate; }

        /// <summary>
        /// Returns the last datetime in which the playlist was updated.
        /// </summary>
        public DateTime LastUpdated { get => songsList.LastUpdated; }

        /// <summary>
        /// The collection of Songs in the playlist.
        /// </summary>
        public Songs Songs { get => songsList; }
        #endregion 

        #region constructors
        /// <summary>
        /// Creates an empty instance of the Playlist Class. Do not use this in Normal circumstances
        /// </summary>
        public Playlist()
        {
            playlistName = string.Empty;
            playlistPath = string.Empty;
            creationDate = DateTime.Now;
            lastUpdated = DateTime.Now;
            songsList = new Songs();
        }

        /// <summary>
        /// Creates a playlist.
        /// </summary>
        /// <param name="name"></param>
        public Playlist(string name)
        {
            if (!PlaylistTools.PlaylistAlreadyExists(name))
            {
                playlistName = name;
                // To Remove spaces from the name so a file with that name can be created.
                string tempPath = name.Trim(' ');
                playlistPath = Application.StartupPath + tempPath + ".xml";
                creationDate = DateTime.Now;
                lastUpdated = DateTime.Now;
                songsList = new Songs(playlistName, playlistPath);
                CreatePlaylist();
            }
            else
            {
                throw new Exception("A playlist with that name, " + name + ",already exists");
            }
        }

        /// <summary>
        /// Creates a playlist by using an already constructed Songs object, and a name and path 
        /// of the playlist. This is mainly used for loading existing playlists from the database.
        /// </summary>
        /// <param name="listOfSongs"></param>
        /// <param name="name"></param>
        /// <param name="path"></param>
        public Playlist(Songs listOfSongs, string name, string path)
        {
            playlistName = name;
            playlistPath = path;
            // Should include these two as Attributes in the database
            creationDate = DateTime.Now;
            lastUpdated = DateTime.Now;
            songsList = listOfSongs;
            if (!InternalSettings.prepareForVirtualMode)
            {
                PrepareSongsArts();
            }
        }
        #endregion

        #region instanceMethods

        /// <summary>
        /// Rereads the Songs from the database.
        /// </summary>
        public void Reload()
        {
            var tempSongList = PlaylistTools.PlaylistReader(playlistName, playlistPath);
            if (tempSongList != null)
            {
                Songs newSongs = new Songs(tempSongList, playlistName, playlistPath);
                songsList.Clear();
                songsList = newSongs;
            }
            else
            { throw new Exception("An Error was encountered reading from the playlist Database"); }
        }

        /// <summary>
        /// Extracts the album arts of the songs in the playlist ahead of time, to improve performance later on.
        /// </summary>
        public void PrepareSongsArts()
        {
            Parallel.ForEach(songsList.ToList(), song =>
            {
                song.PrepareArt();
            });
            GC.WaitForPendingFinalizers();
            int gen = GC.MaxGeneration;
            GC.Collect(gen, GCCollectionMode.Aggressive);
        }

        /// <summary>
        /// Disposes the loaded album arts of the songs collection and clears memory.
        /// </summary>
        /// <param name="deleteTempImages"></param>
        public void UnloadSongArts(bool deleteTempImages = false)
        {
            for (int i = 0; i < songsList.Count; i++)
            {
                var song = songsList[i];
                song.UnloadArt(deleteTempImages);
            }
            GC.WaitForPendingFinalizers();
            int gen = GC.MaxGeneration;
            GC.Collect(gen, GCCollectionMode.Aggressive);
        }

        /// <summary>
        /// Deletes the playlist.
        /// </summary>
        public async void Remove()
        {
            await Task.Run(() =>
            {
                songsList.Clear();
                RemovePlaylist(playlistPath);
                playlistName = string.Empty;
                playlistPath = string.Empty;
                lastUpdated = DateTime.Now;
            });
        }

        /// <summary>
        /// Clears the songs that exists in the playlist and optionally removes them from the playlist.
        /// </summary>
        public void Clear(bool removeAllSongs)
        {
            // Could also add the option of removing all of the songs
            songsList.Clear();
        }

        /// <summary>
        /// Sorts the songs inside the playlist based on their Title (0), Artist (1), Album (2), 
        /// Duration(seconds)(3), and Genre(4).
        /// </summary>
        /// <param name="columnNumber"></param>
        public void Sort(int columnNumber)
        {
            songsList = songsList.Sort(columnNumber);
        }

        /// <summary>
        /// Saves the playlist songs.
        /// </summary>
        public void Save()
        {
            PlaylistTools.PlaylistWriter(playlistPath, playlistName, songsList.ToArray());
            lastUpdated = DateTime.Now;
        }
        #endregion

        #region privateInnerWorkings

        /// <summary>
        /// Use to Change the name of the playlist (both the database file and the mentioned name within it).
        /// </summary>
        /// <param name="newPlaylistName"></param>
        /// <returns></returns>
        private string? SetPlaylistName(string newPlaylistName)
        {
            if (DataFilteringTools.IsAcceptablePlaylistName(newPlaylistName))
            {
                try
                {
                    string tempNewPlaylistPath = Application.StartupPath + newPlaylistName + ".xml";
                    XmlDocument playlistDatabase = new();
                    XmlElement? rootEle;
                    playlistDatabase.LoadXml(playlistPath);
                    rootEle = playlistDatabase.DocumentElement;
                    if (rootEle != null) { rootEle.SetAttribute("playlistName", newPlaylistName); }
                    System.IO.File.Move(playlistPath, tempNewPlaylistPath);
                    return newPlaylistName;
                }
                catch
                { return playlistName; }
            }
            else { return null; }
        }

        /// <summary>
        /// To create an empty Xml for this playlist
        /// </summary>
        private void CreatePlaylist()
        {
            XDocument playlistDatabase = PlaylistTools.DatabaseElementCreator(playlistPath, playlistName, songsList.ToArray());
            PlaylistTools.XmlWritingTool(playlistDatabase, playlistPath);
            PlaylistTools.PlaylistRecorder(playlistName, playlistPath);
        }

        /// <summary>
        /// Removes the database of a playlist by using the provided path.
        /// </summary>
        /// <param name="playlistPath"></param>
        private static void RemovePlaylist(string playlistPath)
        {
            var temp = playlistPath.Split('.', StringSplitOptions.None);
            if (System.IO.File.Exists(playlistPath) && temp.Last().ToLower() == "xml")
            {
                System.IO.File.Delete(playlistPath);
            }
        }
        #endregion
    }
}
