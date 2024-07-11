using System.Windows.Forms;

namespace KhiLibrary
{
    /// <summary>
    /// Holds the playlist's songs and provides tools for adding and removing songs.
    /// </summary>
    public class Songs
    {
        #region fields
        private readonly string ownerPlaylistName;
        private readonly string ownerPlaylistPath;
        private DateTime songsListLastUpdated;
        private TimeSpan songsListTotalPlaytime;
        private List<Song> songs;
        #endregion

        #region properties
        // Acessors
        /// <summary>
        /// Returns the name of the playlist this list of songs belongs to.
        /// </summary>
        public string PlaylistName { get => ownerPlaylistName; }
        /// <summary>
        /// Returns the DateTime of the last time this list of songs was updated.
        /// </summary>
        internal DateTime LastUpdated { get => songsListLastUpdated; }
        /// <summary>
        /// Returns the sum of the durations of the songs in this list.
        /// </summary>
        internal TimeSpan TotalPlayTime { get => songsListTotalPlaytime; }
        /// <summary>
        /// the number of songs in the list
        /// </summary>
        public int Count
        {
            get
            {
                if (songs != null) { return songs.Count; }
                else { return 0; }
            }
        }
        #endregion

        #region constructors
        /// <summary>
        /// Constructs an empty instance of Songs class
        /// </summary>
        public Songs()
        {
            ownerPlaylistName = string.Empty;
            ownerPlaylistPath = string.Empty;
            songsListLastUpdated = DateTime.Now;
            songsListTotalPlaytime = TimeSpan.Zero;
            songs = [];
        }

        /// <summary>
        /// Constructs an instance of the Songs class
        /// </summary>
        /// <param name="playlistName"></param>
        /// <param name="playlistPath"></param>
        public Songs(string playlistName, string playlistPath)
        {
            ownerPlaylistName = playlistName;
            ownerPlaylistPath = playlistPath;
            songsListLastUpdated = DateTime.Now;
            songsListTotalPlaytime = TimeSpan.Zero;
            songs = [];
        }

        /// <summary>
        /// Creates a Songs object using an already connstructed list of songs(List of type Song), in addition 
        /// to the playlist's name and path. This is mainly used for loading an already existing playlist 
        /// from the database.
        /// </summary>
        /// <param name="songsList"></param>
        /// <param name="playlistName"></param>
        /// <param name="playlistPath"></param>
        public Songs(List<Song> songsList, string playlistName, string playlistPath)
        {
            songs = songsList;
            ownerPlaylistName = playlistName;
            ownerPlaylistPath = playlistPath;
            songsListLastUpdated = DateTime.Now;
            songsListTotalPlaytime = calculateTotallPlaytime();
        }
        #endregion

        /// <summary>
        /// Returns the song with the provided index. If requested index is bigger than the number of 
        /// songs or if no song has been added yet, returns null.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Song this[int index]
        {
            get
            {
                if (songs != null && songs.Count >= index)
                {
                    return songs[index];
                }
                else
                {
                    throw new ArgumentNullException();
                }
            }
            set
            {
                if (value is not null && value is Song)
                {
                    if (songs != null && songs.Count >= index)
                    {
                        songs[index] = value;
                    }
                    else
                    {
                        throw new InvalidOperationException("Index is out of range.");
                    }
                }
                else
                {
                    throw new ArgumentException("Value must be an instance of the Song class and not null.");
                }
            }
        }

        #region instanceMethods
        /// <summary>
        /// Returns an Enumerator that iterates through songs in Songs collection.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Song> GetEnumerator()
        {
            return songs.GetEnumerator();
        }

        /// <summary>
        /// For adding an already consctructed song
        /// </summary>
        /// <param name="newSong"></param>
        public void AddSong(Song newSong)
        {
            bool isDuplicate = DataFilteringTools.CheckForDuplicate(newSong, ownerPlaylistPath);
            if (!isDuplicate)
            {
                songs.Add(newSong);
                songsListLastUpdated = DateTime.Now;
                //PlaylistTools.SongWriter(ownerPlaylistPath, ownerPlaylistName, newSong);
                calculateTotallPlaytime();
            }
        }

