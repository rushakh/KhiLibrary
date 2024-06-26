namespace KhiLibrary
{
    public class Playlists
    {
        //private int count;
        private List<Playlist> playlistsList;

        public int Count { get => playlistsList.Count; }

        public List<Playlist> PlaylistsList { get => playlistsList; }

        public Playlists()
        {
            playlistsList = new List<Playlist>();
        }

        #region instanceMethods
        /// <summary>
        /// Indexer -Returns the playlist with the provided index. If requested index is bigger than the number of 
        /// playlists or if no playlist has been added yet, returns null.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public Playlist this[int index]
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
        /// If there exists previously made and saved playlists, will load them into this instance
        /// </summary>
        public async void LoadExistingDatabases()
        {
            await Task.Run(() =>
            {
                var existingPlaylists = KhiUtils.PlaylistTools.ReadPlaylistsRecords();
                if (existingPlaylists != null && existingPlaylists.Count > 0)
                {
                    foreach (var pList in existingPlaylists)
                    {
                        var tempSongs = KhiUtils.PlaylistTools.PlaylistReader (pList.Key, pList.Value);
                        if (tempSongs != null)
                        {
                            Songs newSongs = new Songs(tempSongs, pList.Key, pList.Value);
                            Playlist newPlaylist = new Playlist(newSongs, pList.Key, pList.Value);
                            AddPlaylist(newPlaylist);
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Creates and adds a database with the specified name to the database.
        /// </summary>
        /// <param name="playlistName"></param>
        public void AddPlaylist(string playlistName)
        {
            if (KhiUtils.PlaylistTools.IsAcceptablePlaylistName(playlistName))
            {
                Playlist newPlaylist = new(playlistName);
                if (playlistsList != null) { playlistsList.Add(newPlaylist); }
                else
                { 
                    playlistsList = new List<Playlist>();
                    playlistsList.Add(newPlaylist);
                }
                
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
        /// Adds a collection of Playlist (List<Playlist>) to the database.
        /// </summary>
        /// <param name="newPlaylistList"></param>
        public void AddRange(List<Playlist> newPlaylistList)
        {
            if (newPlaylistList != null & newPlaylistList.Count > 0)
            {
                foreach (Playlist playlist in newPlaylistList)
                {
                    playlistsList.Add(playlist);
                }
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
        /// Search for a playlist by its name (case incensitive).
        /// </summary>
        /// <param name="playlistName"></param>
        /// <returns></returns>
        public Playlist? FindPlaylist(string playlistName)
        {
            foreach (Playlist playlist in playlistsList)
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
        /// Remove
        /// </summary>
        /// <param name="toBeRemovePlaylist"></param>
        public async void RemovePlaylist(Playlist toBeRemovePlaylist)
        {
            await Task.Run(() =>
            {
                playlistsList.Remove(toBeRemovePlaylist);
            });
        }

        #endregion

        #region privateInnerWorkings

        #endregion
    }
}
