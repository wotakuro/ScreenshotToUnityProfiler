using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Profiling;

#if DEBUG
namespace UTJ.SS2Profiler
{

    internal class ScreenShotLogic : System.IDisposable
    {

        private const int FRAME_NUM = 8;
        private const string CAPTURE_CMD_SAMPLE = "ScreenToRt";

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
        private byte[] tagInfo;
        private CommandBuffer commandBuffer;

        public ScreenShotLogic(int width , int height)
        {
            frames = new DataInfo[FRAME_NUM];
            for (int i = 0; i < FRAME_NUM; ++i)
            {
                frames[i].renderTexture = new RenderTexture(width, height, 0);
                frames[i].renderTexture.name = "ss2profiler_" + i;
                frames[i].isRequest = false;
                frames[i].fromEnd = 5;
            }
            this.tagInfo = new byte[12];
            this.WriteToTagInfoShort(width, 4);
            this.WriteToTagInfoShort(height, 6);
        }

        private void WriteToTagInfo(int val,int idx)
        {
            tagInfo[idx + 0] = (byte)((val >> 0 )& 0xff);
            tagInfo[idx + 1] = (byte)((val >> 8 ) & 0xff);
            tagInfo[idx + 2] = (byte)((val >> 16) & 0xff);
            tagInfo[idx + 3] = (byte)((val >> 24) & 0xff);
        }
        private void WriteToTagInfoShort(int val, int idx)
        {
            tagInfo[idx  + 0] = (byte)((val >> 0) & 0xff);
            tagInfo[idx  + 1] = (byte)((val >> 8) & 0xff);
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

                    Profiler.EmitFrameMetaData(ScreenShotToProfiler.MetadataGuid, 
                        frames[idx].id , req.request.GetData<byte>());
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
            if(commandBuffer == null) { 
                commandBuffer = new CommandBuffer();
                commandBuffer.name = "ScreenCapture";
            }
            commandBuffer.Clear();

            for (int i = 0; i < FRAME_NUM; ++i)
            {
                if (!IsAvailable(i))
                {
                    continue;
                }
                frames[i].id = id;
                this.WriteToTagInfo(id, 0);
                this.WriteToTagInfoShort(Screen.width, 8);
                this.WriteToTagInfoShort(Screen.height, 10);
                Profiler.EmitFrameMetaData(ScreenShotToProfiler.MetadataGuid, ScreenShotToProfiler.InfoTag, tagInfo);
                var rt = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);                
                ScreenCapture.CaptureScreenshotIntoRenderTexture(rt);
                commandBuffer.BeginSample(CAPTURE_CMD_SAMPLE);
                commandBuffer.Blit(rt, frames[i].renderTexture);
                commandBuffer.EndSample(CAPTURE_CMD_SAMPLE);
                Graphics.ExecuteCommandBuffer(commandBuffer);
                RenderTexture.ReleaseTemporary(rt);
                commandBuffer.Clear();
                return i;
            }
            return -1;
        }


    }


}

#endif