# ScreenshotToUnityProfiler

[![openupm](https://img.shields.io/npm/v/com.utj.screenshot2profiler?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.utj.screenshot2profiler/)

## about
Embed Screenshot to Unity Profiler protocol.<br />
![ScreenshotToUnityProfiler](Documentation~/image.gif "ScreenshotToUnityProfiler")

## requirement
- 2019.3 or newer.<br />

## reccomend
- the platforms that supports System.supportsAsyncGPUReadback (Mobile vulkan or metal....)<br />
 Support sync readback from 1.1.0 , however it's very slow....

## install

via [OpenUPM](https://openupm.com/packages/com.utj.screenshot2profiler/) (requires [openupm-cli](https://github.com/openupm/openupm-cli#openupm-cli)).

```
openupm add com.utj.screenshot2profiler
```

## how to use
1.calling this at Runtime. <br />

UTJ.SS2Profiler.ScreenShotToProfiler.Instance.Initialize(); <br />
or <br />
UTJ.SS2Profiler.ScreenShotToProfiler.Instance.Initialize(w,h);<br />
( w,h means recording texture size).

2.call "Tools -> ProfilerScreenshot" from Menu. <br />
And then window will be displayed.


## change
<pre>
version 1.1.0 -> add sync readback option for gles.