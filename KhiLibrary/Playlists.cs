using System.Windows.Media;

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
            playlistsList = new List<Playlist>();
            if (loadExistingPlaylists) 
            {
                LoadExistingDatabases();
            }           
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
        /// Indexer -Returns the playlist with the provided index. If requested index is bigger than the number of 
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
                if (tempFoundPlaylist != null)
                { return tempFoundPlaylist; }
                else
                { return null;}
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
                    throw new ArgumentOutOfRangeException("A playlist with the specified name does not exist.");
                }
            }
        }

        /// <summary>
        /// If there exists previously made and saved playlists, will asynchronously load them into this instance
        /// </summary>
        public void LoadExistingDatabases()
        {
            playlistsList.Clear();
            var existingPlaylists = KhiUtils.PlaylistTools.ReadPlaylistsRecords();
            if (existingPlaylists != null && existingPlaylists.Count > 0)
            {
                foreach (var pList in existingPlaylists)
                {
                    var tempSongs = KhiUtils.PlaylistTools.PlaylistReader(pList.Key, pList.Value);
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
            if (KhiUtils.PlaylistTools.IsAcceptablePlaylistName(playlistName))
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
        /// Adds a collection of Playlist (List<Playlist>) to the database.
        /// </summary>
        /// <param name="newPlaylistList"></param>
        public void AddRange(List<Playlist> newPlaylistList)
        {
            if (newPlaylistList != null && newPlaylistList.Count > 0)
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
