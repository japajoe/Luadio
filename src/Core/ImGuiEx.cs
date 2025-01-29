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
using System.Numerics;
using System.Runtime.InteropServices;
using ImGuiNET;

namespace Luadio
{
    public static class ImGuiEx
    {
        private static Vector4 buttonColor = new Vector4(0.17f, 0.18f, 0.19f, 1.00f);
        private static Vector4 buttonHoverColor = new Vector4(0.20f, 0.22f, 0.24f, 1.00f);
        private static Vector4 buttonActiveColor = new Vector4(0.23f, 0.26f, 0.29f, 1.00f);

        public static bool Button(string text, Vector2 size = default(Vector2))
        {
            ImGui.PushStyleColor(ImGuiCol.Button, buttonColor);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, buttonHoverColor);
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, buttonActiveColor);
            bool result = false;
            if(size == Vector2.Zero)
                result = ImGui.Button(text);
            else
                result = ImGui.Button(text, size);
            ImGui.PopStyleColor(3);
            return result;
        }

        public static bool Knob(string label, ref float value, float min, float max, int snapSteps)
        {
            var io = ImGui.GetIO();
            var style = ImGui.GetStyle();

            const float radiusOuter = 20.0f;
            var cursorPosition = ImGui.GetCursorScreenPos();
            var center = new Vector2(cursorPosition.X + radiusOuter, cursorPosition.Y + radiusOuter);
            float lineHeight = ImGui.GetTextLineHeight();
            var drawList = ImGui.GetWindowDrawList();

            ImGui.InvisibleButton(label, new Vector2(radiusOuter * 2, radiusOuter * 2 + lineHeight + style.ItemInnerSpacing.Y));
            bool valueChanged = false;
            bool isActive = ImGui.IsItemActive();
            bool isHovered = ImGui.IsItemHovered();
            bool isDragging = ImGui.IsMouseDragging(ImGuiMouseButton.Left);

            float t = (value - min) / (max - min);

            float gamma = (float)Math.PI / 4.0f;
            float alpha = ((float)Math.PI - gamma) * t * 2.0f + gamma;

            if(isActive && isDragging)
            {
                Vector2 mousePosition = ImGui.GetIO().MousePos;
                alpha = (float)Math.Atan2(mousePosition.X - center.X, center.Y - mousePosition.Y) + (float)Math.PI;
                alpha = Math.Max(gamma, Math.Min(2.0f * (float)Math.PI - gamma, alpha));
                float val = 0.5f * (alpha - gamma) / ((float)Math.PI - gamma);

                if(snapSteps > 0)
                {
                    float stepSize = (max - min) / snapSteps;
                    float snappedValue = min + (float)Math.Round(val * snapSteps) * stepSize;
                    value = Math.Clamp(snappedValue, min, max);
                }
                else
                {
                    value = val * (max - min) + min;
                }

                valueChanged = true;
            }

            const float ANGLE_MIN = 3.141592f * 0.75f;
            const float ANGLE_MAX = 3.141592f * 2.25f;

            float angle = ANGLE_MIN + (ANGLE_MAX - ANGLE_MIN) * t;
            float angleCos = (float)Math.Cos(angle);
            float angle_sin = (float)Math.Sin(angle);
            float radiusInner = radiusOuter * 0.40f;
            drawList.AddCircleFilled(center, radiusOuter, ImGui.GetColorU32(ImGuiCol.FrameBg), 16);
            drawList.AddLine(new Vector2(center.X + angleCos * radiusInner, center.Y + angle_sin * radiusInner), new Vector2(center.X + angleCos * (radiusOuter - 2), center.Y + angle_sin * (radiusOuter - 2)), ImGui.GetColorU32(ImGuiCol.SliderGrabActive), 2.0f);
            drawList.AddCircleFilled(center, radiusInner, ImGui.GetColorU32(isActive ? ImGuiCol.FrameBgActive : isHovered ? ImGuiCol.FrameBgHovered : ImGuiCol.FrameBg), 16);
            //drawList->AddText(ImVec2(cursorPosition.x, cursorPosition.y + radiusOuter * 2 + style.ItemInnerSpacing.y), ImGui.GetColorU32(ImGuiCol_Text), label);

            if (isActive || isHovered)
            {
                ImGui.SetNextWindowPos(new Vector2(cursorPosition.X - style.WindowPadding.X, cursorPosition.Y - lineHeight - style.ItemInnerSpacing.Y - style.WindowPadding.Y));
                ImGui.BeginTooltip();
                ImGui.Text($"{label} {value:F3}");
                ImGui.EndTooltip();
            }

            return valueChanged;
        }

        const char ColorMarkerStart = '{';
        const char ColorMarkerEnd = '}';

        private static bool ProcessInlineHexColor(string text, ref System.Numerics.Vector4 color)
        {
            int hexCount = text.Length;
            if (hexCount == 6 || hexCount == 8)
            {
                string hex = text;
                if (uint.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out uint hexColor))
                {
                    color.X = (float)((hexColor & 0x00FF0000) >> 16) / 255.0f;
                    color.Y = (float)((hexColor & 0x0000FF00) >> 8) / 255.0f;
                    color.Z = (float)(hexColor & 0x000000FF) / 255.0f;
                    color.W = 1.0f;

                    if (hexCount == 8)
                    {
                        color.W = (float)((hexColor & 0xFF000000) >> 24) / 255.0f;
                    }

                    return true;
                }
            }

            return false;
        }

        public static void TextWithColors(string format, params object[] args)
        {
            string tempStr = format;
            
            // if(args != null)
            //     tempStr = string.Format(format, args);
            // else
            //tempStr = format;

            bool pushedColorStyle = false;
            int textStart = 0;
            int textCur = 0;
            while (textCur < tempStr.Length)
            {
                if (tempStr[textCur] == ColorMarkerStart)
                {
                    // Print accumulated text
                    if (textCur != textStart)
                    {
                        ImGui.TextUnformatted(tempStr.AsSpan(textStart, textCur - textStart));
                        ImGui.SameLine(0.0f, 0.0f);
                    }

                    // Process color code
                    int colorStart = textCur + 1;
                    do
                    {
                        ++textCur;
                    }
                    while (textCur < tempStr.Length && tempStr[textCur] != ColorMarkerEnd);

                    // Change color
                    if (pushedColorStyle)
                    {
                        ImGui.PopStyleColor();
                        pushedColorStyle = false;
                    }

                    Vector4 textColor = new Vector4();
                    if (ProcessInlineHexColor(tempStr.Substring(colorStart, textCur - colorStart), ref textColor))
                    {
                        ImGui.PushStyleColor(ImGuiCol.Text, textColor);
                        pushedColorStyle = true;
                    }

                    textStart = textCur + 1;
                }
                else if (tempStr[textCur] == '\n')
                {
                    // Print accumulated text and go to the next line
                    ImGui.TextUnformatted(tempStr.AsSpan(textStart, textCur - textStart));
                    textStart = textCur + 1;
                }

                ++textCur;
            }

            if (textCur != textStart)
            {
                ImGui.TextUnformatted(tempStr.AsSpan(textStart, textCur - textStart));
            }
            else
            {
                ImGui.NewLine();
            }

            if (pushedColorStyle)
            {
                ImGui.PopStyleColor();
            }
        }

        public static void ShowMemoryViewer(IntPtr memoryPointer, int memorySize, int columnsPerRow = 16)
        {
            if (memoryPointer == IntPtr.Zero || memorySize <= 0)
            {
                Console.WriteLine("Invalid memory pointer or size.");
                return;
            }

            if (columnsPerRow <= 0)
            {
                Console.WriteLine("Columns per row must be greater than 0.");
                return;
            }

            ImGui.Begin("MemoryHexViewer");

            for (int i = 0; i < memorySize; i += columnsPerRow)
            {
                int rowSize = Math.Min(columnsPerRow, memorySize - i);
                IntPtr currentPointer = memoryPointer + i;

                // Display the address
                ImGui.Text($"{currentPointer.ToInt64():X8}: ");

                // Display the hex values
                for (int j = 0; j < rowSize; j++)
                {
                    byte value = System.Runtime.InteropServices.Marshal.ReadByte(currentPointer + j);
                    ImGui.Text($"{value:X2} ");
                    ImGui.SameLine();
                }

                // Display the ASCII representation
                ImGui.Text(" | ");
                for (int j = 0; j < rowSize; j++)
                {
                    byte value = System.Runtime.InteropServices.Marshal.ReadByte(currentPointer + j);
                    char c = (char)value;
                    ImGui.Text($"{ (char.IsControl(c) ? '.' : c) }");
                    ImGui.SameLine();
                }

                ImGui.NewLine();
            }

            ImGui.End();
        }

        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr igFindWindowByName(string name);

        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr igFindWindowByID(uint id);

        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr igGetCurrentWindow();

        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr igGetCurrentContext();

        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr igGetInputTextState(uint id);

        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint igImHashStr(IntPtr data, ulong data_size, uint seed);

        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint ImGuiWindow_GetID_Str(IntPtr self, string str, string str_end);

        public static IntPtr FindWindowByName(string name)
        {
            
            return igFindWindowByName(name);
        }

        public static IntPtr FindWindowById(uint id)
        {
            return igFindWindowByID(id);
        }

        public static IntPtr GetCurrentWindow()
        {
            return igGetCurrentWindow();
        }

        public static IntPtr GetCurrentContext()
        {
            return igGetCurrentContext();
        }

        public static ImVector GetWindowsVectorFromContext(IntPtr context)
        {
            if(context == IntPtr.Zero)
                return default;
            return Marshal.PtrToStructure<ImVector>(IntPtr.Add(context, 5128));
        }

        public static unsafe ImGuiWindowFlags GetWindowFlags(IntPtr window)
        {
            if(window == IntPtr.Zero)
                return ImGuiWindowFlags.None;

            IntPtr flags = window + 0x14;
            int f = *(int*)flags;
            return (ImGuiWindowFlags)f;
        }

        public static unsafe uint GetWindowId(IntPtr window)
        {
            if(window == IntPtr.Zero)
                return 0;

            IntPtr address = IntPtr.Add(window, 16);
            return *(uint*)address;
        }

        public static unsafe Vector2 GetWindowScroll(IntPtr window)
        {
            if(window == IntPtr.Zero)
                return Vector2.Zero;

            IntPtr addrX = IntPtr.Add(window, 204);
            IntPtr addrY = IntPtr.Add(window, 208);
            float x = *(float*)addrX.ToPointer();
            float y = *(float*)addrY.ToPointer();
            return new Vector2(x, y);
        }

        public static unsafe Vector2 GetWindowScrollMax(IntPtr window)
        {
            if(window == IntPtr.Zero)
                return Vector2.Zero;

            IntPtr addrX = IntPtr.Add(window, 212);
            IntPtr addrY = IntPtr.Add(window, 216);
            float x = *(float*)addrX.ToPointer();
            float y = *(float*)addrY.ToPointer();
            return new Vector2(x, y);
        }

        public static unsafe IntPtr GetWindowParent(IntPtr window)
        {
            if(window == IntPtr.Zero)
                return IntPtr.Zero;

            IntPtr address = IntPtr.Add(window, 952); //ParentWindow
            //IntPtr address = IntPtr.Add(window, 960); //ParentWindowInBeginStack
            //IntPtr address = IntPtr.Add(window, 968); //RootWindow
            //IntPtr address = IntPtr.Add(window, 976); //RootWindowPopupTree
            //IntPtr address = IntPtr.Add(window, 984); //RootWindowDockTree
            //IntPtr address = IntPtr.Add(window, 992); //RootWindowForTitleBarHighlight
            //IntPtr address = IntPtr.Add(window, 1000); //RootWindowForNav
            //IntPtr address = IntPtr.Add(window, 1008); //ParentWindowForFocusRoute

            return address;
        }

        public static unsafe IntPtr GetWindowName(IntPtr window)
        {
            if(window == IntPtr.Zero)
                return IntPtr.Zero;

            IntPtr address = IntPtr.Add(window, 8);
            return address;
            //return Marshal.PtrToStringUTF8(address);
        }

        public static unsafe Vector2 GetWindowSize(IntPtr window)
        {
            if(window == IntPtr.Zero)
                return Vector2.Zero;

            IntPtr addrX = IntPtr.Add(window, 96);
            IntPtr addrY = IntPtr.Add(window, 100);
            float x = *(float*)addrX.ToPointer();
            float y = *(float*)addrY.ToPointer();
            return new Vector2(x, y);
        }



        public static uint ImHashStr(IntPtr str)
        {
            return igImHashStr(str, 0, 0);
        }

        public static IntPtr GetInputTextState(uint id)
        {
            return igGetInputTextState(id);
        }

        public static unsafe uint GetInputTextStateId(IntPtr state)
        {
            if(state == IntPtr.Zero)
            {
                Console.WriteLine("State is null");
                return 0;
            }
            IntPtr address = IntPtr.Add(state, 8);
            return *(uint*)address;
        }

        //IDStack
        
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ImGuiWindow
    {

    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ImVector
    {
        public int size;
        public int capacity;
        public IntPtr data;

        public IntPtr GetAt(int index)
        {
            if(index >= size)
                return IntPtr.Zero;
            
            // Calculate the address of the pointer at the given index
            // Assuming each pointer is of size IntPtr.Size
            IntPtr pointerAddress = IntPtr.Add(data, index * IntPtr.Size);
            
            // Read the pointer at that address
            return *(IntPtr*)pointerAddress;
        }
    }
}