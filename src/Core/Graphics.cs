using OpenTK.Graphics.OpenGL;

namespace Luadio
{
    public struct Viewport
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
    }

    public static class Graphics
    {
        private static Viewport viewport = new Viewport();
        private static ImGuiController imgui;

        internal static void Initialize()
        {
            viewport.X = 0;
            viewport.Y = 0;
            viewport.Width = 512;
            viewport.Height = 512;
            imgui = new ImGuiController();
        }

        internal static void Deinitialize()
        {
            imgui.Dispose();
        }

        internal static Viewport GetViewport()
        {
            return viewport;
        }

        internal static void SetViewport(float x, float y, float width, float height)
        {
            viewport.X = x;
            viewport.Y = y;
            viewport.Width = width;
            viewport.Height = height;
            GL.Viewport((int)x, (int)y, (int)width, (int)height);
        }

        internal static void NewFrame()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.ClearColor(0.10f, 0.105f, 0.11f, 1.00f);

            imgui.NewFrame();
        }

        internal static void EndFrame()
        {
            imgui.EndFrame();
        }
    }
}