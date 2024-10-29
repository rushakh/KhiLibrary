using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KhiLibrary
{
    public static class Example
    {
        /// <summary>
        /// The examples for using this library along with the instructions. Don't call the method.
        /// </summary>
        static void Examples()
        {
            // *It's best to use this library through the MusicLibrary class (it's a static class).
            // *There are 2 static classes that are used as entry points, one is the MusicLibrary, and the other is MusicPlayer.
            // ***There is a certain hierarchy inside. MusicLibrary-> Playlists-> Playlist--> Songs--> Song
            // MusicLibrary is a container for Playlists, while playlists is obviously a container for Instances of the Playlist class
            // with some additional features.
            // A Playlist is a container for a collection called Songs in addition to other functionalities.
            // Songs itself is a container for instances of the Song class, plus the additional functionalities
            // and Song is the smallest element of the hierarchy. Instances of the Song class contain the properties of 
            // an audio file, like the title, album, artist, album art cover, etc. while also having other functionalities.
            // There are other classes beside these that do most of the work but are not public.
            //
            // *MusicPlayer can be used at the same time as MusicLibrary but logically you can't Play a song that is not there
            // MusicPlayer basically goes through a Queue of Song objects, playing them one by one. It will replay the queue if
            // set to repeat, the same song of set to single repeat, and play the songs randomely when set to shuffle.
            // There is also an Equalizers collection that is used for sound mixing.
            // *It has to be mentioned that MusicPlayer is built mostly on top of NAudio while the Song class gets the song properties
            // and does the modifications using ATL (by Zeugma).
            // 
            // Forst Off, MusicLibrary. You'll find 2 things there, one is a Playlists collection, the other is a Settings class
            // that can be used to change some program-wide behaviors and Paths.
            // You can access the playlists, Like this:
            var myPlaylists = MusicLibrary.Playlists;
            // Can access individual playlists by Index or searching. 2 default playlists are always created and can be accessed through properties
            // as well (All Songs, and Favorites)
            // All Songs is the default playlist that everything should be added to, while Favorites is there for Song objects that are set to as favorites.
            var allSongsPlaylist = MusicLibrary.Playlists.AllSongs;
            var favoritesPlaylist = MusicLibrary.Playlists.Favorites;
            // New playlists can also be added to this collection, You can either create a playlist by passing a name as
            // parameter (Recommended) like this:
            MusicLibrary.Playlists.Add("New Playlist");
            // Or you could also add an already constructed playlist:
            Playlist myPlaylist = new Playlist("my playlist");
            MusicLibrary.Playlists.Add(myPlaylist);
            // or a range of playlists using AddRange()
            // special characters are not to be used when naming the playlist as the name is also used for
            // the database file as well.
            // As mentioned, you can access the playlists in the collection through searching and index:
            var foundNewPlaylist = MusicLibrary.Playlists.Find("New Playlist");
            var foundNewPlaylistByIndex = MusicLibrary.Playlists[3];
            var foundMyPlaylist = MusicLibrary.Playlists["my playlist"];
            // You can remove the playlists using:
            MusicLibrary.Playlists.Remove(myPlaylist);
            MusicLibrary.Playlists.RemoveAt(3);
            // Or directly using the playlist's method:
            myPlaylist.Remove();
            // Every playlist holds a collection inside, an instance of the class Songs that contains
            // the music added to the playlist. You can add a song to the playlist by using its Songs property.
            // Note: obviously you can't add to a playlist after Removing it, but this is just an example
            // You can add it by passing the music file's full path as the parameter:
            myPlaylist.Songs.Add("C:\\Downloads\\Some cool song.mp3");
            // or by passing an instance of the Song class:
            Song newSong = new Song("C:\\Downloads\\Some cool song.mp3");
            myPlaylist.Songs.Add(newSong);
            // Do keep in mind that many behaviors are not implemented at this level, when a song is added, to a new playlist
            // it does not automatically get added to All Song as well, that has to be done manually; it does not get Saved to
            // the database automatically either, that has to be done by calling Save():
            myPlaylist.Save();
            // you could also save all of the playlists by calling Playlists.Save()
            // The specific songs can be accessed by either index or searching. The search can be be done using exact title,
            // album, and artist of the song:
            var foundSong = myPlaylist.Songs.FindExact("We Rise", "Aviators", "Modern Mythology");
            Song firstSong = myPlaylist.Songs[0];
            // or with just Title or Artist (they can also optionally just be a part of the whole title)
            List<Song>? foundSongs = myPlaylist.Songs.Find("Rise", true, false);
            // every Song object contains properties that returns information about the music file
            string musicTitle = firstSong.Title;
            string musicArtist = firstSong.Artist;
            string musicAlbum = firstSong.Album;
            TimeSpan musicDuration = firstSong.Duration;
            Image musicAlbumArt = firstSong.Art;
            Image musicAlbumArtThumbnail = firstSong.Thumbnail;
            // etc.
            // For MusicPlayer you first need to add songs to the Queue. you can either add one or multiple songs:
            firstSong.AddToQueue();
            // Or a Songs collection:
            myPlaylist.Songs.AddToQueue();
            // or a playlist:
            myPlaylist.AddToQueue();
            // You can also add the songs from inside the queue:
            MusicPlayer.Queue.AddToQueue(firstSong);
            // Or add range
            // Note: the queue is not a generic Queue<T>, but a custom class that can behave the same way with additional
            // functions and properties. 
            // There are several properties inside MusicPlayer that you can change at any time (some changes only take effect from
            // the next song onwards)
            // To get the current Song that's playing or ,better put, the song at the top of the queue:
            Song songAtTop = MusicPlayer.Queue.CurrentSong;
            // To play a song after adding to the queue just call Play():
            MusicPlayer.Play();
            // You can also directly Play an Audio File that has not yet been added to the database or used to create a Song object:
            MusicPlayer.Play("C:\\Downloads\\Some Song.flac");
            // You can skip forward or backwards for a number of seconds during playback:
            // 30 seconds
            MusicPlayer.SkipForwardOrBack(30);
            // Use negative integars for skipping backwards:
            MusicPlayer.SkipForwardOrBack(-30);
            // You can skip to the next or previous song in the queue as well (if there is any other):
            MusicPlayer.SkipToNextSong();
            MusicPlayer.SkipToPreviousSong();
            // You can also pause which freezes the playback, or stop which resets the playback (meaning that if you hit playback again
            // the song that was playing, will now play from the beginning instead of continueing)
            MusicPlayer.Pause();
            MusicPlayer.Stop();
            // It's also possible to specify the playback to go to a specified TimeSpan and start playing from there:
            MusicPlayer.SetCurrentTimeInPlayback(TimeSpan.FromSeconds(73d));
            // To not set to more than the actual duration, get the max timespan first:
            TimeSpan songLength = MusicPlayer.GetPlayingSongMaxDuration();
            // If you want to tweak the sound, you can use the equalizer. For now there is only 1 default profile which
            // is what I use with my ear buds but you can modify it, and also add more:
            KhiLibrary.Equalizer newEQ = new Equalizer("New EQ");
            // You'll have to set from BandZeroGain to BandNineGain manually, and then call UpdateBands()
            // Every band gain is for a certain frequency which you can find out about by reading the summary.
            // But to be brief it's a standard 10-band equalizer that starts from 31.5 Hz up to 16 kHz.
            newEQ.BandZeroGain = 5f;
            newEQ.BandOneGain = -10f;
            //etc
            newEQ.UpdateBands();
            MusicPlayer.EqualizerProfiles.Add(newEQ);
            // Additionally, if you plan on using this specific EQ again, you should also call Save()
            MusicPlayer.EqualizerProfiles.SaveEqualizers();
            // Then set this as the equalizer you want to use:
            MusicPlayer.Equalizer = newEQ;
            // To enable the Equalizer:
            MusicPlayer.UseEqualizer = true;
            // Now it's enabled, the next song that plays will apply this equalizer but if you want to do it now
            // you'll have to update. You also have to update when you change the band gain of the equalizer you're
            // using during playback:
            MusicPlayer.UpdateEqualizer();
        }
    }
}
