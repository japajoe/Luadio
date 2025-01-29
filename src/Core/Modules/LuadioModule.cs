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