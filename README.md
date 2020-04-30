# ScreenshotToUnityProfiler

## about
Embed Screenshot to Unity Profiler protocol.<br />
![ScreenshotToUnityProfiler](Documentation~/image.gif "ScreenshotToUnityProfiler")

## requirement
- 2019.3 or newer.<br />
- support System.supportsAsyncGPUReadback platform only<br />
  (Mobile vulkan or metal....)

## how to use
1.calling this at Runtime. <br />

UTJ.SS2Profiler.ScreenShotToProfiler.Instance.Initialize(); <br />
or <br />
UTJ.SS2Profiler.ScreenShotToProfiler.Instance.Initialize(w,h);<br />
( w,h means recording texture size).


2.call "Tools -> ProfilerScreenshot" from Menu. <br />
And then window will be displayed.
