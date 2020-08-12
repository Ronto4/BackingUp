using System;
using System.Collections.Generic;
using System.Text.Json;

namespace BackingUpConsole.CoreFunctions
{
    public class SettingsProperty
    {
        public enum UsedType
        {
            Integer,
            FloatingPoint,
            String,
            Array
        }
        public UsedType Type { get; set; }
        public UsedType? TypeOfArray { get; set; }
        private int IntegerValue = 0;
        private string StringValue = string.Empty;
        private double FloatingPointValue = 0.0f;
        private SettingsProperty[] ArrayValue = new SettingsProperty[0];
        public dynamic Value
        {
            get =>
                Type switch
                {
                    UsedType.Integer => IntegerValue,
                    UsedType.FloatingPoint => FloatingPointValue,
                    UsedType.String => StringValue,
                    UsedType.Array => ArrayValue,
                    _ => throw new NotImplementedException()
                };
            set
            {
                if (value is null)
                {
                    return;
                }
                switch (Type)
                {
                    case UsedType.Integer:
                        IntegerValue = value is JsonElement eli ? eli.GetInt32() : Convert.ToInt32(value);
                        break;
                    case UsedType.FloatingPoint:
                        FloatingPointValue = value is JsonElement elf ? elf.GetDouble() : Convert.ToDouble(value);
                        break;
                    case UsedType.String:
                        StringValue = value is JsonElement els ? els.GetString() : Convert.ToString(value);
                        break;
                    case UsedType.Array:
                        List<SettingsProperty> array = new List<SettingsProperty>();
                        var valarray = value is JsonElement ela ? ela.EnumerateArray() : value;
                        foreach (var elem in valarray)
                        {
                            if (elem is null)
                                continue;
                            if (elem is SettingsProperty sp)
                            {
                                array.Add(sp);
                                continue;
                            }
                            SettingsProperty entry;
                            try
                            {
                                entry = JsonSerializer.Deserialize<SettingsProperty>(elem.ToString());
                            }
                            catch
                            {
                                entry = new SettingsProperty(TypeOfArray ?? UsedType.Integer, elem);
                            }
                            array.Add(entry);
                        }
                        ArrayValue = array.ToArray();
                        break;
                }
            }
        }
        public static implicit operator string(SettingsProperty prop) => prop.Type == UsedType.String ? prop.Value : throw new InvalidCastException($"SettingsProperty is of type {prop.Type} and thus cannot be converted to string.");
        public static implicit operator int(SettingsProperty prop) => prop.Type == UsedType.Integer ? prop.Value : throw new InvalidCastException($"SettingsProperty is of type {prop.Type} and thus cannot be converted to int.");
        public static implicit operator double(SettingsProperty prop) => prop.Type == UsedType.FloatingPoint ? prop.Value : throw new InvalidCastException($"SettingsProperty is of type {prop.Type} and thus cannot be converted to float.");
        public static implicit operator SettingsProperty[](SettingsProperty prop) => prop.Type == UsedType.Array ? prop.Value : throw new InvalidCastException($"SettingsProperty is of type {prop.Type} and thus cannot be converted to Array.");
        public static implicit operator SettingsProperty(string value) => new SettingsProperty(UsedType.String, value);
        public static implicit operator SettingsProperty(int value) => new SettingsProperty(UsedType.Integer, value);
        public static implicit operator SettingsProperty(double value) => new SettingsProperty(UsedType.FloatingPoint, value);
        public static implicit operator SettingsProperty(SettingsProperty[] value) => new SettingsProperty(UsedType.Array, value, value.Length > 0 ? value[0].Type : UsedType.Integer);
        public SettingsProperty()
        {
        }
        public SettingsProperty(UsedType type, dynamic? initialValue = default, UsedType? arrayType = null)
        {
            Type = type;
            if (Type == UsedType.Array)
            {
                if (arrayType is null)
                    throw new ArgumentException("The arrayType must be specified whenever an array is used.");

                TypeOfArray = arrayType;
            }
            if ((initialValue is null) == false)
                Value = initialValue;
        }
    }
}