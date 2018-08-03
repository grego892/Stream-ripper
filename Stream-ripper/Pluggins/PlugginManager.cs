﻿using System;
using System.Linq;
using StreamRipper.Builders;
using StreamRipper.Models;
using StreamRipper.Models.Events;

namespace StreamRipper.Pluggins
{
    public class PluginManager
    {
        public Action<MetadataChangedEventArg> OnMetadataChanged { get; }

        public Action<StreamUpdateEventArg> OnStreamUpdate { get; }
        
        public Action<StreamStartedEventArg> OnStreamStarted { get; }
        
        public Action<SongChangedEventArg> OnSongChanged { get; }

        private SongInfo _songInfo;
                
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="onMetadataChanged"></param>
        /// <param name="onStreamUpdate"></param>
        /// <param name="onStreamStarted"></param>
        /// <param name="onSongChanged"></param>
        public PluginManager(Action<MetadataChangedEventArg> onMetadataChanged,
            Action<StreamUpdateEventArg> onStreamUpdate, Action<StreamStartedEventArg> onStreamStarted,
            Action<SongChangedEventArg> onSongChanged)
        {            
            OnSongChanged = ActionEventHandlerBuilder<SongChangedEventArg>.New()
                .SetActionHandler(onSongChanged)
                .WrapAsync()
                .Build();
            
            OnMetadataChanged = ActionEventHandlerBuilder<MetadataChangedEventArg>.New()
                .SetActionHandler(onMetadataChanged)
                // Trigger song change event
                .AddBeforeExecution(_ =>
                {
                    if (_songInfo.SongMetadata != null)
                        OnSongChanged(new SongChangedEventArg {SongInfo = _songInfo});
                })
                // Hold on to the metadata
                .AddAfterExecution(x =>
                {
                    _songInfo.SongMetadata = x.SongMetadata;
                })
                .WrapAsync()
                .Build();

            OnStreamUpdate = ActionEventHandlerBuilder<StreamUpdateEventArg>.New()
                .SetActionHandler(onStreamUpdate)
                // Update the bytes array
                .AddAfterExecution(x =>
                {
                    // _songInfo.Bytes = (_songInfo.Bytes ?? new byte[0]).Concat(x.SongRawPartial).ToArray();
                })
                .WrapAsync()
                .Build();
            
            OnStreamStarted = ActionEventHandlerBuilder<StreamStartedEventArg>.New()
                .SetActionHandler(onStreamStarted)
                .WrapAsync()
                // Initialize the buffer
                .AddAfterExecution(_ => _songInfo = new SongInfo())
                .Build();
        }
    }
}