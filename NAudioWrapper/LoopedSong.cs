using NAudio.Wave;
using System;

namespace NAudioWrapper
{
    /// <summary>
    /// An extension to the Song object - it loops a song forever until manually stopped
    /// </summary>
    public class LoopedSong : ISong, IDisposable
    {
        bool _disposed;

        int _loops;

        Song song;
        /// <summary>
        /// Whether or not the song was stopped manually. true if manual stop, false if natural, i.e. the song ended
        /// </summary>
        private bool _manualStop = false;

        /// <summary>
        /// Gets the URI of the song.
        /// </summary>
        /// <value>
        /// The URI.
        /// </value>
        public string URI
        {
            get { return song.URI; }
        }

        /// <summary>
        /// Gets the play state of the songs.
        /// </summary>
        /// <value>
        /// The value of the play state.
        /// </value>
        public PlaybackState PlayState
        {
            get { return song.PlayState; }
        }

        /// <summary>
        /// Occurs when [playback stopped].
        /// </summary>
        public event EventHandler<StoppedEventArgs> PlaybackStopped;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoopedSong" /> class.
        /// </summary>
        /// <param name="URI">The URI to the song to be played</param>
        /// <param name="play">Whether or not to start it immediately</param>
        /// <param name="loops">The amount of times to loop the song. -1 for infinite times, 0 for one play, no loops, 1 for two plays, one loop etc.</param>
        public LoopedSong(string URI, bool play = false, int loops = -1)
        {
            _loops = loops;

            song = new Song(URI, play);
            song.PlaybackStopped += song_PlaybackStopped;
        }

        /// <summary>
        /// Handles the PlaybackStopped event of the song control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="StoppedEventArgs"/> instance containing the event data.</param>
        void song_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            if (!_manualStop)
            {
                if (_loops > 0)
                {
                    _loops--;
                    song.Play(0);
                }
                else if (_loops == -1)
                {
                    song.Play(0);
                }
            }
        }

        /// <summary>
        /// Plays this song.
        /// </summary>
        public void Play()
        {
            if (song != null)
            {
                _manualStop = false;
                song.Play();
            }
        }

        /// <summary>
        /// Play and seek in this song.
        /// </summary>
        /// <param name="seek">The amount to seek in milliseconds.</param>
        public void Play(int seek)
        {
            if (song != null)
            {
                _manualStop = false;
                song.Play(seek);
            }
        }

        /// <summary>
        /// Pauses this song.
        /// </summary>
        public void Pause()
        {
            if (song != null)
                song.Pause();
        }

        /// <summary>
        /// Sets position to 0 and pauses the song. Does not release any resources.
        /// </summary>
        public void Stop()
        {
            if (song != null)
            {
                _manualStop = true;
                song.Stop();
            }
        }

        public void Seek(int milliseconds)
        {
            song.Seek(milliseconds);
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
                    if (song != null)
                    {
                        this.Stop();                        
                        song.Dispose();
                    }
                }

                song = null;
                _disposed = true;
            }
        }

    }
}
