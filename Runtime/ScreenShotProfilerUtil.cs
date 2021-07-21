using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UTJ.SS2Profiler
{
    public class ScreenShotProfilerUtil
    {
        public static RenderTextureFormat GetRenderTextureFormat(ScreenShotToProfiler.TextureCompress comp)
        {
            switch (comp)
            {
                case ScreenShotToProfiler.TextureCompress.RGB_565:
                case ScreenShotToProfiler.TextureCompress.JPG:
                    return RenderTextureFormat.RGB565;
            }
            return RenderTextureFormat.ARGB32;
        }
        public static TextureFormat GetTextureFormat(ScreenShotToProfiler.TextureCompress comp)
        {
            switch (comp)
            {
                case ScreenShotToProfiler.TextureCompress.RGB_565:
                case ScreenShotToProfiler.TextureCompress.JPG:
                    return TextureFormat.RGB565;
            }
            return TextureFormat.RGBA32;
        }


    }
}
