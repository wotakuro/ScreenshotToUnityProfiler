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

        private bool isInitialize = false;

        const string ArgForceOption = "--profilerSS";

        const string ArgForceResolution = "--profilerSS-resolution";
        const string ArgForceFormat = "--profilerSS-format";


        const int Invalid = -1;

        static int forceLaunchOption = Invalid;
        static int forceWidth = Invalid;
        static int forceHeight = Invalid;
        static int forceFormat = Invalid;

#endif
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnRuntimeInitialize()
        {
            var args = System.Environment.GetCommandLineArgs();
            int length = args.Length;

            for(int i = 0; i < length; ++i)
            {
                if ( string.IsNullOrEmpty(args[i] ) )
                {
                    continue;
                }
                
                if(args[i].StartsWith( ArgForceResolution) )
                {
                    var resolutionVal = GetParameter(args[i]);
                    GetResolution(resolutionVal, out forceWidth, out forceHeight);
                }
                else if (args[i].StartsWith( ArgForceFormat) )
                {
                    var formatParam = GetParameter(args[i]);
                    forceFormat = GetFormat(formatParam);
                }
                else if (args[i].StartsWith(ArgForceOption))
                {
                    var optionVal = GetParameter(args[i]);
                    if (optionVal == "enable")
                    {
                        forceLaunchOption = 1;
                    }
                    else if (optionVal == "disable")
                    {
                        forceLaunchOption = 0;
                    }
                }
            }
            if(forceLaunchOption == 1)
            {
                Instance.Initialize();
            }
        }


        private static int GetFormat(string param)
        {
            switch (param)
            {
                case "rgb_565":
                    return (int)TextureCompress.RGB_565;
                case "png":
                    return (int)TextureCompress.PNG;
                case "jpg_bufferrgb565":
                case "jpg":
                case "jpeg":
                    return (int)TextureCompress.JPG_BufferRGB565;
                case "jpg_Bufferrgba":
                    return (int)TextureCompress.JPG_BufferRGBA;
                case "none":
                    return (int)TextureCompress.None;
            }
            return Invalid;
        }

        private static void GetResolution(string param,out int width,out int height)
        {
            int paramIndex = 0;
            width = 0;
            height = 0;
            int length = param.Length;
            for(int i = 0; i < length; ++i)
            {
                if( '0' <= param[i] && param[i] <= '9')
                {
                    switch(paramIndex)
                    {
                        case 0:
                            width = width * 10 + (param[i]- '0');
                            break;
                        case 1:
                            height = height * 10 + (param[i] - '0');
                            break;
                    }
                }
                else if (param[i] == 'x' )
                {
                    paramIndex++;
                }
            }
        }

        private static string GetParameter(string arg)
        {
            int eqIndex = arg.IndexOf('=');
            if(eqIndex == -1)
            {
                return "";
            }
            return arg.Substring(eqIndex+1).ToLower();
        }

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
#if !UNITY_EDITOR
            if(forceLaunchOption == 0)
            {
                return false;
            }
            if (forceWidth != Invalid)
            {
                width = forceWidth;
            }
            if (forceHeight != Invalid)
            {
                height = forceHeight;
            }
            if(forceFormat != Invalid)
            {
                compress = (TextureCompress)forceFormat;
            }
#endif

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
            if (isInitialize)
            {
                return;
            }
            if(width ==0 || height == 0) { 
                return;
            }
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
            isInitialize = true;
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
            isInitialize = false;
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