using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Rendering;
using Unity.Collections;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using UnityEngine.Profiling;

namespace UTJ.SS2Profiler
{
    public class ScreenShotToProfiler
    {
        public static readonly Guid MetadataGuid = new Guid("4389DCEB-F9B3-4D49-940B-E98482F3A3F8");
        public static readonly int InfoTag = -1;

        public static ScreenShotToProfiler Instance { get; private set; } = new ScreenShotToProfiler();

#if DEBUG
        private ScreenShotLogic renderTextureBuffer;
        private GameObject behaviourGmo;
        private int frameIdx = 0;
        private int lastRequestIdx = -1;

        private CustomSampler captureSampler;
        private CustomSampler updateSampler;
#endif

        public bool Initialize()
        {
            if (Screen.width > Screen.height)
            {
                return Initialize(192,128);
            }
            else
            {
                return Initialize(128, 192);
            }
        }

        public bool Initialize(int width , int height)
        {
            if (!SystemInfo.supportsAsyncGPUReadback)
            {
                return false;
            }
#if DEBUG
            if (renderTextureBuffer != null) { return false; }
            InitializeLogic(width,height);
#endif
            return true;
        }
        private void InitializeLogic(int width,int height)
        {
#if DEBUG
            renderTextureBuffer = new ScreenShotLogic(width, height);
            var behaviourGmo = new GameObject();
            behaviourGmo.hideFlags = HideFlags.HideAndDontSave;
            GameObject.DontDestroyOnLoad(behaviourGmo);
            var behaviour = behaviourGmo.AddComponent<ScreenShotBehaviour>();

            this.captureSampler = CustomSampler.Create("ScreenshotToProfiler.Capture");
            this.updateSampler = CustomSampler.Create("ScreenshotToProfiler.Update");
            behaviour.captureFunc += this.Capture;
            behaviour.updateFunc += this.Update;
#endif
        }

        public void Destroy()
        {
#if DEBUG
            if (behaviourGmo)
            {
                GameObject.Destroy(behaviourGmo);
            }
            if (renderTextureBuffer != null) { renderTextureBuffer.Dispose(); }
            renderTextureBuffer = null;
#endif
        }

#if DEBUG
        private void Update()
        {
            this.updateSampler.Begin();
            if ( SystemInfo.supportsAsyncGPUReadback) {
                renderTextureBuffer.AsyncReadbackRequestAtIdx(lastRequestIdx);
                renderTextureBuffer.UpdateAsyncRequest();
            }
            else
            {
                renderTextureBuffer.ReadBackSyncAtIdx(lastRequestIdx);
            }

            this.updateSampler.End();
        }

        private void Capture()
        {
            captureSampler.Begin();
            lastRequestIdx = renderTextureBuffer.CaptureScreen(frameIdx);
            renderTextureBuffer.UpdateAsyncRequest();
            ++frameIdx;
            captureSampler.End();
        }
#endif

    }

}