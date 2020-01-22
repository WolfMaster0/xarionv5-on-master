// This file is part of the OpenNos NosTale Emulator Project.
// 
// This program is licensed under a deviated version of the Fair Source License,
// granting you a non-exclusive, non-transferable, royalty-free and fully-paid-up
// license, under all of the Licensor's copyright and patent rights, to use, copy, prepare
// derivative works of, publicly perform and display the Software, subject to the
// conditions found in the LICENSE file.
// 
// THIS FILE IS PROVIDED "AS IS", WITHOUT WARRANTY OR
// CONDITION, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. THE AUTHORS HEREBY DISCLAIM ALL LIABILITY, WHETHER IN
// AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE.
using OpenNos.Data;
using System;
using System.Collections.Generic;
using OpenNos.GameObject.Event;
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject
{
    public class Portal : PortalDTO
    {
        #region Members

        private Guid _destinationMapInstanceId;

        private Guid _sourceMapInstanceId;

        #endregion

        #region Instantiation

        public Portal() => OnTraversalEvents = new List<EventContainer>();

        public Portal(PortalDTO input)
        {
            OnTraversalEvents = new List<EventContainer>();
            DestinationMapId = input.DestinationMapId;
            DestinationX = input.DestinationX;
            DestinationY = input.DestinationY;
            IsDisabled = input.IsDisabled;
            PortalId = input.PortalId;
            SourceMapId = input.SourceMapId;
            SourceX = input.SourceX;
            SourceY = input.SourceY;
            Type = input.Type;
        }

        #endregion

        #region Properties

        public Guid DestinationMapInstanceId
        {
            get
            {
                if (_destinationMapInstanceId == default && DestinationMapId != -1)
                {
                    _destinationMapInstanceId = ServerManager.GetBaseMapInstanceIdByMapId(DestinationMapId);
                }
                return _destinationMapInstanceId;
            }
            set => _destinationMapInstanceId = value;
        }

        public List<EventContainer> OnTraversalEvents { get; set; }

        public Guid SourceMapInstanceId
        {
            get
            {
                if (_sourceMapInstanceId == default)
                {
                    _sourceMapInstanceId = ServerManager.GetBaseMapInstanceIdByMapId(SourceMapId);
                }
                return _sourceMapInstanceId;
            }
            set => _sourceMapInstanceId = value;
        }

        #endregion

        #region Methods

        public string GenerateGp() => $"gp {SourceX} {SourceY} {ServerManager.GetMapInstance(DestinationMapInstanceId)?.Map.MapId ?? 0} {Type} {PortalId} {(IsDisabled ? 1 : 0)}";

        #endregion
    }
}