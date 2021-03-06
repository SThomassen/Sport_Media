﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace MixedRealityToolkit.UX.ToolTips
{
    /// <summary>
    /// A background with 'fake' inertia
    /// Useful for soft or liquid objects
    /// </summary>
    public class ToolTipBackgroundBlob : ToolTipBackground
    {
        /// <summary>
        /// Which transforms to use for each type of distortion
        /// See the ToolTipBalloon prefab for an example of which transforms to target
        /// </summary>
        [Header("Transform targets")]
        [SerializeField]
        private Transform positionTarget = null;

        [SerializeField]
        private Transform rotationTarget = null;

        [SerializeField]
        private Transform distortionTarget = null;

        [SerializeField]
        private Transform attachPointOffset = null;

        [Header("Blob settings")]
        [SerializeField]
        [Range(0f, 5f)]
        private float blobInertia = 0.5f;

        [SerializeField]
        [Range(0f, 1f)]
        private float blobDistortion = 0.1f;

        [SerializeField]
        [Range(0f, 1f)]
        private float blobRotation = 0.1f;

        [SerializeField]
        [Range(0.1f, 5f)]
        private float positionCorrectionStrength = 1f;

        [SerializeField]
        [Range(0.1f, 5f)]
        private float distortionCorrectionStrength = 1f;

        [SerializeField]
        [Range(0.1f, 5f)]
        private float rotationCorrectionStrength = 1f;

        [SerializeField]
        private Vector3 blobOffset = Vector3.zero;

        [SerializeField]
        private MeshFilter backgroundRendererMeshFilter = null;

        private const float MaxInertia = 5f;
        private const float MaxDistortion = 1f;
        private const float MaxRotation = 1f;
        private const float MinPositionCorrection = 0.1f;
        private const float MinDistortionCorrection = 0.1f;
        private const float MinRotationCorrection = 0.1f;
        private const float MaxPositionCorrection = 5f;
        private const float MaxDistortionCorrection = 5f;
        private const float MaxRotationCorrection = 5f;

        private readonly Bounds defaultBounds = new Bounds(Vector3.zero, Vector3.one);
        private Vector3 lastPosition = Vector3.zero;
        private Vector3 velocity = Vector3.zero;
        private Vector3 distortion = Vector3.zero;
        private Vector3 rotation = Vector3.zero;
        private Bounds localContentBounds = new Bounds(Vector3.zero, Vector3.one);
        private Bounds inertialContentBounds = new Bounds(Vector3.zero, Vector3.one);

        public float BlobInertia
        {
            get
            {
                return blobInertia;
            }
            set
            {
                blobInertia = Mathf.Clamp(value, 0, MaxInertia);
            }
        }

        public float BlobDistortion
        {
            get
            {
                return blobDistortion;
            }
            set
            {
                blobDistortion = Mathf.Clamp(value, 0, MaxDistortion);
            }
        }

        public float BlobRotation
        {
            get
            {
                return blobRotation;
            }
            set
            {
                blobRotation = Mathf.Clamp(value, 0, MaxRotation);
            }
        }

        public float PositionCorrectionStrength
        {
            get
            {
                return positionCorrectionStrength;
            }
            set
            {
                positionCorrectionStrength = Mathf.Clamp(value, MinPositionCorrection, MaxPositionCorrection);
            }
        }

        public float DistortionCorrectionStrength
        {
            get
            {
                return distortionCorrectionStrength;
            }
            set
            {
                distortionCorrectionStrength = Mathf.Clamp(value, MinDistortionCorrection, MaxDistortionCorrection);
            }
        }

        public float RotationCorrectionStrength
        {
            get
            {
                return rotationCorrectionStrength;
            }
            set
            {
                rotationCorrectionStrength = Mathf.Clamp(value, MinRotationCorrection, MaxRotationCorrection);
            }
        }

        public Vector3 BlobOffset
        {
            get
            {
                return blobOffset;
            }
            set
            {
                blobOffset = value;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            lastPosition = positionTarget.position;
            inertialContentBounds = defaultBounds;
            localContentBounds = defaultBounds;
            velocity = Vector3.zero;
        }

        protected override void ScaleToFitContent()
        {
            // Get the local size of the content - this is the scale of the text under the content parent
            Vector3 localContentSize = toolTip.LocalContentSize;
            Vector3 localContentOffset = toolTip.LocalContentOffset;

            // Get the size of the mesh and use this to adjust the local content size on the x / y axis
            // This will accommodate meshes that aren't built to 1,1 scale
            Bounds meshBounds = backgroundRendererMeshFilter.sharedMesh.bounds;
            localContentSize.x /= meshBounds.size.x;
            localContentSize.y /= meshBounds.size.y;
            localContentSize.z = 1;

            localContentBounds.size = Vector3.one; //localContentSize;
            localContentBounds.center = localContentOffset;
        }

        private void Update()
        {
            // Adjust center and size by velocity
            Vector3 currentPosition = positionTarget.position;
            velocity = Vector3.Lerp(velocity, lastPosition - currentPosition, 1f / blobInertia * Time.deltaTime);
            Vector3 currentDistortion = -velocity * blobDistortion;
            distortion = Vector3.Lerp(distortion, currentDistortion, 1f / blobDistortion * Time.deltaTime);

            inertialContentBounds.center = inertialContentBounds.center + velocity;
            Vector3 size = inertialContentBounds.size + distortion;
            inertialContentBounds.size = size;

            Vector3 currentRotation = Vector3.zero;
            currentRotation.x = velocity.x * 360;
            currentRotation.z = velocity.z * 360;
            currentRotation.y = velocity.y * 360;
            currentRotation = rotationTarget.TransformDirection(currentRotation);
            rotation = Vector3.Lerp(rotation, currentRotation, 1f / blobRotation * Time.deltaTime);

            // Correct the center and size
            inertialContentBounds.center = Vector3.Lerp(inertialContentBounds.center, localContentBounds.center, Time.deltaTime * positionCorrectionStrength);
            inertialContentBounds.size = Vector3.Lerp(inertialContentBounds.size, localContentBounds.size, Time.deltaTime * distortionCorrectionStrength);
            rotationTarget.localRotation = Quaternion.Lerp(positionTarget.localRotation, Quaternion.identity, Time.deltaTime * rotationCorrectionStrength);

            // Apply the center and size
            positionTarget.localPosition = inertialContentBounds.center + blobOffset;
            distortionTarget.localScale = inertialContentBounds.size;
            rotationTarget.Rotate(velocity * blobRotation * 360);

            // Adjust the tool tip attach position
            toolTip.AttachPointPosition = attachPointOffset.position;

            lastPosition = currentPosition;
        }
    }
}