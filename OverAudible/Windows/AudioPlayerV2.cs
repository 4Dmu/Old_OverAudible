using OverAudible.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OverAudible.Models;
using System.IO;
using System.Timers;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using ShellUI.Controls;
using OverAudible.ViewModels;

namespace OverAudible.Windows
{
    public class AudioPlayerV2 : BaseViewModel
    {
        private const string _audioFileExtension = ".m4b";
        private const string _timeFormat = "hh':'mm':'ss";
        private const int _timerInterval = 1000;
        private readonly IDataService<Item> _dataService;
        private string _folder;
        private string _asin;
        private List<FileInfo> _bookParts;
        private List<FileInfo> _allFiles;
        public AudibleApi.Common.ContentMetadata _contentMetadata { get; set; }
        private Timer _timer;

        public event Action? PlaybackStarted;
        public event Action? PlaybackStopped;
        public event Action<TimeSpan>? Skipped;
        public event Action<AudibleApi.Common.Chapter>? ChapterChanged;
        public event Action<ElapsedEventArgs>? TimerTicked;
        public event Action EndOfFileReached;


        bool isPlaying;
        public bool IsPlaying { get => isPlaying; set => SetProperty(ref isPlaying , value); }

        int currentChapterIndex;
        public int CurrentChapterIndex
        {
            get => currentChapterIndex;
            set
            {
                if (SetProperty(ref currentChapterIndex, value))
                    OnPropertyChanged(nameof(CurrentChapter));
            }
        }

        int ellapsedTime;
        public int EllapsedTime
        {
            get => ellapsedTime;
            set
            {
                if (SetProperty(ref ellapsedTime, value))
                    OnPropertyChanged(nameof(FormatEllapsedTime));
            }
        }

        int remainingTime;
        public int RemainingTime
        {
            get => remainingTime;
            set
            {
                if (SetProperty(ref remainingTime, value))
                    OnPropertyChanged(nameof(FormatRemainingTime));
            }
        }

        public PlaylistV2 Playlist { get; set; }
        public AudibleApi.Common.Chapter CurrentChapter => _contentMetadata.ChapterInfo.Chapters[CurrentChapterIndex];

        public string FormatEllapsedTime => TimeSpan.FromSeconds(EllapsedTime).ToString(_timeFormat);
        public string FormatRemainingTime => TimeSpan.FromSeconds(RemainingTime).ToString(_timeFormat);

        public bool IsAtEndOfFile { get; private set; }

        public AudioPlayerV2(IDataService<Item> dataService, string folder)
        {
            _dataService = dataService;
            _folder = folder;
            _bookParts = new List<FileInfo>();
            _allFiles = new List<FileInfo>();
            _contentMetadata = new();
            _timer = new(_timerInterval);
            Init();
        }

        private void Init()
        {
            GetFiles();
            GetMetadata();
            CreatePlayer();
            Initialize();
        }

        private void GetFiles()
        {
            string[] files = Directory.GetFiles(_folder);
            foreach (string file in files)
            {
                FileInfo fileInfo = new FileInfo(file);
                _allFiles.Add(fileInfo);
                
                if (fileInfo.Extension == _audioFileExtension)
                    _bookParts.Add(fileInfo);
            }
        }

        private void GetMetadata()
        {
            string filter = "_Cover";
            char startFilterChar = filter[0];
            _asin = _allFiles.FirstOrDefault(x => x.Name.Contains(filter), null).Name;

            if (!string.IsNullOrWhiteSpace(_asin))
            {
                _asin = _asin.Remove(_asin.IndexOf(startFilterChar));
                _contentMetadata = _dataService.GetByIdWithMetadata(_asin).GetAwaiter().GetResult().Item2;
            }
        }

        private void CreatePlayer()
        {
            Playlist = new(_bookParts.Select(x => x.FullName).ToList());
        }

        private void Initialize()
        {
            _timer.Elapsed += OnTimerEllapsed;
            RemainingTime = GetSeconds(CurrentChapter.LengthMs);
        }

        private void OnTimerEllapsed(object? sender, ElapsedEventArgs e)
        {
            if (CurrentChapterIndex == _contentMetadata.ChapterInfo.Chapters.Length - 1 && EllapsedTime + 1 == GetSeconds(CurrentChapter.LengthMs))
            {
                EndOfFile();
                return;
            }
            EllapsedTime++;
            RemainingTime--;

            if (EllapsedTime == GetSeconds(CurrentChapter.LengthMs))
                ChangeChapter(1);
            else
                TimerTicked?.Invoke(e);
        }

