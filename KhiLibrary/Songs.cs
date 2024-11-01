﻿
using System.Collections;

namespace KhiLibrary
{
    /// <summary>
    /// Holds the playlist's songs and provides tools for adding and removing songs.
    /// </summary>
    public class Songs : IList<Song>
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

        /// <summary>
        /// Indicates if the collection is readonly (*It's not).
        /// </summary>
        public bool IsReadOnly { get=>  false; }
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

        #region instanceMethods

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

        IEnumerator IEnumerable.GetEnumerator() { return songs.GetEnumerator(); }

        /// <summary>
        /// Returns an IEnumerator that can be used to iterates through the songs in this collection.
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
        public void Add(Song newSong)
        {
            bool isDuplicate = DataFilteringTools.CheckForDuplicate(newSong, ownerPlaylistPath);
            if (!isDuplicate)
            {
                songs.Add(newSong);
                songsListLastUpdated = DateTime.Now;
                calculateTotallPlaytime();
            }
        }

        /// <summary>
        /// Adds a song to the playlist using its path.
        /// </summary>
        /// <param name="audioPath"></param>
        public void Add(string audioPath)
        {
            if (InternalSettings.doNotAddDuplicateSongs)
            {
                bool isDuplicate = DataFilteringTools.CheckIfDuplicate(audioPath, ownerPlaylistPath);
                if (!isDuplicate)
                {
                    Song newSong = new(audioPath);
                    songs.Add(newSong);
                    songsListLastUpdated = DateTime.Now;
                    calculateTotallPlaytime();
                }
            }
            else
            {
                bool isMusic = DataFilteringTools.IsAcceptableFormat(audioPath);
                if (isMusic)
                {
                    Song newSong = new(audioPath);
                    songs.Add(newSong);
                    songsListLastUpdated = DateTime.Now;
                    calculateTotallPlaytime();
                }
            }
        }

