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

namespace Luadio
{
    public static class Code
    {
        public static readonly string source = @"local ffi = require('ffi')
local luadio = require('luadio')
local wavetable = require('wavetable')
local oscillator = require('oscillator')

local table1 = wavetable.create_with_wave_type(wavetable.wavetype.sine, 1024)
local table2 = wavetable.create_with_wave_type(wavetable.wavetype.sine, 1024)
local osc1 = oscillator.new(oscillator.wavetype.sine, 440, 0.5, 44100)
local osc2 = oscillator.new(oscillator.wavetype.sine, 440, 0.5, 44100)

[SliderFloat(20, 880)]
frequency = 440.0

[SliderFloat(0.01, 10.0)]
lfo = 3.3

[SliderFloat(0.0, 1.0)]
lfoDepth = 1.0

[SliderFloat(0.0, 1.0)]
gain = 0.1

[Checkbox]
bypass = false

--Runs after compilation
function on_start()

end

--Runs when stop is clicked
function on_stop()

end

--Runs every frame
function on_update(deltaTime)

end

--Runs on separate thread
function on_audio_read(data, length, channels)
    if bypass == true then
        return
    end

    local pData = ffi.cast('float*', data)

    osc1:set_frequency(lfo)
    osc2:set_frequency(frequency)

    for i = 0, length - 1, channels do
        local sample = osc1:get_value()
        sample = osc2:get_modulated_value(lfoDepth * sample) * gain

        pData[i] = sample
        if channels == 2 then
            pData[i + 1] = sample
        end
    end
end";
    }
}