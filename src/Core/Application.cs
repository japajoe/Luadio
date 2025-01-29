using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using LuaNET;
using LuaNET.Modules;
using MiniAudioEx;
using ImGuiColorTextEditNet;
using ImGuiNET;
using System.Numerics;

namespace Luadio
{
    public sealed class Application
    {
        private Window window;
        private AudioSource audioSource;
        private LuaState L;
        private Tokenizer tokenizer;
        private List<LuaField> fields;
        private AudioData audioData;
        private ConcurrentQueue<string> logQueue;
        private List<LuaModule> modules;
        private ImGuiConsole console;
        private ConcurrentQueue<LuaFieldInfo> fieldQueue;
        private TextEditor textEditor;

        public Application()
        {
            tokenizer = new Tokenizer();
            fields = new List<LuaField>();
            audioData = new AudioData(4096);
            logQueue = new ConcurrentQueue<string>();
            modules = new List<LuaModule>();
            console = new ImGuiConsole();
            fieldQueue = new ConcurrentQueue<LuaFieldInfo>();
            window = new Window(512, 512, "Luadio", WindowFlags.VSync);
        }

        public void Run()
        {
            window.Load += OnLoad;
            window.Closing += OnClosing;
            window.NewFrame += OnNewFrame;
            window.Create();
        }

        private void OnLoad()
        {
            L = Lua.NewState();

            if(!L.IsNull)
            {
                Lua.OpenLibs(L);
                modules.Add(new LuaNETModule());
                modules.Add(new LuadioModule());
                modules.Add(new WavetableModule());
                modules.Add(new OscillatorModule());

                for(int i = 0; i < modules.Count; i++)
                    modules[i].Initialize(L);

                LuadioModule.LogMessage += OnLogMessage;
            }

            audioSource = new AudioSource();
            audioSource.Read += OnAudioRead;

            textEditor = new TextEditor
            {
                AllText = Code.source,
                SyntaxHighlighter = new LuaStyleHighlighter()
            };
        }

        private void OnClosing()
        {
            audioSource.Stop();

            Lua.Close(L);
        }

        private void OnNewFrame(float deltaTime)
        {
            //ImGui.ShowStyleEditor();
            ShowMenu();
            ShowEditor();
            ShowInspector();
            ShowLog();
        }

        private bool showDialog = false;
        private ImFileDialogInfo dialog;

