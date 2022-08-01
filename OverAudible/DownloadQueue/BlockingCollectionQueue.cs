using AudibleApi;
using Dinah.Core.Net.Http;
using OverAudible.API;
using OverAudible.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OverAudible.Models;

namespace OverAudible.DownloadQueue
{
    public class BlockingCollectionQueue : IDownloadQueue
    {
        private BlockingCollection<QueueFile> _jobs = new BlockingCollection<QueueFile>();
        public volatile List<QueueFile> queueFiles = new List<QueueFile>();
        private volatile float percentComplete;
        private volatile QueueFile? currentJob;
        private System.Timers.Timer _timer;
        private readonly IDataService<Item> _dataService;

        public BlockingCollectionQueue(IDataService<Item> dataService)
        {
            _dataService = dataService;
            _timer = new System.Timers.Timer(500);
            _timer.Elapsed += TimerTick;
            var thread = new Thread(new ThreadStart(OnStart));
            thread.IsBackground = true;
            thread.Start();
        }

        ~BlockingCollectionQueue()
        {
            _timer.Dispose();
        }

        private void TimerTick(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (currentJob != null)
            {
                ProgressChanged?.Invoke(new ProgressChangedObject(currentJob.asin, currentJob.name, new DownloadProgress() { ProgressPercentage = percentComplete }));
            }
        }

        public event Action<ProgressChangedObject>? ProgressChanged;
        public event Action? QueueEmptied;
        public event Action<int>? UpdatedQueueCount;

        public void Enqueue(QueueFile job)
        {
            _jobs.Add(job);
            queueFiles.Add(job);
            UpdatedQueueCount?.Invoke(queueFiles.Count);
        }

        private async void OnStart()
        {
            foreach (var job in _jobs.GetConsumingEnumerable(CancellationToken.None))
            {
                _timer.Enabled = true;
                var d = new Progress<DownloadProgress>();
                EventHandler<DownloadProgress> del = (object? sender, DownloadProgress e) => { if (e.ProgressPercentage is not null) { percentComplete = (float)e.ProgressPercentage; currentJob = job; } };
                d.ProgressChanged += del;

                var api = await ApiClient.GetInstance();

                await api.Api.DownloadAsync(job.asin, new LibraryPath(Constants.DownloadFolder), new AsinTitlePair(job.asin, job.name), d);

                var m = await api.Api.GetLibraryBookMetadataAsync(job.asin);
                await _dataService.UpdateMetadata(job.asin, m);

                var book = await _dataService.GetById(job.asin);
                if (book is not null && book.ProductImages is not null && book.ProductImages.The500 is not null)
                    await IDownloadQueue.DownloadImageAsync(Constants.DownloadFolder + @"\" +job.asin, job.asin + "_Cover", book.ProductImages.The500 );

                d.ProgressChanged -= del;
                _timer.Enabled = false;

                queueFiles.Remove(job);

                UpdatedQueueCount?.Invoke(_jobs.Count);

                if (_jobs.Count == 0)
                    QueueEmptied?.Invoke();
            }
        }

        public List<QueueFile> GetQueue()
        {
            //MessageBox.Show(_jobs.GetConsumingEnumerable().Count().ToString());
            return queueFiles;
        }
    }
}
