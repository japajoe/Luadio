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
using ImGuiNET;

namespace Luadio
{
    public static class GUIStyle
    {
        private static ImFontPtr iconFont;

        public static ImFontPtr IconFont
        {
            get => iconFont;
        }

        public static void SetStyle()
        {
            var colors = ImGui.GetStyle().Colors;
            var bg = new Vector4(0.10f, 0.10f, 0.11f, 1.00f);
            var bgHovered = new Vector4(0.20f, 0.22f, 0.24f, 1.00f);
            var bgActive = new Vector4(0.23f, 0.26f, 0.29f, 1.00f);
            var frameBg = new Vector4(0.15f, 0.16f, 0.17f, 1.00f);
            var menuBg = new Vector4(0.10f, 0.11f, 0.11f, 1.00f);
            var text = new Vector4(0.86f, 0.87f, 0.88f, 1.00f);
            var grab = new Vector4(0.17f, 0.18f, 0.19f, 1.00f);
            var checkMark = new Vector4(0.26f, 0.59f, 0.98f, 1.00f);

            colors[(int)ImGuiCol.Text]                   = text;
            colors[(int)ImGuiCol.TextDisabled]           = new Vector4(0.50f, 0.50f, 0.50f, 1.00f);
            colors[(int)ImGuiCol.WindowBg]               = bg;
            colors[(int)ImGuiCol.ChildBg]                = bg;
            colors[(int)ImGuiCol.PopupBg]                = frameBg;
            colors[(int)ImGuiCol.Border]                 = bg;
            colors[(int)ImGuiCol.BorderShadow]           = new Vector4(0.14f, 0.16f, 0.18f, 1.00f);
            colors[(int)ImGuiCol.FrameBg]                = frameBg;
            colors[(int)ImGuiCol.FrameBgHovered]         = bgHovered;
            colors[(int)ImGuiCol.FrameBgActive]          = bgActive;
            colors[(int)ImGuiCol.TitleBg]                = bg;
            colors[(int)ImGuiCol.TitleBgActive]          = bg;
            colors[(int)ImGuiCol.TitleBgCollapsed]       = bg;
            colors[(int)ImGuiCol.MenuBarBg]              = menuBg;
            colors[(int)ImGuiCol.ScrollbarBg]            = bg;
            colors[(int)ImGuiCol.ScrollbarGrab]          = grab;
            colors[(int)ImGuiCol.ScrollbarGrabHovered]   = bgHovered;
            colors[(int)ImGuiCol.ScrollbarGrabActive]    = bgActive;
            colors[(int)ImGuiCol.CheckMark]              = checkMark;
            colors[(int)ImGuiCol.SliderGrab]             = bgHovered;
            colors[(int)ImGuiCol.SliderGrabActive]       = checkMark;
            colors[(int)ImGuiCol.Button]                 = bg;
            colors[(int)ImGuiCol.ButtonHovered]          = bgHovered;
            colors[(int)ImGuiCol.ButtonActive]           = bg;
            colors[(int)ImGuiCol.Header]                 = grab;
            colors[(int)ImGuiCol.HeaderHovered]          = bgHovered;
            colors[(int)ImGuiCol.HeaderActive]           = bgActive;
            colors[(int)ImGuiCol.Separator]              = bg;
            colors[(int)ImGuiCol.SeparatorHovered]       = bgHovered;
            colors[(int)ImGuiCol.SeparatorActive]        = bgActive;
            colors[(int)ImGuiCol.ResizeGrip]             = grab;
            colors[(int)ImGuiCol.ResizeGripHovered]      = bgHovered;
            colors[(int)ImGuiCol.ResizeGripActive]       = bgActive;
            colors[(int)ImGuiCol.TabHovered]             = menuBg;
            colors[(int)ImGuiCol.Tab]                    = menuBg;
            colors[(int)ImGuiCol.TabSelected]            = menuBg;
            colors[(int)ImGuiCol.TabSelectedOverline]    = menuBg;
            colors[(int)ImGuiCol.TabDimmed]              = menuBg;
            colors[(int)ImGuiCol.TabDimmedSelected]      = bg;
            colors[(int)ImGuiCol.TabDimmedSelectedOverline]  = menuBg;
            colors[(int)ImGuiCol.DockingPreview]         = new Vector4(0.26f, 0.59f, 0.98f, 0.70f);
            colors[(int)ImGuiCol.DockingEmptyBg]         = new Vector4(0.20f, 0.20f, 0.20f, 1.00f);
            colors[(int)ImGuiCol.PlotLines]              = text;
            colors[(int)ImGuiCol.PlotLinesHovered]       = bgActive;
            colors[(int)ImGuiCol.PlotHistogram]          = text;
            colors[(int)ImGuiCol.PlotHistogramHovered]   = bgActive;
            colors[(int)ImGuiCol.TableHeaderBg]          = menuBg;
            colors[(int)ImGuiCol.TableBorderStrong]      = menuBg;
            colors[(int)ImGuiCol.TableBorderLight]       = menuBg;
            colors[(int)ImGuiCol.TableRowBg]             = bg;
            colors[(int)ImGuiCol.TableRowBgAlt]          = menuBg;
            colors[(int)ImGuiCol.TextLink]               = checkMark;
            colors[(int)ImGuiCol.TextSelectedBg]         = bgActive;
            colors[(int)ImGuiCol.DragDropTarget]         = new Vector4(1.00f, 1.00f, 0.00f, 0.90f);
            colors[(int)ImGuiCol.NavHighlight]           = checkMark;
            colors[(int)ImGuiCol.NavWindowingHighlight]  = new Vector4(1.00f, 1.00f, 1.00f, 0.70f);
            colors[(int)ImGuiCol.NavWindowingDimBg]      = new Vector4(0.80f, 0.80f, 0.80f, 0.20f);
            colors[(int)ImGuiCol.ModalWindowDimBg]       = new Vector4(0.10f, 0.10f, 0.11f, 0.50f);

            var style = ImGui.GetStyle();
            style.FrameBorderSize = 0.0f;
            style.FrameRounding = 2.0f;
            style.WindowBorderSize = 1.0f;
            style.PopupBorderSize = 1.0f;
            style.ScrollbarSize = 12.0f;
            style.ScrollbarRounding = 2.0f;
            style.GrabMinSize = 7.0f;
            style.GrabRounding = 2.0f;
            style.TabRounding = 2.0f;

            style.WindowPadding = new Vector2(5.0f, 5.0f);
            style.FramePadding = new Vector2(4.0f, 3.0f);
            style.ItemSpacing = new Vector2(6.0f, 4.0f);
            style.ItemInnerSpacing = new Vector2(4.0f, 4.0f);
            style.TabBarBorderSize = 0;
            style.WindowBorderSize = 0;

            var io = ImGui.GetIO();
            io.Fonts.AddFontDefault();

            unsafe
            {
                fixed(byte *pFont = &OpenFontIcons.data[0])
                {
                    var nativeConfig = ImGuiNative.ImFontConfig_ImFontConfig();
                    // nativeConfig->OversampleH = 8;
                    // nativeConfig->OversampleV = 8;
                    // nativeConfig->RasterizerMultiply = 1.0f;
                    // nativeConfig->GlyphOffset = new Vector2(0);

                    nativeConfig->MergeMode = 1;
                    nativeConfig->GlyphMinAdvanceX = 13.0f; // Use if you want to make the icon monospaced

                    IntPtr data = new IntPtr(pFont);
                    iconFont = io.Fonts.AddFontFromMemoryTTF(data, OpenFontIcons.data.Length, 14, nativeConfig);
                }
            }

            io.Fonts.Build();
        }

