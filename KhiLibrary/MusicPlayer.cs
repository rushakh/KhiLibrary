﻿using NAudio.Wave;

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
        private static bool playbackStoppedManually = false;
        private static bool isFirstTime = true;
        private static int numberOfBuffers = 3;
        private static int desiredLatency = 200;
        private static float desiredVolume = (float)1.0;
        private static bool enableSingleSongLoop;

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
        public static float Volume { get { return desiredVolume; } set { desiredVolume = value; } }

        /// <summary>
        /// Enables the looping of a single song in the queue. Enabling single song loop, will disable [queue] loop.
        /// </summary>
        public static bool EnableSingleSongLoop
        {
            get => enableSingleSongLoop;
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
            get => playbackQueue.EnableLoop;
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
                    Queue.ClearQueue();
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
                //playbackQueue.CurrentSong.UnloadArt();
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
                //playbackQueue.CurrentSong.UnloadArt();
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
            if (chosenSong != null)
            {
                chosenSong.CurrentTime = timeForSongToStartPlayingFrom;
            }
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
                AudioPlayer.Init(chosenSong);
                AudioPlayer.Play();
            }
            catch
            {
                chosenSong = new AudioFileReader(playbackQueue.CurrentSong.Path);
                AudioPlayer.Init(chosenSong);
                AudioPlayer.Play();
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
                    playbackQueue.MoveToNext();
                    Play();
                }
            }
            else
            {
                playbackStoppedManually = false;
            }
        }
    }
}