        private void ShowMenu()
        {
            if (ImGui.BeginMainMenuBar()) 
            {
                if (ImGui.BeginMenu("File")) 
                {
                    if (ImGui.MenuItem("Open")) 
                    {
                        showDialog = true;

                        dialog = new ImFileDialogInfo();
                        dialog.type = ImGuiFileDialogType.OpenFile;
                        dialog.title = "Open File";
                        dialog.fileName = "test.lua";
                        dialog.directoryPath = new System.IO.DirectoryInfo("./");
                    }
                    if (ImGui.MenuItem("Save")) 
                    {
                        showDialog = true;

                        dialog = new ImFileDialogInfo();
                        dialog.type = ImGuiFileDialogType.SaveFile;
                        dialog.title = "Save File";
                        dialog.fileName = "test.lua";
                        dialog.directoryPath = new System.IO.DirectoryInfo("./");
                    }
                    if (ImGui.MenuItem("Exit")) 
                    {
                        // Handle Exit action
                        // For example, you might want to set a flag to close the application                        
                    }
                    ImGui.EndMenu();
                }
                ImGui.EndMainMenuBar();
            }

            if(showDialog)
            {
                if(ImGuiFileDialog.FileDialog(ref showDialog, dialog))
                {
                    switch(dialog.type)
                    {
                        case ImGuiFileDialogType.OpenFile:
                            if(System.IO.File.Exists(dialog.fileName))
                            {
                                textEditor.AllText = System.IO.File.ReadAllText(dialog.fileName);
                            }
                            break;
                        case ImGuiFileDialogType.SaveFile:
                            System.IO.File.WriteAllText(dialog.fileName, textEditor.AllText);
                            OnLogMessage("Saved file as: " + dialog.fileName);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private void ShowEditor()
        {
            if(ImGui.Begin("Compile"))
            {
                audioData.SetLock(true);

                ImGui.PushStyleColor(ImGuiCol.PlotLines, new Vector4(174.0f / 255.0f, 112.0f / 255.0f, 1.0f, 1.0f));
                ImGui.PlotLines("##plot", ref audioData.Data[0], audioData.Length, 0, null, -1.0f, 1.0f, new Vector2(128, 64));
                ImGui.PopStyleColor(1);

                audioData.SetLock(false);

                ImGui.SameLine();

                if(ImGuiEx.Button("Play/Stop"))
                {
                    CompileAndRun();
                }

                textEditor.Render("TextEditor");

                ImGui.End();
            }
        }

        private void ShowInspector()
        {            
            if(ImGui.Begin("Inspector"))
            {
                for(int i = 0; i < fields.Count; i++)
                {
                    switch(fields[i].type)
                    {
                        case LuaFieldType.SliderFloat:
                        {
                            LuaFieldFloat field = (LuaFieldFloat)fields[i];
                            if(ImGui.SliderFloat(fields[i].name, ref field.value, field.min, field.max))
                            {
                                PushFieldToQueue(field);
                            }
                            break;
                        }
                        case LuaFieldType.SliderInt:
                        {
                            LuaFieldInt field = (LuaFieldInt)fields[i];
                            if(ImGui.SliderInt(fields[i].name, ref field.value, field.min, field.max))
                            {
                                PushFieldToQueue(field);
                            }
                            break;
                        }
                        case LuaFieldType.InputFloat:
                        {
                            LuaFieldFloat field = (LuaFieldFloat)fields[i];
                            if(ImGui.InputFloat(field.name, ref field.value))
                            {
                                field.value = Math.Clamp(field.value, field.min, field.max);
                                PushFieldToQueue(field);
                            }
                            break;
                        }
                        case LuaFieldType.InputInt:
                        {
                            LuaFieldInt field = (LuaFieldInt)fields[i];
                            if(ImGui.InputInt(field.name, ref field.value))
                            {
                                field.value = Math.Clamp(field.value, field.min, field.max);
                                PushFieldToQueue(field);
                            }
                            break;
                        }
                        case LuaFieldType.DragFloat:
                        {
                            LuaFieldFloat field = (LuaFieldFloat)fields[i];
                            if(ImGui.DragFloat(field.name, ref field.value))
                            {
                                field.value = Math.Clamp(field.value, field.min, field.max);
                                PushFieldToQueue(field);
                            }
                            break;
                        }
                        case LuaFieldType.DragInt:
                        {
                            LuaFieldInt field = (LuaFieldInt)fields[i];
                            if(ImGui.DragInt(field.name, ref field.value))
                            {
                                field.value = Math.Clamp(field.value, field.min, field.max);
                                PushFieldToQueue(field);
                            }
                            break;
                        }
                        case LuaFieldType.Checkbox:
                        {
                            LuaFieldBool field = (LuaFieldBool)fields[i];
                            if(ImGui.Checkbox(field.name, ref field.value))
                            {
                                PushFieldToQueue(field);
                            }
                            break;
                        }
                        default:
                            break;
                    }
                }
                ImGui.End();
            }
        }

        private void ShowLog()
        {
            if(ImGui.Begin("Log"))
            {
                if(ImGuiEx.Button("Clear"))
                {
                    console.ClearLog();
                }

                console.Draw("Log");
            }

            if(logQueue.Count > 0)
            {
                while(logQueue.Count > 0)
                {
                    if(logQueue.TryDequeue(out string message))
                    {
                        message = "[" + DateTime.Now.ToString() + "] " + message;
                        console.AddLog(message);
                        Console.WriteLine(message);
                    }
                }
            }
        }

        private void CompileAndRun()
        {
            if(!audioSource.IsPlaying)
            {
                string code = textEditor.AllText;

                var tokens = tokenizer.Tokenize(code);
                
                code = Compiler.ParseCode(tokens, code);
                fields = Compiler.GetFields(tokens);

                int result = Lua.DoString(L, code);

                if(result == 0)
                {
                    OnLogMessage("Compile ok");
                    audioSource.Play();
                }
                else
                {
                    string error = Lua.ToString(L, -1);
                    OnLogMessage("{FF0000}Error: {FFFFFF}" + error);
                    Lua.Pop(L, 1);
                    Console.WriteLine("Error: " + error);
                }
            }
            else
                audioSource.Stop();
        }

        private void OnAudioRead(AudioBuffer<float> framesOut, ulong frameCount, int channels)
        {
            Lua.GetGlobal(L, "on_audio_read");

            if(Lua.IsFunction(L, -1))
            {
                UpdateFields();

                Lua.PushLightUserData(L, framesOut.Pointer);
                Lua.PushInteger(L, framesOut.Length);
                Lua.PushInteger(L, channels);
                if(Lua.PCall(L, 3, 0, 0) == 0)
                {
                    audioData.SetData(framesOut);
                }
            }

            int top = Lua.GetTop(L);

            if(top > 0)
            {
                Lua.Pop(L, top);
            }
        }

        private void OnLogMessage(string message)
        {
            logQueue.Enqueue(message);
        }

        private void PushFieldToQueue(LuaField field)
        {
            //Don't queue data if audio isn't playing
            if(!audioSource.IsPlaying)
                return;

            LuaFieldInfo info = new LuaFieldInfo();
            info.type = field.type;
            info.name = field.name;
            
            switch(field.type)
            {
                case LuaFieldType.DragFloat:
                case LuaFieldType.InputFloat:
                case LuaFieldType.SliderFloat:
                {
                    LuaFieldFloat f = (LuaFieldFloat)field;
                    info.valueAsFloat = f.value;
                    break;
                }
                case LuaFieldType.DragInt:
                case LuaFieldType.InputInt:
                case LuaFieldType.SliderInt:
                {
                    LuaFieldInt f = (LuaFieldInt)field;
                    info.valueAsInt = f.value;
                    break;
                }
                case LuaFieldType.Checkbox:
                {
                    LuaFieldBool f = (LuaFieldBool)field;
                    info.valueAsBool = f.value;
                    break;
                }
            }

            fieldQueue.Enqueue(info);
        }

        private void UpdateFields()
        {
            if(fieldQueue.Count == 0)
                return;

            while(fieldQueue.TryDequeue(out LuaFieldInfo field))
            {
                switch(field.type)
                {
                    case LuaFieldType.DragFloat:
                    case LuaFieldType.InputFloat:
                    case LuaFieldType.SliderFloat:
                    {
                        Compiler.PushFloat(L, field.name, field.valueAsFloat);
                        break;
                    }
                    case LuaFieldType.DragInt:
                    case LuaFieldType.InputInt:
                    case LuaFieldType.SliderInt:
                    {
                        Compiler.PushInt(L, field.name, field.valueAsInt);
                        break;
                    }
                    case LuaFieldType.Checkbox:
                    {
                        Compiler.PushBool(L, field.name, field.valueAsBool);
                        break;
                    }
                }
            }
        }
    }
}