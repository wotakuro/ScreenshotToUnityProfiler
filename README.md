# ScreenshotToUnityProfiler

[![openupm](https://img.shields.io/npm/v/com.utj.screenshot2profiler?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.utj.screenshot2profiler/)
<br />
[日本語はコチラ](README.ja.md)<br />

## about
Embed Screenshot to Unity Profiler protocol.<br />
![ScreenshotToUnityProfiler](Documentation~/image.gif "ScreenshotToUnityProfiler")

## requirement
- 2019.4 or newer.<br />

## reccomend
- the platforms that supports System.supportsAsyncGPUReadback (Mobile vulkan or metal....)<br />
 Support sync readback from 1.1.0 , however it's very slow....<br />

## install

via [OpenUPM](https://openupm.com/packages/com.utj.screenshot2profiler/) (requires [openupm-cli](https://github.com/openupm/openupm-cli#openupm-cli)).

```
openupm add com.utj.screenshot2profiler
```

## how to use
1.Initialize. <br />
Place the "ScreenShotProfiler.prefab" to the scene.<br />
![ScreenshotToUnityProfiler](Documentation~/ScreenShotPrefab.png "Place Prefab")<br />
(You can customize the settings at Inspector.)<br />

or callling this method to Initialize.
UTJ.SS2Profiler.ScreenShotToProfiler.Instance.Initialize(); <br />
or <br />
UTJ.SS2Profiler.ScreenShotToProfiler.Instance.Initialize(w,h);<br />
( w,h means recording texture size).

2.call "Tools -> ProfilerScreenshot" from Menu. <br />
And then window will be displayed.

### Capture specific image instead of ScreenCapture
This is a sample that use RenderTexture instead of ScreenCapture.
```
RenderTexture captureRenderTexture;

ScreenShotToProfiler.Instance.captureBehaviour = (target) => {
    CommandBuffer commandBuffer = new CommandBuffer();
    commandBuffer.name = "ScreenCapture";
    commandBuffer.Blit(captureRenderTexture, target);
    Graphics.ExecuteCommandBuffer(commandBuffer);
};
```
[Whole Sample Code](Sample~/SwitchSample.cs)<br />


## command line options
By adding command line options at runtime, you can forcefully change the behavior. (Disabled when running Editor)

### Enable/disable with "--profilerSS"
By adding the option "--profilerSS=enable", you can force the screenshot to be taken immediately after startup. <br />
Screenshot can be forcibly disabled by adding the option "--profilerSS=disable"

### Change resolution with "--profilerSS-resolution"
You can now set the "width x height" of Texture, like "--profilerSS-resolution=256x192".

### Format change with "--profilerSS-format"
Texture can be compressed into Jpg by using "--profilerSS-format=JPG_BUFFERRGB565".

Option value list
- "NONE" → RGBA 32bit uncompressed setting
- "RGB_565" → RGB565(16bit) uncompressed setting
- "PNG" → RGBA 32Bit PNG compression setting
- "JPG_BUFFERRGBA" → RGBA 32bit JPEG compression setting
- "JPG" / "JPG_BUFFERRGB565" → RGB565 JPEG compression settings

###How to pass arguments
Windows::Start from command line<br />
    Example: Sample.exe --profilerSS=enable --profilerSS-resolution=640x480 --profilerSS-format=jpg <br />
Android :: Start in Adb shell<br />
    Example: adb shell am start -n com.utj.test[package name]/com.unity3d.player.UnityPlayerActivity[activity name] -e "unity --profilerSS=disable"<br />

## change
<pre>
version 1.5.0
  Added command line option
  Display resolution

version 1.4.0
  Add ColorSpace convert.

version 1.3.0
  Add custom ProfilerModule for Unity 2021.2 or lator.

version 1.2.1
  fix release build error 

version 1.2.0
  fix NullError when there is no data in Profiler.
  add Texture compress option.
  add RenderTexture capture instead of Screenshot.
  add "ScreenshotToUnityProfiler.prefab"

version 1.1.0
  add sync readback option for gles.
</pre>
