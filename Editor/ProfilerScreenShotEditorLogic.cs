using UnityEngine;
using Unity.Collections;
using UnityEditorInternal;
using UnityEditor.Profiling;

namespace UTJ.SS2Profiler.Editor
{
    internal struct TagInfo
    {
        public int id;
        public int width;
        public int height;
        public int originWidth;
        public int originHeight;
        public ScreenShotToProfiler.TextureCompress compress;
    }
    internal class ProfilerScreenShotEditorLogic
    {
        public static bool TryGetTagInfo(int frameIdx, out TagInfo tagInfo)
        {
            HierarchyFrameDataView hierarchyFrameDataView =
            ProfilerDriver.GetHierarchyFrameDataView(frameIdx, 0, HierarchyFrameDataView.ViewModes.Default, 0, false); ;
            if (hierarchyFrameDataView == null || !hierarchyFrameDataView.valid)
            {
                tagInfo = default(TagInfo);
                return false;
            }
            NativeArray<byte> bytes =
                hierarchyFrameDataView.GetFrameMetaData<byte>(ScreenShotToProfiler.MetadataGuid, ScreenShotToProfiler.InfoTag);
            if (bytes != null && bytes.Length >= 12)
            {
                tagInfo = GenerateTagInfo(bytes);
                return true;
            }
            tagInfo = default(TagInfo);
            return false;
        }
        public static Texture2D GenerateTagTexture(TagInfo info, int idx,int tryFrame = 10)
        {
            Texture2D texture = null;

            for (int i = idx; i < idx + tryFrame; ++i)
            {
                HierarchyFrameDataView hierarchyFrameDataView =
                    ProfilerDriver.GetHierarchyFrameDataView(i, 0, HierarchyFrameDataView.ViewModes.Default, 0, false);
                if (hierarchyFrameDataView == null || !hierarchyFrameDataView.valid)
                {
                    continue;
                }
                NativeArray<byte> bytes =
                    hierarchyFrameDataView.GetFrameMetaData<byte>(ScreenShotToProfiler.MetadataGuid, info.id);

                if (bytes.IsCreated && bytes.Length > 16)
                {
                    texture = new Texture2D(info.width, info.height, ScreenShotProfilerUtil.GetTextureFormat(info.compress), false);
                    switch (info.compress)
                    {
                        case ScreenShotToProfiler.TextureCompress.None:
                        case ScreenShotToProfiler.TextureCompress.RGB_565:
                            texture.LoadRawTextureData(bytes);
                            texture.Apply();
                            break;
                        case ScreenShotToProfiler.TextureCompress.PNG:
                        case ScreenShotToProfiler.TextureCompress.JPG_BufferRGB565:
                        case ScreenShotToProfiler.TextureCompress.JPG_BufferRGBA:
                            texture.LoadImage(bytes.ToArray());
                            texture.Apply();
                            break;
                    }
                    break;
                }
            }
            return texture;
        }

        private static TagInfo GenerateTagInfo(NativeArray<byte> data)
        {
            TagInfo info = new TagInfo();
            info.id = GetIntData(data, 0);
            info.width = GetShortData(data, 4);
            info.height = GetShortData(data, 6);
            info.originWidth = GetShortData(data, 8);
            info.originHeight = GetShortData(data, 10);
            if (data.Length > 12)
            {
                info.compress = (ScreenShotToProfiler.TextureCompress)data[12];
            }
            else
            {
                info.compress = ScreenShotToProfiler.TextureCompress.None;
            }
            return info;
        }
        private static int GetIntData(NativeArray<byte> data, int idx)
        {
            int val = (data[idx + 0]) +
                (data[idx + 1] << 8) +
                (data[idx + 2] << 16) +
                (data[idx + 3] << 24);
            return val;
        }
        private static int GetShortData(NativeArray<byte> data, int idx)
        {
            int val = (data[idx + 0]) +
                (data[idx + 1] << 8);
            return val;
        }


    }
}
