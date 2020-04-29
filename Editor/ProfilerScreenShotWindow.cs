using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Profiling;
using Unity.Collections;
using UnityEditorInternal;
using System.Reflection;
using UnityEngine.Rendering;

namespace UTJ.SS2Profiler
{
    public class ProfilerScreenShotWindow : EditorWindow
    {
        private struct TagInfo
        {
            public int id;
            public int width;
            public int height;
            public int originWidth;
            public int originHeight;
        }


        [MenuItem("Tools/ProfilerScreenshot")]
        public static void Create()
        {
            EditorWindow.GetWindow<ProfilerScreenShotWindow>();
        }

        private FlipYTextureResolver drawTextureInfo;


        private int lastPreviewFrameIdx;
        private bool isAutoReflesh = false;
        private bool isYFlip = false;

        private Vector2Int outputSize = new Vector2Int();

        private void OnEnable()
        {
            drawTextureInfo = new FlipYTextureResolver();
            Reflesh(GetProfilerActiveFrame());
        }
        private void OnDisable()
        {
            if (drawTextureInfo != null)
            {
                drawTextureInfo.Dispose();
                drawTextureInfo = null;
            }
        }

        private void Reflesh(int frameIdx,bool force = false)
        {
            if(lastPreviewFrameIdx == frameIdx)
            {
                return;
            }
            HierarchyFrameDataView hierarchyFrameDataView =
                ProfilerDriver.GetHierarchyFrameDataView(frameIdx, 0, HierarchyFrameDataView.ViewModes.Default, 0, false); ;
            NativeArray<byte> bytes =
                hierarchyFrameDataView.GetFrameMetaData<byte>(ScreenShotToProfiler.MetadataGuid, ScreenShotToProfiler.InfoTag);
            if (bytes != null && bytes.Length >= 12)
            {
                var tagInfo = GenerateTagInfo(bytes);
                SetOutputSize(tagInfo);
                var texture = GenerateTagTexture(tagInfo,frameIdx);
                this.drawTextureInfo.SetupToRenderTexture(texture);
                Object.DestroyImmediate(texture);
            }
            else
            {
                this.drawTextureInfo.SetupToRenderTexture(null);
            }
            lastPreviewFrameIdx = frameIdx;
        }

        private void SetOutputSize(TagInfo info)
        {
            float textureAspect = info.width / (float)info.height;
            float originAspect = info.originWidth / (float)info.originHeight;

            if (originAspect < textureAspect)
            {
                this.outputSize.x = info.width;
                this.outputSize.y = info.width * info.originHeight / info.originWidth;
            }
            else
            {
                this.outputSize.x = info.height * info.originWidth / info.originHeight;
                this.outputSize.y = info.height;
            }

        }

        private TagInfo GenerateTagInfo(NativeArray<byte> data)
        {
            TagInfo info = new TagInfo();
            info.id = GetIntData(data, 0);
            info.width = GetShortData(data, 4);
            info.height = GetShortData(data, 6);
            info.originWidth = GetShortData(data, 8);
            info.originHeight = GetShortData(data, 10);
            return info;
        }
        private int GetIntData(NativeArray<byte> data, int idx)
        {
            int val = (data[idx + 0] ) +
                (data[idx + 1] << 8 )+
                (data[idx + 2] << 16) +
                (data[idx + 3] << 24);
            return val;
        }
        private int GetShortData(NativeArray<byte> data, int idx)
        {
            int val = (data[idx + 0]) +
                (data[idx + 1] << 8) ;
            return val;
        }

        private Texture2D GenerateTagTexture(TagInfo info,int idx)
        {
            Texture2D texture = null;

            for (int i = idx; i < idx + 10; ++i)
            {
                HierarchyFrameDataView hierarchyFrameDataView =
                    ProfilerDriver.GetHierarchyFrameDataView(i, 0, HierarchyFrameDataView.ViewModes.Default, 0, false);
                NativeArray<byte> bytes =
                    hierarchyFrameDataView.GetFrameMetaData<byte>(ScreenShotToProfiler.MetadataGuid, info.id);
                
                if( bytes.IsCreated && bytes.Length > 16  )
                {
                    texture = new Texture2D(info.width, info.height,TextureFormat.RGBA32,false);
                    texture.LoadRawTextureData(bytes);
                    texture.Apply();
                    break;
                }
            }
            return texture;
        }

        private void Update()
        {
            if(!this.isAutoReflesh) { return; }
            var frameIdx = this.GetProfilerActiveFrame();
            if( lastPreviewFrameIdx != frameIdx)
            {
                this.Reflesh(frameIdx);
                this.Repaint();
            }
        }

        private void OnGUI()
        {
            this.isAutoReflesh = EditorGUILayout.Toggle("AutoReflesh", this.isAutoReflesh);

            if (!this.isAutoReflesh)
            {
                if (GUILayout.Button("Reflesh",GUILayout.Width(100)))
                {
                    this.Reflesh(GetProfilerActiveFrame());
                }
            }
            EditorGUILayout.Space();

            this.isYFlip = EditorGUILayout.Toggle("Flip Y", this.isYFlip);
            EditorGUILayout.Space();
            drawTextureInfo.SetFlip(this.isYFlip);

            var drawTexture = drawTextureInfo.drawTexture;
            if (drawTexture != null)
            {
                var rect = EditorGUILayout.GetControlRect(GUILayout.Width(outputSize.x), 
                    GUILayout.Height(outputSize.y));
                EditorGUI.DrawTextureTransparent(rect, drawTexture);
            }
        }

        private int GetProfilerActiveFrame()
        {
            var window = GetProfilerWindow();
            if (window == null)
            {
                return ProfilerDriver.lastFrameIndex;
            }
            var type = window.GetType();
            var method = type.GetMethod("GetActiveVisibleFrameIndex",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            return (int)method.Invoke(window,null);
        }
        private static EditorWindow GetProfilerWindow()
        {
            EditorWindow profilerWindow = null;

            EditorWindow[] windows = Resources.FindObjectsOfTypeAll<EditorWindow>();
            if (windows != null && windows.Length > 0)
            {
                foreach (var window in windows)
                    if (window.GetType().Name == "ProfilerWindow")
                        profilerWindow = window;
            }
            return profilerWindow;

        }
    }
}