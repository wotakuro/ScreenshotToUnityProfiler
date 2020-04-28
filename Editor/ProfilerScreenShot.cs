using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UTJ.SS2Profiler
{
    public class ProfilerScreenShot : EditorWindow
    {
        [MenuItem("Tools/ProfilerScreenshot")]
        public static void Create()
        {
            EditorWindow.GetWindow<ProfilerScreenShot>();
        }

        private Texture2D drawTexture;

        private void OnEnable()
        {
            var bytes = System.IO.File.ReadAllBytes("test-0.data");
            var colors = new Color[bytes.Length/4];


            drawTexture = new Texture2D(192, 128,TextureFormat.ARGB32,false);
            //            drawTexture.LoadRawTextureData(bytes);
            for( int i = 0; i < colors.Length; ++i)
            {
                colors[i].b = bytes[i * 4 + 0] / 255.0f;
                colors[i].g = bytes[i * 4 + 1] / 255.0f;
                colors[i].r = bytes[i * 4 + 2] / 255.0f;
                colors[i].a = bytes[i * 4 + 3] / 255.0f;
            }

            drawTexture.SetPixelData(colors, 0);
            drawTexture.Apply();
        }

        private void OnGUI()
        {
            var rect = new Rect(10, 10, 192, 128);
            EditorGUI.DrawTextureTransparent(rect, this.drawTexture);
            if(RenderTextureBuffer.dbg != null)
            {
                rect.y += 150;
                EditorGUI.DrawTextureTransparent(rect, this.drawTexture);

            }
        }
    }
}