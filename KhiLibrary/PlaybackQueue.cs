
namespace KhiLibrary
{
    /// <summary>
    /// A List of Song objects that are to be played, duplicate songs cannnot 
    /// be added. Allows for reading and manipulating this queue in addition to
    /// having loop modes (single song and queue) and shuffle mode.
    /// </summary>
    public class PlaybackQueue
    {
        private List<Song> songsQueue;
        private int[] randomIndices;
        private Random random;
        private int currentIndex;
        private bool enableLoop;
        private bool enableShuffle;
        private bool isShuffled;
        private Song currentSong;

        /// <summary>
        /// Returns the current songs in the playback queue.
        /// </summary>
        internal List<Song> Queue
        {
            get
            {
                return songsQueue;
            }
        }

        /// <summary>
        /// The current index that will be used for selecting current Song.
        /// </summary>
        public int CurrentUnAdultratedIndex { get { return currentIndex; } }

        /// <summary>
        /// Enables shuffle mode for the queue, shuffling the songs in the queue.
        /// </summary>
        internal bool EnableShuffle
        {
            get
            {
                return enableShuffle;
            }
            set
            {
                enableShuffle = value;
                if (enableShuffle)
                {
                    ShuffleIndices();
                    isShuffled = true;
                }
            }
        }

        /// <summary>
        /// Enables loop mode in the queue.
        /// </summary>
        internal bool EnableLoop
        {
            get => enableLoop;
            set => enableLoop = value;
        }

        /// <summary>
        /// Returns the current song in the queue. Returns an empty Song object if there are no songs in the queue.
        /// </summary>
        public Song CurrentSong 
        { 
            get
            {
                if (currentSong.Path == string.Empty)
                {
                    currentSong = songsQueue[GetRandomIndex(currentIndex)];
                }
                return currentSong;
            }
            set 
            { 
                currentSong = value;
                UpdateQueue();
            }
        }

        /// <summary>
        /// Internal constructor that initializes and sets the default values.
        /// </summary>
        internal PlaybackQueue()
        {
            songsQueue = new List<Song>();
            randomIndices = Array.Empty<int>();
            random = new Random();
            currentIndex = 0;
            enableLoop = false;
            enableShuffle = false;
            isShuffled = false;
            currentSong = new Song();
        }

        /// <summary>
        /// Returns an IEnumerator that can be used to iterates through the songs in this queue.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Song> GetEnumerator()
        {
            return songsQueue.GetEnumerator();
        }

        /// <summary>
        /// Adds a song to the end of the queue.
        /// </summary>
        /// <param name="song"></param>
        public void AddToQueue(Song song)
        {
            if (!songsQueue.Contains(song))
            {
                songsQueue.Add(song);
                if (enableShuffle)
                {
                    ShuffleIndices();
                }
            }
            UpdateQueue();
        }

        /// <summary>
        /// Adds a range of Song objects to the end of the queue.
        /// </summary>
        /// <param name="songs"></param>
        public void AddRangeToQueue(List<Song> songs)
        {
            List<Song> temp = new List<Song>();
            foreach (Song song in songs)
            {
                if (!songsQueue.Contains(song))
                {
                    temp.Add(song);
                }

            }
            songsQueue.AddRange(temp);
            if (enableShuffle)
            {
                ShuffleIndices();
            }
            UpdateQueue();
        }

        /// <summary>
        /// Clears the items in the queue.
        /// </summary>
        public void Clear ()
        {
            songsQueue.Clear();
            randomIndices = Array.Empty<int>();
            currentIndex = 0;
            random = new Random();
            UpdateQueue();
        }

        /// <summary>
        /// The number of songs in the queue.
        /// </summary>
        /// <returns></returns>
        public int Count ()
        {
            return songsQueue.Count;
        }

        /// <summary>
        /// Determines if a specific song exists within the queue.
        /// </summary>
        /// <param name="specificSong"></param>
        /// <returns></returns>
        public bool Contains (Song specificSong)
        {
            return songsQueue.Contains(specificSong);
        }

        /// <summary>
        /// Returns the next song in the queue without changing the queue.
        /// </summary>
        /// <returns></returns>
        public Song PeekNext()
        {
            int tempIndex = currentIndex + 1;
            int index = GetRandomIndex(tempIndex);
            return songsQueue[index];
        }

        /// <summary>
        /// Returns the last Song in the queue without changing the queue.
        /// </summary>
        /// <returns></returns>
        public Song PeekLast()
        {
            int tempIndex = GetRandomIndex(songsQueue.Count - 1);
            return songsQueue[tempIndex];
        }

