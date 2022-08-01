using Dinah.Core.Net.Http;
using OverAudible.API;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace OverAudible.DownloadQueue
{
    public record QueueFile(string asin, string name);

    public interface IDownloadQueue
    {
        public void Enqueue(QueueFile job);
        public event Action<ProgressChangedObject>? ProgressChanged;
        public event Action? QueueEmptied;
        public event Action<int>? UpdatedQueueCount;
        public List<QueueFile> GetQueue();

        public static async Task DownloadImageAsync(string directoryPath, string fileName, Uri uri)
        {
            using var httpClient = new HttpClient();

            // Get the file extension
            var uriWithoutQuery = uri.GetLeftPart(UriPartial.Path);
            var fileExtension = Path.GetExtension(uriWithoutQuery);

            // Create file path and ensure directory exists
            var path = Path.Combine(directoryPath, $"{fileName}{fileExtension}");
            Directory.CreateDirectory(directoryPath);

            // Download the image and write to the file
            var imageBytes = await httpClient.GetByteArrayAsync(uri);
            await File.WriteAllBytesAsync(path, imageBytes);
        }
    }

    public class MyQueue : IDownloadQueue
    {
        public event Action<ProgressChangedObject>? ProgressChanged;
        public event Action? QueueEmptied;
        public event Action<int>? UpdatedQueueCount;
        private bool isBusy;

        public Queue<QueueFile> queue = new Queue<QueueFile>();

        public async void Enqueue(QueueFile job)
        {
            queue.Enqueue(job);
            UpdatedQueueCount?.Invoke(queue.Count);
            await Task.Run(DoWork);
        }

        private async Task DoWork()
        {
            if (queue.Count == 0 || isBusy)
                return;

            isBusy = true;

            QueueFile file = queue.First();

            var prog = new Progress<DownloadProgress>();
            EventHandler<DownloadProgress> progHandler = (object? sender, DownloadProgress p) => 
            {
                ProgressChanged?.Invoke(new ProgressChangedObject(file.asin, file.name, p));
            };
            prog.ProgressChanged += progHandler;

            

            var api = await ApiClient.GetInstance();

            await api.Api.DownloadAsync(file.asin, new AudibleApi.LibraryPath(Constants.DownloadFolder), 
                new AudibleApi.AsinTitlePair(file.asin, file.name), prog);

            prog.ProgressChanged -= progHandler;

            queue.Dequeue();

            isBusy = false;

            await DoWork();
        }

        public List<QueueFile> GetQueue()
        {
            return queue.ToList();
        }
    }
}