        private int GetSeconds(long lengthMs)
        {
            return (int)(lengthMs / 1000);
        }

        private int GetSeconds(double lengthMs)
        {
            return (int)(lengthMs / 1000);
        }

        public void Play()
        {
            IsPlaying = true;
            Playlist.Play();
            _timer.Start();
            PlaybackStarted?.Invoke();
        }

        public void Pause()
        {
            IsPlaying = false;
            Playlist.Pause();
            _timer.Stop();
            PlaybackStopped?.Invoke();
        }

        private void EndOfFile()
        {
            IsAtEndOfFile = true;
            EndOfFileReached?.Invoke();
            Pause();
            ChangeChapter(_contentMetadata.ChapterInfo.Chapters[0]);
            IsAtEndOfFile = false;
        }

        public void Skip(TimeSpan amount, Direction direction)
        {
            var c = Playlist.GetTotalTimeUpToCurrentReader().Add(Playlist.GetCurrentPosition());
            var add = (int)(c.Add(amount).TotalMilliseconds / 1000);
            var sub = (int)(c.Subtract(amount).TotalMilliseconds / 1000);
            var total = (int)(TotalUpToCurrentChapterMillieseconds() / 1000);

            if (add >= total && direction == Direction.Forward)
            {
                ChangeChapter(1);
                return;
            }

            if (sub <= GetSeconds(CurrentChapter.StartOffsetMs) && direction == Direction.Backward)
            {
                ChangeChapter(-1);
                return;
            }

            switch (direction)
            {
                case Direction.Forward:
                    if (Playlist.GetCurrentPosition().Add(amount) > Playlist.GetTotalTime())
                        return;
                    Playlist.Skip(amount,false);
                    EllapsedTime += (int)amount.TotalSeconds;
                    RemainingTime -= (int)amount.TotalSeconds;
                    Skipped?.Invoke(amount);
                    break;
                case Direction.Backward:
                    if (Playlist.GetCurrentPosition().Subtract(amount) < TimeSpan.Zero && CurrentChapter == _contentMetadata.ChapterInfo.Chapters[0])
                        return;
                    Playlist.Skip(amount, true);
                    EllapsedTime -= (int)amount.TotalSeconds;
                    RemainingTime += (int)amount.TotalSeconds;
                    Skipped?.Invoke(amount);
                    break;
            }
           
        }

        private double TotalUpToCurrentChapterMillieseconds()
        {
            long ms = 0;
            for (int i = 0; i <= CurrentChapterIndex; i++)
            {
                ms += _contentMetadata.ChapterInfo.Chapters[i].LengthMs;
            }

            return ms;
        }

        public void ChangeChapter(AudibleApi.Common.Chapter chapter)
        {
            int index = _contentMetadata.ChapterInfo.Chapters.ToList().IndexOf(chapter);

            if (index < 0 || index > _contentMetadata.ChapterInfo.Chapters.Length)
            {

                EndOfFile();
                return;
            }



            CurrentChapterIndex = index;
            EllapsedTime = 0;

            if (CurrentChapter == _contentMetadata.ChapterInfo.Chapters.Last())
            {
                int extraTime = (int)TotalChapterSeconds() - GetSeconds(Playlist.GetTotalTime().TotalMilliseconds);
                extraTime = Math.Abs(extraTime);
                CurrentChapter.LengthMs += extraTime * 1000;
                OnPropertyChanged(nameof(CurrentChapter));
            }

            Playlist.GoToPosition(TimeSpan.FromSeconds(CurrentChapter.StartOffsetSec));
            RemainingTime = GetSeconds(CurrentChapter.LengthMs);
            ChapterChanged?.Invoke(CurrentChapter);
        }

        public void ChangeChapter(int direction)
        {
            if (CurrentChapterIndex + direction < 0 || CurrentChapterIndex + direction >= _contentMetadata.ChapterInfo.Chapters.Length)
            {
                EndOfFile();
                return;
            }

            CurrentChapterIndex += direction;
            EllapsedTime = 0;

            if (CurrentChapter == _contentMetadata.ChapterInfo.Chapters.Last())
            {
                int extraTime = (int)TotalChapterSeconds() - GetSeconds(Playlist.GetTotalTime().TotalMilliseconds);
                extraTime = Math.Abs(extraTime);
                CurrentChapter.LengthMs += extraTime * 1000;
                OnPropertyChanged(nameof(CurrentChapter));
            }

            Playlist.GoToPosition(TimeSpan.FromSeconds(CurrentChapter.StartOffsetSec));
            RemainingTime = GetSeconds(CurrentChapter.LengthMs);
            ChapterChanged?.Invoke(CurrentChapter);
        }

