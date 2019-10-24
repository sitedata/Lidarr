using System.Linq;
using NLog;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.History;
using NzbDrone.Core.MediaFiles.TrackImport;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Specifications
{
    public class AlreadyImportedSpecification : IImportDecisionEngineSpecification<LocalAlbumRelease>
    {
        private readonly IHistoryService _historyService;
        private readonly Logger _logger;

        public AlreadyImportedSpecification(IHistoryService historyService,
                                            Logger logger)
        {
            _historyService = historyService;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Database;

        public Decision IsSatisfiedBy(LocalAlbumRelease localAlbumRelease, DownloadClientItem downloadClientItem)
        {
            if (downloadClientItem == null)
            {
                _logger.Debug("No download client information is available, skipping");
                return Decision.Accept();
            }

            var albumRelease = localAlbumRelease.AlbumRelease;

            if (!albumRelease.Tracks.Value.Any(x => x.HasFile))
            {
                _logger.Debug("Skipping already imported check for album without files");
                return Decision.Accept();
            }

            var albumHistory = _historyService.GetByAlbum(albumRelease.AlbumId, null);
            var lastImported = albumHistory.FirstOrDefault(h => h.EventType == HistoryEventType.DownloadImported);

            if (lastImported == null)
            {
                return Decision.Accept();
            }

            // TODO: Ignore last imported check if the same release was grabbed again
            // See: https://github.com/Sonarr/Sonarr/issues/2393

            if (lastImported.DownloadId == downloadClientItem.DownloadId)
            {
                _logger.Debug("Album previously imported");
                return Decision.Reject("Album already imported");
            }

            return Decision.Accept();
        }
    }
}
