using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using OverAudible.DownloadQueue;
using OverAudible.Models;
using OverAudible.Services;

namespace OverAudible.Windows
{
    /// <summary>
    /// Interaction logic for Player.xaml
    /// </summary>
    public partial class Player : Window
    {
        private bool dragStarted = false;

        private double oldValue = 0.0;

        public Item Book
        {
            get { return (Item)GetValue(BookProperty); }
            set { SetValue(BookProperty, value); }
        }
        public static readonly DependencyProperty BookProperty =
            DependencyProperty.Register("Book", typeof(Item), typeof(Player));

        public ImageSource Media
        {
            get { return (ImageSource)GetValue(MediaProperty); }
            set { SetValue(MediaProperty, value); }
        }
        public static readonly DependencyProperty MediaProperty =
            DependencyProperty.Register("Media", typeof(ImageSource), typeof(Player));

        public AudioPlayerV2 AudioPlayer
        {
            get { return (AudioPlayerV2)GetValue(AudioPlayerProperty); }
            set { SetValue(AudioPlayerProperty, value); }
        }
        public static readonly DependencyProperty AudioPlayerProperty =
            DependencyProperty.Register("AudioPlayer", typeof(AudioPlayerV2), typeof(Player));

        public bool  ShowChapters
        {
            get { return (bool )GetValue(ShowChaptersProperty); }
            set { SetValue(ShowChaptersProperty, value); }
        }
        public static readonly DependencyProperty ShowChaptersProperty =
            DependencyProperty.Register("ShowChapters", typeof(bool ), typeof(Player), new PropertyMetadata(false, ShowChaptersCallback));

        private static void ShowChaptersCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Player p = d as Player;
            p.ShowModal = true;
        }

        public bool ShowSpeed
        {
            get { return (bool)GetValue(ShowSpeedProperty); }
            set { SetValue(ShowSpeedProperty, value); }
        }
        public static readonly DependencyProperty ShowSpeedProperty =
            DependencyProperty.Register("ShowSpeed", typeof(bool), typeof(Player), new PropertyMetadata(false, ShowSpeedCallback));

        private static void ShowSpeedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Player p = d as Player;
            p.ShowModal = true;
        }

        public bool ShowModal
        {
            get { return (bool)GetValue(ShowModalProperty); }
            set { SetValue(ShowModalProperty, value); }
        }
        public static readonly DependencyProperty ShowModalProperty =
            DependencyProperty.Register("ShowModal", typeof(bool), typeof(Player), new PropertyMetadata(false));


        private SynchronizationContext synchronizationContext;

        public event PropertyChangedEventHandler? PropertyChanged;

        private class MsToSConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is long val)
                {
                    return val / 1000;
                }
                return 0;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        public Player(IDataService<Item> data,Item book)
        {
            InitializeComponent();

            synchronizationContext = SynchronizationContext.Current;

            this.DataContext = this;

            Book = book;

            AudioPlayer = new AudioPlayerV2(data, Constants.DownloadFolder + @"\" + Book.Asin);
            AudioPlayer.TimerTicked += OnTimerTick;
            AudioPlayer.ChapterChanged += OnChapterChanged;
            AudioPlayer.Skipped += OnSkiped;
            AudioPlayer.EndOfFileReached += OnEndOfFile;

            Binding binding = new();
            binding.Source = this;
            binding.Path = new PropertyPath("AudioPlayer.CurrentChapter.LengthMs");
            binding.Converter = new MsToSConverter();

            sldr.SetBinding(Slider.MaximumProperty, binding);

            Media = AppExtensions.GetImageFromString(Constants.DownloadFolder + $@"\{Book.Asin}\{Book.Asin}_Cover.jpg");

        }

        private void OnSkiped(TimeSpan obj)
        {
            synchronizationContext.Post(o =>
            {
                sldr.Value = (int)AudioPlayer.EllapsedTime;
            }, null);
        }

        private void OnEndOfFile()
        {
            synchronizationContext.Post(o =>
            {
                pckPausePlay.Kind = MaterialDesignThemes.Wpf.PackIconKind.PlayCircleFilled;
            }, null);
        }


        private void OnChapterChanged(AudibleApi.Common.Chapter obj)
        {
            synchronizationContext.Post(o =>
            {
                sldr.Value = 0;
            },null);
        }

        private void OnTimerTick(ElapsedEventArgs obj)
        {
            synchronizationContext.Post(o =>
            {
                sldr.Value++;
            }, null);
        }

        private void Speed_Click(object sender, MouseButtonEventArgs e)
        {
            ShowSpeed = true;
        }

        private void Chapters_Click(object sender, MouseButtonEventArgs e)
        {
            ShowChapters = !ShowChapters;
        }

        private void Bookmark_Click(object sender, MouseButtonEventArgs e)
        {

        }

        private void SkipBack_Click(object sender, MouseButtonEventArgs e)
        {
            AudioPlayer.ChangeChapter(-1);
        }

        private void SkipNext_Click(object sender, MouseButtonEventArgs e)
        {
            AudioPlayer.ChangeChapter(1);
        }

        private void Rewind30_Click(object sender, MouseButtonEventArgs e)
        {
            AudioPlayer.Skip(TimeSpan.FromSeconds(30), AudioPlayerV2.Direction.Backward);
        }

        private void FastForward30_Click(object sender, MouseButtonEventArgs e)
        {
            AudioPlayer.Skip(TimeSpan.FromSeconds(30),AudioPlayerV2.Direction.Forward);
        }

        private void PlayPause_Click(object sender, MouseButtonEventArgs e)
        {

            if (sender is MaterialDesignThemes.Wpf.PackIcon icon)
            {

                if (AudioPlayer.IsPlaying)
                {
                    AudioPlayer.Pause();
                    icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.PlayCircleFilled;
                }
                else
                {
                    AudioPlayer.Play();
                    icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.PauseCircleFilled;
                }
            }

            
        }

        private void Slider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            this.dragStarted = false;
            int i;
            /*
            if (sldr.Value == sldr.Maximum)
            {
                AudioPlayer.ChangeChapter(1);
                return;
            }    
            if (sldr.Value == sldr.Minimum)
            {
                AudioPlayer.ChangeChapter(-1);
                return;
            }
            */

            if (oldValue < sldr.Value)
            {
                i = (int)(sldr.Value - oldValue);
                AudioPlayer.Skip(TimeSpan.FromSeconds(i), AudioPlayerV2.Direction.Forward);
            }
            if (sldr.Value < oldValue)
            {
                i = (int)(oldValue - sldr.Value);
                AudioPlayer.Skip(TimeSpan.FromSeconds(i), AudioPlayerV2.Direction.Backward);
            }
        }

        private void Slider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            oldValue = sldr.Value;
            this.dragStarted = true;
        }

        private void ModalClose_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ShowChapters = false;
            ShowSpeed = false;
            ShowModal = false;
        }

        private void ChapterInstance_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Grid g)
            {
                g.Background = new SolidColorBrush(Colors.Black) { Opacity = 0.3};
            }
        }

        private void ChapterInstance_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Grid g)
            {
                g.Background = Brushes.Transparent;
            }
        }

        private void ChapterInstance_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Grid g)
            {
                foreach (var child in g.Children)
                {
                    if (child is TextBlock t)
                    {
                        var c = AudioPlayer._contentMetadata.ChapterInfo.Chapters.FirstOrDefault(x => x.Title == t.Text);

                        if (c != null)
                        {
                            AudioPlayer.ChangeChapter(c);
                            ShowChapters = false;
                            ShowModal = false;
                        }
                    }
                }
            }
        }
    }
}
