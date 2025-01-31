
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
using LuaNET;
using LuaNET.Interop;
using LuaNET.Modules;

namespace Luadio
{
    public sealed class LuadioModule : LuaModule
    {
        public delegate void LogEvent(string message);
        public delegate void QueueAudioEvent(string filepath);

        public static event LogEvent LogMessage;
        public static event QueueAudioEvent QueueAudio;

        private string source = @"local ffi = require ('ffi')
local luanet = require('luanet')

local csharp = {}
csharp.Print = luanet.findMethod('Print', 'void (__cdecl*)(char*)')
csharp.Play = luanet.findMethod('Play', 'void (__cdecl*)(void)')
csharp.PlayFromFile = luanet.findMethod('PlayFromFile', 'void (__cdecl*)(char*)')

local luadio = {}

local function c_string(str)
    if type(str) == 'number' then
        str = tostring(str)
    end

    local c_str = ffi.new('char[?]', #str + 1)
    ffi.copy(c_str, str)
    return c_str
end

function luadio.print(message)
    local c_message = c_string(message)
    csharp.Print(c_message)
end

function luadio.play(...)
    local args = {...}
    local numArgs = #args
    if numArgs == 1 then
        local c_filepath = c_string(filepath)
        csharp.PlayFromFile(c_filepath)
    else
        csharp.Play()
    end
end

-- Override print function with our own
print = luadio.print

-- Metatable to prevent overwriting
local mt = {
    __newindex = function(table, key, value)
        error('Attempt to modify read-only method: ' .. key)
    end,
}

setmetatable(luadio, mt)

return luadio";

        public override void Initialize(LuaState L)
        {
            //Register the marked methods before loading the module
            RegisterExternalMethods();

            //Note that this file path assumes that test.lua is in the same directory as the executable
            LuaModuleLoader.RegisterFromString(L, "luadio", source);
        }

        [LuaExternalMethod]
        private static unsafe void Print(byte* text)
        {
            string s = new string((sbyte*)text);
            LogMessage?.Invoke(s);
        }

        [LuaExternalMethod]
        private static unsafe void Play()
        {
            QueueAudio?.Invoke(string.Empty);
        }

        [LuaExternalMethod]
        private static unsafe void PlayFromFile(byte* filepath)
        {
            string s = new string((sbyte*)filepath);
            QueueAudio?.Invoke(s);
        }
    }
}