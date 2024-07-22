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
        internal int sizeIndex = 0;
        internal int colorSpaceIndex = 0;

        private Material drawMaterialObj;
        internal Material drawMaterial
        {
            get
            {
                if (!this.drawMaterialObj)
                {
                    Shader shader = AssetDatabase.LoadAssetAtPath<Shader>("Packages/com.utj.screenshot2profiler/Editor/Shader/DebugColorSpace.shader");
                    this.drawMaterialObj = new Material(shader);
                }
                return drawMaterialObj;
            }
        }

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
        private Label originSize;
        private Toggle yFlipToggle;
        private DropdownField sizeField;
        private DropdownField colorSpaceField;
        private IMGUIContainer imageBody;
        private Texture2D screenshotTexture;
        private TagInfo currentTagInfo;

        private enum EOutputMode:byte
        {
            FitWindow = 0,
            Origin = 1,
        }

        private enum EColorSpaceMode : byte
        {
            NoConvert = 0,
            LinearToGamma=1,
            GammaToLinear=2,
        }


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
            var sizeChoices = new System.Collections.Generic.List<string>();
            sizeChoices.Add("FitWindow");
            sizeChoices.Add("Origin");
            var colorSpaceChoices = new System.Collections.Generic.List<string>();
            colorSpaceChoices.Add("No Convert");
            colorSpaceChoices.Add("Gamma->Linear");
            colorSpaceChoices.Add("Linear->Gamma");

            yFlipToggle = ve.Q<Toggle>("FlipYToggle");
            yFlipToggle.value = screenShotModule.yFlip;

            sizeField = ve.Q<DropdownField>("SizeMode");
            sizeField.choices = sizeChoices;
            sizeField.index = screenShotModule.sizeIndex;

            colorSpaceField = ve.Q<DropdownField>("ColorSpaceMode");
            colorSpaceField.choices = colorSpaceChoices;
            colorSpaceField.index = screenShotModule.colorSpaceIndex;

            imageBody = ve.Q<IMGUIContainer>("TextureOutIMGUI");
            imageBody.onGUIHandler += OnGUITextureOut;

            originSize = ve.Q<Label>("OriginalSize");
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
            var rect = new Rect(0, 0, currentTagInfo.width, currentTagInfo.height);

            screenShotModule.yFlip = yFlipToggle.value;
            screenShotModule.sizeIndex = sizeField.index;
            screenShotModule.colorSpaceIndex = colorSpaceField.index;

            if (this.sizeField.index == (int)EOutputMode.FitWindow)
            {
                rect = FitWindowSize(currentTagInfo,
                    new Rect(0, 0,
                    this.imageBody.contentRect.width,
                    this.imageBody.contentRect.height));
            }


            if (screenshotTexture)
            {
                var drawMaterial = screenShotModule.drawMaterial;
                var mode = (EColorSpaceMode)this.colorSpaceField.index;
                this.SetupColorSpaceKeyword(drawMaterial, mode);
                this.SetYFlip(drawMaterial, yFlip);
                if (drawMaterial)
                {
                    drawMaterial.mainTexture = screenshotTexture;
                    EditorGUI.DrawPreviewTexture(rect, screenshotTexture, drawMaterial);
                }
                else
                {
                    EditorGUI.DrawPreviewTexture(rect, screenshotTexture);
                }
            }
        }
        private Rect FitWindowSize(TagInfo tag , Rect areaRect) {
            float xparam = (areaRect.width - (areaRect.x * 2.0f) )/(float)tag.originWidth;
            float yparam = (areaRect.height - (areaRect.y * 2.0f) )/(float)tag.originHeight;

            if (xparam > yparam)
            {
                areaRect.width = tag.originWidth*yparam;
                areaRect.height = tag.originHeight*yparam;
            }
            else
            {
                areaRect.width = tag.originWidth * xparam;
                areaRect.height = tag.originHeight*xparam;
            }
            return areaRect;
        }

        private void OnSelectedFrameIndexChanged(long selectedFrameIndex) {
            int idx = (int)selectedFrameIndex;

            if (ProfilerScreenShotEditorLogic.TryGetTagInfo(idx, out currentTagInfo))
            {
                if (screenshotTexture)
                {
                    UnityEngine.Object.DestroyImmediate(screenshotTexture);
                }
                screenshotTexture = ProfilerScreenShotEditorLogic.GenerateTagTexture(currentTagInfo, idx);
                if (originSize!=null)
                {
                    originSize.text = currentTagInfo.width + "x" + currentTagInfo.height+" original:"+
                        currentTagInfo.originWidth +"x"+currentTagInfo.originHeight + "::compress " + currentTagInfo.compress.ToString();
                }
            }
        }


        private void SetYFlip(Material mat, bool flag)
        {
            const string Keyword = "FLIP_Y";
            if (mat)
            {
                if (flag)
                {
                    mat.EnableKeyword(Keyword);
                }
                else
                {
                    mat.DisableKeyword(Keyword);
                }
            }
        }

        private void SetupColorSpaceKeyword(Material mat,EColorSpaceMode mode)
        {
            if (!mat) { return; }
            switch (mode)
            {
                case EColorSpaceMode.NoConvert:
                    mat.DisableKeyword("LINEAR_TO_GAMMMA");
                    mat.DisableKeyword("GAMMA_TO_LINEAR");
                    break;
                case EColorSpaceMode.LinearToGamma:
                    mat.DisableKeyword("GAMMA_TO_LINEAR");
                    mat.EnableKeyword("LINEAR_TO_GAMMMA");
                    break;
                case EColorSpaceMode.GammaToLinear:
                    mat.DisableKeyword("LINEAR_TO_GAMMMA");
                    mat.EnableKeyword("GAMMA_TO_LINEAR");
                    break;
            }
        }


    }
}
#endif