#if UNITY_2021_2_OR_NEWER
using System;
using Unity.Profiling;
using Unity.Profiling.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace UTJ.SS2Profiler.Editor
{
    [Serializable]
    [ProfilerModuleMetadata("ScreenShot")]
    public class ProfilerWindowScreenShotModule : ProfilerModule
    {

        static readonly ProfilerCounterDescriptor[] k_ChartCounters = new ProfilerCounterDescriptor[]
        {
           new ProfilerCounterDescriptor("DummyChart", ProfilerCategory.Scripts),
        };

        public ProfilerWindowScreenShotModule() : base(k_ChartCounters,
            ProfilerModuleChartType.Line)
        {
        }
        public override ProfilerModuleViewController CreateDetailsViewController()
        {
            return new ScreenShotModuleDetailsViewController(this.ProfilerWindow);
        }

    }

    // 

    public class ScreenShotModuleDetailsViewController : ProfilerModuleViewController
    {
        public ScreenShotModuleDetailsViewController(ProfilerWindow profilerWindow) : base(profilerWindow) {
            this.ProfilerWindow.SelectedFrameIndexChanged += OnSelectedFrameIndexChanged;
        }
        private Toggle yFlipToggle;
        private DropdownField sizeField;
        private VisualElement imageBody;
        private Texture2D screenshotTexture;

        protected override VisualElement CreateView()
        {
            string path = "Packages/com.utj.screenshot2profiler/Editor/UXML/ScreenshotProfilerModule.uxml";
            var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);
            var element = tree.CloneTree();
            InitVisualElement(element);
            return element;
        }

        private void InitVisualElement(VisualElement ve)
        {
            yFlipToggle = ve.Q<Toggle>("FlipYToggle");
            sizeField = ve.Q<DropdownField>("SizeMode");
            imageBody = ve.Q<VisualElement>("ImageBody");
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposing)
                return;

            ProfilerWindow.SelectedFrameIndexChanged -= OnSelectedFrameIndexChanged;
            base.Dispose(disposing);
        }

        void OnSelectedFrameIndexChanged(long selectedFrameIndex) {
            Debug.Log("OnSelectedFrameChanged " + (imageBody == null));
            if (imageBody == null)
            {
                return;
            }
            int idx = (int)selectedFrameIndex;
            TagInfo tagInfo;

            if (ProfilerScreenShotEditorLogic.TryGetTagInfo(idx, out tagInfo))
            {
                if (screenshotTexture)
                {
                    UnityEngine.Object.DestroyImmediate(screenshotTexture);
                }
                screenshotTexture = ProfilerScreenShotEditorLogic.GenerateTagTexture(tagInfo, idx);
                imageBody.style.backgroundImage = this.screenshotTexture;
                imageBody.style.width = tagInfo.width*3;
                imageBody.style.height = tagInfo.height*3;
            }
            else
            {
                imageBody.style.backgroundImage = null;
            }

        }


    }
}
#endif