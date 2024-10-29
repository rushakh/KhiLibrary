using NAudio.Wave;

namespace KhiLibrary
{
    /// <summary>
    /// The music player that plays songs according to the queue, allows for simple play, pause, skip and previous, ...
    /// In addition to having Loop modes and shuffle mode. More specific actions should be done within the Queue.
    /// </summary>
    public static class MusicPlayer
    {
        private static PlaybackQueue playbackQueue = new PlaybackQueue();
        private static AudioFileReader? chosenSong;
        private static WaveOutEvent AudioPlayer = new WaveOutEvent();
        private static Equalizers equalizerProfiles = new Equalizers(true);
        private static Equalizer equalizer = equalizerProfiles.EqualizerProfiles[0];
        private static bool playbackStoppedManually = false;
        private static bool isFirstTime = true;
        private static int numberOfBuffers = 3;
        private static int desiredLatency = 200;
        private static float desiredVolume = (float)1.0;
        private static bool enableSingleSongLoop;
        private static bool useEqualizer = false;

        /// <summary>
        /// The queue that is used for playback. Songs should be added here
        /// </summary>
        public static PlaybackQueue Queue { get { return playbackQueue; } }

        /// <summary>
        /// The number of buffers that are used in playback.
        /// </summary>
        public static int NumberOfBuffers { get { return numberOfBuffers; } set { numberOfBuffers = value; } }

        /// <summary>
        /// The desired latency in milliseconds
        /// </summary>
        public static int DesiredLatency { get { return desiredLatency; } set { desiredLatency = value; } }

        /// <summary>
        /// The volume of the song, 1.0 is the maximum.
        /// </summary>
        public static float Volume 
        { 
            get
            {
                return desiredVolume;
            }
            set
            {
                desiredVolume = value;
                AudioPlayer.Volume = desiredVolume;
            }
        }

        /// <summary>
        /// Enables the looping of a single song in the queue. Enabling single song loop, will disable [queue] loop.
        /// </summary>
        public static bool EnableSingleSongLoop
        {
            get
            {
                return enableSingleSongLoop;
            }
            set
            {
                enableSingleSongLoop = value;
                if (enableSingleSongLoop)
                {
                    playbackQueue.EnableLoop = false;
                }
            }
        }

        /// <summary>
        /// Enables loop mode in the queue. Enabling loop, will disable single song loop.
        /// </summary>
        public static bool EnableLoop
        {
            get
            {
                return playbackQueue.EnableLoop;
            }
            set
            {
                playbackQueue.EnableLoop = value;
                if (!playbackQueue.EnableLoop)
                {
                    enableSingleSongLoop = false;
                }
            }
        }

        /// <summary>
        /// Enables shuffle mode for the queue, shuffling the songs in the queue.
        /// </summary>
        public static bool EnableShuffle { get => playbackQueue.EnableShuffle; set => playbackQueue.EnableShuffle = value; }

        /// <summary>
        /// Enables and disables the equalizer. Change equalizer setting using the Equalizer.
        /// </summary>
        public static bool UseEqualizer
        {
            get => useEqualizer;
            set
            {
                useEqualizer = value;
                UpdateEqualizer();
            }
        }

        /// <summary>
        /// A collection of Equalizer profiles. Can add or remove from it. By default the first profile (index of 0) is used.
        /// </summary>
        public static Equalizers EqualizerProfiles { get => equalizerProfiles; set => equalizerProfiles = value; }

        /// <summary>
        /// A 10 band equalizer, change the gains using its properties, then update
        /// </summary>
        public static Equalizer Equalizer { get => equalizer; set => equalizer = value; }

        /// <summary>
        /// Shows the current state of the Music Player.
        /// </summary>
        public static PlaybackState PlaybackState { get => AudioPlayer.PlaybackState;}

        /// <summary>
        /// Plays the first Song that was added to the queue.
        /// </summary>
        public static void Play()
        {
            try
            {
                // If the musicplayer was paused, then it will resume playback without reseting it.
                if (AudioPlayer.PlaybackState == PlaybackState.Paused)
                {
                    AudioPlayer.Play();
                }
                else
                {
                    PrepareAndPlayTheSong();
                }
            }
            catch
            {
                PrepareAndPlayTheSong();
            }
        }

        /// <summary>
        /// Plays an audio file that is not in the application's database. Use this for example when an audio file 
        /// is chosen to be played using this application from the Open With context menu or somethign similar. 
        /// </summary>
        /// <param name="audioFilePath"></param>
        public static void Play (string audioFilePath)
        {
            try
            {
                if (System.IO.File.Exists(audioFilePath))
                {
                    Song unincorporatedAudio = new Song(audioFilePath, true);
                    Queue.Clear();
                    Queue.AddToQueue(unincorporatedAudio);
                    Queue.MoveToNext();
                    PrepareAndPlayTheSong();
                }
            }
            catch { }
        }

        /// <summary>
        /// Pauses audio playback, which can then be resumed.
        /// </summary>
        public static void Pause ()
        {
            AudioPlayer.Pause();
        }

        /// <summary>
        /// Stops the audio playback and resets.
        /// </summary>
        public static void Stop()
        {
            playbackStoppedManually = true;
            AudioPlayer.Stop();
        }

        /// <summary>
        /// Skips forward to the next song in the queue and plays it.
        /// </summary>
        public static void SkipToNextSong()
        {
            try
            {
                playbackQueue.MoveToNext();
                PrepareAndPlayTheSong();
            }
            catch
            {
                PrepareAndPlayTheSong();
            }
        }

        /// <summary>
        /// Goes back to and plays the previous song in the queue.
        /// </summary>
        public static void SkipToPreviousSong()
        {
            try
            {
                playbackQueue.MoveBack();
                PrepareAndPlayTheSong();
            }
            catch
            {
                PrepareAndPlayTheSong();
            }
        }

