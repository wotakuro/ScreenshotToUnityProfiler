
using UnityEngine;
using UTJ.SS2Profiler;

namespace UTJ.SS2Profiler
{
    public class ScreenShotToProfilerBehaviour : MonoBehaviour
    {
        [SerializeField]
        private int width = 192;
        [SerializeField]
        private int height = 128;
        [SerializeField]
        private ScreenShotToProfiler.TextureCompress textureCompress = ScreenShotToProfiler.TextureCompress.RGB_565;
        [SerializeField]
        private bool allowSync = true;

        void Awake()
        {
            ScreenShotToProfiler.Instance.Initialize(width, height, textureCompress, allowSync);
            Destroy(this.gameObject);
        }
    }
}