using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;
using System.IO;

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
            SongDictionary.Add(URI, s);
            SongList.Add(s);
            return s;
        }

        /// <summary>
        /// Adds an instance of an object with the ISong interface implemented.
        /// </summary>
        /// <param name="song">The song to add.</param>
        /// <param name="play">if set to <c>true</c> then the song will automatically play once added.</param>
        public void AddSong(ISong song, bool play = false)
        {
            SongList.Add(song);
            if (play)
                song.Play();
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

    /// <summary>
    /// An interface meant to be implemented by audio objects
    /// </summary>
    public interface ISong : IDisposable
    {
        /// <summary>
        /// Gets the URI of the song.
        /// </summary>
        /// <value>
        /// The URI.
        /// </value>
        string URI { get; }
        /// <summary>
        /// Gets the play state of the songs.
        /// </summary>
        /// <value>
        /// The value of the play state.
        /// </value>
        PlaybackState PlayState { get; }

        /// <summary>
        /// Occurs when [playback stopped].
        /// </summary>
        event EventHandler<StoppedEventArgs> PlaybackStopped;

        /// <summary>
        /// Plays this song.
        /// </summary>
        void Play();
        /// <summary>
        /// Play and seek in this song.
        /// </summary>
        /// <param name="seek">The amount to seek in milliseconds.</param>
        void Play(int seek);
        /// <summary>
        /// Pauses this song.
        /// </summary>
        void Pause();
        /// <summary>
        /// Sets position to 0 and pauses the song. Does not release any resources.
        /// </summary>
        void Stop();
    }

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

        private IWaveProvider MakeSong(string URI)
        {
            reader = new AudioFileReader(URI);
            reduce = new BlockAlignReductionStream(reader);
            //return new Wave16ToFloatProvider(new Wave32To16Stream(reader));
            return reader;
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

    /// <summary>
    /// An extension to the Song object - it chains several Song objects together into one seamless song
    /// </summary>
    public class ChainedSong : ISong, IDisposable
    {
        bool _disposed;

        Queue<Song> SongQueue;
        List<Song> SongList;

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
            get { return SongQueue.Peek().URI; }
        }

        /// <summary>
        /// Gets the play state of the songs.
        /// </summary>
        /// <value>
        /// The value of the play state.
        /// </value>
        public PlaybackState PlayState
        {
            get { return SongQueue.Peek().PlayState; }
        }

        /// <summary>
        /// Occurs when [playback stopped].
        /// </summary>
        public event EventHandler<StoppedEventArgs> PlaybackStopped;

        /// <summary>
        /// Notifies the playback stopped event.
        /// </summary>
        /// <param name="e">The <see cref="StoppedEventArgs"/> instance containing the event data.</param>
        private void NotifyPlaybackStopped(StoppedEventArgs e)
        {
            if (PlaybackStopped != null)
            {
                PlaybackStopped(this, e);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChainedSong"/> class.
        /// </summary>
        /// <param name="URIs">The list of URIs containing the absolute path of the songs to be played.</param>
        public ChainedSong(IEnumerable<string> URIs)
        {
            SongList = new List<Song>();
            foreach (String s in URIs)
            {
                Song song = new Song(s);
                song.PlaybackStopped += song_PlaybackStopped;
                SongList.Add(song);
            }
            SongQueue = new Queue<Song>(SongList);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChainedSong"/> class.
        /// </summary>
        /// <param name="songs">The songs to be played.</param>
        public ChainedSong(IEnumerable<Song> songs)
        {
            SongList = new List<Song>();
            foreach (Song song in songs)
            {
                song.PlaybackStopped += song_PlaybackStopped;
                SongList.Add(song);
            }
            SongQueue = new Queue<Song>(SongList);
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
                SongQueue.Dequeue();
                if (SongQueue.Count > 0 && !_disposed)
                {
                    Play();
                }
                else
                {
                    NotifyPlaybackStopped(e);
                }
            }
        }

        /// <summary>
        /// Plays this song.
        /// </summary>
        public void Play()
        {
            if (SongQueue.Count > 0)
            {
                SongQueue.Peek().Play(0);
                _manualStop = false;
            }
        }

        /// <summary>
        /// Play and seek in this song.
        /// </summary>
        /// <param name="seek">The amount to seek in milliseconds.</param>
        public void Play(int seek)
        {
            if (SongQueue.Count > 0)
            {
                SongQueue.Peek().Play(seek);
                _manualStop = false;
            }
        }

        /// <summary>
        /// Pauses this song.
        /// </summary>
        public void Pause()
        {
            if (SongQueue.Count > 0)
                SongQueue.Peek().Pause();
        }

        /// <summary>
        /// Sets position to 0 and pauses the song. Does not release any resources.
        /// </summary>
        public void Stop()
        {
            if (SongQueue.Count > 0)
            {
                _manualStop = true;
                SongQueue.Peek().Stop();
                //In case of manual stop, re add all songs to the queue to make it seem as if the chain are one large song instead
                SongQueue.Clear();
                SongQueue = new Queue<Song>(SongList);
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
                        while (SongList.Count > 0)
                        {
                            if (SongList[0] != null)
                            {
                                SongList[0].Dispose();
                                SongList.RemoveAt(0);
                            }
                        }
                        SongList = null;
                    }
                }
                _disposed = true;
            }
        }
    }

    /// <summary>
    /// An extension to the Song object - it loops a song forever until manually stopped
    /// </summary>
    public class LoopedSong : ISong, IDisposable
    {
        bool _disposed;

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
        /// Initializes a new instance of the <see cref="LoopedSong"/> class.
        /// </summary>
        /// <param name="URI">The URI to the song to be played</param>
        /// <param name="play">Whether or not to start it immediately</param>
        public LoopedSong(string URI, bool play = false)
        {
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
                song.Play(0);
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
                        song.Dispose();
                        song = null;
                    }
                }
                _disposed = true;
            }
        }

    }
}
