using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using StbImageSharp;
using System.Numerics;
using System.Reflection;
using Szeminarium;

namespace GrafikaSzeminarium
{
    internal class Program
    {
        private static IWindow graphicWindow;
        private static IKeyboard keyboard;
        private static GL Gl;
        private static ImGuiController imGuiController;

        private static ModelObjectDescriptor cube;
        private static ModelObjectDescriptor ground;
        private static ModelObjectDescriptor man;
        private static ModelObjectDescriptor house;
        private static ModelObjectDescriptor rockWall;
        private static ModelObjectDescriptor ammo;
        private static ModelObjectDescriptor trees;
        private static ModelObjectDescriptor bigTrees;
        private static ModelObjectDescriptor foxy;
        private static ModelObjectDescriptor skybox;

        private static CameraDescriptor camera = new CameraDescriptor();
        private static CubeArrangementModel cubeArrangementModel = new CubeArrangementModel();

        private const string ModelMatrixVariableName = "uModel";
        private const string NormalMatrixVariableName = "uNormal";
        private const string ViewMatrixVariableName = "uView";
        private const string ProjectionMatrixVariableName = "uProjection";
        private const string LightColorVariableName = "uLightColor";
        private const string LightPositionVariableName = "uLightPos";
        private const string ViewPositionVariableName = "uViewPos";
        private const string ShinenessVariableName = "uShininess";
        private const string TextureVariableName = "uTexture";

        private static float shininess = 50;
        private static uint program;
        static void Main(string[] args)
        {
            WindowOptions windowOptions = WindowOptions.Default;
            windowOptions.Title = "Grafika szeminárium";
            windowOptions.Size = new Vector2D<int>(500, 500);
            graphicWindow = Window.Create(windowOptions);

            graphicWindow.Load += GraphicWindow_Load;
            graphicWindow.Update += GraphicWindow_Update;
            graphicWindow.Render += GraphicWindow_Render;
            graphicWindow.Closing += GraphicWindow_Closing;

            graphicWindow.Run();
        }

        private static void GraphicWindow_Closing()
        {
            cube.Dispose();
            ground.Dispose();
            man.Dispose();
            foxy.Dispose();
            rockWall.Dispose();
            ammo.Dispose();
            trees.Dispose();
            bigTrees.Dispose();
            Gl.DeleteProgram(program);
        }

