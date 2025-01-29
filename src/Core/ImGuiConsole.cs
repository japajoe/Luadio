using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;

namespace Luadio
{
    public delegate bool ImGuiProcessCommandCallback(string text);

    public sealed unsafe class ImGuiConsole
    {
        public event ImGuiProcessCommandCallback ProcessCommand;
        public bool autoScroll = true;
        public bool scrollToBottom = false;
        public bool reclaim_focus = false;
        
        private string inputBuf = string.Empty;
        private const int inputBufMaxLength = 256;
        private readonly RingBuffer<string> items;
        //private readonly List<string> items;
        private readonly List<string> commands;
        private readonly List<string> history;
        private int historyPos;    // -1: new line, 0..History.Size-1 browsing history.       
        
        private bool showInputField = false;
        private ImGuiInputTextCallback textEditCallback;
        private static ImGuiConsole instance;

        public static ImGuiConsole Instance
        {
            get => instance;
        }

        public bool BypassLog
        {
            get;
            set;
        }

        public int HistoryPos
        {
            get
            {
                return historyPos;
            }
        }

        public ImGuiConsole()
        {
            BypassLog = false;

            textEditCallback = TextEditCallback;
            //items = new List<string>();
            items = new RingBuffer<string>();
            commands = new List<string>();
            history = new List<string>();

            ClearLog();

            historyPos = -1;
            commands.Add("/help");
            commands.Add("/history");
            commands.Add("/clear");
            autoScroll = true;
            scrollToBottom = false;

            instance = this;
            
            AddLog("Luadio {FFFF00}1.0.0");
        }

        public bool HasFocus
        {
            get
            {
                return reclaim_focus == true && showInputField == true;
            }
        }

        public void Focus()
        {
            reclaim_focus = true;
            showInputField = true;
        }

        public void Unfocus()
        {
            reclaim_focus = false;
            showInputField = false;
        }

        static int Stricmp(string str1, string str2)
        { 
            if(str1.ToUpper() == str2.ToUpper())
                return 0;
            
            return 1;
        }

        static int Strnicmp(string str1, string str2, int n) 
        { 
            int d = 0;
            int index = 0;
            str1 = str1.ToUpper();
            str2 = str2.ToUpper();

            while(n > 0 && d == 0)
            {
                d = str1[index] - str2[index];
                n--;
                index++;
            }

            return d; 
        }

        static string Strdup(string str)
        {
            return str;
        }

        static string Strtrim(string str)
        { 
            return str.TrimEnd();
        }

        public void ClearLog()
        {
            items.Clear();
        }

        public void AddLog(string text)
        {            
            items.Add(text);
        }

        ImGuiWindowFlags noWindowFlags = 0;

        void BeginNoWindowFlags()
        {
            noWindowFlags |= ImGuiWindowFlags.NoTitleBar;
            noWindowFlags |= ImGuiWindowFlags.NoResize;
            noWindowFlags |= ImGuiWindowFlags.NoMove;
            noWindowFlags |= ImGuiWindowFlags.NoScrollbar;	
            noWindowFlags |= ImGuiWindowFlags.NoBackground;	
            noWindowFlags |= ImGuiWindowFlags.NoScrollWithMouse;
        }

        void EndNoWindowFlags()
        {
            noWindowFlags = 0;
        }

