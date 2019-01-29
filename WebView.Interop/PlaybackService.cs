using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Foundation.Metadata;
using Windows.Media.Core;
using Windows.Media.Playback;

namespace WebView.Interop
{
    public delegate void CurrentStateChangedDelegate(object source, object e);

    /// <summary>
    /// The central authority on playback in the application
    /// providing access to the player and active playlist.
    /// </summary>
    [AllowForWeb]
    public sealed class PlaybackService
    {
        static PlaybackService instance;

        /// <summary>
        /// This application only requires a single shared MediaPlayer
        /// that all pages have access to. The instance could have 
        /// also been stored in Application.Resources or in an 
        /// application defined data model.
        /// </summary>
        public static PlaybackService Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new PlaybackService();

                }

                return instance;
            }
        }

        public MediaPlayer Player { get; private set; }

        public PlaybackService()
        {
            // Create the player instance
            Player = new MediaPlayer();
            Player.AutoPlay = false;
            Player.CurrentStateChanged += Player_CurrentStateChanged;
        }

        public event CurrentStateChangedDelegate CurrentStateChanged;
        private void Player_CurrentStateChanged(MediaPlayer sender, object args)
        {
            CurrentStateChanged?.Invoke(this, args as EventArgs);
        }

        public void Play()
        {
            this.Player.Play();
        }
        public void Pause()
        {
            this.Player.Pause();
        }
        public string Current()
        {
            return (this.Player.Source as MediaPlaybackList).CurrentItem.Source.ToString();
        }
        public void MoveNext()
        {
            (this.Player.Source as MediaPlaybackList).MoveNext();
        }
        public void MovePrevious()
        {
            (this.Player.Source as MediaPlaybackList).MovePrevious();
        }
        public void SkipTo(uint index)
        {
            (this.Player.Source as MediaPlaybackList).MoveTo(index);
        }
        public bool AutoPlay
        {
            set { this.Player.AutoPlay = value; }
            get { return this.Player.AutoPlay; }
        }
        public bool IsLoopingEnabled
        {
            set { this.Player.IsLoopingEnabled = value; }
            get { return this.Player.IsLoopingEnabled; }
        }
        public double Volume
        {
            get { return this.Player.Volume; }
            set { this.Player.Volume = value; }
        }

        public void AddPlayList(string playList)
        {
            JsonObject json = JsonObject.Parse(playList);
            JsonArray songs = json["mediaList"].GetObject()["items"].GetArray();

            foreach(JsonValue song in songs)
            {
                string songURI = song.GetObject()["mediaUri"].GetString();

                AddItem(songURI);
            }
        }

        public void AddItem(string uriString)
        {
            Uri s = new Uri(uriString);
            if (this.Player.Source == null)
                this.Player.Source = new MediaPlaybackList();


            MediaPlaybackItem item = new MediaPlaybackItem(MediaSource.CreateFromUri(s));
            (this.Player.Source as MediaPlaybackList).Items.Add(item);
        }
    }
}
