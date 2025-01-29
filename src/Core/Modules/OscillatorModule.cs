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
    public sealed class OscillatorModule : LuaModule
    {
        private string source = @"-- oscillator.lua
local oscillator = {}
oscillator.__index = oscillator

-- wavetype Enum
oscillator.wavetype = {}
oscillator.wavetype.sine = 1
oscillator.wavetype.square = 2
oscillator.wavetype.triangle = 3
oscillator.wavetype.saw = 4

local TAU = 2 * math.pi

-- Constructor
function oscillator.new(type, frequency, amplitude, sampleRate)
    local self = setmetatable({}, oscillator)
    self.type = type
    self.frequency = frequency
    self.amplitude = amplitude
    self.phase = 0.0
    self.sampleRate = sampleRate
    self:set_phase_increment()
    self:set_wave_function()
    return self
end

-- Set wave function based on type
function oscillator:set_wave_function()
    if self.type == oscillator.wavetype.saw then
        self.waveFunc = self.get_saw_sample
    elseif self.type == oscillator.wavetype.sine then
        self.waveFunc = self.get_sine_sample
    elseif self.type == oscillator.wavetype.square then
        self.waveFunc = self.get_square_sample
    elseif self.type == oscillator.wavetype.triangle then
        self.waveFunc = self.get_triangle_sample
    end
end

-- Set phase increment based on frequency
function oscillator:set_phase_increment()
    self.phaseIncrement = TAU * self.frequency / self.sampleRate
end

function oscillator:set_type(type)
    self.type = type
    self:set_wave_function()
end

function oscillator:set_frequency(frequency)
    self.frequency = frequency
    self:set_phase_increment()
end

function oscillator:set_phase(phase)
    self.phase = phase
end

function oscillator:set_amplitude(amplitude)
    self.amplitude = amplitude
end

-- Reset phase
function oscillator:reset()
    self.phase = 0
end

-- Get current value
function oscillator:get_value()
    local result = self.waveFunc(self.phase)
    self.phase = (self.phase + self.phaseIncrement) % TAU
    return result * self.amplitude
end

-- Get value at a specific phase
function oscillator:get_value_at_phase(phase)
    return self.waveFunc(phase) * self.amplitude
end

-- Get modulated value
function oscillator:get_modulated_value(phase)
    local result = self.waveFunc(self.phase + phase)
    self.phase = (self.phase + self.phaseIncrement) % TAU
    return result * self.amplitude
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

-- Wave functions
function oscillator.get_saw_sample(phase)
    phase = phase / TAU
    return 2.0 * phase - 1.0
end

function oscillator.get_sine_sample(phase)
    return math.sin(phase)
end

function oscillator.get_square_sample(phase)
    return math.sign(math.sin(phase))
end

function oscillator.get_triangle_sample(phase)
    phase = phase / TAU
    return 2 * math.abs(2 * (phase - 0.5)) - 1
end

return oscillator";

        public override void Initialize(LuaState L)
        {
            //Register the marked methods before loading the module
            RegisterExternalMethods();

            //Note that this file path assumes that test.lua is in the same directory as the executable
            LuaModuleLoader.RegisterFromString(L, "oscillator", source);
        }
    }
}