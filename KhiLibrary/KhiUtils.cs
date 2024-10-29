using System.Drawing;

namespace KhiLibrary
{
    /// <summary>
    /// Contains an assortment of tools that do not belong under definite categories (yet).
    /// </summary>
    public static class KhiUtils
    {
        /// <summary>
        /// Will check if the file at the specified location is in use by another process and locked. Returns true if in use by another 
        /// process or other IO Exceptions, otherwise returns false.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        internal static bool IsFileLocked(string filePath)
        {
            bool isLocked = false;
            try
            {
                using (FileStream checkIfLockedStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    // Don't need to do anything with the stream, only to check if a stream can form. if it can, then the file is not locked.
                    checkIfLockedStream.Close();
                    isLocked = false;
                }
            }
            catch (IOException e) when ((e.HResult & 0x0000FFFF) == 32 || (e.HResult & 0x0000FFFF) == 33)
            {
                isLocked = true;
            }
            return isLocked;
        }
        
        /// <summary>
        /// Exports the playlist to a .M3U8 file that can be used in other applications. Returns the path to the exported file.
        /// </summary>
        /// <param name="playlistName"></param>
        /// <param name="playlistSongs"></param>
        /// <returns></returns>
        internal static string ExportPlaylistAsM3u (string playlistName , List<Song> playlistSongs)
        {
            string firstLine = "#EXTM3U";
            string secondLine = "#" + playlistName + ".m3u8";
            string exportPlaylistPath;
            try
            {
                exportPlaylistPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "\\Playlists\\" + playlistName + ".m3u8";
            }
            catch 
            {
                exportPlaylistPath = InternalSettings.playlistsFolder + "Exported Playlists\\" + playlistName + ".m3u8";
            }
            List<string> playlistSongsPaths = new List<string>(playlistSongs.Count + 2);
            playlistSongsPaths.Add(firstLine);
            playlistSongsPaths.Add(secondLine);
            foreach (Song song in playlistSongs)
            {
                if (song.Path != string.Empty)
                {
                    playlistSongsPaths.Add(song.Path);
                }
            }
            playlistSongsPaths.TrimExcess();
            System.IO.File.WriteAllLines(exportPlaylistPath, playlistSongsPaths, System.Text.Encoding.UTF8);
            return exportPlaylistPath;
        }

        /// <summary>
        /// Reads an .m3u or m3u8 file and returns the playlist's name and location of songs it contains.
        /// </summary>
        /// <param name="m3uOrM3u8FilePath"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        internal static (string, string[]) ExtractSongsPathsFromM3uPlaylist(string m3uOrM3u8FilePath)
        {
            // shouldnt be void
            string importedPlaylistName;
            List<string> importedPlaylistSongsPaths = new List<string>();
            if (System.IO.File.Exists(m3uOrM3u8FilePath))
            {
                var tempImported = System.IO.File.ReadAllLines(m3uOrM3u8FilePath, System.Text.Encoding.Default);
                // For Getting the playlistName, could have also extracted it from the file name, but that can be changed easily or by mistake.
                string secondLine = tempImported[1];
                // This is almost always the case but just in case, I'll use conditional.
                if (secondLine.EndsWith (".m3u") || secondLine.EndsWith(".m3u8"))
                {
                    int dotIndex = secondLine.LastIndexOf('.');
                    importedPlaylistName = secondLine.Remove(dotIndex);
                }
                else
                {
                    importedPlaylistName = secondLine;
                }
                // Getting the songs paths
                for (int i = 2; i < tempImported.Length -2; i++)
                {
                    importedPlaylistSongsPaths.Add(tempImported[i]);
                }
                return (importedPlaylistName, importedPlaylistSongsPaths.ToArray());
            }
            else
            {
                throw new FileNotFoundException("The specified file at ** " + m3uOrM3u8FilePath + " ** does not exist");
            }
        }

        /// <summary>
        /// Deletes all the temporary images (album arts) in the temp folder.
        /// </summary>
        public static void ClearTemporaryImages()
        {
            try
            {
                if (System.IO.Directory.Exists(InternalSettings.tempArtsFolder))
                {
                    string[] artsToDelete = Directory.GetFiles(InternalSettings.tempArtsFolder, "*.png", SearchOption.TopDirectoryOnly);
                    foreach (string art in artsToDelete)
                    {
                        try
                        {
                            System.IO.File.Delete(art);
                        }
                        catch { continue; }
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// Extracts a number of songs' embedded album art, saving them to the specified directory.
        /// </summary>
        /// <param name="audioFilesPaths"></param>
        /// <param name="artDestinationDirectory"></param>
        public static void AlbumArtExtractorBatch(string[] audioFilesPaths, string artDestinationDirectory)
        {
            for (int i = 0; i < audioFilesPaths.Length; i++)
            {
                string audioFilePath = audioFilesPaths[i];
                AlbumArtExtractor(audioFilePath, artDestinationDirectory);
            }
            GC.WaitForPendingFinalizers();
            int gen = GC.MaxGeneration;
            GC.Collect(gen, GCCollectionMode.Aggressive);
        }

        /// <summary>
        /// Extracts an audio file's embedded album art, saving it to the specified location.
        /// </summary>
        /// <param name="audioFilePath"></param>
        /// <param name="artDestinationDirectory"></param>
        public static void AlbumArtExtractor(string audioFilePath, string artDestinationDirectory)
        {
            try
            {
                ATL.Track? track = new ATL.Track(audioFilePath);
                string fileName = System.IO.Path.GetFileNameWithoutExtension(audioFilePath);
                string destination = artDestinationDirectory;
                if (!destination.EndsWith('\\')) { destination = destination + "\\"; }
                string artPath = destination + fileName + ".png";
                if (track.EmbeddedPictures.Any() && track.EmbeddedPictures[0] is not null)
                {
                    var pic = track.EmbeddedPictures[0];
                    if (pic.PictureData != null && pic.NativeFormat != Commons.ImageFormat.Unsupported)
                    {
                        MemoryStream memory = new MemoryStream(pic.PictureData, true);
                        Image? tempArt = Image.FromStream(memory);
                        tempArt.Save(artPath, System.Drawing.Imaging.ImageFormat.Png);
                        tempArt.Dispose();
                        tempArt = null;
                        memory.Dispose();
                    }
                }
                track = null;
            }
            catch
            { }
        }

        /// <summary>
        /// ***NOT YET WRITTEN. Use to read xml databases (of playlists) that have been corrupted.
        /// </summary>
        /// <param name="playlistPath"></param>
        /// <returns></returns>
        public static Playlist ReadCorruptedDatabase(string playlistPath)
        {
            Playlist playlist = new Playlist();
            // I'd guess I'll have to read the xml document as sth like txt, line by line
            // and set rules for each line --> sth like:
            // foreach (string line in documentLines) 
            // {
            //      if (line.Contains(Song / Title / Album / etc))
            //          {
            //              string tempTitle = (string)line.Skip (the first parts of xml element);
            //              string title = (string)tempTitle.SkipLast (the last parts of xml element) 
            //          }
            // }
            return playlist;
        }
    }
}
