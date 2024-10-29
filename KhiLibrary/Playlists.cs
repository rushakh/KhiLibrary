
namespace KhiLibrary
{
    /// <summary>
    /// An object containing a collection of Playlist objects.
    /// </summary>
    public class Playlists : IList<Playlist>
    {
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
        /// The default playlist where all songs are added to at first.
        /// </summary>
        public Playlist AllSongs
        {
            get
            {
                try
                {
                    Playlist allSongsPlaylist = this["All Songs"];
                    return allSongsPlaylist;
                }
                catch (KeyNotFoundException)
                {
                    playlistsList.Add(new Playlist("All Songs"));
                    Playlist allSongsPlaylist = this["All Songs"];
                    return allSongsPlaylist;
                }
            }
        }

        /// <summary>
        /// Returns the Favorites playlists which contains the songs marked as favorite.
        /// </summary>
        public Playlist Favorites
        {
            get
            {
                try
                {
                    Playlist favoriteSongsPlaylist = this["Favorites"];
                    return favoriteSongsPlaylist;
                }
                catch (KeyNotFoundException)
                {
                    playlistsList.Add(new Playlist("Favorites"));
                    Playlist favoriteSongsPlaylist = this["Favorites"];
                    return favoriteSongsPlaylist;
                }
            }
        }

