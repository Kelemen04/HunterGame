using Silk.NET.Maths;
using Silk.NET.OpenGL;
using StbImageSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using Szeminarium;

namespace GrafikaSzeminarium
{
    internal class ObjectResourceReader
    {
        class MaterialData
        {
            public string Name { get; set; }
            public string DiffuseMap { get; set; }
        }

        public static unsafe ModelObjectDescriptor CreateObjectFromResource(GL Gl, string resourceName, ImageResult textureImage = null)
        {
            List<float[]> objVertices = new List<float[]>();
            List<float[]> objNormals = new List<float[]>();
            List<float[]> objUVs = new List<float[]>();

            List<(int v, int vt, int vn)[]> objFaceVertices = new();
            Dictionary<string, MaterialData> materials = new();
            string currentMaterial = null;

            string fullResourceName = "GrafikaSzeminarium.Resources." + resourceName;
            using (var objStream = typeof(ObjectResourceReader).Assembly.GetManifestResourceStream(fullResourceName))
            using (var objReader = new StreamReader(objStream))
            {
                while (!objReader.EndOfStream)
                {
                    var line = objReader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    int spaceIndex = line.IndexOf(' ');
                    if (spaceIndex == -1)
                        continue;

                    var lineClassifier = line.Substring(0, spaceIndex);
                    var lineData = line.Substring(spaceIndex + 1).Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

                    switch (lineClassifier)
                    {
                        case "v":
                            float[] vertex = new float[3];
                            for (int i = 0; i < Math.Min(3, lineData.Length); i++)
                                vertex[i] = float.Parse(lineData[i], CultureInfo.InvariantCulture);
                            objVertices.Add(vertex);
                            break;

                        case "vt":
                            float[] uv = new float[2];
                            for (int i = 0; i < Math.Min(2, lineData.Length); i++)
                                uv[i] = float.Parse(lineData[i], CultureInfo.InvariantCulture);
                            objUVs.Add(uv);
                            break;

                        case "vn":
                            float[] normal = new float[3];
                            for (int i = 0; i < Math.Min(3, lineData.Length); i++)
                                normal[i] = float.Parse(lineData[i], CultureInfo.InvariantCulture);
                            objNormals.Add(normal);
                            break;

                        case "f":
                            var faceVertices = lineData.Select(v => v.Split('/')).ToArray();
                            var faceIndices = new (int v, int vt, int vn)[faceVertices.Length];
                            for (int i = 0; i < faceVertices.Length; i++)
                            {
                                int vIndex = -1, vtIndex = -1, vnIndex = -1;

                                if (faceVertices[i].Length >= 1 && int.TryParse(faceVertices[i][0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int vi))
                                    vIndex = vi - 1;
                                if (faceVertices[i].Length >= 2 && !string.IsNullOrEmpty(faceVertices[i][1]) && int.TryParse(faceVertices[i][1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int vti))
                                    vtIndex = vti - 1;
                                if (faceVertices[i].Length == 3 && int.TryParse(faceVertices[i][2], NumberStyles.Integer, CultureInfo.InvariantCulture, out int vni))
                                    vnIndex = vni - 1;

                                faceIndices[i] = (vIndex, vtIndex, vnIndex);
                            }
                            objFaceVertices.Add(faceIndices);
                            break;

                        case "mtllib":
                            string mtlFileName = lineData[0];
                            Console.WriteLine(mtlFileName);
                            string mtlResourceName = "GrafikaSzeminarium.Resources." + mtlFileName;
                            if(mtlResourceName != null)
                            {
                                using (var mtlStream = typeof(ObjectResourceReader).Assembly.GetManifestResourceStream(mtlResourceName))
                                using (var mtlReader = new StreamReader(mtlStream))
                                {
                                    MaterialData currentMat = null;
                                    while (!mtlReader.EndOfStream)
                                    {
                                        var mtlLine = mtlReader.ReadLine();
                                        if (string.IsNullOrWhiteSpace(mtlLine)) continue;

                                        var parts = mtlLine.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                                        if (parts.Length < 2) continue;

                                        switch (parts[0])
                                        {
                                            case "newmtl":
                                                currentMat = new MaterialData { Name = parts[1] };
                                                materials[parts[1]] = currentMat;
                                                break;
                                            case "map_Kd":
                                                if (currentMat != null)
                                                    currentMat.DiffuseMap = parts[1];
                                                break;
                                        }
                                    }
                                }
                            }
                           
                            break;

                        case "usemtl":
                            currentMaterial = lineData[0];
                            break;
                    }
                }
            }

            if (objNormals.Count == 0)
            {
                for (int i = 0; i < objVertices.Count; i++)
                    objNormals.Add(new float[] { 0f, 0f, 0f });

                foreach (var face in objFaceVertices)
                {
                    var p0 = new Vector3(objVertices[face[0].v][0], objVertices[face[0].v][1], objVertices[face[0].v][2]);
                    var p1 = new Vector3(objVertices[face[1].v][0], objVertices[face[1].v][1], objVertices[face[1].v][2]);
                    var p2 = new Vector3(objVertices[face[2].v][0], objVertices[face[2].v][1], objVertices[face[2].v][2]);

                    var normal = Vector3.Cross(p1 - p0, p2 - p0);
                    normal = normal.LengthSquared() > 0 ? Vector3.Normalize(normal) : Vector3.UnitY;

                    for (int i = 0; i < face.Length; i++)
                    {
                        var n = objNormals[face[i].v];
                        var nVec = new Vector3(n[0], n[1], n[2]) + normal;
                        objNormals[face[i].v] = new float[] { nVec.X, nVec.Y, nVec.Z };
                        face[i] = (face[i].v, face[i].vt, face[i].v);
                    }
                }

                for (int i = 0; i < objNormals.Count; i++)
                {
                    var n = objNormals[i];
                    var nVec = new Vector3(n[0], n[1], n[2]);
                    nVec = nVec.LengthSquared() > 0 ? Vector3.Normalize(nVec) : Vector3.UnitY;
                    objNormals[i] = new float[] { nVec.X, nVec.Y, nVec.Z };
                }
            }

            Dictionary<(int pos, int norm, int uv), uint> uniqueVertices = new();
            List<float> glVertices = new();
            List<uint> glIndexArray = new();
            uint nextIndex = 0;

            foreach (var face in objFaceVertices)
            {
                for (int i = 1; i < face.Length - 1; i++)
                {
                    var triangle = new[] { face[0], face[i], face[i + 1] };

                    foreach (var vert in triangle)
                    {
                        var key = (vert.v, vert.vn, vert.vt);

                        if (!uniqueVertices.TryGetValue(key, out uint index))
                        {
                            uniqueVertices[key] = nextIndex;
                            index = nextIndex++;

                            var pos = objVertices[vert.v];
                            glVertices.AddRange(pos);

                            var norm = (vert.vn >= 0 && vert.vn < objNormals.Count) ? objNormals[vert.vn] : new float[] { 0f, 0f, 0f };
                            glVertices.AddRange(norm);

                            var uvData = (vert.vt >= 0 && vert.vt < objUVs.Count) ? objUVs[vert.vt] : new float[] { 0f, 0f };
                            glVertices.AddRange(uvData);
                        }
                        glIndexArray.Add(index);
                    }
                }
            }

            if (textureImage == null && currentMaterial != null && materials.TryGetValue(currentMaterial, out var matData))
            {
                if (!string.IsNullOrEmpty(matData.DiffuseMap))
                {
                    string texResName = "GrafikaSzeminarium.Resources." + matData.DiffuseMap;
                    using var stream = typeof(ObjectResourceReader).Assembly.GetManifestResourceStream(texResName);
                    textureImage = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
                }
            }

            uint vertexSize = (3 + 3 + 2) * sizeof(float);
            uint vao = Gl.GenVertexArray();
            Gl.BindVertexArray(vao);

            uint vertices = Gl.GenBuffer();
            Gl.BindBuffer(BufferTargetARB.ArrayBuffer, vertices);
            Gl.BufferData<float>(BufferTargetARB.ArrayBuffer, glVertices.ToArray(), BufferUsageARB.StaticDraw);

            Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, vertexSize, (void*)0);
            Gl.EnableVertexAttribArray(0);
            Gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, vertexSize, (void*)(3 * sizeof(float)));
            Gl.EnableVertexAttribArray(1);
            Gl.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, vertexSize, (void*)(6 * sizeof(float)));
            Gl.EnableVertexAttribArray(2);

            uint indices = Gl.GenBuffer();
            Gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, indices);
            Gl.BufferData<uint>(BufferTargetARB.ElementArrayBuffer, glIndexArray.ToArray(), BufferUsageARB.StaticDraw);

            Gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
            Gl.BindVertexArray(0);

            uint? texture = null;
            if (textureImage != null)
            {
                texture = Gl.GenTexture();
                Gl.BindTexture(GLEnum.Texture2D, texture.Value);

                fixed (byte* data = textureImage.Data)
                {
                    Gl.TexImage2D(GLEnum.Texture2D, 0, (int)GLEnum.Rgba8, (uint)textureImage.Width, (uint)textureImage.Height, 0, GLEnum.Rgba, GLEnum.UnsignedByte, data);
                }

                Gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapS, (int)GLEnum.Repeat);
                Gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapT, (int)GLEnum.Repeat);
                Gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)GLEnum.LinearMipmapLinear);
                Gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int)GLEnum.Linear);

                Gl.GenerateMipmap(GLEnum.Texture2D);
                Gl.BindTexture(GLEnum.Texture2D, 0);
            }

            return new ModelObjectDescriptor(Gl, vao, vertices, 0, indices, (uint)glIndexArray.Count, texture);
        }
    }
}
