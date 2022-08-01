using Dinah.Core.Net.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OverAudible.DownloadQueue
{
    public class ProgressChangedObject
    {
        public string Asin { get; set; }
        public string Title { get; set; }
        public DownloadProgress downloadProgress { get; set; }

        public ProgressChangedObject(string asin, string title, DownloadProgress progress)
        {
            Asin = asin;
            Title = title;
            downloadProgress = progress;
        }
    }
}