        /// <summary>
        /// Returns the song at the inputted index without changing the queue.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Song PeekSongAt(int index)
        {
            int tempIndex = GetRandomIndex(index);
            return songsQueue[tempIndex];
        }

        /// <summary>
        /// Insert a song at the inputted index.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="song"></param>
        public void InsertAt (int index, Song song)
        {
            songsQueue.Insert(index, song);
            if (enableShuffle)
            {
                ShuffleQueue();
            }
            UpdateQueue();
        }

        /// <summary>
        /// Inser a range of Song objects starting from the inputed index.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="songsList"></param>
        public void InsertRangeAt (int index, List<Song> songsList)
        {
            songsQueue.InsertRange(index, songsList);
            if (enableShuffle)
            {
                ShuffleQueue();
            }
            UpdateQueue();
        }

        /// <summary>
        /// Skips the specified number of times in the queue.
        /// </summary>
        /// <param name="numberOfMovesForward"></param>
        public void Move (int numberOfMovesForward)
        {
            int tempIndex = currentIndex + numberOfMovesForward;
            currentIndex = CheckAndReturnCorrectIndex(tempIndex);
            int maybeRandom = GetRandomIndex(currentIndex);
            currentSong = songsQueue[maybeRandom];
        }

        /// <summary>
        /// Skips to the specified index in the queue
        /// </summary>
        /// <param name="index"></param>
        public void MoveTo (int index)
        {
            int tempIndex = CheckAndReturnCorrectIndex(index);
            currentIndex = CheckAndReturnCorrectIndex(tempIndex);
            int maybeRandom = GetRandomIndex(currentIndex);
            currentSong = songsQueue[maybeRandom];
        }

        /// <summary>
        /// Moves back to the previous song in the queue.
        /// </summary>
        public void MoveBack ()
        {
            int tempIndex = CheckAndReturnCorrectIndex(currentIndex -1);
            currentIndex = CheckAndReturnCorrectIndex(tempIndex);
            int maybeRandom = GetRandomIndex(currentIndex);
            currentSong = songsQueue[maybeRandom];
        }

        /// <summary>
        /// Moves forward to the next song in the queue.
        /// </summary>
        public void MoveToNext ()
        {
            int tempIndex = currentIndex + 1;
            currentIndex = CheckAndReturnCorrectIndex(tempIndex);
            int maybeRandom = GetRandomIndex(currentIndex);
            currentSong = songsQueue[maybeRandom];
        }

        /// <summary>
        /// Removes a song from the queue. ***SHOULD Look out how this might impact playback.
        /// </summary>
        /// <param name="song"></param>
        public void RemoveFromQueue (Song song)
        {
            songsQueue.Remove(song);
            if (enableShuffle)
            {
                ShuffleQueue();
            }
            UpdateQueue();
        }

        /// <summary>
        /// Removes a song from the queue using its index. ***SHOULD Look out how this might impact playback.
        /// </summary>
        /// <param name="index"></param>
        public void RemoveAt (int index)
        {
            songsQueue.RemoveAt(index);
            if (enableShuffle)
            {
                ShuffleQueue();
            }
            UpdateQueue();
        }

        /// <summary>
        /// Removes multiple songs from the queue.
        /// </summary>
        /// <param name="songs"></param>
        public void RemoveRangeFromQueue(List<Song> songs)
        {
            foreach (Song song in songs)
            {
                songsQueue.Remove(song);
            }
            if (enableShuffle)
            {
                ShuffleQueue();
            }
            UpdateQueue();
        }

        /// <summary>
        /// Shuffles the queue elements and enables shuffle mode.
        /// </summary>
        public void ShuffleQueue()
        {
            ShuffleIndices();
            isShuffled = true;
            enableShuffle = true;
        }

