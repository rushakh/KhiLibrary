using System.Xml.Linq;

namespace KhiLibrary
{
    /// <summary>
    /// An assortment of tools for checking if data fits the application's criteria and 
    /// can be used, or whether they are duplicates and already exist within the database.
    /// </summary>
    internal static class DataFilteringTools
    {
        /// <summary>
        /// Compares two strings and can be set to either consider or ignore ordinal case. returns true if the strings are equal 
        /// and returns false if they are not. Also returns false if at least one of the objects is null or an exception is encountered.
        /// </summary>
        /// <param name="firstString"></param>
        /// <param name="secondString"></param>
        /// <param name="ignoreOrdinal"></param>
        /// <returns></returns>
        internal static bool AreTheSame(string? firstString, string? secondString, bool ignoreOrdinal = false)
        {
            try
            {
                StringComparison comparison;
                if (firstString is null || secondString is null)
                { return false; }
                if (ignoreOrdinal)
                {
                    comparison = StringComparison.OrdinalIgnoreCase;
                }
                else
                {
                    comparison = StringComparison.Ordinal;
                }
                bool areEqual = string.Equals(firstString, secondString, comparison);
                return areEqual;
            }
            catch
            { return false; }
        }

        /// <summary>
        /// Checks if the song is a duplicate. In case the xml database doesnt exist, returns false, 
        /// and returns true in case the new song is a duplicate or null.
        /// </summary>
        /// <param name="newSong"></param>
        /// <param name="playlistPath"></param>
        /// <returns></returns>
        internal static bool CheckForDuplicate(Song newSong, string playlistPath)
        {
            if (System.IO.File.Exists(playlistPath) && newSong != null)
            {
                XDocument playlistDataBase = XDocument.Load(playlistPath);
                XElement? playlistSongs = playlistDataBase.Root; //the document root node
                bool isDuplicate = CheckForDuplicate(newSong, playlistSongs);
                return isDuplicate;
            }
            else if (newSong == null) { return true; }
            else { return false; }
        }

        /// <summary>
        /// Checks if the song already exists within the playlist database. Returns true if it is a duplicate, 
        /// returns false if not ir in case of exception.
        /// </summary>
        /// <param name="music"></param>
        /// <param name="playlistSongs"></param>
        /// <returns></returns>
        private static bool CheckForDuplicate(Song music, XElement? playlistSongs)
        {
            bool isDuplicate = false;
            try
            {
                if (playlistSongs != null)
                {
                    foreach (XElement playlistSong in playlistSongs.Elements())
                    {
                        XElement? path = playlistSong.Element("Path");
                        if (path != null)
                        {
                            if (AreTheSame(music.Path, path.Value))
                            {
                                isDuplicate = true;
                                break;
                            }
                        }
                        // If the path is not similar then at least it's not the same file, but it might still be a duplicate
                        if (isDuplicate == false)
                        {
                            XElement? title = playlistSong.Element("Title");
                            XElement? artist = playlistSong.Element("Artist");
                            XElement? album = playlistSong.Element("Album");

                            if (title != null && artist != null && album != null)
                            {
                                if (AreTheSame(title.Value, music.Title) && AreTheSame(artist.Value, music.Artist) && AreTheSame(album.Value, music.Album))
                                {
                                    isDuplicate = true;
                                    break;
                                }
                            }
                        }
                    }
                }
                return isDuplicate;
            }
            catch
            { return isDuplicate; }
        }

        /// <summary>
        /// Checks to see if the audio files' extentions are compatible with the application, and 
        /// returns the suitable ones. If none of them are suitable, returns an empty array
        /// </summary>
        /// <param name="ChosenAudioFilePaths"></param>
        /// <returns></returns>
        internal static string[] FilterFilesBasedOnExtention(string[] ChosenAudioFilePaths)
        {
            List<string> filteredFilePaths = [];
            foreach (string filePath in ChosenAudioFilePaths)
            {
                if (IsAcceptableFormat(filePath)) { filteredFilePaths.Add(filePath); }
            }
            return filteredFilePaths.ToArray();
        }

        /// <summary>
        /// Checks if the chosen audio file that is to be processed is of an acceptable extension or not.
        /// </summary>
        /// <param name="audioFilePath"></param>
        /// <returns></returns>
        internal static bool IsAcceptableFormat(string audioFilePath)
        {
            string[] formats = [".mp3", ".wav", ".flac", ".aiff", ".wma", ".m4a", ".pcm", ".aac", ".oog", ".alac"];
            try
            {
                bool isAcceptable = false;
                var ext = Path.GetExtension(audioFilePath).Trim().ToLower();
                if (formats.Contains(ext))
                { isAcceptable = true; }
                return isAcceptable;
            }
            catch { return false; }
        }

