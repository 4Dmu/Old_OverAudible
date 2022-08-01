using AudibleApi;
using Dinah.Core.Net.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OverAudible.DownloadQueue
{
    public class NoDedicatedThreadQueue : IDownloadQueue
    {
        private Queue<QueueFile> _queue = new Queue<QueueFile>();
        private bool _delegateQueuedOrRunning = false;
        public event Action<ProgressChangedObject>? ProgressChanged;
        public event Action? QueueEmptied;
        public event Action<int>? UpdatedQueueCount;

        private readonly AudibleApi.Api _api;
        private readonly LibraryPath _libraryPath;
        private volatile float percentComplete;

        public void Enqueue(QueueFile job)
        {
            lock (_queue)
            {
                _queue.Enqueue(job);
                if (!_delegateQueuedOrRunning)
                {
                    _delegateQueuedOrRunning = true;
                    ThreadPool.UnsafeQueueUserWorkItem(ProcessQueuedItems, null);
                }
            }
        }

        private async void ProcessQueuedItems(object ignored)
        {
            while (true)
            {
                QueueFile item;
                lock (_queue)
                {
                    if (_queue.Count == 0)
                    {
                        _delegateQueuedOrRunning = false;
                        break;
                    }

                    item = _queue.Dequeue();
                }

                try
                {
                    var t = new System.Timers.Timer(500);
                    System.Timers.ElapsedEventHandler d1 = (object? state, System.Timers.ElapsedEventArgs e) => { ProgressChanged?.Invoke(new ProgressChangedObject(item.asin, item.name, new DownloadProgress() { ProgressPercentage = percentComplete })); };
                    t.Elapsed += d1;
                    t.Enabled = true;
                    await Task.Run(async () =>
                    {
                        var d = new Progress<DownloadProgress>();
                        EventHandler<DownloadProgress> del = (object? sender, DownloadProgress e) => { if (e.ProgressPercentage is not null) { percentComplete = (float)e.ProgressPercentage; } /*OnFileProgressChanged(sender, e, new ProgressChangedObject(file.asin, file.name, e));*/ };
                        d.ProgressChanged += del;
                        await _api.DownloadAsync(item.asin, _libraryPath, new AsinTitlePair(item.asin, item.name), d);
                        d.ProgressChanged -= del;
                        t.Enabled = false;
                        t.Elapsed -= d1;
                    });
                }
                catch
                {
                    ThreadPool.UnsafeQueueUserWorkItem(ProcessQueuedItems, null);
                    throw;
                }
            }
        }

        public List<QueueFile> GetQueue()
        {
            throw new NotImplementedException();
        }

        public NoDedicatedThreadQueue(AudibleApi.Api api, LibraryPath libraryPath)
        {
            _api = api;
            _libraryPath = libraryPath;
        }
    }
}
