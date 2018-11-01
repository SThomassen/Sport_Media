﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using MixedRealityToolkit.InputModule.InputSources;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MixedRealityToolkit.InputModule.EventData
{
    /// <summary>
    /// Describes placement of objects events.
    /// </summary>
    public class PlacementEventData : BaseInputEventData
    {
        /// <summary>
        /// The game object that is being placed.
        /// </summary>
        public GameObject ObjectBeingPlaced { get; private set; }

        public PlacementEventData(EventSystem eventSystem) : base(eventSystem) { }

        public void Initialize(IInputSource inputSource, uint sourceId, object tag, GameObject objectBeingPlaced)
        {
            BaseInitialize(inputSource, sourceId, tag);
            ObjectBeingPlaced = objectBeingPlaced;
        }
    }
}
