using AudibleApi;
using Dinah.Core.Net.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OverAudible.DownloadQueue
{
    public class DownloadQueue : IDownloadQueue
    {
        private List<QueueFile> Files { get; set; }
        public event Action<ProgressChangedObject>? ProgressChanged;
        public event Action? QueueEmptied;
        public event Action<int>? UpdatedQueueCount;

        private volatile float percentComplete;
        private readonly Api _api;
        private readonly LibraryPath _libraryPath;

        public async Task AddFileToQueue(QueueFile file, CancellationToken cancellationToken)
        {
            Files.Add(file);
            await DownloadFile(file, cancellationToken);
            Files.Remove(file);
        }

        public Task DownloadFile(QueueFile file, CancellationToken cancellationToken)
        {
            var t = new System.Timers.Timer(500);
            System.Timers.ElapsedEventHandler d1 = (object? state, System.Timers.ElapsedEventArgs e) => { ProgressChanged?.Invoke(new ProgressChangedObject(file.asin, file.name, new DownloadProgress() { ProgressPercentage = percentComplete })); };
            t.Elapsed += d1;
            t.Enabled = true;
            return Task.Run(async () =>
            {
                await Task.Delay(200);
                if (cancellationToken.IsCancellationRequested)
                    return;
                var d = new Progress<DownloadProgress>();
                EventHandler<DownloadProgress> del = (object? sender, DownloadProgress e) => { if (e.ProgressPercentage is not null) { percentComplete = (float)e.ProgressPercentage; } /*OnFileProgressChanged(sender, e, new ProgressChangedObject(file.asin, file.name, e));*/ };
                d.ProgressChanged += del;
                await _api.DownloadAsync(file.asin, _libraryPath, new AsinTitlePair(file.asin, file.name), d);
                d.ProgressChanged -= del;
                t.Enabled = false;
                t.Elapsed -= d1;
            });
        }

        private void OnFileProgressChanged(object? sender, DownloadProgress e, ProgressChangedObject progress)
        {
            //ProgressChanged?.Invoke(progress);
            Debug.WriteLine(e.ProgressPercentage.ToString());
        }

        public void Enqueue(QueueFile job)
        {
            throw new NotImplementedException();
        }

        public List<QueueFile> GetQueue()
        {
            throw new NotImplementedException();
        }

        public DownloadQueue(Api api, LibraryPath libraryPath)
        {
            Files = new();
            _api = api;
            _libraryPath = libraryPath;
        }



    }
}
