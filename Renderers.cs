using Silk.NET.Maths;
using Silk.NET.OpenGL;
using System.Numerics;
using Szeminarium;

namespace GrafikaSzeminarium
{
    public static class Renderers
    {
        private static CubeArrangementModel cubeArrangementModel = new CubeArrangementModel();
        public static void DrawSkyBox(GL gl, uint program, ModelObjectDescriptor skybox, Vector3D<float> cameraPosition)
        {
            var modelMatrixSkyBox = Matrix4X4.CreateScale(1000f) * Matrix4X4.CreateTranslation(cameraPosition);
            SetModelMatrix(gl, program, modelMatrixSkyBox);

            int textureLocation = gl.GetUniformLocation(program, "uTexture");
            gl.Uniform1(textureLocation, 0);
            gl.ActiveTexture(TextureUnit.Texture0);
            gl.BindTexture(TextureTarget.Texture2D, skybox.Texture.Value);

            DrawModelObject(gl, skybox);
            gl.BindTexture(TextureTarget.Texture2D, 0);
        }
        public static void DrawGround(GL gl, uint program, ModelObjectDescriptor ground)
        {
            var groundMatrix = Matrix4X4.CreateScale(500f) * Matrix4X4.CreateTranslation(0f, -3f, 0f);
            SetModelMatrix(gl, program, groundMatrix);

            int textureLocation = gl.GetUniformLocation(program, "uTexture");
            gl.Uniform1(textureLocation, 0);
            gl.ActiveTexture(TextureUnit.Texture0);
            gl.BindTexture(TextureTarget.Texture2D, ground.Texture.Value);

            DrawModelObject(gl, ground);
            gl.BindTexture(TextureTarget.Texture2D, 0);
        }
        public static void DrawMan(GL gl, uint program, ModelObjectDescriptor man, Vector3 position,float manYaw)
        {
            var manMatrix = Matrix4X4.CreateScale(0.25f) * Matrix4X4.CreateRotationY(CubeArrangementModel.DegreesToRadians(manYaw)) * Matrix4X4.CreateTranslation<float>(new Vector3D<float>(position.X, position.Y, position.Z));

            SetModelMatrix(gl, program, manMatrix);

            int textureLocation = gl.GetUniformLocation(program, "uTexture");
            gl.Uniform1(textureLocation, 0);
            gl.ActiveTexture(TextureUnit.Texture0);

            if (man.Texture.HasValue)
                gl.BindTexture(TextureTarget.Texture2D, man.Texture.Value);
            else
                gl.BindTexture(TextureTarget.Texture2D, 0);

            DrawModelObject(gl, man);
            gl.BindTexture(TextureTarget.Texture2D, 0);
        }
        public static void DrawHouse(GL gl, uint program, ModelObjectDescriptor house)
        {
            var houseMatrix = Matrix4X4.CreateScale(3f) * Matrix4X4.CreateTranslation<float>(new Vector3D<float>(0f, -3f, -45f));

            SetModelMatrix(gl, program, houseMatrix);

            int textureLocation = gl.GetUniformLocation(program, "uTexture");
            gl.Uniform1(textureLocation, 0);
            gl.ActiveTexture(TextureUnit.Texture0);

            if (house.Texture.HasValue)
                gl.BindTexture(TextureTarget.Texture2D, house.Texture.Value);
            else
                gl.BindTexture(TextureTarget.Texture2D, 0);

            DrawModelObject(gl, house);
            gl.BindTexture(TextureTarget.Texture2D, 0);
        }
        public static void DrawWalls(GL gl, uint program, ModelObjectDescriptor wall)
        {
            var wallMatrix = Matrix4X4.CreateScale(0.7f) * Matrix4X4.CreateTranslation<float>(new Vector3D<float>(0f, -3f, 45f));

            SetModelMatrix(gl, program, wallMatrix);

            int textureLocation = gl.GetUniformLocation(program, "uTexture");
            gl.Uniform1(textureLocation, 0);
            gl.ActiveTexture(TextureUnit.Texture0);

            if (wall.Texture.HasValue)
                gl.BindTexture(TextureTarget.Texture2D, wall.Texture.Value);
            else
                gl.BindTexture(TextureTarget.Texture2D, 0);

            DrawModelObject(gl, wall);
            gl.BindTexture(TextureTarget.Texture2D, 0);
        }
        public static void DrawAmmo(GL gl, uint program, ModelObjectDescriptor ammo,float rotation)
        {
            var ammoMatrix = Matrix4X4.CreateScale(0.5f) * Matrix4X4.CreateRotationX(CubeArrangementModel.DegreesToRadians(90f)) *  Matrix4X4.CreateRotationY(CubeArrangementModel.DegreesToRadians(rotation)) * Matrix4X4.CreateTranslation<float>(new Vector3D<float>(5f, -1f, -35f));

            SetModelMatrix(gl, program, ammoMatrix);

            int textureLocation = gl.GetUniformLocation(program, "uTexture");
            gl.Uniform1(textureLocation, 0);
            gl.ActiveTexture(TextureUnit.Texture0);

            if (ammo.Texture.HasValue)
                gl.BindTexture(TextureTarget.Texture2D, ammo.Texture.Value);
            else
                gl.BindTexture(TextureTarget.Texture2D, 0);

            DrawModelObject(gl, ammo);
            gl.BindTexture(TextureTarget.Texture2D, 0);
        }

