using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Profiling;
using Unity.Collections;
using UnityEditorInternal;

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
            Reflesh(ProfilerDriver.firstFrameIndex);
        }

        private void Reflesh(int frameIdx)
        {

            HierarchyFrameDataView hierarchyFrameDataView =
                ProfilerDriver.GetHierarchyFrameDataView(frameIdx, 0, HierarchyFrameDataView.ViewModes.Default, 0, false); ;


            NativeArray<byte> bytes =
                hierarchyFrameDataView.GetFrameMetaData<byte>(ScreenShotToProfiler.imageBinary, 0);


            drawTexture = new Texture2D(192, 128, TextureFormat.RGBA32, false);
            drawTexture.LoadRawTextureData(bytes);
            drawTexture.Apply();
        }

        private void OnGUI()
        {

            var rect = new Rect(10, 10, 192, 128);
            EditorGUI.LabelField(rect, new GUIContent(this.drawTexture));
        }

        private int GetProfilerActiveFrame()
        {
            var window = GetProfilerWindow();
            if (window == null)
            {
                Reflesh(ProfilerDriver.firstFrameIndex);
            }
            return 0;
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