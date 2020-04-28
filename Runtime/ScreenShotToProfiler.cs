using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Rendering;
using Unity.Collections;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace UTJ.SS2Profiler
{
    public class ScreenShotToProfiler
    {
        public static readonly Guid imageBinary = new Guid("4389DCEB-F9B3-4D49-940B-E98482F3A3F8");
        public static readonly Guid imageTag = new Guid("C9A4768A-8E99-6E3F-817F-BFCBF492D3C5");

        public static ScreenShotToProfiler Instance { get; private set; } = new ScreenShotToProfiler();

        private RenderTextureBuffer renderTextureBuffer;
        private int frameIdx = 0;
        private int lastRequestIdx = -1;


        public bool Initialize()
        {
            if (!SystemInfo.supportsAsyncGPUReadback)
            {
                return false;
            }
            renderTextureBuffer = new RenderTextureBuffer();
            var gmo = new GameObject();
            gmo.hideFlags = HideFlags.HideAndDontSave;
            var behaviour = gmo.AddComponent<ScreenShotBehaviour>();

            behaviour.captureFunc += this.Capture;
            behaviour.updateFunc += this.Update;
            return true;
        }

        private void Update()
        {
            
            renderTextureBuffer.AsyncReadbackRequestAtIdx(lastRequestIdx);
            renderTextureBuffer.Update();

        }




        private void Capture()
        {
            lastRequestIdx = renderTextureBuffer.CaptureScreen(frameIdx);
            renderTextureBuffer.Update();
            ++frameIdx;
        }

    }

}