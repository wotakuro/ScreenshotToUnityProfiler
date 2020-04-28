using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Rendering;
using Unity.Collections;

namespace UTJ.SS2Profiler
{
    public class RenderTextureBuffer :System.IDisposable{

        public static Texture2D dbg;
        public const int FRAME_NUM = 16;

        private struct DataInfo
        {
            public RenderTexture renderTexture;
            public AsyncGPUReadbackRequest request;
            public int id;
            public bool isRequest;
            public NativeArray<byte> data;
        }

        private DataInfo[] frames;

        public RenderTextureBuffer()
        {
            frames = new DataInfo[FRAME_NUM];
            for(int i = 0; i < FRAME_NUM; ++i)
            {
                frames[i].renderTexture = new RenderTexture(192, 128,0);
                frames[i].renderTexture.name = "ss2profiler_" + i;
                frames[i].data = new NativeArray<byte>(192 * 128 * 4, Allocator.Persistent);
                
                for( int j = 0;j< frames[i].data.Length; ++j)
                {
                    frames[i].data[j] = 0;
                }

                frames[i].isRequest = false;
            }
        }

        public void Dispose()
        {
            for (int i = 0; i < FRAME_NUM; ++i)
            {
                frames[i].renderTexture.Release();
                frames[i].data.Dispose();
                frames[i].isRequest = false;
            }
            frames = null;
        }

        public void Update()
        {

            for (int i = 0; i < FRAME_NUM; ++i)
            {
                if (!frames[i].isRequest)
                {
                    continue;
                }
                frames[i].request.Update();
                if (frames[i].request.done)
                {
                    dbg = new Texture2D(192, 128, TextureFormat.RGBA32, false);
                    dbg.SetPixels(frames[i].request.GetData<Color>().ToArray());
                    dbg.Apply();
                    System.IO.File.WriteAllBytes("test-" + i + ".data", frames[i].request.GetData<byte>().ToArray());
                    frames[i].isRequest = false;
                }
            }
        }

        public bool Request(int id)
        {
            for (int i = 0; i < FRAME_NUM; ++i)
            {
                if(!frames[i].isRequest)
                {
                    frames[i].id = id;
                    var texture = Resources.Load<Texture>("20191004_153112");
                    CommandBuffer cmd = new CommandBuffer();
                    cmd.SetRenderTarget(frames[i].renderTexture);
                    cmd.Blit(texture, frames[i].renderTexture);
                    Graphics.ExecuteCommandBuffer(cmd);
                    return true;
                }
            }
            return false;
        }

        public void GetFromGPU(int i)
        {
            //            frames[i].request = AsyncGPUReadback.RequestIntoNativeArray(ref frames[i].data, frames[i].renderTexture, 0);
            frames[i].request = AsyncGPUReadback.Request(frames[i].renderTexture);
            frames[i].isRequest = true;
        }

    }

    public class ScreenShotToProfiler
    {
        public static readonly Guid kMyProjectId = new Guid("4389DCEB-F9B3-4D49-940B-E98482F3A3F8");
        public static readonly int kTextureInfoTag = 0;
        public static readonly int kTextureDataTag = 1;
        
        public static ScreenShotToProfiler Instance { get; private set; } = new ScreenShotToProfiler();

        private RenderTextureBuffer renderTextureBuffer;

        public bool Initialize()
        {
            if (!SystemInfo.supportsAsyncGPUReadback)
            {
                return false;
            }
            renderTextureBuffer = new RenderTextureBuffer();
            renderTextureBuffer.Request(0);
            return true;
        }

        public void Next()
        {
            renderTextureBuffer.GetFromGPU(0);
        }

        public void Update()
        {
            renderTextureBuffer.Update(); 
        }

    }

}