        private static void GraphicWindow_Load()
        {
            Gl = graphicWindow.CreateOpenGL();
            var inputContext = graphicWindow.CreateInput();

            foreach(var keybord in inputContext.Keyboards)
            {
                keybord.KeyDown += Keyboard_KeyDown;
            }

            keyboard = inputContext.Keyboards[0];

            foreach (var mouse in inputContext.Mice)
            {
                mouse.MouseMove += OnMouseMove;
            }

            graphicWindow.FramebufferResize += s => Gl.Viewport(s);
            imGuiController = new ImGuiController(Gl, graphicWindow, inputContext);

            cube = ModelObjectDescriptor.CreateCube(Gl);
            ground = ModelObjectDescriptor.CreateGround(Gl);
            man = ObjectResourceReader.CreateObjectFromResource(Gl, "man.obj");
            house = ObjectResourceReader.CreateObjectFromResource(Gl, "cartoon_house.obj");
            rockWall = ObjectResourceReader.CreateObjectFromResource(Gl, "CaveWalls3.obj");
            ammo = ObjectResourceReader.CreateObjectFromResource(Gl, "ammo2.obj");
            trees = ObjectResourceReader.CreateObjectFromResource(Gl, "trees.obj");
            bigTrees = ObjectResourceReader.CreateObjectFromResource(Gl, "bigTree.obj");
            foxy = ObjectResourceReader.CreateObjectFromResource(Gl, "Foxy.obj");
            skybox = ModelObjectDescriptor.CreateSkyBox(Gl);

            Gl.ClearColor(System.Drawing.Color.White);
            Gl.Enable(EnableCap.DepthTest);
            Gl.DepthFunc(DepthFunction.Lequal);

            uint vshader = Gl.CreateShader(ShaderType.VertexShader);
            uint fshader = Gl.CreateShader(ShaderType.FragmentShader);

            Gl.ShaderSource(vshader, GetEmbeddedResourceAsString("Shaders.VertexShader.vert"));
            Gl.CompileShader(vshader);
            Gl.GetShader(vshader, ShaderParameterName.CompileStatus, out int vStatus);
            if (vStatus != (int)GLEnum.True)
                throw new Exception("Vertex shader failed to compile: " + Gl.GetShaderInfoLog(vshader));

            Gl.ShaderSource(fshader, GetEmbeddedResourceAsString("Shaders.FragmentShader.frag"));
            Gl.CompileShader(fshader);
            Gl.GetShader(fshader, ShaderParameterName.CompileStatus, out int fStatus);
            if (fStatus != (int)GLEnum.True)
                throw new Exception("Fragment shader failed to compile: " + Gl.GetShaderInfoLog(fshader));

            program = Gl.CreateProgram();
            Gl.AttachShader(program, vshader);
            Gl.AttachShader(program, fshader);
            Gl.LinkProgram(program);

            Gl.DetachShader(program, vshader);
            Gl.DetachShader(program, fshader);
            Gl.DeleteShader(vshader);
            Gl.DeleteShader(fshader);

            Gl.GetProgram(program, GLEnum.LinkStatus, out var status);
            if (status == 0)
            {
                Console.WriteLine($"Error linking shader {Gl.GetProgramInfoLog(program)}");
            }
        }

        private static string GetEmbeddedResourceAsString(string resourceRelativePath)
        {
            string resourceFullPath = Assembly.GetExecutingAssembly().GetName().Name + "." + resourceRelativePath;
            using (var resStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceFullPath))
            using (var resStreamReader = new StreamReader(resStream))
            {
                return resStreamReader.ReadToEnd();
            }
        }

        private static void Keyboard_KeyDown(IKeyboard keyboard, Key key, int arg3)
        {
            switch (key)
            {
                case Key.Left: camera.GoLeft(); break;
                case Key.Right: camera.GoRight(); break;
                case Key.Down: camera.GoBack(); break;
                case Key.Up: camera.GoFront(); break;
                case Key.Space: cubeArrangementModel.AnimationEnabled = !cubeArrangementModel.AnimationEnabled; break;
                case Key.F: camera.firstPerson = !camera.firstPerson; break;
            }
        }

        private static void GraphicWindow_Update(double deltaTime)
        {
            Vector3 movement = Vector3.Zero;

            if (keyboard.IsKeyPressed(Key.W))
                movement += new Vector3((float)camera.Front.X, 0, (float)camera.Front.Z);
            if (keyboard.IsKeyPressed(Key.S))
                movement -= new Vector3((float)camera.Front.X, 0, (float)camera.Front.Z);
            if (keyboard.IsKeyPressed(Key.A))
                movement -= new Vector3((float)camera.Right.X, 0, (float)camera.Right.Z);
            if (keyboard.IsKeyPressed(Key.D))
                movement += new Vector3((float)camera.Right.X, 0, (float)camera.Right.Z);

            if (movement != Vector3.Zero)
            {
                movement = Vector3.Normalize(movement);
                cubeArrangementModel.SetManDirectionFromCamera(movement);
            }
            else
            {
                cubeArrangementModel.SetManDirectionFromCamera(Vector3.Zero);
            }

            cubeArrangementModel.ManYaw = -camera.yaw + 90f;
            cubeArrangementModel.AdvanceTime(deltaTime);
            imGuiController.Update((float)deltaTime);
        }

        private static float lastX = 0;
        private static float lastY = 0;
        private static bool firstMouse = true;

