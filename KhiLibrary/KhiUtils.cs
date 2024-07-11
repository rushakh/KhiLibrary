using System.Drawing;

namespace KhiLibrary
{
    /// <summary>
    /// Contains the methods needed to process audio files' data, tools for playlists and data filtering
    /// </summary>
    public class KhiUtils
    {
        internal static void ClearTemporaryImages()
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

        internal static void AlbumArtExtractorBatch(string[] audioFilesPaths, string artDestinationDirectory)
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

        internal static void AlbumArtExtractor(string audioFilePath, string artDestinationDirectory)
        {
            try
            {
                ATL.Track? track = new ATL.Track(audioFilePath);
                string fileName = System.IO.Path.GetFileNameWithoutExtension(audioFilePath);
                string destination = artDestinationDirectory;
                if (!destination.EndsWith("\\")) { destination = destination + "\\"; }
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
    }
}
