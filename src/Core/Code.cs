namespace Luadio
{
    public static class Code
    {
        public static readonly string source = @"local ffi = require('ffi')
local wavetable = require('wavetable')
local oscillator = require('oscillator')

local table1 = wavetable.create_with_wave_type(wavetable.wavetype.sine, 1024)
local table2 = wavetable.create_with_wave_type(wavetable.wavetype.sine, 1024)
local osc1 = oscillator.new(oscillator.wavetype.sine, 440, 0.5, 44100)
local osc2 = oscillator.new(oscillator.wavetype.sine, 440, 0.5, 44100)

local PI = 3.14159265359
local counter = 0

[SliderFloat(20, 880)]
frequency = 440.0

[SliderFloat(0.01, 10.0)]
lfo = 3.3

[SliderFloat(0.0, 1.0)]
lfoDepth = 1.0

[SliderFloat(0.0, 1.0)]
gain = 0.1

[Checkbox]
logCounter = false

function on_audio_read(data, length, channels)
    local pData = ffi.cast('float*', data)

    if logCounter == true then
        print('Counter {00FF00}' .. counter)
    end

    local sample = 0.0

    osc1:set_frequency(lfo)
    osc2:set_frequency(frequency)

    for i = 0, length - 1, channels do
        --local lfoValue = table1:get_value(lfo, 44100) * lfoDepth

        --sample = table2:get_value(frequency, 44100) * gain * (1 + lfoValue)
        local sample = osc1:get_value()
        sample = osc2:get_modulated_value(lfoDepth * sample) * gain

        pData[i] = sample
        if channels == 2 then
            pData[i + 1] = sample
        end

        counter = counter + 1
    end
end";
    }
}