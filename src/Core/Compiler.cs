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

using System.Collections.Generic;
using System.Runtime.InteropServices;
using LuaNET;

namespace Luadio
{
    public static class Compiler
    {
        private static Dictionary<string,LuaFieldType> numericTypes = new Dictionary<string, LuaFieldType>()
        {
            { "sliderfloat", LuaFieldType.SliderFloat },
            { "sliderint", LuaFieldType.SliderInt },
            { "inputfloat", LuaFieldType.InputFloat },
            { "knobfloat", LuaFieldType.KnobFloat },
            { "inputint", LuaFieldType.InputInt },
            { "dragfloat", LuaFieldType.DragFloat },
            { "dragint", LuaFieldType.DragInt },
            { "checkbox", LuaFieldType.Checkbox }
        };

        public static string ParseCode(List<Tokenizer.Token> tokens, string code)
        {           
            //When we insert comments, token positions increase by 2 characters each time
            int offset = 0;

            for(int i = 0; i < tokens.Count; i++)
            {
                if(tokens[i].Type != Tokenizer.TokenType.SquareBracketOpen)
                    continue;
                
                int tokenIndex = i;

                if(IsNumericAttributeTypeA(tokens, tokenIndex))
                {
                    int index = tokens[tokenIndex+0].Position + offset;
                    offset += 2;
                    code = code.Insert(index, "--");
                }
                else if(IsNumericAttributeTypeB(tokens, tokenIndex))
                {
                    int index = tokens[tokenIndex+0].Position + offset;
                    offset += 2;
                    code = code.Insert(index, "--");
                }
                else if(IsBooleanAttributeType(tokens, tokenIndex))
                {
                    int index = tokens[tokenIndex+0].Position + offset;
                    offset += 2;
                    code = code.Insert(index, "--");
                }
            }

            return code;
        }

        public static List<LuaField> GetFields(List<Tokenizer.Token> tokens)
        {
            List<LuaField> fields = new List<LuaField>();

            for(int i = 0; i < tokens.Count; i++)
            {
                if(tokens[i].Type != Tokenizer.TokenType.SquareBracketOpen)
                    continue;
                
                int tokenIndex = i;

                if(IsNumericAttributeTypeA(tokens, tokenIndex) || IsNumericAttributeTypeB(tokens, tokenIndex))
                {
                    string fieldType = tokens[tokenIndex+1].Value.ToLower();

                    LuaFieldType type = LuaFieldType.SliderFloat;

                    if(!numericTypes.ContainsKey(fieldType))
                        continue;
                    
                    type = numericTypes[fieldType];

                    switch(type)
                    {
                        case LuaFieldType.DragFloat:
                        case LuaFieldType.InputFloat:
                        case LuaFieldType.SliderFloat:
                        {
                            LuaFieldFloat field = new LuaFieldFloat();
                            field.type = type;

                            field.name = tokens[tokenIndex+8].Value;

                            if(!float.TryParse(tokens[tokenIndex+3].Value, out field.min))
                                continue;
                            
                            if(!float.TryParse(tokens[tokenIndex+5].Value, out field.max))
                                continue;
                            
                            if(!float.TryParse(tokens[tokenIndex+10].Value, out field.value))
                                continue;

                            fields.Add(field);
                            break;
                        }
                        case LuaFieldType.KnobFloat:
                        {
                            LuaFieldFloat field = new LuaFieldFloat();
                            field.type = type;

                            field.name = tokens[tokenIndex+10].Value;

                            if(!float.TryParse(tokens[tokenIndex+3].Value, out field.min))
                                continue;
                            
                            if(!float.TryParse(tokens[tokenIndex+5].Value, out field.max))
                                continue;

                            if(!int.TryParse(tokens[tokenIndex+7].Value, out field.steps))
                                continue;
                            
                            if(!float.TryParse(tokens[tokenIndex+12].Value, out field.value))
                                continue;

                            fields.Add(field);
                            break;
                        }
                        case LuaFieldType.DragInt:
                        case LuaFieldType.InputInt:
                        case LuaFieldType.SliderInt:
                        {
                            LuaFieldInt field = new LuaFieldInt();
                            field.type = type;

                            field.name = tokens[tokenIndex+8].Value;

                            if(!int.TryParse(tokens[tokenIndex+3].Value, out field.min))
                                continue;
                            
                            if(!int.TryParse(tokens[tokenIndex+5].Value, out field.max))
                                continue;
                            
                            if(!int.TryParse(tokens[tokenIndex+10].Value, out field.value))
                                continue;

                            fields.Add(field);
                            break;
                        }
                        default:
                            break;
                    }
                }
                else if(IsBooleanAttributeType(tokens, tokenIndex))
                {
                    string fieldType = tokens[tokenIndex+1].Value.ToLower();
                    
                    LuaFieldBool field = new LuaFieldBool();
                    field.type = LuaFieldType.Checkbox;
                    field.name = tokens[tokenIndex+3].Value;
                    
                    if(!bool.TryParse(tokens[tokenIndex+5].Value, out field.value))
                        continue;
                    
                    fields.Add(field);
                }
            }

            return fields;
        }

