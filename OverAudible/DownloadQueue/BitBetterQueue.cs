using AudibleApi;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OverAudible.DownloadQueue
{
    public class BitBetterQueue : IDownloadQueue
    {
        private ConcurrentQueue<QueueFile> _jobs = new ConcurrentQueue<QueueFile>();
        private readonly AudibleApi.Api _api;
        private readonly LibraryPath _libraryPath;

        public BitBetterQueue()
        {
            var thread = new Thread(new ThreadStart(OnStart));
            thread.IsBackground = true;
            thread.Start();
        }

        public event Action<ProgressChangedObject>? ProgressChanged;
        public event Action? QueueEmptied;
        public event Action<int>? UpdatedQueueCount;

        public void Enqueue(QueueFile job)
        {
            _jobs.Enqueue(job);
        }

        private void OnStart()
        {
            while (true)
            {
                if (_jobs.TryDequeue(out QueueFile result))
                {
                    Console.WriteLine(result);
                }
            }
        }

        public List<QueueFile> GetQueue()
        {
            throw new NotImplementedException();
        }

        public BitBetterQueue(AudibleApi.Api api, LibraryPath libraryPath)
        {
            _api = api;
            _libraryPath = libraryPath;
        }
    }
}