        public void Draw(string title)
        {
            BeginNoWindowFlags();
            
            if (!ImGui.Begin(title, noWindowFlags))
            {
                ImGui.End();
                return;
            }

            bool copy_to_clipboard = false;
            
            Vector4 bg = ImGui.GetStyle().Colors[(int)ImGuiCol.FrameBg];
            ImGui.PushStyleColor(ImGuiCol.ChildBg, bg);

            //float footer_height_to_reserve = ImGui.GetStyle().ItemSpacing.Y + ImGui.GetFrameHeightWithSpacing(); // 1 separator, 1 input text
            //float footer_height_to_reserve = 0;
            //ImGui.BeginChild("ScrollingRegion", new Vector2(0, -footer_height_to_reserve), false, ImGuiWindowFlags.HorizontalScrollbar); // Leave room for 1 separator + 1 InputText
            ImGui.BeginChild("ScrollingRegion", Vector2.Zero, ImGuiChildFlags.None, ImGuiWindowFlags.HorizontalScrollbar); // Leave room for 1 separator + 1 InputText
            
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(4,1)); // Tighten spacing
            if (copy_to_clipboard)
                ImGui.LogToClipboard();
            for (int i = 0; i < items.Count; i++)
            {
                string item = items.GetAt(i);

                // Normally you would store more information in your item (e.g. make Items[] an array of structure, store color/type etc.)
                bool pop_color = false;
                
                if (item.Contains("[error]"))
                { 
                    ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 0.4f, 0.4f, 1.0f)); 
                    pop_color = true;                
                }
                else if (item.StartsWith("# "))
                { 
                    ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 0.8f, 0.6f, 1.0f)); 
                    pop_color = true; 
                }
                
                ImGuiEx.TextWithColors(item);
                
                if (pop_color)
                    ImGui.PopStyleColor();
            }

            if (copy_to_clipboard)
                ImGui.LogFinish();

            if (scrollToBottom || (autoScroll && ImGui.GetScrollY() >= ImGui.GetScrollMaxY()))
                ImGui.SetScrollHereY(1.0f);

            scrollToBottom = false;

            ImGui.PopStyleVar();
            ImGui.EndChild();

            ImGui.PopStyleColor(1);

            ImGui.End();

            EndNoWindowFlags();
        }

        void ExecCommand(string command_line)
        {
            if(!BypassLog)
            {
                if(command_line[0] != '/')
                    AddLog(command_line);
            }

            // Insert into history. First find match and delete it so it can be pushed to the back. This isn't trying to be smart or optimal.
            historyPos = -1;
            for (int i = history.Count-1; i >= 0; i--)
            {
                if (Stricmp(history[i], command_line) == 0)
                {
                    history.RemoveAt(i);
                    break;
                }
            }

            history.Add(command_line);

            // Process command
            if (Stricmp(command_line, "/CLEAR") == 0)
            {
                ClearLog();
            }
            else if (Stricmp(command_line, "/HELP") == 0)
            {
                AddLog("Commands:");
                for (int i = 0; i < commands.Count; i++)
                    AddLog(commands[i]);
            }
            else if (Stricmp(command_line, "/HISTORY") == 0)
            {
                int first = history.Count - 10;
                for (int i = first > 0 ? first : 0; i < history.Count; i++)
                    AddLog(history[i]);
            }
            else
            {
                if(ProcessCommand != null)
                {
                    if(!ProcessCommand(command_line))
                    {
                        AddLog("Unknown command: " + command_line + "\n");
                    }
                }
            }

            // On commad input, we scroll to bottom even if AutoScroll==false
            scrollToBottom = true;
        }

        private unsafe int TextEditCallback(ImGuiInputTextCallbackData *data)
        {
            switch (data->EventFlag)
            {
                case ImGuiInputTextFlags.CallbackHistory:
                {
                    int prev_history_pos = historyPos;

                    if (data->EventKey == ImGuiKey.UpArrow)
                    {
                        if (historyPos == -1)
                            historyPos = history.Count - 1;
                        else if (historyPos > 0)
                            historyPos--;
                    }
                    else if (data->EventKey == ImGuiKey.DownArrow)
                    {
                        if (historyPos != -1)
                            if (++historyPos >= history.Count)
                                historyPos = -1;
                    }

                    // A better implementation would preserve the data on the current input line along with cursor position.
                    if (prev_history_pos != historyPos)
                    {
                        string history_str = (historyPos >= 0) ? history[historyPos] : "";
                        var ptr = new ImGuiInputTextCallbackDataPtr(data);
                        ptr.DeleteChars(0, data->BufTextLen);
                        ptr.InsertChars(0, history_str);
                    }

                    break;
                }
                default:
                    break;
            }

            return 0;
        }
    }
}