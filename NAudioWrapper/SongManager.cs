using NAudio.Wave;
using System;
using System.Collections.Generic;

namespace NAudioWrapper
{
    /// <summary>
    /// A manager that handles objects that have implemented the ISong interface
    /// </summary>
    public class SongManager : IDisposable
    {
        private bool _disposed = false;

        private List<ISong> SongList = null;
        public Dictionary<string, ISong> SongDictionary = null;

        /// <summary>
        /// Occurs when [any playback stopped].
        /// </summary>
        public event EventHandler<StoppedEventArgs> AnyPlaybackStopped;

        /// <summary>
        /// Initializes a new instance of the <see cref="SongManager"/> class.
        /// </summary>
        public SongManager()
        {
            SongDictionary = new Dictionary<string, ISong>();
            SongList = new List<ISong>();
        }

        /// <summary>
        /// Adds a song to the manager.
        /// </summary>
        /// <param name="URI">The URI to be played.</param>
        /// <param name="play">if set to <c>true</c> then the song will automatically play once added.</param>
        /// <returns>The song to be played</returns>
        public Song AddSong(string URI, bool play = false)
        {
            Song s = new Song(URI, play);
            AddSong(s, play);
            return s;
        }

        /// <summary>
        /// Adds an instance of an object with the ISong interface implemented.
        /// </summary>
        /// <param name="song">The song to add.</param>
        /// <param name="play">if set to <c>true</c> then the song will automatically play once added.</param>
        public void AddSong(ISong song, bool play = false)
        {
            song.PlaybackStopped += song_PlaybackStopped;

            SongDictionary.Add(song.URI, song);
            SongList.Add(song);
            if (play)
                song.Play();
        }

        void song_PlaybackStopped(object sender, NAudio.Wave.StoppedEventArgs e)
        {
            //Pass through the event
            if (AnyPlaybackStopped != null)
                AnyPlaybackStopped(sender, e);
        }

        /// <summary>
        /// Plays all added songs.
        /// </summary>
        public void PlayAll()
        {
            for (int i = 0; i < SongList.Count; i++)
            {
                SongList[i].Play();
            }
        }

        /// <summary>
        /// Plays and seeks all added songs.
        /// </summary>
        /// <param name="seek">The amount to seek in milliseconds.</param>
        public void PlayAll(int seek)
        {
            for (int i = 0; i < SongList.Count; i++)
            {
                SongList[i].Play(seek);
            }
        }

        /// <summary>
        /// Pauses all added songs.
        /// </summary>
        public void PauseAll()
        {
            for (int i = 0; i < SongList.Count; i++)
            {
                SongList[i].Pause();
            }
        }

        /// <summary>
        /// Resumes all added songs.
        /// </summary>
        public void ResumeAll()
        {
            for (int i = 0; i < SongList.Count; i++)
            {
                SongList[i].Play();
            }
        }

        /// <summary>
        /// Stops all added songs.
        /// </summary>
        public void StopAll()
        {
            for (int i = 0; i < SongList.Count; i++)
            {
                SongList[i].Stop();
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (SongList != null)
                    {
                        for (int i = 0; i < SongList.Count; i++)
                        {
                            if (SongList[i] != null)
                            {
                                SongList[i].Dispose();
                                SongList[i] = null;
                            }
                        }
                        SongList.Clear();
                        SongList = null;
                    }
                    if (SongDictionary != null)
                    {
                        SongDictionary.Clear();
                        SongDictionary = null;
                    }
                }
            }
        }
    }
}