        private static bool IsNumericAttributeTypeA(List<Tokenizer.Token> tokens, int currentIndex)
        {
            if(currentIndex + 10 >= tokens.Count)
                return false;

            if( tokens[currentIndex+0].Type == Tokenizer.TokenType.SquareBracketOpen &&
                tokens[currentIndex+1].Type == Tokenizer.TokenType.Identifier &&
                tokens[currentIndex+2].Type == Tokenizer.TokenType.ParenthesisOpen &&
                tokens[currentIndex+3].Type == Tokenizer.TokenType.Number &&
                tokens[currentIndex+4].Type == Tokenizer.TokenType.Comma &&
                tokens[currentIndex+5].Type == Tokenizer.TokenType.Number &&
                tokens[currentIndex+6].Type == Tokenizer.TokenType.ParenthesisClose &&
                tokens[currentIndex+7].Type == Tokenizer.TokenType.SquareBracketClose)
            {
                string fieldType = tokens[currentIndex+1].Value.ToLower();
                return numericTypes.ContainsKey(fieldType);
            }

            return false;
        }

        private static bool IsNumericAttributeTypeB(List<Tokenizer.Token> tokens, int currentIndex)
        {
            if(currentIndex + 12 >= tokens.Count)
                return false;

            if( tokens[currentIndex+0].Type == Tokenizer.TokenType.SquareBracketOpen &&
                tokens[currentIndex+1].Type == Tokenizer.TokenType.Identifier &&
                tokens[currentIndex+2].Type == Tokenizer.TokenType.ParenthesisOpen &&
                tokens[currentIndex+3].Type == Tokenizer.TokenType.Number &&
                tokens[currentIndex+4].Type == Tokenizer.TokenType.Comma &&
                tokens[currentIndex+5].Type == Tokenizer.TokenType.Number &&
                tokens[currentIndex+6].Type == Tokenizer.TokenType.Comma &&
                tokens[currentIndex+7].Type == Tokenizer.TokenType.Number &&
                tokens[currentIndex+8].Type == Tokenizer.TokenType.ParenthesisClose &&
                tokens[currentIndex+9].Type == Tokenizer.TokenType.SquareBracketClose)
            {
                string fieldType = tokens[currentIndex+1].Value.ToLower();
                return numericTypes.ContainsKey(fieldType);
            }

            return false;
        }

        private static bool IsBooleanAttributeType(List<Tokenizer.Token> tokens, int currentIndex)
        {
            if(currentIndex + 6 >= tokens.Count)
                return false;

            if( tokens[currentIndex+0].Type == Tokenizer.TokenType.SquareBracketOpen &&
                tokens[currentIndex+1].Type == Tokenizer.TokenType.Identifier &&
                tokens[currentIndex+2].Type == Tokenizer.TokenType.SquareBracketClose)
            {
                string fieldType = tokens[currentIndex+1].Value.ToLower();
                if(fieldType == "checkbox")
                    return true;
            }

            return false;
        }

        public static void PushFloat(LuaState L, string fieldName, float value)
        {
            Lua.GetGlobal(L, fieldName); // Push someValue onto the stack
            if (Lua.IsNumber(L, -1)) 
            { 
                Lua.Pop(L, 1); // Remove the old value from the stack
                Lua.PushNumber(L, value); // Push the new value onto the stack
                Lua.SetGlobal(L, fieldName); // Set the global variable someValue
            } else 
            {
                Lua.Pop(L, 1); // Remove the old value from the stack
            }
        }

        public static void PushInt(LuaState L, string fieldName, int value)
        {
            Lua.GetGlobal(L, fieldName); // Push someValue onto the stack
            if (Lua.IsNumber(L, -1)) 
            { 
                Lua.Pop(L, 1); // Remove the old value from the stack
                Lua.PushInteger(L, value); // Push the new value onto the stack
                Lua.SetGlobal(L, fieldName); // Set the global variable someValue
            } else 
            {
                Lua.Pop(L, 1); // Remove the old value from the stack
            }
        }

        public static void PushBool(LuaState L, string fieldName, bool value)
        {
            Lua.GetGlobal(L, fieldName); // Push someValue onto the stack
            if (Lua.IsBoolean(L, -1)) 
            { 
                Lua.Pop(L, 1); // Remove the old value from the stack
                Lua.PushBoolean(L, value); // Push the new value onto the stack
                Lua.SetGlobal(L, fieldName); // Set the global variable someValue
            } else 
            {
                Lua.Pop(L, 1); // Remove the old value from the stack
            }
        }
    }

    public enum LuaFieldType
    {
        Checkbox,
        DragFloat,
        DragInt,
        InputFloat,
        InputInt,
        SliderFloat,
        SliderInt,
        KnobFloat
    }

    public class LuaField
    {
        public LuaFieldType type;
        public string name;
    }

    public class LuaFieldFloat : LuaField
    {
        public float value;
        public float min;
        public float max;
        public int steps;
    }

    public class LuaFieldInt : LuaField
    {
        public int value;
        public int min;
        public int max;
    }

    public class LuaFieldBool : LuaField
    {
        public bool value;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct LuaFieldInfo
    {
        [FieldOffset(0)]
        public int valueAsInt;
        [FieldOffset(0)]
        public float valueAsFloat;
        [FieldOffset(0)]
        public bool valueAsBool;
        [FieldOffset(4)]
        public LuaFieldType type;
        [FieldOffset(8)]
        public string name;
    }
}