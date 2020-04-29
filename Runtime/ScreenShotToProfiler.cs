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
        public static readonly Guid MetadataGuid = new Guid("4389DCEB-F9B3-4D49-940B-E98482F3A3F8");
        public static readonly int InfoTag = -1;

        public static ScreenShotToProfiler Instance { get; private set; } = new ScreenShotToProfiler();

        private ScreenShotLogic renderTextureBuffer;
        private GameObject behaviourGmo;
        private int frameIdx = 0;
        private int lastRequestIdx = -1;


        public bool Initialize(int width , int height)
        {
            if (!SystemInfo.supportsAsyncGPUReadback)
            {
                return false;
            }
            renderTextureBuffer = new ScreenShotLogic(width , height);
            var behaviourGmo = new GameObject();
            behaviourGmo.hideFlags = HideFlags.HideAndDontSave;
            GameObject.DontDestroyOnLoad(behaviourGmo);
            var behaviour = behaviourGmo.AddComponent<ScreenShotBehaviour>();

            behaviour.captureFunc += this.Capture;
            behaviour.updateFunc += this.Update;
            return true;
        }

        public void Destroy()
        {
            GameObject.Destroy(behaviourGmo);
            if (renderTextureBuffer != null) { renderTextureBuffer.Dispose(); }
            renderTextureBuffer = null;
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