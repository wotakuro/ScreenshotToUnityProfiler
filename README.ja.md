# ScreenshotToUnityProfiler

[![openupm](https://img.shields.io/npm/v/com.utj.screenshot2profiler?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.utj.screenshot2profiler/)

## このツールについて
Unity Profiler上に、Screenshotを載せることが出来ます.<br />
![ScreenshotToUnityProfiler](Documentation~/image.gif "ScreenshotToUnityProfiler")

## 必要な環境
- Unity 2019.3以上.<br />

## 推奨環境
- System.supportsAsyncGPUReadbackをサポートしている環境 (モバイルでは vulkan または metal。※2021.2以降であればGLES3でも可)<br />
 supportsAsyncGPUReadbackが未対応でも動作はしますが、非常に思いため推奨できません。<br />

## インストールについて

[OpenUPM](https://openupm.com/packages/com.utj.screenshot2profiler/) (requires [openupm-cli](https://github.com/openupm/openupm-cli#openupm-cli)) 経由について

```
openupm add com.utj.screenshot2profiler
```

## 利用方法
1.初期化方法について. <br />
"ScreenShotProfiler.prefab" をシーン上に配置してください.<br />
![ScreenshotToUnityProfiler](Documentation~/ScreenShotPrefab.png "Place Prefab")<br />
(Inspector上で設定変更が可能です。)<br />

もしくは下記の初期化メソッドを呼び出してください
UTJ.SS2Profiler.ScreenShotToProfiler.Instance.Initialize(); <br />
もしくは <br />
UTJ.SS2Profiler.ScreenShotToProfiler.Instance.Initialize(w,h);<br />
( w,h はレコードするテクスチャサイズです).

2."Tools -> ProfilerScreenshot" をメニューから呼び出してください。 <br />
スクリーンショットが表示されるウィンドウが現れます

### スクリーンキャプチャの代わりに任意の画像を表示する方法
下記はRenderTextureをScreenShotに載せるサンプルです
```
RenderTexture captureRenderTexture;

ScreenShotToProfiler.Instance.captureBehaviour = (target) => {
    CommandBuffer commandBuffer = new CommandBuffer();
    commandBuffer.name = "ScreenCapture";
    commandBuffer.Blit(captureRenderTexture, target);
    Graphics.ExecuteCommandBuffer(commandBuffer);
};
```
[サンプルC#コード](Sample~/SwitchSample.cs)<br />

## 変更履歴
<pre>
version 1.2.0
  Profilerデータがないときに、NullReference　Exceptionが出る問題を修正
  テクスチャ圧縮の追加
   Screenshotではなく任意の画像を載せるInterfaceの追加
  "ScreenshotToUnityProfiler.prefab"の追加

version 1.1.0
  supportsAsyncGPUReadbackに対応していないケースへの対応
</pre>
