using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;
using UnityEngine.Profiling;

namespace UTJ.SS2Profiler
{
    public class RenderTextureBuffer : System.IDisposable
    {

        public const int FRAME_NUM = 8;

        private struct DataInfo
        {
            public RenderTexture renderTexture;
            public int id;
            public bool isRequest;
            public int fromEnd;
        }
        private struct RequestInfo
        {
            public AsyncGPUReadbackRequest request;
            public int idx;
        }

        private Queue<RequestInfo> requests = new Queue<RequestInfo>();

        private DataInfo[] frames;

        public RenderTextureBuffer()
        {
            frames = new DataInfo[FRAME_NUM];
            for (int i = 0; i < FRAME_NUM; ++i)
            {
                frames[i].renderTexture = new RenderTexture(192, 128, 0);
                frames[i].renderTexture.name = "ss2profiler_" + i;
                frames[i].isRequest = false;
                frames[i].fromEnd = 5;
            }
        }

        public void Dispose()
        {
            for (int i = 0; i < FRAME_NUM; ++i)
            {
                frames[i].renderTexture.Release();
                frames[i].isRequest = false;
            }
            frames = null;
        }

        public void Update()
        {
            for( int i = 0; i < FRAME_NUM; ++i)
            {
                if (!frames[i].isRequest)
                {
                    frames[i].fromEnd++;
                }
            }
            while (requests.Count > 0)
            {
                var req = requests.Peek();
                int idx = req.idx;
                if (req.request.hasError)
                {
                    Debug.LogError("GPU readback error detected.");
                    //[req.idx]   
                    frames[idx].isRequest = false;
                    frames[idx].fromEnd = 0;

                    requests.Dequeue();                    
                }
                else if (req.request.done)
                {

                    Profiler.EmitFrameMetaData(ScreenShotToProfiler.imageBinary, 
                        0 /* frames[idx].id */, req.request.GetData<byte>());
                    frames[idx].isRequest = false;
                    frames[idx].fromEnd = 0;

                    requests.Dequeue();
                }
                else
                {
                    break;
                }
            }

        }

        public void AsyncReadbackRequestAtIdx(int idx)
        {
            if ( idx < 0 || idx >= FRAME_NUM)
            {
                return;
            }
            if (!IsAvailable(idx) ) { return; }
            Debug.Log("AsyncReadbackRequestAtIdx " + idx);

            //            var request = AsyncGPUReadback.RequestIntoNativeArray(ref frames[idx].data, frames[idx].renderTexture, 0);
            var request = AsyncGPUReadback.Request(frames[idx].renderTexture);
            requests.Enqueue( new RequestInfo { request = request, idx = idx });

            frames[idx].isRequest = true;
            frames[idx].fromEnd = 0;
        }

        private bool IsAvailable(int idx)
        {
            return ((!frames[idx].isRequest) && frames[idx].fromEnd >=2 );
        }

        public int CaptureScreen(int id)
        {
            for (int i = 0; i < FRAME_NUM; ++i)
            {
                if (!IsAvailable(i))
                {
                    continue;
                }
                frames[i].id = id;
                Profiler.EmitFrameMetaData(ScreenShotToProfiler.imageBinary, frames[i].id, new byte[4]);
                var rt = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);                
                ScreenCapture.CaptureScreenshotIntoRenderTexture(rt);
                CommandBuffer cmd = new CommandBuffer();
                cmd.Blit(rt, frames[i].renderTexture);
                Graphics.ExecuteCommandBuffer(cmd);
                RenderTexture.ReleaseTemporary(rt);
                return i;
            }
            return -1;
        }


    }


}