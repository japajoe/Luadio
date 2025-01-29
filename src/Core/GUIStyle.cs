using System;
using System.Numerics;
using ImGuiNET;

namespace Luadio
{
    public static class GUIStyle
    {
        private static ImFontPtr defaultFont;

        public static ImFontPtr DefaultFont
        {
            get => defaultFont;
        }
        
        public static void SetStyle()
        {
            var style = ImGui.GetStyle();
            var colors = style.Colors;

            // Base Colors
            Vector4 bgColor = new Vector4(0.10f, 0.105f, 0.11f, 1.00f);
            Vector4 lightBgColor = new Vector4(0.15f, 0.16f, 0.17f, 1.00f);
            Vector4 panelColor = new Vector4(0.17f, 0.18f, 0.19f, 1.00f);
            Vector4 panelHoverColor = new Vector4(0.20f, 0.22f, 0.24f, 1.00f);
            Vector4 panelActiveColor = new Vector4(0.23f, 0.26f, 0.29f, 1.00f);
            Vector4 textColor = new Vector4(0.86f, 0.87f, 0.88f, 1.00f);
            Vector4 textDisabledColor = new Vector4(0.50f, 0.50f, 0.50f, 1.00f);
            Vector4 borderColor = new Vector4(0.14f, 0.16f, 0.18f, 1.00f);
            Vector4 buttonColor = bgColor;

            // Text
            colors[(int)ImGuiCol.Text] = textColor;
            colors[(int)ImGuiCol.TextDisabled] = textDisabledColor;

            // Windows
            colors[(int)ImGuiCol.WindowBg] = bgColor;
            colors[(int)ImGuiCol.ChildBg] = bgColor;
            colors[(int)ImGuiCol.PopupBg] = bgColor;
            colors[(int)ImGuiCol.Border] = borderColor;
            colors[(int)ImGuiCol.BorderShadow] = borderColor;

            // Headers
            colors[(int)ImGuiCol.Header] = panelColor;
            colors[(int)ImGuiCol.HeaderHovered] = panelHoverColor;
            colors[(int)ImGuiCol.HeaderActive] = panelActiveColor;

            // Buttons
            colors[(int)ImGuiCol.Button] = buttonColor;
            colors[(int)ImGuiCol.ButtonHovered] = panelHoverColor;
            colors[(int)ImGuiCol.ButtonActive] = buttonColor;

            // Frame BG
            colors[(int)ImGuiCol.FrameBg] = lightBgColor;
            colors[(int)ImGuiCol.FrameBgHovered] = panelHoverColor;
            colors[(int)ImGuiCol.FrameBgActive] = panelActiveColor;

            // Tabs
            colors[(int)ImGuiCol.Tab] = panelColor;
            colors[(int)ImGuiCol.TabHovered] = panelHoverColor;
            //colors[(int)ImGuiCol.TabUnfocused] = panelColor;
            colors[(int)ImGuiCol.TabDimmed] = panelColor;

            //colors[(int)ImGuiCol.TabActive] = bgColor;
            //colors[(int)ImGuiCol.TabUnfocusedActive] = bgColor;
            colors[(int)ImGuiCol.TabDimmedSelected] = bgColor;

            // Title
            colors[(int)ImGuiCol.TitleBg] = bgColor;
            colors[(int)ImGuiCol.TitleBgActive] = bgColor;
            colors[(int)ImGuiCol.TitleBgCollapsed] = bgColor;

            // Scrollbar
            colors[(int)ImGuiCol.ScrollbarBg] = bgColor;
            colors[(int)ImGuiCol.ScrollbarGrab] = panelColor;
            colors[(int)ImGuiCol.ScrollbarGrabHovered] = panelHoverColor;
            colors[(int)ImGuiCol.ScrollbarGrabActive] = panelActiveColor;

            // Checkmark
            colors[(int)ImGuiCol.CheckMark] = new Vector4(0.26f, 0.59f, 0.98f, 1.00f);

            // Slider
            colors[(int)ImGuiCol.SliderGrab] = panelHoverColor;
            colors[(int)ImGuiCol.SliderGrabActive] = panelActiveColor;

            // Resize Grip
            colors[(int)ImGuiCol.ResizeGrip] = panelColor;
            colors[(int)ImGuiCol.ResizeGripHovered] = panelHoverColor;
            colors[(int)ImGuiCol.ResizeGripActive] = panelActiveColor;

            // Separator
            colors[(int)ImGuiCol.Separator] = bgColor;
            colors[(int)ImGuiCol.SeparatorHovered] = panelHoverColor;
            colors[(int)ImGuiCol.SeparatorActive] = panelActiveColor;

            // Plot
            colors[(int)ImGuiCol.PlotLines] = textColor;
            colors[(int)ImGuiCol.PlotLinesHovered] = panelActiveColor;
            colors[(int)ImGuiCol.PlotHistogram] = textColor;
            colors[(int)ImGuiCol.PlotHistogramHovered] = panelActiveColor;

            // Text Selected BG
            colors[(int)ImGuiCol.TextSelectedBg] = panelActiveColor;

            // Modal Window Dim Bg
            colors[(int)ImGuiCol.ModalWindowDimBg] = new Vector4(0.10f, 0.105f, 0.11f, 0.5f);

            // Tables
            colors[(int)ImGuiCol.TableHeaderBg] = panelColor;
            colors[(int)ImGuiCol.TableBorderStrong] = borderColor;
            colors[(int)ImGuiCol.TableBorderLight] = borderColor;
            colors[(int)ImGuiCol.TableRowBg] = bgColor;
            colors[(int)ImGuiCol.TableRowBgAlt] = lightBgColor;





            // Styles
            style.FrameBorderSize = 0.0f;
            style.FrameRounding = 2.0f;
            style.WindowBorderSize = 1.0f;
            style.PopupBorderSize = 1.0f;
            style.ScrollbarSize = 12.0f;
            style.ScrollbarRounding = 2.0f;
            style.GrabMinSize = 7.0f;
            style.GrabRounding = 2.0f;
            style.TabBorderSize = 1.0f;
            style.TabRounding = 2.0f;

            // Reduced Padding and Spacing
            style.WindowPadding = new Vector2(5.0f, 5.0f);
            style.FramePadding = new Vector2(4.0f, 3.0f);
            style.ItemSpacing = new Vector2(6.0f, 4.0f);
            style.ItemInnerSpacing = new Vector2(4.0f, 4.0f);

            // Font Scaling
            var io = ImGui.GetIO();
            // io.FontGlobalScale = 0.95f;

            io.Fonts.AddFontDefault();
            // float baseFontSize = 18.0f;
            // float iconFontSize = baseFontSize * 2.0f / 3.0f;

            // unsafe
            // {
            //     fixed(byte *pFont = &Font.data[0])
            //     {
            //         var nativeConfig = ImGuiNative.ImFontConfig_ImFontConfig();
            //         nativeConfig->OversampleH = 8;
            //         nativeConfig->OversampleV = 8;
            //         nativeConfig->RasterizerMultiply = 1.0f;
            //         nativeConfig->GlyphOffset = new Vector2(0);

            //         IntPtr data = new IntPtr(pFont);
            //         defaultFont = io.Fonts.AddFontFromMemoryTTF(data, Font.data.Length, 14, nativeConfig);
            //     }
            // }
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