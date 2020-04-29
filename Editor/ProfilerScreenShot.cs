using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Profiling;
using Unity.Collections;
using UnityEditorInternal;
using System.Reflection;

namespace UTJ.SS2Profiler
{
    public class ProfilerScreenShot : EditorWindow
    {
        private struct TagInfo
        {
            public int id;
            public int width;
            public int height;
        }


        [MenuItem("Tools/ProfilerScreenshot")]
        public static void Create()
        {
            EditorWindow.GetWindow<ProfilerScreenShot>();
        }

        private Texture2D drawTexture;
        private int lastPreviewFrameIdx;
        private bool isAutoReflesh = false;
        private bool isYFlip = false;

        private void OnEnable()
        {
            Reflesh(GetProfilerActiveFrame());
        }
        private void OnDisable()
        {
            
        }

        private void Reflesh(int frameIdx)
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
                drawTexture = GenerateTagTexture(tagInfo,frameIdx);
            }
            else
            {
                drawTexture = null;
            }
            lastPreviewFrameIdx = frameIdx;
        }

        private TagInfo GenerateTagInfo(NativeArray<byte> data)
        {
            TagInfo info = new TagInfo();
            info.id = GetIntData(data, 0);
            info.width = GetIntData(data, 1);
            info.height = GetIntData(data, 2);
            return info;
        }
        private int GetIntData(NativeArray<byte> data, int idx)
        {
            int val = (data[idx * 4 + 0] ) +
                (data[idx * 4 + 1] << 8 )+
                (data[idx * 4 + 2] << 16) +
                (data[idx * 4 + 3] << 24);
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

            if (drawTexture != null)
            {
                var rect = EditorGUILayout.GetControlRect(GUILayout.Width(drawTexture.width), GUILayout.Height(drawTexture.height));
                EditorGUI.LabelField(rect, new GUIContent(this.drawTexture));
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