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
using LuaNET.Modules;

namespace Luadio
{
    public sealed class WavetableModule : LuaModule
    {
        private string source = @"local wavetable = {}
wavetable.__index = wavetable

-- Wave calculator interface
local wavecalculator = {}
wavecalculator.__index = wavecalculator

function wavecalculator:get_value(phase)
    error('get_value method not implemented')
end

-- Wavetable constructor with wavecalculator
function wavetable.new(calculator, length)
    local self = setmetatable({}, wavetable)
    self.data = {}
    self.length = length
    self.index = 0
    self.phase = 0
    self.phaseIncrement = 0
    self.TAU = 2 * math.pi

    local phaseIncrement = (2 * math.pi) / length

    for i = 0, length - 1 do
        self.data[i + 1] = calculator:get_value(i * phaseIncrement) -- Lua arrays are 1-indexed
    end

    return self
end

-- Get value method for frequency and sample rate
function wavetable:get_value(frequency, sampleRate)
    local phase = self.phase > 0.0 and (self.phase / self.TAU) or 0.0

    self.phaseIncrement = self.TAU * frequency / sampleRate
    self.phase = (self.phase + self.phaseIncrement) % self.TAU

    self.index = math.floor(phase * self.length)
    local t = phase * self.length - self.index

    local i1 = (self.index % self.length) + 1
    local i2 = ((self.index + 1) % self.length) + 1

    if i1 < 1 or i2 < 1 then
        return 0.0
    end

    local value1 = self.data[i1]
    local value2 = self.data[i2]
    return self:interpolate(value1, value2, t)
end

-- Interpolation method
function wavetable:interpolate(value1, value2, t)
    return value1 + (value2 - value1) * t
end

wavetable.wavetype = {}
wavetable.wavetype.sine = 1
wavetable.wavetype.square = 2
wavetable.wavetype.triangle = 3
wavetable.wavetype.saw = 4

-- Wave calculator implementations
local function create_wave_calculator(wave_type)
    local calculator = setmetatable({}, wavecalculator)

    function calculator:get_value(phase)
        if wave_type == wavetable.wavetype.sine then
            return math.sin(phase)
        elseif wave_type == wavetable.wavetype.square then
            return math.sign(math.sin(phase))
        elseif wave_type == wavetable.wavetype.triangle then
            phase = phase / (2 * math.pi)
            return 2.0 * math.abs(2 * (phase - 0.5)) - 1.0
        elseif wave_type == wavetable.wavetype.sawtooth then
            phase = phase / (2 * math.pi)
            return 2.0 * phase - 1.0
        else
            error('Unknown wave type: ' .. wave_type)
        end
    end

    return calculator
end

-- Factory function to create wavetable with specified wave type
function wavetable.create_with_wave_type(wave_type, bufferSize)
    local calculator = create_wave_calculator(wave_type)
    return wavetable.new(calculator, bufferSize)
end

-- Math sign function
function math.sign(x)
    if x > 0 then
        return 1
    elseif x < 0 then
        return -1
    else
        return 0
    end
end

-- Metatable to prevent overwriting
local mt = {
    __newindex = function(table, key, value)
        error('Attempt to modify read-only method: ' .. key)
    end,
}

setmetatable(wavetable, mt)

-- Expose the create_with_wave_type function directly
return setmetatable({
    create_with_wave_type = wavetable.create_with_wave_type
}, { __index = wavetable })";

        public override void Initialize(LuaState L)
        {
            //Register the marked methods before loading the module
            RegisterExternalMethods();

            //Note that this file path assumes that test.lua is in the same directory as the executable
            LuaModuleLoader.RegisterFromString(L, "wavetable", source);
        }
    }
}