        /// <summary>
        /// Adds a song to the playlist using its path.
        /// </summary>
        /// <param name="audioPath"></param>
        public void AddSong(string audioPath)
        {
            Song newSong = new(audioPath);
            if (InternalSettings.doNotAddDuplicateSongs)
            {
                bool isDuplicate = DataFilteringTools.CheckForDuplicate(newSong, ownerPlaylistPath);
                if (!isDuplicate)
                {
                    //PlaylistTools.SongWriter(ownerPlaylistPath, ownerPlaylistName, newSong);
                    songs.Add(newSong);
                    songsListLastUpdated = DateTime.Now;
                    calculateTotallPlaytime();
                }
            }
            else
            {
                songs.Add(newSong);
                songsListLastUpdated = DateTime.Now;
                calculateTotallPlaytime();
            }
        }

        /// <summary>
        /// Adds several songs to the playlist using their paths.
        /// </summary>
        /// <param name="audioPaths"></param>
        public void AddRange(string[] audioPaths)
        {
            var actualAudioPaths = DataFilteringTools.FilterFilesBasedOnExtention(audioPaths);
            string[] checkedAudios;
            if (InternalSettings.doNotAddDuplicateSongs)
            {
                checkedAudios = DataFilteringTools.FilterDuplicates(actualAudioPaths, ownerPlaylistPath);
            }
            else
            {
                checkedAudios = actualAudioPaths;
            }
            if (checkedAudios != null && checkedAudios.Length > 0)
            {
                List<Song> tempSongsList = [];
                /*
                for (int i =0; i<checkedAudios.Length; i++)
                {
                    string audioPath = audioPaths[i];
                    Song newSong = new Song(audioPath);
                    tempSongsList.Add(newSong);
                }
                */
                Parallel.ForEach(checkedAudios, audioPath =>
                {
                    Song newSong = new(audioPath);
                    tempSongsList.Add(newSong);
                });
                songs.AddRange(tempSongsList.ToArray());
                songs.TrimExcess();
                tempSongsList.Clear();
                tempSongsList = new List<Song>(1);
                checkedAudios = null;
                actualAudioPaths = null;
                calculateTotallPlaytime();

                GC.WaitForPendingFinalizers();
                int gen = GC.MaxGeneration;
                GC.Collect(gen, GCCollectionMode.Aggressive);
            }
        }

        /// <summary>
        /// Adds an Array of songs to the playlist
        /// </summary>
        /// <param name="newSongs"></param>
        public void AddRange(Song[] newSongs)
        {
            if (newSongs != null && newSongs.Length > 0)
            {
                if (InternalSettings.doNotAddDuplicateSongs)
                {
                    Song[] checkedNewSongs = DataFilteringTools.FilterDuplicates(newSongs, ownerPlaylistPath);
                    if (checkedNewSongs.Length > 0)
                    {
                        songs.AddRange(checkedNewSongs);
                        songsListLastUpdated = DateTime.Now;
                        calculateTotallPlaytime();
                    }
                }
                else
                {
                    songs.AddRange(newSongs);
                    songsListLastUpdated = DateTime.Now;
                    calculateTotallPlaytime();
                }
                
            }
        }

        /// <summary>
        /// Clears the song items of the playlist.
        /// </summary>
        public void Clear()
        {
            songs.Clear();
            songsListTotalPlaytime = TimeSpan.Zero;
        }

        /// <summary>
        /// *In Need Of Improvements- Finds and returns a song within the list using its Title (case insensitive), and optionally for a more accurate result, 
        /// using its Artist and Album.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="artist"></param>
        /// <param name="album"></param>
        /// <returns></returns>
        public Song? Find(string title, string artist = "", string album = "")
        {
            Song foundSong;
            List<Song> tempSongs = [];
            if (songs != null && songs.Count > 0)
            {
                foreach (Song song in songs)
                {
                    if (DataFilteringTools.AreTheSame(song.Title, title, true) && DataFilteringTools.AreTheSame(song.Artist, artist, true) &&
                        DataFilteringTools.AreTheSame(song.Album, album, true))
                    {
                        foundSong = song;
                        return foundSong;
                    }
                    else if (DataFilteringTools.AreTheSame(song.Title, title, true) && DataFilteringTools.AreTheSame(song.Artist, artist))
                    {
                        tempSongs.Add(song);
                    }
                    else if (DataFilteringTools.AreTheSame(song.Title, title, true) && DataFilteringTools.AreTheSame(song.Album, album))
                    {
                        tempSongs.Add(song);
                    }
                    else if (DataFilteringTools.AreTheSame(song.Title, title, true))
                    {
                        tempSongs.Add(song);
                    }
                    else
                    { return null; }

                    if (tempSongs.Count == 0) { return null; }
                    else { return tempSongs[0]; }
                }
            }
            else
            { return null; }
            // If the code has reached up to here, then sth is wrong
            return null;
        }

