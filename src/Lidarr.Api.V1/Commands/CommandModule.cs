using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common;
using NzbDrone.Common.TPL;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.ProgressMessaging;
using Lidarr.SignalR;
using Lidarr.Http;
using Lidarr.Http.Extensions;
using Lidarr.Http.Validation;

namespace Lidarr.Api.V1.Commands
{
    public class CommandModule : LidarrRestModuleWithSignalR<CommandResource, CommandModel>, IHandle<CommandUpdatedEvent>
    {
        private readonly IManageCommandQueue _commandQueueManager;
        private readonly IServiceFactory _serviceFactory;
        private readonly Debouncer _debouncer;
        private readonly Dictionary<int, CommandResource> _pendingUpdates;

        public CommandModule(IManageCommandQueue commandQueueManager,
                             IBroadcastSignalRMessage signalRBroadcaster,
                             IServiceFactory serviceFactory)
            : base(environment, signalRBroadcaster)
        {
            _commandQueueManager = commandQueueManager;
            _serviceFactory = serviceFactory;

            GetResourceById = GetCommand;
            CreateResource = StartCommand;
            GetResourceAll = GetStartedCommands;
            DeleteResource = CancelCommand;

            PostValidator.RuleFor(c => c.Name).NotBlank();

            _debouncer = new Debouncer(SendUpdates, TimeSpan.FromSeconds(0.1));
            _pendingUpdates = new Dictionary<int, CommandResource>();
        }

        private CommandResource GetCommand(int id)
        {
            return _commandQueueManager.Get(id).ToResource();
        }

        private int StartCommand(CommandResource commandResource)
        {
            var commandType =
                _serviceFactory.GetImplementations(typeof (Command))
                               .Single(c => c.Name.Replace("Command", "")
                                             .Equals(commandResource.Name, StringComparison.InvariantCultureIgnoreCase));

            dynamic command = Request.Body.FromJson(commandType);
            command.Trigger = CommandTrigger.Manual;
            command.SuppressMessages = !command.SendUpdatesToClient;
            command.SendUpdatesToClient = true;

            var trackedCommand = _commandQueueManager.Push(command, CommandPriority.Normal, CommandTrigger.Manual);
            return trackedCommand.Id;
        }

        private List<CommandResource> GetStartedCommands()
        {
            return _commandQueueManager.All().ToResource();
        }

        private void CancelCommand(int id)
        {
            _commandQueueManager.Cancel(id);
        }

        public void Handle(CommandUpdatedEvent message)
        {
            if (message.Command.Body.SendUpdatesToClient)
            {
                lock (_pendingUpdates)
                {
                    _pendingUpdates[message.Command.Id] = message.Command.ToResource();
                }

                _debouncer.Execute();
            }
        }

        private void SendUpdates()
        {
            lock (_pendingUpdates)
            {
                var pendingUpdates = _pendingUpdates.Values.ToArray();
                _pendingUpdates.Clear();

                foreach (var pendingUpdate in pendingUpdates)
                {
                    BroadcastResourceChange(ModelAction.Updated, pendingUpdate);

                    if (pendingUpdate.Name == typeof(MessagingCleanupCommand).Name.Replace("Command", "") &&
                        pendingUpdate.Status == CommandStatus.Completed)
                    {
                        BroadcastResourceChange(ModelAction.Sync);
                    }
                }
            }
        }
    }
}