        /// <summary>
        /// Adds several songs to the playlist using their paths. Optionally, will add songs in parallel which 
        /// will be faster for large number of songs but in that case the songs will not be added in the order they 
        /// were given and the process will be accompanied by a sudden spike in RAM and CPU usage.
        /// </summary>
        /// <param name="audioPaths"></param>
        /// <param name="AddSongsInParallel"></param>
        public void AddRange(string[] audioPaths , bool AddSongsInParallel = false)
        {
            var actualAudioPaths = DataFilteringTools.FilterFilesBasedOnExtention(audioPaths);
            string[]? checkedAudios;
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
                if (AddSongsInParallel)
                {
                    Parallel.ForEach(checkedAudios, audioPath =>
                    {
                        Song newSong = new(audioPath);
                        tempSongsList.Add(newSong);
                    });
                }
                else
                {
                    for (int i = 0; i < checkedAudios.Length; i++)
                    {
                        string audioPath = checkedAudios[i];
                        Song newSong = new Song(audioPath);
                        tempSongsList.Add(newSong);
                    }
                }
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
        /// Adds the songs in this collection to the playback queue.
        /// </summary>
        public void AddToQueue ()
        {
            MusicPlayer.Queue.AddRangeToQueue(songs);
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
        /// Determines if the specified song exists within the collection.
        /// </summary>
        /// <param name="song"></param>
        /// <returns></returns>
        public bool Contains(Song song)
        {
            return songs.Contains(song);
        }

        /// <summary>
        /// Copies the collection to a one dimensional array starting from the specified index.
        /// </summary>
        /// <param name="destinationArray"></param>
        /// <param name="startingIndex"></param>
        public void CopyTo(Song[] destinationArray, int startingIndex)
        {
            songs.CopyTo(destinationArray, startingIndex);
        }

        /// <summary>
        /// Finds and returns a song within the list using its Title (case insensitive), and optionally for a more accurate result, 
        /// using its Artist and Album.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="artist"></param>
        /// <param name="album"></param>
        /// <returns></returns>
        public Song? FindExact(string title, string artist, string album)
        {
            // Using Linq query, even though its very readable, seems to introduce unnecessary overhead in this case. 
            // This works for now while still being readable (or so I think).
            // If you know of any alternative ways that favors performance while remaining readable, please let me know.  
            Song foundSong;
            List<Song> tempSongs = [];
            if (songs != null && songs.Count > 0)
            {
                for (int i = 0; i <songs.Count; i++)
                {
                    Song song = songs[i];
                    if (DataFilteringTools.AreTheSame(song.Title, title) &&
                        DataFilteringTools.AreTheSame(song.Artist, artist) &&
                        DataFilteringTools.AreTheSame(song.Album, album))
                    {
                        foundSong = song;
                        return foundSong;
                    }
                }
                return null;
            }
            else
            { return null; }
        }

        /// <summary>
        /// Finds and returns a list of songs (type Song) with the specified Title or Artist. By default searches for 
        /// title (set <paramref name="isTitle"/> to false to search for artist). Set <paramref name="withExactWord"/> to 
        /// false to also include results that contain the specified word as well.
        /// </summary>
        /// <param name="titleOrArtist"></param>
        /// <param name="isTitle"></param>
        /// <param name="withExactWord"></param>
        /// <returns></returns>
        public List<Song>? Find(string titleOrArtist, bool isTitle = true, bool withExactWord = true)
        {
            // Using Linq query, even though its very readable, seems to introduce unnecessary overhead in this case. 
            // This works for now while still being readable (or so I think).
            // If you know of any alternative ways that favors performance while remaining readable, please let me know.
            if (songs != null && songs.Count > 0)
            {
                // A list of partial matches
                List<Song> foundSongs = new List<Song>();
                for (int i =0; i< songs.Count; i++)
                {
                    if (isTitle)
                    {
                        if (withExactWord)
                        {
                            if (DataFilteringTools.AreTheSame(songs[i].Title, titleOrArtist))
                            {
                                foundSongs.Add(songs[i]);
                            }
                        }
                        else
                        {
                            if (songs[i].Title.Contains(titleOrArtist))
                            {
                                foundSongs.Add(songs[i]);
                            }
                        }
                    }
                    else
                    {
                        if (withExactWord)
                        {
                            if (DataFilteringTools.AreTheSame(songs[i].Artist, titleOrArtist))
                            {
                                foundSongs.Add(songs[i]);
                            }
                        }
                        else
                        {
                            if (songs[i].Artist.Contains(titleOrArtist))
                            {
                                foundSongs.Add(songs[i]);
                            }
                        }
                    }
                }
                if (foundSongs.Count > 0) { return foundSongs; }
                else { return null; }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns the index of the specified element within this collection, returns -1 if song is not found.
        /// </summary>
        /// <param name="song"></param>
        /// <returns></returns>
        public int IndexOf (Song song)
        {
            return songs.IndexOf (song);
        }

        /// <summary>
        /// Inserts the specified song at the given index.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="song"></param>
        public void Insert (int index, Song song)
        {
            songs.Insert (index, song);
        }

        /// <summary>
        /// Removes a song from the playlist, removes it from all playlists if removed from All Songs Playlist. 
        /// Returns true if the song is found and completely removed from the database. A false return might indicate 
        /// that while it was removed from the database, the thumbnail still remains.
        /// </summary>
        /// <param name="toBeRemovedSong"></param>
        public bool Remove(Song toBeRemovedSong)
        {
            bool removeFromAllPlaylists = false;
            bool isCompletelyRemoved = false;
            if (this.ownerPlaylistName == "All Songs Playlist") { removeFromAllPlaylists = true; }
            // Removes the song object itself from the list.
            songs.Remove(toBeRemovedSong);
            // To remove the song info and pics from the database and the folders. Will throw an
            // exception if the song had not been saved beforehand
            try
            {
                PlaylistTools.DatabaseTools.SongRemovalHandler(toBeRemovedSong, ownerPlaylistName, removeFromAllPlaylists);
                isCompletelyRemoved = true;
            }
            catch
            {
                if (System.IO.File.Exists(toBeRemovedSong.ThumbnailPath))
                {
                    System.IO.File.Delete(toBeRemovedSong.ThumbnailPath);
                }
            }
            songsListLastUpdated = DateTime.Now;
            calculateTotallPlaytime();
            return isCompletelyRemoved;
        }

        /// <summary>
        /// Removes a song from the playlist using its index, will remove it from all playlists, if is removed 
        /// from the All Songs Playlist.
        /// </summary>
        /// <param name="index"></param>
        public void RemoveAt(int index)
        {
            bool removeFromAllPlaylists = false;
            if (this.ownerPlaylistName == "All Songs Playlist") { removeFromAllPlaylists = true; }
            Song toBeRemovedSong = songs[index];
            // To remove the song info and pics from the database and the folders. Will throw an
            // exception if the song had not been saved beforehand
            try
            {
                PlaylistTools.DatabaseTools.SongRemovalHandler(toBeRemovedSong, ownerPlaylistName, removeFromAllPlaylists);
            }
            catch 
            {
                if (System.IO.File.Exists(toBeRemovedSong.ThumbnailPath))
                {
                    System.IO.File.Delete(toBeRemovedSong.ThumbnailPath);
                }
            }
            songsListLastUpdated = DateTime.Now;
            songs.RemoveAt(index);
        }

        /// <summary>
        /// Sorts the songs inside the collection based on their Title (0), Artist (1), Album (2), 
        /// Duration(3), Genre(4), TrackNumber (5), and PlayedCount (6). Sorts into ascending order 
        /// by default, can be changed to descending.
        /// </summary>
        /// <param name="columnNumber"></param>
        /// <param name="ascending"></param>
        /// <returns></returns>
        public Songs Sort(int columnNumber, bool ascending = true)
        {
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
                case 6:
                    songs.Sort((x, y) => x.PlayedCount.CompareTo(y.PlayedCount));
                    break;
                default:
                    songs.Sort((x, y) => x.Title.CompareTo(y.Title));
                    break;
            }
            if (!ascending)
            {
                songs.Reverse();
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
