using NAudio.Wave;
using System;
using System.IO;

namespace NAudioWrapper
{
    /// <summary>
    /// A basic implementation of the ISong interface
    /// </summary>
    public class Song : ISong, IDisposable
    {
        private bool _disposed = false;

        /// <summary>
        /// Gets the URI of the song.
        /// </summary>
        /// <value>
        /// The URI.
        /// </value>
        public string URI { get; private set; }
        /// <summary>
        /// Gets the play state of the songs.
        /// </summary>
        /// <value>
        /// The value of the play state.
        /// </value>
        public PlaybackState PlayState { get { return output.PlaybackState; } }

        private AudioFileReader reader;
        private BlockAlignReductionStream reduce = null;
        private IWaveProvider provider = null;
        private DirectSoundOut output = null;

        /// <summary>
        /// Occurs when [playback stopped].
        /// </summary>
        public event EventHandler<StoppedEventArgs> PlaybackStopped;

        /// <summary>
        /// Initializes a new instance of the <see cref="Song"/> class.
        /// </summary>
        /// <param name="URI">The URI to be loaded.</param>
        /// <param name="play">if set to <c>true</c> then the song will automatically play once loaded</param>
        public Song(string URI, bool play = false)
        {
            this.URI = URI;
            this.provider = MakeSong(this.URI);
            this.output = new DirectSoundOut(100);
            this.output.Init(provider);

            this.output.PlaybackStopped += output_PlaybackStopped;

            if (play)
                this.Play();
        }

        /// <summary>
        /// Handles the PlaybackStopped event of the output control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="StoppedEventArgs"/> instance containing the event data.</param>
        void output_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            if (PlaybackStopped != null)
            {
                PlaybackStopped(this, e);
            }
        }

        /// <summary>
        /// Plays this song.
        /// </summary>
        public void Play()
        {
            if (output.PlaybackState != PlaybackState.Playing)
                output.Play();
        }

        /// <summary>
        /// Play and seek in this song.
        /// </summary>
        /// <param name="seek">The amount to seek in milliseconds.</param>
        public void Play(int seek)
        {
            reader.Seek(provider.WaveFormat.AverageBytesPerSecond * (seek / 1000), SeekOrigin.Begin);
            Play();
        }

        /// <summary>
        /// Pauses this song.
        /// </summary>
        public void Pause()
        {
            if (output.PlaybackState != PlaybackState.Paused)
                output.Pause();
        }

        /// <summary>
        /// Resumes this song.
        /// </summary>
        public void Resume()
        {
            this.Play();
        }

        /// <summary>
        /// Sets position to 0 and pauses the song. Does not release any resources.
        /// </summary>
        public void Stop()
        {
            //Reset position in case of new start
            reader.Position = 0;
            output.Stop();
        }

        /// <summary>
        /// Seeks to the specified millisecond counting from start of file.
        /// </summary>
        /// <param name="milliseconds">The amount of milliseconds.</param>
        public void Seek(int milliseconds)
        {
            reader.Seek(provider.WaveFormat.AverageBytesPerSecond * (milliseconds / 1000), SeekOrigin.Begin);
        }

        #region PrivateMethods
        private IWaveProvider MakeSong(string URI)
        {
            reader = new AudioFileReader(URI);
            reduce = new BlockAlignReductionStream(reader);
            //return new Wave16ToFloatProvider(new Wave32To16Stream(reader));
            return reader;
        }
        #endregion

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
                    if (output != null)
                    {
                        output.Stop();
                        output.Dispose();
                        output = null;

                    }
                    if (reduce != null)
                    {
                        reduce.Dispose();
                        reduce = null;
                    }

                }
                _disposed = true;
            }
        }
    }
}