        /// <summary>
        /// Sorts the queue based on the songs Title (0), Artist (1), Album (2), Duration(3), Genre(4), TrackNumber (5), 
        /// and PlayedCount (6). Sorts into an ascending order by default, can be changed to descending.
        /// </summary>
        /// <param name="columnNumber"></param>
        /// <param name="ascending"></param>
        public void SortQueue(int columnNumber, bool ascending = true)
        {
            switch (columnNumber)
            {
                case 0:
                    songsQueue.Sort((x, y) => x.Title.CompareTo(y.Title));
                    break;
                case 1:
                    songsQueue.Sort((x, y) => x.Artist.CompareTo(y.Artist));
                    break;
                case 2:
                    songsQueue.Sort((x, y) => x.Album.CompareTo(y.Album));
                    break;
                case 3:
                    songsQueue.Sort((x, y) => x.Duration.CompareTo(y.Duration));
                    break;
                case 4:
                    songsQueue.Sort((x, y) => x.Genres.CompareTo(y.Genres));
                    break;
                case 5:
                    songsQueue.Sort((x, y) => x.TrackNumber.CompareTo(y.TrackNumber));
                    break;
                case 6:
                    songsQueue.Sort((x, y) => x.PlayedCount.CompareTo(y.PlayedCount));
                    break;
                default:
                    songsQueue.Sort((x, y) => x.Title.CompareTo(y.Title));
                    break;
            }
            if (!ascending)
            {
                songsQueue.Reverse();
            }
        }

        /// <summary>
        /// Updates the current Song and current index.
        /// </summary>
        private void UpdateQueue()
        {
            int max = songsQueue.Count;
            int currentPosition = GetRandomIndex(currentIndex);
            if (currentIndex > max)
            {
                if (!songsQueue.Contains(currentSong)) 
                { 
                    currentIndex = 0;
                    currentSong = songsQueue[currentPosition];
                }
                // This means that current index might be wrong
                else
                {
                    currentIndex = songsQueue.IndexOf(currentSong);
                }
                currentIndex = max - 1;
            }
            else if (currentIndex < max)
            {
                if (songsQueue[currentPosition] != currentSong)
                {
                    if (!songsQueue.Contains(currentSong))
                    {
                        currentSong = songsQueue[currentPosition];
                    }
                    else
                    {
                        currentIndex = songsQueue.IndexOf(currentSong);
                    }
                }
            }
            else if (max == 0)
            {
                currentIndex = 0;
                currentSong = new Song();
            }
        }

        /// <summary>
        /// Shuffles the indices, with every index corresponding to another one in the list.
        /// </summary>
        private void ShuffleIndices()
        {
            int length = songsQueue.Count;
            int[] randomIndex = new int[length];
            for (int i = 0; i < length; i++)
            {
                randomIndex[i] = i;
            }
            random.Shuffle(randomIndex);
            random.Shuffle(randomIndex);
            randomIndices = randomIndex;
            isShuffled = true;
        }

        /// <summary>
        /// Checks and corrects the inputed index, considering the loop state. if loop is not enabled while inputed index is
        /// bigger than Queue elements count or a negative number, throws an exception. This method is what makes the loop work.
        /// </summary>
        /// <param name="tempIndex"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private int CheckAndReturnCorrectIndex(int tempIndex)
        {
            int maxIndex = songsQueue.Count - 1;
            if (tempIndex > maxIndex)
            {
                if (enableLoop)
                {
                    // In case Shuffle is enabled and the index is larger than max index, it means that since the queue has
                    // reached the end, it needs to be reshuffled
                    if (enableShuffle)
                    {
                        ShuffleIndices();
                    }
                    int targetIndex = tempIndex % maxIndex;
                    return targetIndex;
                }
                else
                {
                    throw new ArgumentOutOfRangeException("Index out of range, set " + enableLoop + "to true to ignore this or keep the index in mind.");
                }
            }
            else if (tempIndex == maxIndex) { return tempIndex; }
            else
            {
                if (tempIndex < 0)
                {
                    if (enableLoop)
                    {
                        // In case Shuffle is enabled and the index is smaller than 0, it means that it was manually skipped
                        // to this position and the queue shouldn't be reshuffled 
                        int targetIndex = maxIndex + tempIndex;
                        return targetIndex;
                    }
                    else { throw new ArgumentOutOfRangeException("Index out of range, set " + enableLoop + "to true to ignore this or keep the index in mind."); }

                }
                else
                {
                    return tempIndex;
                }
            }
        }

        /// <summary>
        /// If shuffle is enabled, returns the random index corresponding to the inputed index, otherwise returns the inputed index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private int GetRandomIndex(int index)
        {
            if (songsQueue.Count > 0)
            {
                if (enableShuffle)
                {
                    if (randomIndices == null || randomIndices.Length == 0 || isShuffled == false)
                    {
                        ShuffleIndices();
                    }
                    return randomIndices[index];
                }
                else
                { return index; }
            }
            // It will lead to exception when used, no need to throw an exception here, it would be useless.
            else
            {
                return index;
            }
        }
    }
}