        /// <summary>
        /// Checks if the chosen playlist name follows the guidelines and that a playlist with 
        /// the name does not exist.
        /// </summary>
        /// <param name="playlistName"></param>
        /// <returns></returns>
        internal static bool IsAcceptablePlaylistName(string playlistName)
        {
            bool isAcceptable = true;
            char[] unacceptableChars = ['/', '\\', ':', '*', '?', '\"', '<', '>', '|'];
            foreach (char c in unacceptableChars)
            {
                if (playlistName.Contains(c))
                {
                    isAcceptable = false;
                    break;
                }
            }
            // If there already exists a database by this name, returns false
            if (PlaylistTools.PlaylistAlreadyExists(playlistName)) { isAcceptable = false; }
            return isAcceptable;
        }

        /// <summary>
        /// Checks if the songs already exist in the playlist's database, removes the duplicates and 
        /// returns the rest; returns an empty List of type Song if all are duplicates or if the List of songs 
        /// is null.
        /// </summary>
        /// <param name="newSongs"></param>
        /// <param name="playlistPath"></param>
        /// <returns></returns>
        internal static List<Song> FilterDuplicates(List<Song> newSongs, string playlistPath)
        {
            List<Song> filesList = new List<Song>();

            if (System.IO.File.Exists(playlistPath) && newSongs != null)
            {
                XDocument playlistDataBase = XDocument.Load(playlistPath);
                XElement? playlistSongs = playlistDataBase.Root; //the document root node
                foreach (var music in newSongs)
                {
                    bool isDuplicate = CheckForDuplicate(music, playlistSongs);
                    if (isDuplicate == false) { filesList.Add(music); }
                }
                return filesList;
            }
            else if (newSongs == null)
            {
                return filesList;
            }
            else
            {
                return newSongs;
            }
        }

        /// <summary>
        /// Checks if the songs already exist in the playlist's database, removes the duplicates and 
        /// returns the rest; returns an empty array of type Song if all are duplicates or if the array of songs 
        /// is null.
        /// </summary>
        /// <param name="newSongs"></param>
        /// <param name="playlistPath"></param>
        /// <returns></returns>
        internal static Song[] FilterDuplicates(Song[] newSongs, string playlistPath)
        {
            List<Song> filesList = new List<Song>();
            if (System.IO.File.Exists(playlistPath) && newSongs != null)
            {
                XDocument playlistDataBase = XDocument.Load(playlistPath);
                XElement? playlistSongs = playlistDataBase.Root; //the document root node
                foreach (var music in newSongs)
                {
                    bool isDuplicate = CheckForDuplicate(music, playlistSongs);
                    if (isDuplicate == false) { filesList.Add(music); }
                }
                return filesList.ToArray();
            }
            else if (newSongs == null)
            {
                return filesList.ToArray();
            }
            else
            {
                return newSongs;
            }
        }

        /// <summary>
        /// Checks whether the mentioned audio file exists in the playlist. 
        /// </summary>
        /// <param name="audioFilePath"></param>
        /// <param name="playlistPath"></param>
        /// <returns></returns>
        internal static bool CheckIfDuplicate(string audioFilePath, string playlistPath)
        {
            bool isDuplicate = false;
            XDocument playlistDataBase = XDocument.Load(playlistPath);
            XElement? playlistSongs = playlistDataBase.Root; //the document root node
            if (playlistSongs != null && playlistSongs.HasElements)
            {
                foreach (XElement playlistSong in playlistSongs.Elements())
                {
                    XElement? path = playlistSong.Element("Path");
                    if (path != null && AreTheSame(audioFilePath, path.Value))
                    {
                        isDuplicate = true;
                        break;
                    }
                }
            }
            return isDuplicate;
        }

        /// <summary>
        /// Checks if the songs at the specified locations already exist in the playlist's database. 
        /// Returns the paths that are not duplicates as a  <see langword="string"/>[].
        /// </summary>
        /// <param name="audioPaths"></param>
        /// <param name="playlistPath"></param>
        /// <returns></returns>
        internal static string[] FilterDuplicates(string[] audioPaths, string playlistPath)
        {
            List<string> checkedPaths = new List<string>();
            XDocument playlistDataBase = XDocument.Load(playlistPath);
            XElement? playlistSongs = playlistDataBase.Root; //the document root node
            if (playlistSongs != null && playlistSongs.HasElements)
            {
                int i = 0;
                foreach (XElement playlistSong in playlistSongs.Elements())
                {
                    XElement? path = playlistSong.Element("Path");
                    if (path != null && !AreTheSame(audioPaths[i], path.Value))
                    {
                        checkedPaths.Add(audioPaths[i]);
                    }
                    if (i < audioPaths.Length) { i++; }
                }
                return checkedPaths.ToArray();
            }
            else
            { return audioPaths; }
        }
    }

}
