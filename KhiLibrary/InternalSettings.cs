namespace KhiLibrary
{
    /// <summary>
    /// Contains the default paths used by the application, they can be changed through MusicLibrary class.
    /// </summary>
    internal static class InternalSettings
    {
        internal static string applicationPath = System.Windows.Forms.Application.StartupPath;
        
        internal static string albumArtsPath = applicationPath + "\\Album Arts\\";
        internal static string albumArtsThumbnailsPath = applicationPath + "Album Arts Thumbnails\\";
        internal static string tempArtsFolder = applicationPath + "\\Temp\\";
        internal static string playlistsFolder = applicationPath + "\\Playlists\\";
        internal static string allMusicDataBase = playlistsFolder + "AllMusicDataBase.xml";
        internal static string favoriteMusicsDataBase = playlistsFolder + "Favorites.xml";
        internal static string playlistsRecord = playlistsFolder + "PlaylistsRecord.xml";
        internal static string equalizersProfilesPath = applicationPath + "EqualizersProfiles.xml";
        internal static string playlistsBackupsFolder = applicationPath + "\\Backups\\";
        internal static bool doNotAddDuplicateSongs = false;
        internal static bool prepareForVirtualMode = true;

        /// <summary>
        /// Creates the directories needed for the application to function.
        /// </summary>
        internal static void CreateDirectories()
        {
            //if (!System.IO.Directory.Exists(AlbumArtsPath)) { System.IO.Directory.CreateDirectory(AlbumArtsPath); }
            if (!System.IO.Directory.Exists(albumArtsThumbnailsPath)) { System.IO.Directory.CreateDirectory(albumArtsThumbnailsPath); }
            if (!System.IO.Directory.Exists(tempArtsFolder)) { System.IO.Directory.CreateDirectory(tempArtsFolder); }
            if (!System.IO.Directory.Exists(playlistsFolder)) { System.IO.Directory.CreateDirectory(playlistsFolder); }
            if (!System.IO.Directory.Exists(playlistsBackupsFolder)) { System.IO.Directory.CreateDirectory(playlistsBackupsFolder); }
        }
    }
}
