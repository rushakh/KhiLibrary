using System.Windows.Forms;
using System.Xml;

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
        /// Creates a Songs object using an already connstructed list of songs (List<Song>), in addition 
        /// to the playlist's name and path. This is mainly used for loading an already existing playlist 
        /// from the database.
        /// </summary>
        /// <param name="songsList"></param>
        /// <param name="playlistName"></param>
        /// <param name="playlistPath"></param>
        public Songs(List<Song> songsList, string playlistName, string playlistPath)
        {
            songs = songsList;
            ownerPlaylistName= playlistName;
            ownerPlaylistPath= playlistPath;
            songsListLastUpdated = DateTime.Now;
            songsListTotalPlaytime = calculateTotallPlaytime();
        }
        #endregion

        /// <summary>
        /// Indexer -Returns the song with the provided index. If requested index is bigger than the number of 
        /// songs or if no song has been added yet, returns null.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Song this[int index]
        {
            get
            {
                try
                {
                    if (songs != null && songs.Count >= index)
                    {
                        return songs[index];
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
        /// For adding an already consctructed song
        /// </summary>
        /// <param name="newSong"></param>
        public async void AddSong(Song newSong)
        {
            await Task.Run(() =>
            {
                bool isDuplicate = KhiUtils.DataFilteringTools.FilterDuplicates(newSong, ownerPlaylistPath);
                if (!isDuplicate)
                {
                    songs.Add(newSong);
                    songsListLastUpdated = DateTime.Now;
                    KhiUtils.PlaylistTools.SongWriter(ownerPlaylistPath, ownerPlaylistName, newSong);
                    calculateTotallPlaytime();
                }
            });
        }

        /// <summary>
        /// Adds a song to the playlist using its path.
        /// </summary>
        /// <param name="audioPath"></param>
        public async void AddSong(string audioPath)
        {
            await Task.Run(() =>
            {
                Song newSong = new(audioPath);
                bool isDuplicate = KhiUtils.DataFilteringTools.FilterDuplicates(newSong, ownerPlaylistPath);
                if (!isDuplicate)
                {
                    KhiUtils.PlaylistTools.SongWriter(ownerPlaylistPath, ownerPlaylistName, newSong);
                    songs.Add(newSong);
                    songsListLastUpdated = DateTime.Now;
                    calculateTotallPlaytime();
                }
            });
        }

        /// <summary>
        /// Adds several songs to the playlist using their paths.
        /// </summary>
        /// <param name="audioPaths"></param>
        public async void AddRange(string[] audioPaths)
        {
            await Task.Run(() =>
            {
                List<Song> tempSongs = new List<Song>();
                foreach (string audioPath in audioPaths)
                {
                    Song newSong = new(audioPath);
                    tempSongs.Add(newSong);
                }
                List <Song> checkedSongs  = KhiUtils.DataFilteringTools.FilterDuplicates(tempSongs, ownerPlaylistPath);
                if (checkedSongs.Count > 0)
                {
                    foreach (Song song in checkedSongs)
                    {
                        songs.Add(song);
                    }
                    //songs.AddRange(tempSongs);
                    songsListLastUpdated = DateTime.Now;
                    KhiUtils.PlaylistTools.PlaylistWriter(ownerPlaylistPath, ownerPlaylistName, checkedSongs);
                    calculateTotallPlaytime();
                }
                // Better to do nothing, since they already exist in the playlist, no need for an Exception to be thrown.
                else 
                {
                    //Exception duplicateOrEmpty = new Exception("Duplicate files / Empty list");
                    //throw duplicateOrEmpty; 
                }
            });
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
        public Song Find(string title, string artist = "", string album = "")
        {
            Song foundSong;
            List<Song> tempSongs = [];
            if (songs != null && songs.Count > 0)
            {
                foreach (Song song in songs)
                {
                    if (song.Title != null && artist != "" && song.Artist.ToLower() == artist.ToLower() &&
                        album != "" && song.Album.ToLower() == album.ToLower() && song.Title.ToLower() == title.ToLower())
                    {
                        foundSong = song;
                        return foundSong;
                    }
                    else if (song.Title != null && artist != "" && song.Artist.ToLower() == artist.ToLower() &&
                        song.Title.ToLower() == title.ToLower())
                    {
                        tempSongs.Add(song);
                    }
                    else if (song.Title != null && album != "" && song.Album.ToLower() == album.ToLower() &&
                        song.Title.ToLower() == title.ToLower())
                    {
                        tempSongs.Add(song);
                    }
                    else if (song.Title != null && song.Title.ToLower() == title.ToLower())
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
        /// Removes a song from the playlist.
        /// </summary>
        /// <param name="toBeRemovedSong"></param>
        public async void RemoveSong(Song toBeRemovedSong, bool fromAllPlaylists = false)
        {
            await Task.Run(() =>
            {
                // Removes the song object itself from the list.
                songs.Remove(toBeRemovedSong);
                // To remove the song info and pics from the database and the folders.
                SongRemovalHandler(toBeRemovedSong, fromAllPlaylists);
                songsListLastUpdated = DateTime.Now;
                calculateTotallPlaytime();
            });
        }

        /// <summary>
        /// Sorts the songs inside the collection based on their Title (0), Artist (1), Album (2), 
        /// Duration(seconds)(3), and Genre(4).
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

                default:
                    songs.Sort((x, y) => x.Title.CompareTo(y.Title));
                    break;
            }
            return this;
        }

        /// <summary>
        /// Casts the songs collection into an array of type Song.
        /// </summary>
        /// <returns></returns>
        public Song[] ToArray()
        {
            Song[] tempSongsArray = new Song[songs.Count];
            int i = 0;
            foreach (Song music in songs)
            {
                tempSongsArray[i] = music;
            }
            return tempSongsArray;
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


        /// <summary>
        /// Finds playlists, and Removes the specified song from this playlist, and optionally 
        /// from all playlists. 
        /// </summary>
        /// <param name="toBeRemovedSong"></param>
        /// <param name="fromAllPlaylists"></param>
        private void SongRemovalHandler(Song toBeRemovedSong, bool fromAllPlaylists = false)
        {
            XmlElement AllSongs;  //the document root node
            XmlDocument musicDatabase;
            // Checking to find active databases to search in.
            List<string> existingDatabases = [];
            string[] tempExistingDatabases = System.IO.Directory.GetFiles(Application.StartupPath, "*.xml*", SearchOption.TopDirectoryOnly);
            foreach (string database in tempExistingDatabases)
            {
                // Just to make sure
                if (System.IO.Path.GetExtension(database).ToUpper() == ".XML")
                {
                    if (fromAllPlaylists == false)
                    {
                        if (ownerPlaylistName != null && database.ToUpper().Contains(ownerPlaylistName.ToUpper()))
                        {
                            existingDatabases.Add(database);
                            break;
                        }
                    }
                    else { existingDatabases.Add(database); }
                }
            }
            foreach (string database in existingDatabases)
            {
                KhiUtils.SongInfoTools.RemoveSongInfoAndPics(toBeRemovedSong, database);
            }
        }
        #endregion
    }
}