        public static void DrawShootingAmmo(GL gl, uint program, ModelObjectDescriptor ammo,Vector3 pos)
        {
            var ammoMatrix = Matrix4X4.CreateScale(0.1f) * Matrix4X4.CreateRotationX(CubeArrangementModel.DegreesToRadians(90f)) * Matrix4X4.CreateTranslation(new Vector3D<float>(pos.X,pos.Y,pos.Z));

            SetModelMatrix(gl, program, ammoMatrix);
            
            int textureLocation = gl.GetUniformLocation(program, "uTexture");
            gl.Uniform1(textureLocation, 0);
            gl.ActiveTexture(TextureUnit.Texture0);

            if (ammo.Texture.HasValue)
                gl.BindTexture(TextureTarget.Texture2D, ammo.Texture.Value);
            else
                gl.BindTexture(TextureTarget.Texture2D, 0);

            DrawModelObject(gl, ammo);
            gl.BindTexture(TextureTarget.Texture2D, 0);
        }
        public static void DrawTrees(GL gl, uint program, ModelObjectDescriptor trees)
        {
            Vector3D<float>[] treePositions = new Vector3D<float>[]
            {
                new Vector3D<float>(20f, -1f, 35f),
                new Vector3D<float>(0f, -1f, 35f),
                new Vector3D<float>(-20f, -1f, 35f),
                new Vector3D<float>(-35f, -1f, 40f),
                new Vector3D<float>(35f, -1f, 40f),
                new Vector3D<float>(25f, -1f, 15f),
                new Vector3D<float>(5f, -1f, 15f),
                new Vector3D<float>(-25f, -1f, 15f),
                new Vector3D<float>(-40f, -1f, 20f),
                new Vector3D<float>(40f, -1f, 20f),
                new Vector3D<float>(20f, -1f, -5f),
                new Vector3D<float>(0f, -1f, -5f),
                new Vector3D<float>(-20f, -1f, -5f),
                new Vector3D<float>(-35f, -1f, 0f),
                new Vector3D<float>(35f, -1f, 0f),
            };

            foreach (var position in treePositions)
            {
                var modelMatrix = Matrix4X4.CreateScale(2f) * Matrix4X4.CreateTranslation(position);
                SetModelMatrix(gl, program, modelMatrix);

                int textureLocation = gl.GetUniformLocation(program, "uTexture");
                gl.Uniform1(textureLocation, 0);
                gl.ActiveTexture(TextureUnit.Texture0);

                if (trees.Texture.HasValue)
                    gl.BindTexture(TextureTarget.Texture2D, trees.Texture.Value);
                else
                    gl.BindTexture(TextureTarget.Texture2D, 0);

                DrawModelObject(gl, trees);
                gl.BindTexture(TextureTarget.Texture2D, 0);
            }
        }
        public static void DrawBigTrees(GL gl, uint program, ModelObjectDescriptor bigTrees)
        {
            Vector3D<float>[] treePositions = new Vector3D<float>[]
            {
                new Vector3D<float>(20f, -3f, -45f),
                new Vector3D<float>(-20f, -3f, -45f),
            };

            foreach (var position in treePositions)
            {
                var modelMatrix = Matrix4X4.CreateScale(3f) * Matrix4X4.CreateTranslation(position);
                SetModelMatrix(gl, program, modelMatrix);

                int textureLocation = gl.GetUniformLocation(program, "uTexture");
                gl.Uniform1(textureLocation, 0);
                gl.ActiveTexture(TextureUnit.Texture0);

                if (bigTrees.Texture.HasValue)
                    gl.BindTexture(TextureTarget.Texture2D, bigTrees.Texture.Value);
                else
                    gl.BindTexture(TextureTarget.Texture2D, 0);

                DrawModelObject(gl, bigTrees);
                gl.BindTexture(TextureTarget.Texture2D, 0);
            }
        }
        public static void DrawFoxy(GL gl, uint program, ModelObjectDescriptor foxy, List<Vector3> foxyPositions, List<bool> foxyAlive, float rad)
        {
            for (int i = 0;i < foxyPositions.Count;i++)
            {
                if (foxyAlive[i])
                {
                    var pos = foxyPositions[i];
                    var foxyMatrix = Matrix4X4.CreateScale(0.5f) * Matrix4X4.CreateRotationY(MathF.PI / 180f * (-75f - rad)) * Matrix4X4.CreateTranslation(new Vector3D<float>(pos.X, pos.Y, pos.Z));
                    if (pos.X > 0)
                    {
                        foxyMatrix = Matrix4X4.CreateScale(0.5f) * Matrix4X4.CreateRotationY(MathF.PI / 180f * (75f + rad)) * Matrix4X4.CreateTranslation(new Vector3D<float>(pos.X, pos.Y, pos.Z));
                    }

                    SetModelMatrix(gl, program, foxyMatrix);

                    int textureLocation = gl.GetUniformLocation(program, "uTexture");
                    gl.Uniform1(textureLocation, 0);
                    gl.ActiveTexture(TextureUnit.Texture0);

                    if (foxy.Texture.HasValue)
                    {
                        gl.BindTexture(TextureTarget.Texture2D, foxy.Texture.Value);
                        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.LinearMipmapLinear);
                        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
                    }
                    else
                    {
                        gl.BindTexture(TextureTarget.Texture2D, 0);
                    }

                    DrawModelObject(gl, foxy);
                    gl.BindTexture(TextureTarget.Texture2D, 0);
                }
            }
        }
        private static unsafe void SetModelMatrix(GL gl, uint program, Matrix4X4<float> modelMatrix)
        {
            int modelLocation = gl.GetUniformLocation(program, "uModel");
            gl.UniformMatrix4(modelLocation, 1, false, (float*)&modelMatrix);

            int normalLocation = gl.GetUniformLocation(program, "uNormal");
            if (normalLocation != -1)
            {
                var normalMatrix = CalculateNormalMatrix(modelMatrix);
                gl.UniformMatrix3(normalLocation, 1, false, (float*)&normalMatrix);
            }
        }
        private static Matrix3X3<float> CalculateNormalMatrix(Matrix4X4<float> modelMatrix)
        {
            var modelMatrixWithoutTranslation = new Matrix4X4<float>(
                modelMatrix.Row1, modelMatrix.Row2, modelMatrix.Row3, modelMatrix.Row4);

            modelMatrixWithoutTranslation.M41 = 0;
            modelMatrixWithoutTranslation.M42 = 0;
            modelMatrixWithoutTranslation.M43 = 0;
            modelMatrixWithoutTranslation.M44 = 1;

            Matrix4X4.Invert(modelMatrixWithoutTranslation, out var modelInvers);
            return new Matrix3X3<float>(Matrix4X4.Transpose(modelInvers));
        }
        private static unsafe void DrawModelObject(GL gl, ModelObjectDescriptor modelObject)
        {
            gl.BindVertexArray(modelObject.Vao);
            gl.BindBuffer(GLEnum.ElementArrayBuffer, modelObject.Indices);
            gl.DrawElements(PrimitiveType.Triangles, modelObject.IndexArrayLength,DrawElementsType.UnsignedInt, null);
            gl.BindBuffer(GLEnum.ElementArrayBuffer, 0);
            gl.BindVertexArray(0);
        }
    }
}