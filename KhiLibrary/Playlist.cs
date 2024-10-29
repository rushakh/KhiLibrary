using System.Drawing;
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
        internal bool isDefaultPlaylist;
        private string playlistName;
        private string playlistPath;
        private string playlistThumbnailPath;
        //private DateTime creationDate;
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
                if (value is not null && !isDefaultPlaylist)
                {
                    var temp = (PlaylistTools.DatabaseTools.EditPlaylistName(value, playlistName, playlistPath));
                    if (temp != null)
                    {
                        playlistName = temp;
                        playlistPath = InternalSettings.playlistsFolder + value + ".xml";
                    }
                }
                else if (isDefaultPlaylist)
                {
                    throw new InvalidOperationException("Cannot Edit or Remove default playlists");
                }
            }
        }

        /// <summary>
        /// Returns the location of the playlist's xml database.
        /// </summary>
        public string Path { get => playlistPath; }

        /// <summary>
        /// The path to the thumbnail used for this playlist. If changed, will copy the image 
        /// in the new location to the default location.
        /// </summary>
        public string ThumbnailPath
        {
            get
            {
                return playlistThumbnailPath;
            }
            set
            {
                string newImagePath = value;
                if (System.IO.File.Exists(newImagePath))
                {
                    Image newThumbnail = Image.FromFile(newImagePath);
                    if (System.IO.File.Exists(playlistThumbnailPath))
                    {
                        System.IO.File.Delete(playlistThumbnailPath);
                    }
                    newThumbnail.Save(playlistThumbnailPath, System.Drawing.Imaging.ImageFormat.Png);
                    newThumbnail.Dispose();
                }
            }
        }

        /// <summary>
        /// Returns the sum of the songs' duration 
        /// </summary>
        public TimeSpan TotalPlayTime { get => songsList.TotalPlayTime; }

        // This is Not completely removed for now as I might reuse this 
        /// <summary>
        /// The date that the playlist was created on.
        /// </summary>
        //public DateTime CreationDate { get => creationDate; }

        /// <summary>
        /// Returns the last datetime in which the playlist was updated.
        /// </summary>
        public DateTime LastUpdated { get => songsList.LastUpdated; }

        /// <summary>
        /// The collection of Songs in the playlist.
        /// </summary>
        public Songs Songs { get => songsList; }

        /// <summary>
        /// Gets or sets the thumbnail used for this playlist, if there are no thumbnails specified, uses the 
        /// default khiplayer image.
        /// </summary>
        public Image Thumbnail
        {
            get
            {
                if (System.IO.File.Exists(playlistThumbnailPath))
                {
                    return SongInfoTools.FetchSongInfo.GetThumbnail(playlistThumbnailPath);
                }
                else
                {
                    return KhiLibrary.Resources.Khi_Player_Thumbnail;
                }
            }
            set
            {
                if (value is not null)
                {
                    Image newThumbnail = value;
                    string newThumbnailPath = InternalSettings.playlistsFolder + playlistName + ".png";
                    if (System.IO.File.Exists(playlistThumbnailPath))
                    {
                        System.IO.File.Delete(playlistThumbnailPath);
                    }
                    newThumbnail.Save(playlistThumbnailPath, System.Drawing.Imaging.ImageFormat.Png);
                    playlistThumbnailPath = newThumbnailPath;
                    newThumbnail.Dispose();
                }
            }
        }
        #endregion 

        #region constructors
        /// <summary>
        /// Creates an empty instance of the Playlist Class without a database. Do not use this in Normal circumstances
        /// </summary>
        public Playlist()
        {
            playlistName = string.Empty;
            playlistPath = string.Empty;
            //creationDate = DateTime.Now;
            lastUpdated = DateTime.Now;
            songsList = new Songs();
            playlistThumbnailPath = InternalSettings.playlistsFolder + playlistName + ".png";
        }

        /// <summary>
        /// Creates a playlist with the provided name. Throws an exception if a playlist with that 
        /// name already exists. 
        /// </summary>
        /// <param name="name"></param>
        public Playlist(string name)
        {
            if (!PlaylistTools.Records.PlaylistAlreadyExists(name))
            {
                playlistName = name;
                // To Remove spaces from the name so a file with that name can be created.
                playlistPath = InternalSettings.playlistsFolder + playlistName + ".xml";
                //creationDate = DateTime.Now;
                lastUpdated = DateTime.Now;
                songsList = new Songs(playlistName, playlistPath);
                playlistThumbnailPath = InternalSettings.playlistsFolder + playlistName + ".png";
                CreatePlaylist();
                
                if (playlistName == "All Songs" || playlistName == "Favorites")
                {
                    isDefaultPlaylist = true;
                }
            }
            else
            {
                throw new Exception("A playlist with that name, " + name + ",already exists");
            }
        }

        /// <summary>
        /// Creates a playlist by using an already constructed Songs object, a name, and path (database) 
        /// of the playlist. This is mainly used for loading existing playlists from the database and hence a 
        /// database is not created for it by default but <c>Save()</c> can be manually called.
        /// </summary>
        /// <param name="listOfSongs"></param>
        /// <param name="name"></param>
        /// <param name="path"></param>
        /// <exception cref="FileNotFoundException"></exception>
        public Playlist(Songs listOfSongs, string name, string path)
        {
            // First will check if the file exists even though it's not needed at this stage,
            // because it can lead to confusing Exceptions later on. 
            if (System.IO.File.Exists(path))
            {
                playlistName = name;
                playlistPath = path;
                // Should include these two as Attributes in the database
                //creationDate = DateTime.Now;
                lastUpdated = DateTime.Now;
                songsList = listOfSongs;
                playlistThumbnailPath = InternalSettings.playlistsFolder + playlistName + ".png";
                if (playlistName == "All Songs" || playlistName == "Favorites")
                {
                    isDefaultPlaylist = true;
                }
                if (!InternalSettings.prepareForVirtualMode)
                {
                    PrepareSongsArts();
                }
            }
            else
            {
                throw new FileNotFoundException("The file does not exist at the specified location.");
            }
        }
        #endregion

        #region instanceMethods

        /// <summary>
        /// Adds the songs in this playlist to the playback queue.
        /// </summary>
        public void AddToQueue()
        {
            MusicPlayer.Queue.AddRangeToQueue(songsList.ToList());
        }

        /// <summary>
        /// Removes the songs in this playlist from the playback queue.
        /// </summary>
        public void RemoveFromQueue()
        {
            MusicPlayer.Queue.RemoveRangeFromQueue(songsList.ToList());
        }

        /// <summary>
        /// Exports the current playlist as an m3u8 and returns the location of the exported file.
        /// </summary>
        public string ExportPlaylist()
        {
            string exportedPlaylistPath = KhiUtils.ExportPlaylistAsM3u(playlistName, songsList.ToList());
            return exportedPlaylistPath;
        }

        /// <summary>
        /// Rereads the Songs from the database.
        /// </summary>
        public void Reload()
        {
            var tempSongList = PlaylistTools.DatabaseTools.PlaylistReader(playlistName, playlistPath);
            if (tempSongList != null)
            {
                Songs newSongs = new Songs(tempSongList, playlistName, playlistPath);
                songsList.Clear();
                songsList = newSongs;
            }
            else
            { throw new Exception("An unknown error was encountered reading from the playlist Database"); }
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
        /// Deletes the playlist and its database. It will throw an exception If called 
        /// using All Songs playlist or Favorites.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void Remove()
        {
            if (!isDefaultPlaylist)
            {
                songsList.Clear();
                PlaylistTools.DatabaseTools.PlaylistRemovalHandler(playlistName, playlistPath);
            }
            else
            {
                throw new InvalidOperationException("Cannot Edit or Remove default playlists");
            }
        }

        /// <summary>
        /// Clears the songs that exists in the playlist (from the database as well).
        /// </summary>
        public void Clear()
        {
            songsList.Clear();
            PlaylistTools.DatabaseTools.PlaylistWriter(playlistPath, playlistName, songsList.ToArray());
        }

        /// <summary>
        /// Sorts the songs inside the playlist based on their Title (0), Artist (1), Album (2),
        /// Duration(seconds)(3), Genre(4), TrackNumber (5), and PlayedCount (6). Sorts into ascending order by default 
        /// but can be changed to descending.
        /// </summary>
        /// <param name="columnNumber"></param>
        /// /// <param name="ascending"></param>
        public void Sort(int columnNumber, bool ascending = true)
        {
            songsList = songsList.Sort(columnNumber, ascending);
        }

        /// <summary>
        /// Saves the playlist songs.
        /// </summary>
        public void Save()
        {
            PlaylistTools.DatabaseTools.PlaylistWriter(playlistPath, playlistName, songsList.ToArray());
            lastUpdated = DateTime.Now;
        }
        #endregion

        #region privateInnerWorkings     

        /// <summary>
        /// To create an empty Xml for this playlist
        /// </summary>
        private void CreatePlaylist()
        {
            InternalSettings.CreateDirectories();
            XDocument playlistDatabase = PlaylistTools.DatabaseTools.PopulateDatabase(playlistPath, playlistName, songsList.ToArray());
            PlaylistTools.DatabaseTools.XmlWritingTool(playlistDatabase, playlistPath);
            PlaylistTools.Records.PlaylistRecorder(playlistName, playlistPath);
        }
        #endregion
    }
}
