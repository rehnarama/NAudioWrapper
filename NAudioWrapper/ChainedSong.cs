using NAudio.Wave;
using System;
using System.Collections.Generic;

namespace NAudioWrapper
{
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
                    //Re-queue the first songso the URI can be available in the callback
                    SongQueue.Enqueue(SongList[0]);
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
                SongQueue.Peek().Play();
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

        //TODO: Improve this seek code to make it stop current playing track, set to next track, seek accordingly, all with one seek call
        public void Seek(int milliseconds)
        {
            SongQueue.Peek().Seek(milliseconds);
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
}