        private long TotalChapterSeconds()
        {
            return _contentMetadata.ChapterInfo.Chapters.Sum(x => x.LengthMs) / 1000;
        }

        public enum Direction
        {
            Forward,
            Backward,
        }

    }

    public class AudioPlaylist
    {
        private Queue<string> playlist;
        private IWavePlayer player;
        private WaveStream fileWaveStream;

        public AudioPlaylist(List<string> startingPlaylist)
        {
            playlist = new Queue<string>(startingPlaylist);
        }

        public void PlaySong()
        {
            if (playlist.Count < 1)
            {
                return;
            }

            if (player != null && player.PlaybackState != PlaybackState.Stopped)
            {
                player.Stop();
            }
            if (fileWaveStream != null)
            {
                fileWaveStream.Dispose();
            }
            if (player != null)
            {
                player.Dispose();
                player = null;
            }

            player = new WaveOutEvent();
            fileWaveStream = new AudioFileReader(playlist.Dequeue());
            player.Init(fileWaveStream);
            player.PlaybackStopped += (sender, evn) => { PlaySong(); };
            player.Play();
        }
    }

    public class PlaylistV2 : IDisposable
    {
        private List<AudioFileReader> _readers;
        private WaveOutEvent? _outputDevice;
        private List<string> _files;
        private int currentReader = 0;
        private bool disposed = false;
        private bool canBeStopped;

        public PlaylistV2(List<string> files)
        {
            _files = files;
            _readers = new List<AudioFileReader>();
            _outputDevice = new();
            Init();
        }

        private void Init()
        {
            foreach (var f in _files)
            {
                _readers.Add(new AudioFileReader(f));
            }
            _outputDevice.Init(_readers[0]);
            _outputDevice.PlaybackStopped += (s, e) =>
            {
                if (canBeStopped)
                {
                    canBeStopped = false;
                    return;
                }
                ChangeReader();
                _outputDevice.Play();
            };
        }

        private void ChangeReader()
        {
            currentReader++;
            if (currentReader > _readers.Count)
            {
                currentReader = 0;
            }
            _outputDevice.Init(_readers[currentReader]);
        }

        private void ChangeReader(int num)
        {
            currentReader = num;
            if (currentReader > _readers.Count)
            {
                currentReader = 0;
            }
            bool isPlaying = _outputDevice.PlaybackState == PlaybackState.Playing;
            canBeStopped = true;
            _outputDevice.Stop();
            _outputDevice.Init(_readers[currentReader]);
            if (isPlaying)
                _outputDevice.Play();
        }

        public TimeSpan GetCurrentPosition()
        {
            return _readers[currentReader].CurrentTime;
        }

        public TimeSpan GetCurrentReaderTotalTime()
        {
            return _readers[currentReader].TotalTime;
        }

        public TimeSpan GetTotalTime()
        {
            return _readers
                .Select(x => x.TotalTime)
                .Aggregate(TimeSpan.Zero, (subtotal, t) => subtotal.Add(t));
        }

        public TimeSpan GetTotalTimeUpToCurrentReader()
        {
            return _readers
                .GetRange(0, currentReader)
                .Select(x => x.TotalTime)
                .Aggregate(TimeSpan.Zero, (subtotal, t) => subtotal.Add(t));
        }

        public void Skip(TimeSpan amount, bool goBack = false)
        {
            if (goBack)
            {
                var pos = GetCurrentPosition().Subtract(amount);
                if (pos < TimeSpan.Zero)
                {
                    if (currentReader == 0)
                        return;
                    ChangeReader();
                    Skip(amount, goBack);
                }

                _readers[currentReader].CurrentTime = _readers[currentReader].CurrentTime.Subtract(amount);
            }
            else
            {
                var pos = GetCurrentPosition().Add(amount);
                var time = GetCurrentReaderTotalTime();
                if (pos > time)
                {
                    ChangeReader();
                    Skip(amount, goBack);
                }

                _readers[currentReader].CurrentTime = _readers[currentReader].CurrentTime.Add(amount);
            }
        }

