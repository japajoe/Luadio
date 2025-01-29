// MIT License

// Copyright (c) 2025 W.M.R Jap-A-Joe

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Runtime.InteropServices;
using GLFWNet;
using OpenTK;
using OpenTK.Graphics;

namespace Luadio
{
    [Flags]
    public enum WindowFlags
    {
        None = 1 << 0,
        VSync = 1 << 1,
        FullScreen = 1 << 2,
        Maximize = 1 << 3
    }

    public struct Configuration
    {
        public string title;
        public int width;
        public int height;
        public WindowFlags flags;
    }

    public delegate void LoadEvent();
    public delegate void CloseEvent();
    public delegate void NewFrameEvent(float deltaTime);

    public sealed class Window
    {
        public event LoadEvent Load;
        public event CloseEvent Closing;
        public event NewFrameEvent NewFrame;

        private Configuration config;
        private IntPtr window;
        private static IntPtr nativeWindow;

        public static IntPtr NativeWindow
        {
            get
            {
                return nativeWindow;
            }
        }

        public Window(int width, int height, string title, WindowFlags flags = WindowFlags.VSync)
        {
            config.width = width;
            config.height = height;
            config.title = title;
            config.flags = flags;
            window = IntPtr.Zero;
            nativeWindow = IntPtr.Zero;
        }

        public Window(Configuration config)
        {
            this.config = config;
            window = IntPtr.Zero;
            nativeWindow = IntPtr.Zero;
        }

        public void Create()
        {
            if(window != IntPtr.Zero)
            {
                Console.WriteLine("Window is already initialized");
                return;
            }

            if(GLFW.Init() == 0)
            {
                Console.WriteLine("Failed to initialize GLFW");
                return;
            }

            GLFW.WindowHint(GLFW.CONTEXT_VERSION_MAJOR, 3);
            GLFW.WindowHint(GLFW.CONTEXT_VERSION_MINOR, 3);
            GLFW.WindowHint(GLFW.OPENGL_PROFILE, GLFW.OPENGL_CORE_PROFILE);
            GLFW.WindowHint(GLFW.VISIBLE, GLFW.FALSE);
            GLFW.WindowHint(GLFW.SAMPLES, 4);

            if(config.flags.HasFlag(WindowFlags.Maximize))
                GLFW.WindowHint(GLFW.MAXIMIZED, GLFW.TRUE);

            if(config.flags.HasFlag(WindowFlags.FullScreen))
            {
                IntPtr monitor =  GLFW.GetPrimaryMonitor();

                if(GLFW.GetVideoMode(monitor, out GLFWvidmode mode))
                    window = GLFW.CreateWindow(mode.width, mode.height, config.title, monitor, IntPtr.Zero);
                else
                    window = GLFW.CreateWindow(config.width, config.height, config.title, IntPtr.Zero, IntPtr.Zero);
            }
            else
            {
                window = GLFW.CreateWindow(config.width, config.height, config.title, IntPtr.Zero, IntPtr.Zero);
            }

            if(window == IntPtr.Zero)
            {
                GLFW.Terminate();
                Console.WriteLine("Failed to create window");
                return;
            }

            nativeWindow = window;

            GLFW.MakeContextCurrent(window);

            GLFW.SwapInterval(config.flags.HasFlag(WindowFlags.VSync) ? 1 : 0);

            GLLoader.LoadBindings(new GLFWBindingsContext());

            string version = OpenTK.Graphics.OpenGL.GL.GetString(OpenTK.Graphics.OpenGL.StringName.Version);

            if(!string.IsNullOrEmpty(version))
            {
                Console.WriteLine("OpenGL Version: " + version);
            }

            GLFW.SetFramebufferSizeCallback(window, OnWindowResize);
            GLFW.SetWindowPosCallback(window, OnWindowMove);
            GLFW.SetKeyCallback(window, OnKeyPress);
            GLFW.SetCharCallback(window, OnCharPress);
            GLFW.SetMouseButtonCallback(window, OnMouseButtonPress);
            GLFW.SetScrollCallback(window, OnMouseScroll);

            OnInitialize();

            GLFW.ShowWindow(window);

            OpenTK.Graphics.OpenGL.GL.Enable(OpenTK.Graphics.OpenGL.EnableCap.Multisample);
            OpenTK.Graphics.OpenGL.GL.Clear(OpenTK.Graphics.OpenGL.ClearBufferMask.ColorBufferBit);
            OpenTK.Graphics.OpenGL.GL.ClearColor(1, 1, 1, 1);

            while(GLFW.WindowShouldClose(window) == 0)
            {
                OnNewFrame();
                OnEndFrame();
                GLFW.PollEvents();
                GLFW.SwapBuffers(window);
            }

            OnClosing();

            GLFW.DestroyWindow(window);

            window = IntPtr.Zero;
            nativeWindow = IntPtr.Zero;

            GLFW.Terminate();
        }

        private void OnInitialize()
        {
            MiniAudioEx.AudioContext.Initialize(44100, 2);
            Graphics.Initialize();
            Load?.Invoke();
        }        

        private void OnNewFrame()
        {
            Time.NewFrame();
            Input.NewFrame();
            MiniAudioEx.AudioContext.Update();
            Graphics.NewFrame();
            NewFrame?.Invoke(Time.DeltaTime);
        }

        private void OnEndFrame()
        {
            Graphics.EndFrame();
            Input.EndFrame();
        }

        private void OnClosing()
        {
            Closing?.Invoke();
            Graphics.Deinitialize();
            MiniAudioEx.AudioContext.Deinitialize();
        }

        public void Close()
        {
            GLFW.SetWindowShouldClose(NativeWindow, GLFW.TRUE);
        }

        private void OnWindowResize(IntPtr window, int width, int height)
        {
            Graphics.SetViewport(0, 0, width, height);
        }

        private void OnWindowMove(IntPtr window, int x, int y)
        {
            Input.SetWindowPosition(x, y);
        }

        private void OnKeyPress(IntPtr window, int key, int scancode, int action, int mods)
        {
            Input.SetKeyState((KeyCode)key, action > 0 ? 1 : 0);
        }

        private void OnCharPress(IntPtr window, uint codepoint)
        {
            Input.AddInputCharacter(codepoint);
        }

        private void OnMouseButtonPress(IntPtr window, int button, int action, int mods)
        {
            Input.SetButtonState((ButtonCode)button, action > 0 ? 1 : 0);
        }

        private void OnMouseScroll(IntPtr window, double xoffset, double yoffset)
        {
            Input.SetScrollDirection(xoffset, yoffset);
        }
    }

    internal sealed class GLFWBindingsContext : IBindingsContext
    {
        public IntPtr GetProcAddress(string procName)
        {
            return Marshal.GetFunctionPointerForDelegate(GLFW.GetProcAddress(procName));
        }
    }
}