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
using System.Collections.Concurrent;
using System.Collections.Generic;
using LuaNET;
using LuaNET.Modules;
using MiniAudioEx;
using ImGuiColorTextEditNet;
using ImGuiNET;
using System.Numerics;
using System.Threading;

namespace Luadio
{
    public sealed class Application
    {
        private class Graph
        {
            public enum Mode
            {
                Waveform,
                Frequency
            }

            public Mode mode;
            public bool setColor;
            public Vector4 color;

            public Graph()
            {
                mode = Mode.Waveform;
                setColor = false;
                color = new Vector4(174.0f / 255.0f, 112.0f / 255.0f, 1.0f, 1.0f);
            }
        }
        
        private Window window;
        private AudioSource audioSource;
        private LuaState L;
        private Tokenizer tokenizer;
        private List<LuaField> fields;
        private AudioData audioData;
        private FFTBuffer fftBuffer;
        private ConcurrentQueue<string> logQueue;
        private List<LuaModule> modules;
        private ImGuiConsole console;
        private ConcurrentQueue<LuaFieldInfo> fieldQueue;
        private TextEditor textEditor;
        private Graph graph;
        private Mutex luaMutex;

        public Application()
        {
            tokenizer = new Tokenizer();
            fields = new List<LuaField>();
            audioData = new AudioData(4096);
            fftBuffer = new FFTBuffer(4096);
            logQueue = new ConcurrentQueue<string>();
            modules = new List<LuaModule>();
            console = new ImGuiConsole();
            fieldQueue = new ConcurrentQueue<LuaFieldInfo>();
            graph = new Graph();
            luaMutex = new Mutex();
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
            // if(ImGui.Begin("Edit Style"))
            // {
            //     ImGui.ShowStyleEditor();
            //     ImGui.End();
            // }
            
            ShowMenu();
            ShowEditor();
            ShowInspector();
            ShowLog();
            OnUpdate(deltaTime);
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
                        window.Close();
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
                switch(graph.mode)
                {
                    case Graph.Mode.Frequency:
                        audioData.SetLock(true);
                        if(audioSource.IsPlaying)
                            fftBuffer.SetData(audioData.Data);
                        ImGui.PushStyleColor(ImGuiCol.PlotHistogram, graph.color);
                        ImGui.PlotHistogram("##plot_histogram", ref fftBuffer.Data[0], fftBuffer.Length, 0, null, 0.0f, 1.0f, new Vector2(128, 64));
                        ImGui.PopStyleColor(1);
                        audioData.SetLock(false);
                        break;
                    case Graph.Mode.Waveform:
                        audioData.SetLock(true);
                        ImGui.PushStyleColor(ImGuiCol.PlotLines, graph.color);
                        ImGui.PlotLines("##plot_lines", ref audioData.Data[0], audioData.Length, 0, null, -1.0f, 1.0f, new Vector2(128, 64));
                        ImGui.PopStyleColor(1);
                        audioData.SetLock(false);
                        break;
                    default:
                        break;
                }

                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.Text("Left click to change mode, right click to change color");
                    ImGui.EndTooltip();
                }

                if(ImGui.IsItemClicked(ImGuiMouseButton.Left))
                {
                    if(graph.mode == Graph.Mode.Waveform)
                        graph.mode = Graph.Mode.Frequency;
                    else
                        graph.mode = Graph.Mode.Waveform;
                }

                if(ImGui.IsItemClicked(ImGuiMouseButton.Right))
                {
                    graph.setColor = true;
                }

                if(graph.setColor)
                {
                    if(ImGui.Begin("Select Color", ref graph.setColor))
                    {
                        ImGui.ColorPicker4("Pick color", ref graph.color);
                        ImGui.End();
                    }
                }

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
                    Onstart();
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
            {
                OnStop();
                audioSource.Stop();
            }
        }

        private void Onstart()
        {
            luaMutex.WaitOne();

            Lua.GetGlobal(L, "on_start");

            if(Lua.IsFunction(L, -1))
                Lua.PCall(L, 0, 0, 0);

            int top = Lua.GetTop(L);

            if(top > 0)
                Lua.Pop(L, top);

            luaMutex.ReleaseMutex();
        }

        private void OnStop()
        {
            luaMutex.WaitOne();

            Lua.GetGlobal(L, "on_stop");

            if(Lua.IsFunction(L, -1))
                Lua.PCall(L, 0, 0, 0);

            int top = Lua.GetTop(L);

            if(top > 0)
                Lua.Pop(L, top);

            luaMutex.ReleaseMutex();
        }

        private void OnUpdate(float deltaTime)
        {
            if(!audioSource.IsPlaying)
                return;

            luaMutex.WaitOne();

            Lua.GetGlobal(L, "on_update");

            if(Lua.IsFunction(L, -1))
            {
                Lua.PushNumber(L, deltaTime);
                Lua.PCall(L, 1, 0, 0);
            }

            int top = Lua.GetTop(L);

            if(top > 0)
                Lua.Pop(L, top);

            luaMutex.ReleaseMutex();
        }

        private void OnAudioRead(AudioBuffer<float> framesOut, ulong frameCount, int channels)
        {
            luaMutex.WaitOne();

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
                Lua.Pop(L, top);

            luaMutex.ReleaseMutex();
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