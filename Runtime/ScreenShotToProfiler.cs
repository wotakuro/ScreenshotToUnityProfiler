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

        public enum TextureCompress:byte
        {
            None = 0,
            RGB_565 = 1,
            PNG = 2,
            JPG_BufferRGB565 = 3,
            JPG_BufferRGBA = 4,
        }
        public Action<RenderTexture> captureBehaviour
        {
            set
            {
#if DEBUG
                this.renderTextureBuffer.captureBehaviour = value;
#endif
            }
        }

#if DEBUG
        private const string CAPTURE_CMD_SAMPLE = "ScreenToRt";

        private CommandBuffer commandBuffer;

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
                return Initialize(192,128,true);
            }
            else
            {
                return Initialize(128, 192,true);
            }
        }

        public bool Initialize(int width , int height,bool allowSync = false)
        {
#if DEBUG
            Initialize(width, height, TextureCompress.RGB_565, allowSync);
#endif
            return true;
        }

        public bool Initialize(int width, int height, TextureCompress compress, bool allowSync)
        {
#if DEBUG
            if (!SystemInfo.supportsAsyncGPUReadback)
            {
                if (!allowSync)
                {
                    return false;
                }
                else
                {
                    UnityEngine.Debug.LogWarning("SystemInfo.supportsAsyncGPUReadback is false! Profiler Screenshot is very slow...");
                }
                compress = ScreenShotProfilerUtil.FallbackAtNoGPUAsync(compress);
            }
            if (renderTextureBuffer != null) { return false; }
            InitializeLogic(width, height, compress);
#endif
            return true;
        }


        private void InitializeLogic(int width,int height,TextureCompress compress)
        {
#if DEBUG
            renderTextureBuffer = new ScreenShotLogic(width, height, compress);
            renderTextureBuffer.captureBehaviour = this.DefaultCaptureBehaviour;
            this.behaviourGmo = new GameObject();
            this.behaviourGmo.hideFlags = HideFlags.HideAndDontSave;
            GameObject.DontDestroyOnLoad(behaviourGmo);
            var behaviour = this.behaviourGmo.AddComponent<BehaviourProxy>();

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

        public void DefaultCaptureBehaviour(RenderTexture target)
        {
#if DEBUG
            if (commandBuffer == null)
            {
                commandBuffer = new CommandBuffer();
                commandBuffer.name = "ScreenCapture";
            }
            commandBuffer.Clear();
            var rt = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
            ScreenCapture.CaptureScreenshotIntoRenderTexture(rt);
            commandBuffer.BeginSample(CAPTURE_CMD_SAMPLE);
            commandBuffer.Blit(rt,  target);
            commandBuffer.EndSample(CAPTURE_CMD_SAMPLE);
            Graphics.ExecuteCommandBuffer(commandBuffer);
            RenderTexture.ReleaseTemporary(rt);
            commandBuffer.Clear();
#endif
        }

    }

}