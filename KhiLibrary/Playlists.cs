using System.Xml.Linq;

namespace KhiLibrary
{
    /// <summary>
    /// An object containing a collection of Playlist objects.
    /// </summary>
    public class Playlists
    {
        //private int count;
        private List<Playlist> playlistsList;

        /// <summary>
        /// The number of playlists in the collection
        /// </summary>
        public int Count { get => playlistsList.Count; }

        /// <summary>
        /// The collection of playlists.
        /// </summary>
        public List<Playlist> PlaylistsList { get => playlistsList; }

        /// <summary>
        /// Constructs an empty collection to contain Playlist objects.
        /// </summary>
        public Playlists(bool loadExistingPlaylists = false)
        {
            InternalSettings.CreateDirectories();
            playlistsList = new List<Playlist>();
            if (loadExistingPlaylists)
            {
                LoadExistingDatabases();
                if (playlistsList == null || playlistsList.Count == 0)
                {
                    Playlist allSongsPlaylist = new Playlist("All Songs Playlist");
                    playlistsList.Add(allSongsPlaylist);
                }
            }
            else
            {
                Playlist allSongsPlaylist = new Playlist("All Songs Playlist");
                playlistsList.Add(allSongsPlaylist);
            }
            InternalSettings.CreateDirectories();
        }