        /// <summary>
        /// Indicates whether the collection is readonly (it's not).
        /// </summary>
        public bool IsReadOnly {  get => false;}

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
                bool allSongPlaylistExists = false;
                bool favoritePlaylistExists = false;
                foreach (Playlist playlist in playlistsList)
                {
                    if (playlist.Name == "All Songs") { allSongPlaylistExists = true; }
                    if (playlist.Name == "Favorites") { favoritePlaylistExists = true; }
                }
                if (!allSongPlaylistExists)
                {
                    Playlist allSongsPlaylist = new Playlist("All Songs");
                    playlistsList.Add(allSongsPlaylist);
                }
                if (!favoritePlaylistExists)
                {
                    Playlist favorites = new Playlist("Favorites");
                    playlistsList.Add(favorites);
                }
            }
            else
            {
                Playlist allSongsPlaylist = new Playlist("All Songs");
                Playlist favorites = new Playlist("Favorites");
                playlistsList.Add(allSongsPlaylist);
                playlistsList.Add(favorites);
            }
            InternalSettings.CreateDirectories();
        }

        #region instanceMethods

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return playlistsList.GetEnumerator(); }

        /// <summary>
        /// Returns an Enumerator that iterates through playlists in this Playlists collection.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Playlist> GetEnumerator()
        {
            return playlistsList.GetEnumerator();
        }

        /// <summary>
        /// Returns the playlist with the provided index. Throws exceptions if trying to set default 
        /// playlists (e.g., All Songs, Favorites).
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public Playlist this[int index]
        {
            get
            {
                return playlistsList[index];
            }
            set
            {
                if (value is not null && !value.isDefaultPlaylist)
                {
                    if (playlistsList != null && playlistsList.Count >= index)
                    {
                        if (playlistsList[index].isDefaultPlaylist)
                        {
                            throw new InvalidOperationException("Cannot Edit or Remove default playlists");
                        }
                        else
                        {
                            playlistsList[index] = value;
                        }
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException ("Index is out of range.");
                    }
                }
                else
                {
                    throw new ArgumentException("Value must be an instance of the Playlist class and not null.");
                }
            }
        }

        /// <summary>
        /// Returns the playlist with the provided string as Name. Throws exception If requested Playlist
        /// does not exist or when trying to set default playlists.
        /// </summary>
        /// <param name="playlistName"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public Playlist this[string playlistName]
        {
            get
            {
                var tempFoundPlaylist = this.Find(playlistName);
                if (tempFoundPlaylist != null)
                {
                    return tempFoundPlaylist;
                }
                else
                {
                    throw new KeyNotFoundException ("A Playlist with the name <" + playlistName + "> does not exist within the collection.");
                }
            }
            set
            {
                var index = playlistsList.FindIndex(s => s.Name == playlistName);
                if (index != -1)
                {
                    var foundPlaylist = this.Find(playlistName);
                    if (foundPlaylist != null)
                    {
                        if (foundPlaylist.isDefaultPlaylist)
                        {
                            throw new InvalidOperationException("Cannot Edit or Remove default playlists");
                        }
                        else
                        {
                            playlistsList[index] = foundPlaylist;
                        }
                    }
                }
                else
                {
                    throw new KeyNotFoundException("A Playlist with the name <" + playlistName + "> does not exist within the collection.");
                }
            }
        }

        /// <summary>
        /// If there exists previously made and saved playlists, will asynchronously load them into this instance
        /// </summary>
        public void LoadExistingDatabases()
        {
            playlistsList.Clear();
            var existingPlaylists = PlaylistTools.Records.ReadPlaylistsRecords();
            if (existingPlaylists != null && existingPlaylists.Count > 0)
            {
                foreach (var pList in existingPlaylists)
                {
                    var tempSongs = PlaylistTools.DatabaseTools.PlaylistReader(pList.Key, pList.Value);
                    if (tempSongs != null)
                    {
                        Songs newSongs = new Songs(tempSongs, pList.Key, pList.Value);
                        playlistsList.Add(new Playlist(newSongs, pList.Key, pList.Value));
                    }
                }
            }
        }

        /// <summary>
        /// Creates and adds a database with the specified name to the database.
        /// </summary>
        /// <param name="playlistName"></param>
        public void Add(string playlistName)
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
        public void Add(Playlist newPlaylist)
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
        /// Determines if the specified element exists within the collections.
        /// </summary>
        /// <param name="playlist"></param>
        /// <returns></returns>
        public bool Contains (Playlist playlist)
        {
            return playlistsList.Contains(playlist);
        }

        /// <summary>
        /// Copies the collection to the mentioned array starting from the given index.
        /// </summary>
        /// <param name="destinationArray"></param>
        /// <param name="startingIndex"></param>
        public void CopyTo(Playlist[] destinationArray, int startingIndex)
        {
            playlistsList.CopyTo(destinationArray, startingIndex);
        }

        /// <summary>
        /// Searches for a playlist by its exact name and returns it. Returns null if no 
        /// playlist by that name is found.
        /// </summary>
        /// <param name="playlistName"></param>
        /// <returns></returns>
        public Playlist? Find(string playlistName)
        {
            for (int i = 0; i < playlistsList.Count; i++)
            {
                Playlist playlist = playlistsList[i];
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
            // Just in case a playlist with that name already exists.
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
            playlistsList.Add(newImportedPlaylist);
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
        /// Returns the index of the specified playlist (first occurrence). Returns -1 if the specified element does not exists.
        /// </summary>
        /// <param name="playlist"></param>
        /// <returns></returns>
        public int IndexOf(Playlist playlist)
        {
            return playlistsList.IndexOf(playlist);
        }

        /// <summary>
        /// Inserts the given playlist into the collection at the specified index.
        /// </summary>
        /// <param name="destinationIndex"></param>
        /// <param name="playlist"></param>
        public void Insert (int destinationIndex, Playlist playlist)
        {
            playlistsList.Insert(destinationIndex, playlist);
        }

        /// <summary>
        /// Removes the specified playlist from the collection. Will throw an exception if the playlist is 
        /// one of the default playlists (e.g., All Songs, Favorites).
        /// </summary>
        /// <param name="toBeRemovePlaylist"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public bool Remove(Playlist toBeRemovePlaylist)
        {
            if (toBeRemovePlaylist.isDefaultPlaylist)
            {
                throw new InvalidOperationException("Cannot Edit or Remove default playlists");
            }
            else
            {
                return playlistsList.Remove(toBeRemovePlaylist);
            }
        }

        /// <summary>
        /// Removes the playlist at the specified index in the collection. Will throw an exception if the playlist is 
        /// one of the default playlists (e.g., All Songs, Favorites).
        /// </summary>
        /// <param name="index"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void RemoveAt(int index)
        {
            if (playlistsList[index].isDefaultPlaylist)
            {
                throw new InvalidOperationException("Cannot Edit or Remove default playlists");
            }
            else
            {
                playlistsList.RemoveAt(index);
            }
        }

        /// <summary>
        /// Saves all the playlists in the collection to database.
        /// </summary>
        public void Save()
        {
            foreach(Playlist playlist in playlistsList)
            {
                playlist.Save();
            }
        }

        #endregion
    }
}