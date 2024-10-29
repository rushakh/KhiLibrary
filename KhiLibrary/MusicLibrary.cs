
namespace KhiLibrary
{
    /// <summary>
    /// The gateway to accessing the Playlists and Songs. Can change the default locations and certain behaviors through the Settings.
    /// </summary>
    public static class MusicLibrary
    {
        private static Playlists playlistList = new Playlists(true);

        /// <summary>
        /// Collection of this musicLibrary's playlists of type Playlists
        /// </summary>
        public static Playlists Playlists { get { return playlistList; } }

        /// <summary>
        /// The default paths and behaviours of the application.
        /// </summary>
        public static class Settings
        {
            /// <summary>
            /// The location of the program's exe (or dll).
            /// </summary>
            public static string ApplicationPath { get { return InternalSettings.applicationPath; } }
            /// <summary>
            /// The location of the defualt playlist database that will house all of the songs.
            /// </summary>
            public static string AllMusicDataBase { get { return InternalSettings.allMusicDataBase; } set => InternalSettings.allMusicDataBase = value; }          
            /// <summary>
            /// The location of the Favorite collection (***MIGHT NOT USE IT, WILL DECIDE LATER)
            /// </summary>
            public static string FavoritesDataBase { get { return InternalSettings.favoriteMusicsDataBase; } set => InternalSettings.favoriteMusicsDataBase = value; }
            /// <summary>
            /// The location of the database that contains the location of all of the playlists.
            /// </summary>
            public static string PlaylistsRecord { get { return InternalSettings.playlistsRecord; } set => InternalSettings.playlistsRecord = value; }
            /// <summary>
            /// The location of the folder that contains all of the audio files' cover arts.
            /// </summary>
            public static string AlbumArtsPath { get { return InternalSettings.albumArtsPath; } set => InternalSettings.albumArtsPath = value; }
            /// <summary>
            /// The location of the folder that contains the thumbnails of all of the songs' cover arts.
            /// </summary>
            public static string AlbumArtsThumbnailsPath { get { return InternalSettings.albumArtsThumbnailsPath; } set => InternalSettings.albumArtsThumbnailsPath = value; }
            /// <summary>
            /// The location of the Temp Folder in which extracted album arts are saved to.
            /// </summary>
            public static string TempArtsFolder { get { return InternalSettings.tempArtsFolder; } set => InternalSettings.tempArtsFolder = value; }
            /// <summary>
            /// The location of the playlists databases (.xml documents).
            /// </summary>
            public static string PlaylistsFolder { get { return InternalSettings.playlistsFolder; } set => InternalSettings.playlistsFolder = value; }
            /// <summary>
            /// Prevents the same song from being added to the application. default is false.
            /// </summary>
            public static bool DoNotAddDuplicateSongs { get { return InternalSettings.doNotAddDuplicateSongs; } set { InternalSettings.doNotAddDuplicateSongs = value; } }
            /// <summary>
            /// dictates if album arts should be extracted and loaded on demand. default value is true. Set to false to extract the images beforehand.
            /// </summary>
            public static bool PrepareForVirtualMode { get { return InternalSettings.prepareForVirtualMode; } set { InternalSettings.prepareForVirtualMode = value; } }
        }
    }
}