        #region instanceMethods
        /// <summary>
        /// Returns an Enumerator that iterates through playlists in this Playlists collection.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Playlist> GetEnumerator()
        {
            return playlistsList.GetEnumerator();
        }
        /// <summary>
        /// Returns the playlist with the provided index. If requested index is bigger than the number of 
        /// playlists or if no playlist has been added yet, returns null.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public Playlist? this[int index]
        {
            get
            {
                try
                {
                    if (playlistsList != null && playlistsList.Count >= index)
                    {
                        return playlistsList[index];
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
                if (value is not null && value is Playlist)
                {
                    if (playlistsList != null && playlistsList.Count >= index)
                    {
                        playlistsList[index] = value;
                    }
                    else
                    {
                        throw new InvalidOperationException("Index is out of range.");
                    }
                }
                else
                {
                    throw new ArgumentException("Value must be an instance of the Playlist class and not null.");
                }
            }
        }

        /// <summary>
        /// Indexer -Returns the playlist with the provided string as Name. If requested Playlist does not exist 
        /// returns null.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public Playlist? this[string name]
        {
            get
            {
                var tempFoundPlaylist = FindPlaylist(name);
                return tempFoundPlaylist;
            }
            set
            {
                var index = playlistsList.FindIndex(s => s.Name == name);
                if (index != -1)
                {
                    var foundPlaylist = FindPlaylist(name);
                    if (foundPlaylist != null)
                    {
                        playlistsList[index] = foundPlaylist;
                    }
                }
                else
                {
                    throw new ArgumentOutOfRangeException("A playlist with the specified "+ name +" does not exist.");
                }
            }
        }

        /// <summary>
        /// If there exists previously made and saved playlists, will asynchronously load them into this instance
        /// </summary>
        public void LoadExistingDatabases()
        {
            playlistsList.Clear();
            var existingPlaylists = PlaylistTools.ReadPlaylistsRecords();
            if (existingPlaylists != null && existingPlaylists.Count > 0)
            {
                foreach (var pList in existingPlaylists)
                {
                    var tempSongs = PlaylistTools.PlaylistReader(pList.Key, pList.Value);
                    if (tempSongs != null)
                    {
                        Songs newSongs = new Songs(tempSongs, pList.Key, pList.Value);
                        playlistsList.Add(new Playlist(newSongs, pList.Key, pList.Value));
                    }
                }
            }
        }

        /// <summary>
        /// If there exists previously made and saved playlists, will load them into this instance
        /// </summary>
        public async void LoadExistingDatabasesAsync()
        {
            await Task.Run(() =>
            {
                LoadExistingDatabases();
            });
        }

        /// <summary>
        /// Creates and adds a database with the specified name to the database.
        /// </summary>
        /// <param name="playlistName"></param>
        public void AddPlaylist(string playlistName)
        {
            if (DataFilteringTools.IsAcceptablePlaylistName(playlistName))
            {
                playlistsList.Add(new Playlist(playlistName));
            }
            else
            { throw new Exception("Invalid Playlist Name"); }
        }

        /// <summary>
        /// Adds an already constructed playlist to the list.
        /// </summary>
        /// <param name="newPlaylist"></param>
        public void AddPlaylist(Playlist newPlaylist)
        {
            playlistsList.Add(newPlaylist);
        }

        /// <summary>
        /// Adds a collection of playlists (List of type Playlist) to the database.
        /// </summary>
        /// <param name="newPlaylistList"></param>
        public void AddRange(List<Playlist> newPlaylistList)
        {
            if (newPlaylistList != null && newPlaylistList.Count > 0)
            {
                playlistsList.AddRange(newPlaylistList);
            }
        }

        /// <summary>
        /// ***SHOULD ALSO REMOVE FROM DATABASE- Clears the songs in the playlist,
        /// </summary>
        public async void Clear()
        {
            playlistsList.Clear();
            // Remove from data base
            await Task.Run(() =>
            {

            });
        }

        /// <summary>
        /// Search for a playlist by its name (case insensitive).
        /// </summary>
        /// <param name="playlistName"></param>
        /// <returns></returns>
        public Playlist? FindPlaylist(string playlistName)
        {
            foreach (var playlist in playlistsList)
            {
                if (playlist.Name == playlistName)
                {
                    return playlist;
                }
            }
            // If the code reaches here, then nothing has been found
            return null;
        }

        /// <summary>
        /// Exports all the playlists as .m3u8 files
        /// </summary>
        public string[] ExportAllPlaylists ()
        {
            List<string> exportedPlaylistsPaths = new List<string>();
            foreach (Playlist playlist in playlistsList)
            {
                try
                {
                    string exportedFilePath = playlist.ExportPlaylist();
                    exportedPlaylistsPaths.Add(exportedFilePath);
                }
                catch
                { continue; }
            }
            return exportedPlaylistsPaths.ToArray();
        }

        /// <summary>
        /// Imports a playlist (.m3u or .m3u8 files). In case the playlists with the imported playlists' names
        /// already exist, will add a number to the end of their names.
        /// </summary>
        /// <param name="m3uOrM3u8FilePath"></param>
        public void ImportPlaylist (string m3uOrM3u8FilePath)
        {
            (string importedPlaylistName, string[] importedPlaylistSongsPaths) = KhiUtils.ExtractSongsPathsFromM3uPlaylist(m3uOrM3u8FilePath);
            string tempDatabaseName = importedPlaylistName.Trim(' ');
            string playlistDatabasePath = InternalSettings.playlistsFolder + tempDatabaseName + ".xml";
            // Just in case a playlist with that name already exists
            int i = 0;
            while (System.IO.File.Exists(playlistDatabasePath))
            {
                importedPlaylistName = importedPlaylistName + i;
                tempDatabaseName = importedPlaylistName.Trim(' ');
                playlistDatabasePath = InternalSettings.playlistsFolder + tempDatabaseName + ".xml";
                i++;
            }
            Songs importedPlaylistSongs = new Songs(importedPlaylistName, playlistDatabasePath);
            importedPlaylistSongs.AddRange(importedPlaylistSongsPaths);
            Playlist newImportedPlaylist = new Playlist(importedPlaylistSongs, importedPlaylistName, playlistDatabasePath);
        }

        /// <summary>
        /// Imports multiple playlists (.m3u or .m3u8 files). In case the playlists with the imported playlists' names
        /// already exist, will add a number to the end of their names. Optionally, will add the playlists in parallel
        /// which will be faster if the imported playlists are large but as a consequence they will not be added in the 
        /// order they were added, and the process will be accompanied by a sudden spike in RAM and CPU usage.
        /// </summary>
        /// <param name="m3uOrM3u8FilePaths"></param>
        /// <param name="AddPlaylistsInParallel"></param>
        public void ImportPlaylists(string[] m3uOrM3u8FilePaths, bool AddPlaylistsInParallel = false)
        {
            if (AddPlaylistsInParallel)
            {
                Parallel.ForEach(m3uOrM3u8FilePaths, m3uOrM3u8FilePath =>
                {
                    ImportPlaylist(m3uOrM3u8FilePath);
                });
            }
            else
            {
                foreach (string m3uOrM3u8FilePath in m3uOrM3u8FilePaths)
                {
                    ImportPlaylist(m3uOrM3u8FilePath);
                }
            }    
        }

        /// <summary>
        /// Remove
        /// </summary>
        /// <param name="toBeRemovePlaylist"></param>
        public async void RemovePlaylist(Playlist toBeRemovePlaylist)
        {
            await Task.Run(() => playlistsList.Remove(toBeRemovePlaylist));
        }

        #endregion
    }
}