        public void GoToPosition(TimeSpan position)
        {
            List<TimeSpan> readerLengths = _readers.Select(x => x.TotalTime).ToList();
            TimeSpan totalReaderTime = GetTotalTime();

            foreach (TimeSpan readerLength in readerLengths)
            {
                int index = readerLengths.IndexOf(readerLength);

                var length = readerLengths.GetRange(0, index + 1).Aggregate(TimeSpan.Zero, (subtotal, t) => subtotal.Add(t));
                if (position <= length)
                {
                    ChangeReader(readerLengths.IndexOf(readerLength));

                    if (readerLength == readerLengths[0])
                    {
                        _readers[currentReader].CurrentTime = position;
                    }
                    else
                    {
                        var totalTime = GetTotalTimeUpToCurrentReader();
                        var newPos = position.Subtract(totalTime);
                        _readers[currentReader].CurrentTime = newPos;
                    }

                    return;
                }
            }
        }

        public void Play()
        {
            _outputDevice?.Play();
        }

        public void Pause()
        {
            _outputDevice?.Pause();
        }

        public void Dispose()
        {
            Dispose(disposing: true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    _outputDevice.Dispose();
                    foreach (var reader in _readers)
                    {
                        reader.Dispose();
                    }
                }

                // Note disposing has been done.
                disposed = true;
            }
        }
    }

    public class Playlist
    {
        private AudioFileReader? Reader;
        private WaveOutEvent? OutputDevice;
        private List<string> TotalFiles { get; set; }

        private List<TimeSpan> FileLengths { get; set; }
        private string currentFile;
        private Queue<string> Files { get; set; }

        public Playlist(List<string> files)
        {
            Files = new(files);
            TotalFiles = files;
            FileLengths = new();
            Init();
        }

        private void Init()
        {
            OutputDevice = new();
            var f = Files.Dequeue();
            currentFile = f;
            Reader = new(f);
            OutputDevice.Init(Reader);
            OutputDevice.PlaybackStopped += (s, e) =>
            {
                if (Files.Count == 0)
                {
                    Files = new Queue<string>(TotalFiles);
                }

                Init();
                Play();
            };
        }

        public void Play()
        {
            OutputDevice?.Play();
        }

        public void Pause()
        {
            OutputDevice?.Pause();
        }

        public void Skip(TimeSpan amount, AudioPlayerV2.Direction direction)
        {
            if (Reader.CurrentTime.Add(amount) > Reader.TotalTime)
            {
                if (Files.Count == TotalFiles.Count)
                    return;
                Init();
                Skip(amount, direction);
                return;
            }

            switch (direction)
            {
                case AudioPlayerV2.Direction.Forward:
                    Reader.CurrentTime = Reader.CurrentTime.Add(amount);
                    break;
                case AudioPlayerV2.Direction.Backward:
                    if (Reader.CurrentTime.Subtract(amount) < TimeSpan.Zero)
                        return;
                    Reader.CurrentTime = Reader.CurrentTime.Subtract(amount);
                    break;
            }
        }

        public void GoToPosition(TimeSpan positionToGoTo)
        {
            if (positionToGoTo < TimeSpan.Zero)
                return;
            var currentFileTotalTime = Reader.TotalTime;
            var timeUpToCurrentFile = GetTimeUpToCurrentFile();

            var finalTime = positionToGoTo;

            if (currentFile != TotalFiles[0])
                finalTime = positionToGoTo.Subtract(timeUpToCurrentFile);




            if (finalTime > currentFileTotalTime)
            {
                if (Files.Count == TotalFiles.Count)
                    return;
                Init();
                GoToPosition(positionToGoTo);
                return;
            }



            Reader.CurrentTime = finalTime;
        }

        public TimeSpan GetCurrentPosition()
        {
            return Reader.CurrentTime;
        }

        public TimeSpan GetCurrentFileTotalTime()
        {
            return Reader.TotalTime;
        }

        public TimeSpan GetTotalTime()
        {
            TimeSpan t = new();
            FileLengths.Clear();
            foreach (var file in TotalFiles)
            {
                var reader = new AudioFileReader(file);
                t = t.Add(reader.TotalTime);
                FileLengths.Add(reader.TotalTime);
                reader.Dispose();
            }
            return t;
        }

        private TimeSpan GetTimeUpToCurrentFile()
        {
            int index = TotalFiles.IndexOf(currentFile);
            TimeSpan t = new();
            for (int i = 0; i < index; i++)
            {
                var reader = new AudioFileReader(TotalFiles[i]);
                t = t.Add(reader.TotalTime);
            }
            return t;
        }
    }
}
