
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

        public static event LogEvent LogMessage;

        private string source = @"local ffi = require ('ffi')
local luanet = require('luanet')

local csharp = {}
csharp.print = luanet.findMethod('Print', 'void (__cdecl*)(char*)')

local luadio = {}

function luadio.print(message)
    if type(message) == 'number' then
        message = tostring(message)
    end

    local c_message = ffi.new('char[?]', #message + 1)

    ffi.copy(c_message, message)

    csharp.print(c_message)
end

-- Override print function with our own
print = luadio.print

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
    }
}