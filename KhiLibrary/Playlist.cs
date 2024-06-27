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
        // *** Add method for Reading database either here or in MusicLibrary
        // and Add option for the Sort be be either ascending or descending
        // Notes for myself:
        // Check if a playlist with the new name exists --> Done
        // Create an empty xml for the playlist --> Done
        // Add song to playlist --> Write to playlist --> Done
        // Remove song from playlist --> Done
        // Keep all of the songs within --> Done
        // sort orders based on each of the songs' attributes --> make enum for the songs attributes
        // make enums for each of the songs?

        #region fields

        private int index;
        private int count;
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
        /// The Songs in the playlist.
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
            Indexer();
            songsList = new Songs();
        }

        /// <summary>
        /// Creates a playlist.
        /// </summary>
        /// <param name="name"></param>
        public Playlist(string name)
        {
            if (!KhiUtils.PlaylistTools.PlaylistAlreadyExists(name))
            {
                playlistName = name;
                playlistPath = Application.StartupPath + name + ".xml";
                creationDate = DateTime.Now;
                lastUpdated = DateTime.Now;
                songsList = new Songs(playlistName, playlistPath);
                CreatePlaylist();
                Indexer();
                
            }
            else
            {
                // add code to load a previously constructed playlist or throw exception or sth like that
                // Check to see if a playlist with that name exists, gets its path and loads it if there is.
                var tempSongList = KhiUtils.PlaylistTools.PlaylistReader(name);
                if (tempSongList != null)
                {
                    var path = KhiUtils.PlaylistTools.GetPlaylistPath(name);
                    if (path != null)
                    {
                        Songs newSongs = new Songs(tempSongList, name, path);
                        if (newSongs != null) 
                        { 
                            songsList = newSongs;
                            // ADD CODE to get the dates from the database
                            playlistName = string.Empty;
                            playlistPath = string.Empty;
                        }
                    }
                    
                    
                }
                
                throw new Exception("A playlist with that name already exists");
            }
        }

        /// <summary>
        /// Creates a playlist by using an already constructed Songs object, and a name and path 
        /// of the playlist. This is mainly used for loading existing playlists from the database.
        /// </summary>
        /// <param name="listOfSongs"></param>
        /// <param name="name"></param>
        /// <param name="path"></param>
        public Playlist (Songs listOfSongs, string name, string path)
        {
            playlistName = name;
            playlistPath = path;
            // Should include these two as Attributes in the database
            creationDate = DateTime.Now;
            lastUpdated = DateTime.Now;
            songsList = listOfSongs;
        }
        #endregion

        #region instanceMethods

        /// <summary>
        /// Rereads the Songs from the database.
        /// </summary>
        public void Reload ()
        {
            // Add Code here --> Should reread from the database
            var tempSongList = KhiUtils.PlaylistTools.PlaylistReader(playlistName, playlistPath);            
            Songs newSongs = new Songs(tempSongList, playlistName, playlistPath);
            if (newSongs != null)
            {
                songsList.Clear();
                songsList = newSongs;
            }            
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
        /// Clears the songs that exists in the playlist.
        /// </summary>
        public async void Clear()
        {
            // Could also add the option of removing all of the songs
            songsList.Clear();
        }

        /// <summary>
        /// Sorts the songs inside the playlist based on their Title (0), Artist (1), Album (2), 
        /// Duration(seconds)(3), and Genre(4).
        /// </summary>
        /// <param name="columnNumber"></param>
        public async void Sort(int columnNumber)
        {
            await Task.Run(() =>
            {
                songsList = songsList.Sort(columnNumber);
            });
        }

        /// <summary>
        /// Saves the playlist songs.
        /// </summary>
        public async void Save()
        {
            await Task.Run(() =>
            {
                List<Song> tempSongs = KhiUtils.DataFilteringTools.FilterDuplicates(songsList.ToList(), playlistPath);
                if (tempSongs.Count > 0)
                {
                    KhiUtils.PlaylistTools.PlaylistWriter(playlistPath, playlistName, tempSongs);
                }
            });
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
            if (KhiUtils.PlaylistTools.IsAcceptablePlaylistName(newPlaylistName))
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
        private async void CreatePlaylist()
        {
            await Task.Run(() =>
            {
                XDocument playlistDatabase = KhiUtils.PlaylistTools.DatabaseElementCreator(playlistPath, playlistName, songsList.ToList());
                KhiUtils.PlaylistTools.XmlWritingTool(playlistDatabase, playlistPath);
                KhiUtils.PlaylistTools.PlaylistRecorder(playlistName, playlistPath);
            });
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

        /// <summary>
        /// Keeps the number of playlists
        /// </summary>
        /// <param name="removed"></param>
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
        #endregion

    }
}