        public static string GetSettings()
        {
            return settings;
        }

        private static readonly string settings = @"[Window][Debug##Default]
Pos=293,70
Size=788,499
Collapsed=1

[Window][Test]
Pos=7,8
Size=214,158
Collapsed=0

[Window][Compile]
Pos=0,19
Size=292,383
Collapsed=0
DockId=0x00000004,0

[Window][DockSpaceViewport_11111111]
Size=512,512
Collapsed=0

[Window][Dear ImGui Demo]
Pos=493,20
Size=550,680
Collapsed=0

[Window][Inspector]
Pos=294,19
Size=218,493
Collapsed=0
DockId=0x00000002,0

[Window][Log]
Pos=0,404
Size=292,108
Collapsed=0
DockId=0x0000000D,0

[Window][Waveform]
Size=941,104
Collapsed=0
DockId=0x00000005,0

[Window][Select Color]
Pos=60,60
Size=122,160
Collapsed=0

[Window][WindowOverViewport_11111111]
Pos=0,19
Size=512,493
Collapsed=0

[Window][Memory Viewer]
Pos=72,60
Size=604,506
Collapsed=0

[Window][MemoryHexViewer]
Pos=331,138
Size=631,389
Collapsed=0

[Window][Memory]
Pos=574,488
Size=569,213
Collapsed=0
DockId=0x00000008,0

[Window][Current Window Memory]
Pos=422,300
Size=577,212
Collapsed=0
DockId=0x0000000A,0

[Window][Current Context Memory]
Pos=888,559
Size=209,142
Collapsed=0
DockId=0x0000000C,0

[Window][Windows Memory]
Pos=34,411
Size=141,101
Collapsed=0
DockId=0x0000000E,0

[Window][##Inspector]
Pos=664,251
Size=416,244
Collapsed=0

[Window][Save File]
Pos=60,60
Size=740,410
Collapsed=0

[Window][Open File]
Pos=27,56
Size=584,404
Collapsed=0

[Docking][Data]
DockSpace             ID=0x7C6B3D9B Window=0xA87D555D Pos=0,19 Size=512,493 Split=X
  DockNode            ID=0x00000001 Parent=0x7C6B3D9B SizeRef=292,512 Split=Y
    DockNode          ID=0x00000004 Parent=0x00000001 SizeRef=705,383 HiddenTabBar=1 Selected=0x9DF580F2
    DockNode          ID=0x0000000F Parent=0x00000001 SizeRef=705,108 Split=X
      DockNode        ID=0x00000007 Parent=0x0000000F SizeRef=572,213 Split=X Selected=0x64F50EE5
        DockNode      ID=0x00000009 Parent=0x00000007 SizeRef=420,343 Split=X Selected=0x64F50EE5
          DockNode    ID=0x0000000B Parent=0x00000009 SizeRef=32,352 Split=X Selected=0x64F50EE5
            DockNode  ID=0x0000000D Parent=0x0000000B SizeRef=571,303 CentralNode=1 HiddenTabBar=1 Selected=0x64F50EE5
            DockNode  ID=0x0000000E Parent=0x0000000B SizeRef=456,303 Selected=0x4845CB53
          DockNode    ID=0x0000000C Parent=0x00000009 SizeRef=209,352 Selected=0x771B4EE9
        DockNode      ID=0x0000000A Parent=0x00000007 SizeRef=577,343 Selected=0x08A210A0
      DockNode        ID=0x00000008 Parent=0x0000000F SizeRef=569,213 Selected=0xB2805FDB
  DockNode            ID=0x00000002 Parent=0x7C6B3D9B SizeRef=218,512 HiddenTabBar=1 Selected=0xE7039252
DockSpace             ID=0x8B93E3BD Pos=0,0 Size=512,512 Split=Y HiddenTabBar=1 Selected=0x9DF580F2
  DockNode            ID=0x00000005 Parent=0x8B93E3BD SizeRef=1366,104 Selected=0xAB7F5F62
  DockNode            ID=0x00000006 Parent=0x8B93E3BD SizeRef=1366,406 CentralNode=1 HiddenTabBar=1

";
    }
}