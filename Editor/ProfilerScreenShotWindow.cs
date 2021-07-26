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
    internal class ProfilerScreenShotWindow : EditorWindow
    {
        private struct TagInfo
        {
            public int id;
            public int width;
            public int height;
            public int originWidth;
            public int originHeight;
            public ScreenShotToProfiler.TextureCompress compress;
        }
        private enum OutputMode:int
        {
            Origin = 0,
            FitWindow = 1,
        }

        [MenuItem("Tools/ProfilerScreenshot")]
        public static void Create()
        {
            EditorWindow.GetWindow<ProfilerScreenShotWindow>();
        }

        private FlipYTextureResolver drawTextureInfo;
        private Texture originTexture;

        private int lastPreviewFrameIdx;
        private bool isAutoReflesh = true;
        private bool isYFlip = false;
        private OutputMode outputMode;

        private GUIContent[] outputModeSelect = new GUIContent[2]
        {
            new GUIContent("Original Size"),
            new GUIContent("Fit window Size"),
        };

    private Vector2Int outputSize = new Vector2Int();

        private void OnEnable()
        {
            drawTextureInfo = new FlipYTextureResolver();
            Refresh(GetProfilerActiveFrame());
        }
        private void OnDisable()
        {
            if (originTexture)
            {
                Object.DestroyImmediate(originTexture);
            }
            if (drawTextureInfo != null)
            {
                drawTextureInfo.Dispose();
                drawTextureInfo = null;
            }
        }

        private void Refresh(int frameIdx,bool force = false)
        {
            if(lastPreviewFrameIdx == frameIdx && !force)
            {
                return;
            }
            HierarchyFrameDataView hierarchyFrameDataView =
                ProfilerDriver.GetHierarchyFrameDataView(frameIdx, 0, HierarchyFrameDataView.ViewModes.Default, 0, false); ;
            if(hierarchyFrameDataView == null || !hierarchyFrameDataView.valid) { return; }
            NativeArray<byte> bytes =
                hierarchyFrameDataView.GetFrameMetaData<byte>(ScreenShotToProfiler.MetadataGuid, ScreenShotToProfiler.InfoTag);
            if (bytes != null && bytes.Length >= 12)
            {
                var tagInfo = GenerateTagInfo(bytes);
                SetOutputSize(tagInfo);
                if( originTexture)
                {
                    Object.DestroyImmediate(originTexture);
                }
                originTexture = GenerateTagTexture(tagInfo,frameIdx);
                //this.drawTextureInfo.SetupToRenderTexture(originTexture);
            }
            else
            {
                //this.drawTextureInfo.SetupToRenderTexture(null);

                if (originTexture)
                {
                    Object.DestroyImmediate(originTexture);
                }
                originTexture = null;
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
                if( hierarchyFrameDataView == null || !hierarchyFrameDataView.valid)
                {
                    continue;
                }
                NativeArray<byte> bytes =
                    hierarchyFrameDataView.GetFrameMetaData<byte>(ScreenShotToProfiler.MetadataGuid, info.id);

                if (bytes.IsCreated && bytes.Length > 16)
                {
                    texture = new Texture2D(info.width, info.height, ScreenShotProfilerUtil.GetTextureFormat(info.compress), false);
                    switch (info.compress) {
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


        private void Update()
        {
            if(!this.isAutoReflesh) { return; }
            var frameIdx = this.GetProfilerActiveFrame();
            if( lastPreviewFrameIdx != frameIdx)
            {
                this.Refresh(frameIdx);
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
                    this.Refresh(GetProfilerActiveFrame(),true);
                }
            }
            EditorGUILayout.BeginHorizontal();
            this.isYFlip = EditorGUILayout.Toggle("Flip Y", this.isYFlip);
            EditorGUILayout.LabelField("Size",GUILayout.Width(40));
            outputMode = (OutputMode)EditorGUILayout.Popup((int)outputMode, outputModeSelect);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            //drawTextureInfo.SetFlip(this.isYFlip);

            var drawTexture = originTexture; //drawTextureInfo.drawTexture;

            if (drawTexture != null)
            {
                var rect = EditorGUILayout.GetControlRect(GUILayout.Width(outputSize.x), 
                    GUILayout.Height(outputSize.y));
                if (outputMode == OutputMode.FitWindow)
                {
                    rect = FitWindow(rect);
                }

                if (this.isYFlip)
                {
                    rect.y += rect.height;
                    rect.height = -rect.height;
                }
                EditorGUI.DrawTextureTransparent(rect, drawTexture);
            }
        }
        private Rect FitWindow(Rect r)
        {
            if( r.width == 0 || r.height == 0) { return r; }
            var windowPos = this.position;
            float xparam = (position.width - r.x * 2) / r.width;
            float yparam = (position.height - (r.y+5) ) / r.height;

            if( xparam > yparam)
            {
                r.width *= yparam;
                r.height *= yparam;
            }
            else
            {
                r.width *= xparam;
                r.height *= xparam;
            }


            return r;
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