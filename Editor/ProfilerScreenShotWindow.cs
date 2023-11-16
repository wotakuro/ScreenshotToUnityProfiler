using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Profiling;
using Unity.Collections;
using UnityEditorInternal;
using System.Reflection;
using UnityEngine.Rendering;

namespace UTJ.SS2Profiler.Editor
{
    internal class ProfilerScreenShotWindow : EditorWindow
    {
        private enum OutputMode:int
        {
            Origin = 0,
            FitWindow = 1,
        }
        private enum ColorSpaceMode : int
        {
            NoConvert = 0,
            LinearToGamma = 1,
            GammaToLinear = 2,
        }

        [MenuItem("Tools/ProfilerScreenshot")]
        public static void Create()
        {
            EditorWindow.GetWindow<ProfilerScreenShotWindow>();
        }

        private Texture originTexture;

        private int lastPreviewFrameIdx;
        private bool isAutoReflesh = true;
        private bool isYFlip = false;
        private OutputMode outputMode;
        private ColorSpaceMode colorSpaceMode;

        private Material drawMaterial;

        private GUIContent[] outputModeSelect = new GUIContent[2]
        {
            new GUIContent("Original Size"),
            new GUIContent("Fit window Size"),
        };

        private GUIContent[] colorSpaceModeSelect = new GUIContent[3]
        {
            new GUIContent("No Convert"),
            new GUIContent("Gamma->Linear"),
            new GUIContent("Linear->Gamma")
        };

        private Vector2Int outputSize = new Vector2Int();

        private void OnEnable()
        {
            Shader shader = AssetDatabase.LoadAssetAtPath<Shader>("Packages/com.utj.screenshot2profiler/Editor/Shader/DebugColorSpace.shader");
            this.drawMaterial = new Material(shader);
            Refresh(GetProfilerActiveFrame());
        }
        private void OnDisable()
        {
            if (originTexture)
            {
                Object.DestroyImmediate(originTexture);
            }
        }

        private void Refresh(int frameIdx,bool force = false)
        {
            if(lastPreviewFrameIdx == frameIdx && !force)
            {
                return;
            }
            TagInfo tagInfo;

            if (ProfilerScreenShotEditorLogic.TryGetTagInfo(frameIdx, out tagInfo))
            {
                SetOutputSize(tagInfo);
                if( originTexture)
                {
                    Object.DestroyImmediate(originTexture);
                }
                originTexture = ProfilerScreenShotEditorLogic.GenerateTagTexture(tagInfo,frameIdx);
            }
            else
            {
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
            colorSpaceMode = (ColorSpaceMode)EditorGUILayout.Popup((int)colorSpaceMode, colorSpaceModeSelect);
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
                this.SetupMaterialKeyword();
                EditorGUI.DrawPreviewTexture(rect, drawTexture,this.drawMaterial);
            }
        }
        private void SetupMaterialKeyword()
        {
            const string FlipYKeyword = "FLIP_Y";
            if (isYFlip)
            {
                this.drawMaterial.EnableKeyword(FlipYKeyword);
            }
            else
            {
                this.drawMaterial.DisableKeyword(FlipYKeyword);
            }
            switch (this.colorSpaceMode)
            {
                case ColorSpaceMode.NoConvert:
                    this.drawMaterial.DisableKeyword("LINEAR_TO_GAMMMA");
                    this.drawMaterial.DisableKeyword("GAMMA_TO_LINEAR");
                    break;
                case ColorSpaceMode.LinearToGamma:
                    this.drawMaterial.DisableKeyword("GAMMA_TO_LINEAR");
                    this.drawMaterial.EnableKeyword("LINEAR_TO_GAMMMA");
                    break;
                case ColorSpaceMode.GammaToLinear:
                    this.drawMaterial.DisableKeyword("LINEAR_TO_GAMMMA");
                    this.drawMaterial.EnableKeyword("GAMMA_TO_LINEAR");
                    break;
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
#if UNITY_2021_2_OR_NEWER

        private int GetProfilerActiveFrame()
        {
            var window = GetProfilerWindow();
            if (window == null) { return -1; }
            return (int)window.selectedFrameIndex;

        }
        private static ProfilerWindow GetProfilerWindow() {
            ProfilerWindow[] windows = Resources.FindObjectsOfTypeAll<ProfilerWindow>();
            if( windows.Length > 0)
            {
                return windows[0];
            }
            return null;
        }
#else
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
#endif
    }
}