        /// <summary>
        /// Removes a song from the playlist, optionally removing it from all playlists.
        /// </summary>
        /// <param name="toBeRemovedSong"></param>
        /// <param name="fromAllPlaylists"></param>
        public void RemoveSong(Song toBeRemovedSong, bool fromAllPlaylists = false)
        {
                // Removes the song object itself from the list.
                songs.Remove(toBeRemovedSong);
                // To remove the song info and pics from the database and the folders.
                PlaylistTools.SongRemovalHandler(toBeRemovedSong, ownerPlaylistName, fromAllPlaylists);
                songsListLastUpdated = DateTime.Now;
                calculateTotallPlaytime();
        }

        /// <summary>
        /// Removes a song from the playlist using its index, optionally removing it from all playlists.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="fromAllPlaylists"></param>
        public void RemoveAt(int index, bool fromAllPlaylists = false)
        {
            Song toBeRemovedSong = songs[index];
            PlaylistTools.SongRemovalHandler(toBeRemovedSong, ownerPlaylistName, fromAllPlaylists);
            songsListLastUpdated = DateTime.Now;
            songs.RemoveAt(index);
        }

        /// <summary>
        /// Sorts the songs inside the collection based on their Title (0), Artist (1), Album (2), 
        /// Duration(3), Genre(4), TrackNumber (5).
        /// </summary>
        /// <returns></returns>
        public Songs Sort(int columnNumber)
        {
            Song[] toBeSortedPlaylist = ToArray();
            switch (columnNumber)
            {
                case 0:
                    songs.Sort((x, y) => x.Title.CompareTo(y.Title));
                    break;

                case 1:
                    songs.Sort((x, y) => x.Artist.CompareTo(y.Artist));
                    break;

                case 2:
                    songs.Sort((x, y) => x.Album.CompareTo(y.Album));
                    break;

                case 3:
                    songs.Sort((x, y) => x.Duration.CompareTo(y.Duration));
                    break;

                case 4:
                    songs.Sort((x, y) => x.Genres.CompareTo(y.Genres));
                    break;
                case 5:
                    songs.Sort((x, y) => x.TrackNumber.CompareTo(y.TrackNumber));
                    break;

                default:
                    songs.Sort((x, y) => x.Title.CompareTo(y.Title));
                    break;
            }
            return this;
        }

        /// <summary>
        /// Casts the songs collection into an array of type Song, optionally returning a new array without reference to the original.
        /// </summary>
        /// <returns></returns>
        public Song[] ToArray(bool clone = false)
        {
            if (clone)
            {
                Song[] tempSongsArray = songs.ToArray();
                return tempSongsArray;
            }
            else
            {
                if (songs.Count > 0)
                {
                    Song[] songsArray = new Song[songs.Count];
                    int i = 0;
                    foreach (Song song in songs)
                    {
                        songsArray[i] = song;
                        i++;
                    }
                    return songsArray;
                }
                else { return Array.Empty<Song>(); }
            }
        }

        /// <summary>
        /// Casts the songs collection into a list of type Song.
        /// </summary>
        /// <returns></returns>
        public List<Song> ToList()
        {
            return songs;
        }
        #endregion

        #region privateInnerWorkings
        /// <summary>
        /// Calculates the totall play time of the playlist (sum of all songs' duration).
        /// </summary>
        /// <returns></returns>
        private TimeSpan calculateTotallPlaytime()
        {
            TimeSpan tempTotal = TimeSpan.Zero;
            if (songs != null && songs.Count > 0)
            {
                foreach (Song music in songs)
                {
                    tempTotal += music.Duration;
                }
            }
            return tempTotal;
        }

        
        #endregion
    }
}
