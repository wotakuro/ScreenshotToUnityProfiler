using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace UTJ.SS2Profiler
{
    public class FlipYTextureResolver : System.IDisposable
    {
        public RenderTexture drawTexture { get; private set; }

        private bool isFlipY = false;

        private Mesh normalMesh;
        private Mesh flipMesh;
        private Material drawMaterial;

        public FlipYTextureResolver()
        {
            this.flipMesh = CreateMesh(true);
            this.normalMesh = CreateMesh(false);
            this.InitMaterial();
        }

        private void InitMaterial()
        {
            this.drawMaterial = new Material(Shader.Find("Unlit/Texture"));

        }

        private Mesh CreateMesh(bool flip)
        {
            var vertices = new Vector3[] {
                new Vector3 (0f, 0f, 0f),
                new Vector3 (0f, 1f, 0f),
                new Vector3 (1f, 1f, 0f),
                new Vector3 (1f, 0f, 0f)
            };
            var uvs = new Vector2[] {
               new Vector2 (0f, 0f),
               new Vector2 (0f, 1f),
               new Vector2 (1f, 1f),
               new Vector2 (1f, 0f)
           };
            var triangles = new int[] {
                0, 1, 2,
                0, 2, 3
            };
            if (flip)
            {
                for (int i = 0; i < uvs.Length; ++i)
                {
                    uvs[i].y = 1.0f - uvs[i].y;
                }
            }

            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;

            mesh.RecalculateNormals();
            return mesh;
        }

        public void Dispose()
        {
            if (this.drawTexture != null)
            {
                this.drawTexture.Release();
                this.drawTexture = null;
            }
        }

        public void SetFlip(bool flag)
        {
            if (flag == this.isFlipY) { return; }
            this.isFlipY = flag;
            if ( this.drawTexture == null)
            {
                return; 
            }

            var tmpRt = RenderTexture.GetTemporary(this.drawTexture.descriptor);
            CommandBuffer cmd = new CommandBuffer();
            cmd.CopyTexture(this.drawTexture, tmpRt);
            Graphics.ExecuteCommandBuffer(cmd);
            this.DrawTextureToRt(tmpRt, true);
            RenderTexture.ReleaseTemporary(tmpRt);
        }

        public void SetupToRenderTexture(Texture tex)
        {
            this.SetupRenderTexture(tex);
            this.DrawTextureToRt(tex, this.isFlipY);
        }
        private void SetupRenderTexture(Texture tex)
        {
            if (tex == null)
            {
                if (drawTexture != null)
                {
                    drawTexture.Release();
                }
                drawTexture = null;
                return;
            }
            if (drawTexture != null &&
                (drawTexture.width != tex.width || drawTexture.height != tex.height))
            {
                drawTexture.Release();
                drawTexture = null;
            }
            if (drawTexture == null)
            {
                drawTexture = new RenderTexture(tex.width, tex.height, 0);
            }
        }

        private void DrawTextureToRt(Texture tex, bool flip)
        {
            if( tex == null) { return; }
            if(!this.drawMaterial)
            {
                InitMaterial();
            }
            if (!this.flipMesh)
            {
                this.flipMesh = CreateMesh(true);
            }
            if (!this.normalMesh)
            {
                this.normalMesh = CreateMesh(false);
            }
            // draw
            this.drawMaterial.mainTexture = tex;

            var matrix = Matrix4x4.identity;
            matrix.m00 = 2;
            matrix.m11 = 2;
            matrix.m03 = -1;
            matrix.m13 = -1;


            CommandBuffer cmd = new CommandBuffer();
            cmd.SetRenderTarget(drawTexture);
            cmd.SetViewMatrix(Matrix4x4.identity);
            cmd.SetProjectionMatrix(Matrix4x4.identity);
            cmd.ClearRenderTarget(true, true, Color.black);
            if (flip)
            {
                cmd.DrawMesh(this.flipMesh, matrix, this.drawMaterial);
            }
            else
            {
                cmd.DrawMesh(this.normalMesh, matrix, this.drawMaterial);
            }
            Graphics.ExecuteCommandBuffer(cmd);
            this.drawMaterial.mainTexture = null;

        }
    }
}