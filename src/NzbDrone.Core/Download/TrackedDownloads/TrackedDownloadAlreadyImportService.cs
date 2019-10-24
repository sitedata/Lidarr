using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.History;

namespace NzbDrone.Core.Download.TrackedDownloads
{
    public interface ITrackedDownloadAlreadyImportService
    {
        bool IsImported(TrackedDownload trackedDownload, List<History.History> historyItems);
    }

    public class TrackedDownloadAlreadyImportService : ITrackedDownloadAlreadyImportService
    {
        public bool IsImported(TrackedDownload trackedDownload, List<History.History> historyItems)
        {
            if (historyItems.Empty())
            {
                return false;
            }

            var allAlbumsImportedInHistory = trackedDownload.RemoteAlbum.Albums.All(album =>
            {
                var lastHistoryItem = historyItems.FirstOrDefault(h => h.AlbumId == album.Id);

                if (lastHistoryItem == null)
                {
                    return false;
                }

                return lastHistoryItem.EventType == HistoryEventType.DownloadImported;
            });

            return allAlbumsImportedInHistory;
        }
    }
}