        /// <summary>
        /// Skips forward or backward a number of seconds in playback. Negative numbers should be inputted to skip backward.
        /// </summary>
        /// <param name="secondsToSkipForwardOrBackward"></param>
        public static void SkipForwardOrBack (int secondsToSkipForwardOrBackward)
        {
            if (chosenSong != null)
            {
                chosenSong.Skip(secondsToSkipForwardOrBackward);
            }
        }

        /// <summary>
        /// Returns the song's current playback time as TimeSpan. If no song is playing or paused, returns null. 
        /// </summary>
        /// <returns></returns>
        public static TimeSpan GetCurrentTimeInPlayback()
        {
                var currentTime = chosenSong.CurrentTime;
                return currentTime;
        }

        /// <summary>
        /// Changes the song's current position on playback as TimeSpan. Will throw exception if set more than the song's max duration.
        /// </summary>
        /// <param name="timeForSongToStartPlayingFrom"></param>
        public static void SetCurrentTimeInPlayback(TimeSpan timeForSongToStartPlayingFrom)
        {
            // Won't throw exceptions here, Naudio will do that and checking if the time is actually correct would be of no use
            if (chosenSong != null)
            {
                chosenSong.CurrentTime = timeForSongToStartPlayingFrom;
            }
        }

        /// <summary>
        /// Returns the total duration of the song that is playing/paused right now.
        /// </summary>
        /// <returns></returns>
        public static TimeSpan GetPlayingSongMaxDuration()
        {
            var tempTotallTime = chosenSong.TotalTime;
            // need to check if it is in correct format later.
            return tempTotallTime;
        }

        /// <summary>
        /// Disposes the previous Audio Reader and WaveOut, Applies the settings to the Audio Player,
        /// Initilizes it, and plays the song.
        /// </summary>
        private static void PrepareAndPlayTheSong()
        {
            try
            {
                playbackStoppedManually = true;
                AudioPlayer.PlaybackStopped -= AudioPlayer_PlaybackStopped;
                if (chosenSong != null) { chosenSong.Dispose(); }
                if (AudioPlayer != null) { AudioPlayer.Dispose(); }               
                chosenSong = new AudioFileReader(playbackQueue.CurrentSong.Path);
                AudioPlayer = new WaveOutEvent();
                AudioPlayer.PlaybackStopped += AudioPlayer_PlaybackStopped;
                SetAudioPlayerSettings();
                if (useEqualizer)
                {
                    var equalizedSong = equalizer.EqualizeSong(chosenSong);
                    AudioPlayer.Init(equalizedSong);
                }
                else
                {
                    AudioPlayer.Init(chosenSong);
                }
                AudioPlayer.Play();
                playbackStoppedManually = false;
            }
            catch
            {
                chosenSong = new AudioFileReader(playbackQueue.CurrentSong.Path);
                AudioPlayer.Init(chosenSong);
                AudioPlayer.Play();
            }
        }

        /// <summary>
        /// Should be called when a song is playing and the equalizer bands' gains have been updated. 
        /// Only applies the equalizer to the loaded song if UseEqualizer has been set to true otherwise 
        /// simply updates the equalizer bands.
        /// </summary>
        public static void UpdateEqualizer()
        {
            equalizer.UpdateBands();
            int state = 1;
            if (AudioPlayer.PlaybackState == PlaybackState.Playing) { state = 1; }
            else if (AudioPlayer.PlaybackState == PlaybackState.Paused) { state = 2; }
            if (state == 1 || state == 2)
            {
                playbackStoppedManually = true;
                AudioPlayer.PlaybackStopped -= AudioPlayer_PlaybackStopped;
                if (AudioPlayer != null) { AudioPlayer.Dispose(); }
                AudioPlayer = new WaveOutEvent();
                AudioPlayer.PlaybackStopped += AudioPlayer_PlaybackStopped;
                SetAudioPlayerSettings();
                if (useEqualizer)
                {
                    var equalizedSong = equalizer.EqualizeSong(chosenSong);
                    AudioPlayer.Init(equalizedSong);
                }
                else
                {
                    AudioPlayer.Init(chosenSong);
                }
                if (state == 1) { AudioPlayer.Play(); }
                else if (state == 2)
                {
                    AudioPlayer.Play();
                    AudioPlayer.Pause();
                }
                playbackStoppedManually = false;
            }
        }

        /// <summary>
        /// The settings of the audio player.
        /// </summary>
        private static void SetAudioPlayerSettings()
        {
            AudioPlayer.NumberOfBuffers = numberOfBuffers;
            AudioPlayer.DesiredLatency = desiredLatency;
            AudioPlayer.Volume = desiredVolume;
        }

        /// <summary>
        /// This event is called whenever the stream feeding into the Waveout event (Audio Player) returns 0. 
        /// This means this event is called, when playback reaches the end, or when it is stopped manually.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void AudioPlayer_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            if (!playbackStoppedManually)
            {
                AudioPlayer?.Dispose();
                AudioPlayer = new WaveOutEvent();
                if (enableSingleSongLoop)
                {
                    Play();
                }
                else if (playbackQueue.Count() > 1)
                {
                    // Instead of implementing a system to keep track of played songs, will just use Exceptions as a crutch.
                    // (the system is needed when Shuffle is enabled otherwise could just check if currentIndex was
                    // the queue's max index before the call to MoveToNext)
                    try
                    {
                        playbackQueue.MoveToNext();
                        Play();
                    }
                    catch
                    {
                        // This means that the queue has reached the end and loop is not enabled.
                        // For now don't  want to do anything here but might later add sth.
                    }
                }
            }
        }
    }
}
