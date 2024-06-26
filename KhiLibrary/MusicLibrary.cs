namespace KhiLibrary
{
    public class MusicLibrary
    {
        private Playlists playlistList;

        public Playlists Playlists {  get { return playlistList; } }

        public MusicLibrary()
        {
            Playlists playlistCollection = new Playlists();
            playlistList = playlistCollection;
        }
    }
}