        private static void OnMouseMove(IMouse mouse, System.Numerics.Vector2 position)
        {
            if (firstMouse)
            {
                lastX = position.X;
                lastY = position.Y;
                firstMouse = false;
            }

            float xoffset = position.X - lastX;
            float yoffset = lastY - position.Y;
            lastX = position.X;
            lastY = position.Y;

            camera.ProcessMouseMovement(xoffset, yoffset);
        }
        private static unsafe void GraphicWindow_Render(double deltaTime)
        {
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            Gl.UseProgram(program);

            if (camera.firstPerson)
            {
                Vector3 manPos = cubeArrangementModel.ManPosition;
                camera.SetPosition(new Vector3D<float>(manPos.X, manPos.Y + 3.7f, manPos.Z + 0.3f));
            }

            cubeArrangementModel.RotateAmmo();

            SetUniform3(LightColorVariableName, new Vector3(1f, 1f, 1f));
            SetUniform3(LightPositionVariableName, new Vector3(7f, 7f, 7f));
            SetUniform3(ViewPositionVariableName, new Vector3(camera.Position.X, camera.Position.Y, camera.Position.Z));
            SetUniform1(ShinenessVariableName, shininess);

            var viewMatrix = Matrix4X4.CreateLookAt(camera.Position, camera.Target, camera.Up);
            SetMatrix(viewMatrix, ViewMatrixVariableName);

            var projectionMatrix = Matrix4X4.CreatePerspectiveFieldOfView<float>((float)(Math.PI / 2), 1024f / 768f, 0.1f, 10000f);
            SetMatrix(projectionMatrix, ProjectionMatrixVariableName);

            Renderers.DrawSkyBox(Gl, program, skybox, camera.Position);
            Renderers.DrawGround(Gl, program, ground);
            Renderers.DrawMan(Gl, program, man, cubeArrangementModel.ManPosition,cubeArrangementModel.ManYaw);
            Renderers.DrawFoxy(Gl, program, foxy, cubeArrangementModel.FoxyPositions,cubeArrangementModel.rad);
            Renderers.DrawHouse(Gl, program, house);
            Renderers.DrawWalls(Gl, program, rockWall);
            Renderers.DrawAmmo(Gl, program, ammo, cubeArrangementModel.ammoRotate);
            Renderers.DrawTrees(Gl, program, trees);
            Renderers.DrawBigTrees(Gl, program, bigTrees);

            imGuiController.Update((float)deltaTime);

            ImGui.SetNextWindowPos(new System.Numerics.Vector2(graphicWindow.Size.X / 2f, graphicWindow.Size.Y / 2f), ImGuiCond.Always, new System.Numerics.Vector2(0.5f, 0.5f));
            ImGui.Begin("Crosshair", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoBackground);
            ImGui.TextColored(new System.Numerics.Vector4(1, 0, 0, 1), "+");
            ImGui.End();

            imGuiController.Render();
        }
        private static unsafe void SetUniform1(string uniformName, float uniformValue)
        {
            int location = Gl.GetUniformLocation(program, uniformName);
            if (location == -1) throw new Exception($"{uniformName} uniform not found on shader.");
            Gl.Uniform1(location, uniformValue);
            CheckError();
        }
        private static unsafe void SetUniform3(string uniformName, Vector3 uniformValue)
        {
            int location = Gl.GetUniformLocation(program, uniformName);
            if (location == -1) throw new Exception($"{uniformName} uniform not found on shader.");
            Gl.Uniform3(location, uniformValue);
            CheckError();
        }
        private static unsafe void SetMatrix(Matrix4X4<float> mx, string uniformName)
        {
            int location = Gl.GetUniformLocation(program, uniformName);
            if (location == -1) throw new Exception($"{uniformName} uniform not found on shader.");
            Gl.UniformMatrix4(location, 1, false, (float*)&mx);
            CheckError();
        }
        public static void CheckError()
        {
            var error = (ErrorCode)Gl.GetError();
            if (error != ErrorCode.NoError)
                throw new Exception("GL.GetError() returned " + error.ToString());
        }
    }
}