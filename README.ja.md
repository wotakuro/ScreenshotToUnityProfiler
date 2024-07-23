# ScreenshotToUnityProfiler

[![openupm](https://img.shields.io/npm/v/com.utj.screenshot2profiler?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.utj.screenshot2profiler/)

## このツールについて
Unity Profiler上に、Screenshotを載せることが出来ます.<br />
![ScreenshotToUnityProfiler](Documentation~/image.gif "ScreenshotToUnityProfiler")

## 必要な環境
- Unity 2019.4以上.<br />

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

## コマンドラインオプション
実行時にコマンドラインオプションを付けることで、強制的に挙動を変更する事が可能です。(Editor実行時は無効)

### "--profilerSS"による有効・無効
"--profilerSS=enable"という形でオプションを付ける事で、Screenshotを起動直後から強制的に取るようにします。<br />
"--profilerSS=disable"という形でオプションを付ける事で、Screenshotを強制的に無効化できます

### "--profilerSS-resolution"での解像度変更
"--profilerSS-resolution=256x192"のように、Textureの"幅x高さ"を措定できるようになります。<br />

### "--profilerSS-format"でのフォーマット変更 
"--profilerSS-format=JPG_BUFFERRGB565"のようにすることで、TextureをJpg圧縮する事が可能です。<br />

オプションの値一覧 
- "NONE" → RGBA 32bit無圧縮設定
- "RGB_565" → RGB565(16bit) 無圧縮設定
- "PNG" → RGBA 32Bit PNG圧縮設定
- "JPG_BUFFERRGBA" → RGBA 32bit JPEG圧縮設定
- "JPG" / "JPG_BUFFERRGB565" → RGB565 JPEG圧縮設定

### オプション例
    例：Sample.exe --profilerSS=enable --profilerSS-resolution=640x480 --profilerSS-format=jpg <br />
    例：adb shell am start -n com.utj.test[パッケージ名]/com.unity3d.player.UnityPlayerActivity[アクティビティ名] -e "unity --profilerSS=disable"<br />

## 変更履歴
<pre>
version 1.5.0
　コマンドラインオプションを追加
　解像度の表示

version 1.4.0
  カラースペース変換機能を追加

version 1.3.0
  Unity 2021.2以降向けにProfilerのカスタムModuleを追加

version 1.2.1
  リリースビルド時にエラーが出てしまっていたので修正

version 1.2.0
  Profilerデータがないときに、NullReference　Exceptionが出る問題を修正
  テクスチャ圧縮の追加
   Screenshotではなく任意の画像を載せるInterfaceの追加
  "ScreenshotToUnityProfiler.prefab"の追加

version 1.1.0
  supportsAsyncGPUReadbackに対応していないケースへの対応
</pre>
