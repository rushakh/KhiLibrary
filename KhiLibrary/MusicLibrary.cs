namespace KhiLibrary
{
    public class MusicLibrary
    {
        private Playlists playlistList;

        public Playlists Playlists { get => playlistList; }

        public MusicLibrary()
        {
            Song newsong = new();
            Playlist newplaylist = new("test");
            Songs newSongs = new();
            var sth = newplaylist.Songs;
            playlistList = new Playlists();
            playlistList.LoadExistingDatabases();
            //newplaylist.AddSong(newsong);
        }
    }
}
