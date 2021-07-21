using UnityEngine;
using UnityEngine.Rendering;
using UTJ.SS2Profiler;

public class SwitchSample : MonoBehaviour
{
    [SerializeField]
    private RenderTexture captureRt;

    [ContextMenu("SwitchToRT")]
    void SwitchToRT(){
        ScreenShotToProfiler.Instance.captureBehaviour = (target) =>
        {
            CommandBuffer commandBuffer = new CommandBuffer();
            commandBuffer.name = "ScreenCapture";
            commandBuffer.Blit(this.captureRt, target);
            Graphics.ExecuteCommandBuffer(commandBuffer);
        };
    }
    [ContextMenu("SwitchToDefault")]
    void SwitchToDefault()
    {
        ScreenShotToProfiler.Instance.captureBehaviour = ScreenShotToProfiler.Instance.DefaultCaptureBehaviour;
    }

}
