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
        internal bool yFlip = true;

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
            return new ScreenShotModuleDetailsViewController(this,this.ProfilerWindow);
        }

    }

    // 

    public class ScreenShotModuleDetailsViewController : ProfilerModuleViewController
    {
        private ProfilerWindowScreenShotModule screenShotModule;
        private Toggle yFlipToggle;
        private DropdownField sizeField;
        private IMGUIContainer imageBody;
        private Texture2D screenshotTexture;
        private TagInfo currentTagInfo;


        public ScreenShotModuleDetailsViewController(
            ProfilerWindowScreenShotModule module,
            ProfilerWindow profilerWindow) : base(profilerWindow) 
        {
            this.screenShotModule = module;
            this.ProfilerWindow.SelectedFrameIndexChanged += OnSelectedFrameIndexChanged;
        }

        protected override VisualElement CreateView()
        {
            string path = "Packages/com.utj.screenshot2profiler/Editor/UXML/ScreenshotProfilerModule.uxml";
            var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);
            var element = tree.CloneTree();
            InitVisualElement(element);
            this.OnSelectedFrameIndexChanged(ProfilerWindow.selectedFrameIndex);

            element.style.height = new StyleLength(Length.Percent(100.0f));
            return element;
        }

        private void InitVisualElement(VisualElement ve)
        {
            var choices = new System.Collections.Generic.List<string>();
            choices.Add("Origin");
            choices.Add("FitWindow");

            yFlipToggle = ve.Q<Toggle>("FlipYToggle");
            yFlipToggle.value = screenShotModule.yFlip;

            sizeField = ve.Q<DropdownField>("SizeMode");
            sizeField.choices = choices;
            sizeField.index = 0;

            imageBody = ve.Q<IMGUIContainer>("TextureOutIMGUI");
            imageBody.onGUIHandler += OnGUITextureOut;
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposing)
                return;

            ProfilerWindow.SelectedFrameIndexChanged -= OnSelectedFrameIndexChanged;
            base.Dispose(disposing);
        }

        private void OnGUITextureOut()
        {
            bool yFlip = this.yFlipToggle.value;
            var rect = new Rect(10, 10, currentTagInfo.width, currentTagInfo.height);

            screenShotModule.yFlip = yFlipToggle.value;
            
            if (yFlip)
            {
                rect.y += rect.height;
                rect.height = -rect.height;
            }
            if (screenshotTexture)
            {
                EditorGUI.DrawTextureTransparent(rect, screenshotTexture);
            }        
        }

        void OnSelectedFrameIndexChanged(long selectedFrameIndex) {
            int idx = (int)selectedFrameIndex;

            if (ProfilerScreenShotEditorLogic.TryGetTagInfo(idx, out currentTagInfo))
            {
                if (screenshotTexture)
                {
                    UnityEngine.Object.DestroyImmediate(screenshotTexture);
                }
                screenshotTexture = ProfilerScreenShotEditorLogic.GenerateTagTexture(currentTagInfo, idx);
            }

        }


    }
}
#endif