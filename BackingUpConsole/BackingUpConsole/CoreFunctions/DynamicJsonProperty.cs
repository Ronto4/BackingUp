using System;
using System.Collections.Generic;
using System.Text.Json;

namespace BackingUpConsole.CoreFunctions
{
    public class DynamicJsonProperty
    {
        public enum UsedType
        {
            Integer,
            FloatingPoint,
            String,
            Array,
            Boolean
        }
        public UsedType Type { get; set; }
        public UsedType? TypeOfArray { get; set; }
        private int IntegerValue = 0;
        private string StringValue = string.Empty;
        private double FloatingPointValue = 0.0f;
        private DynamicJsonProperty[] ArrayValue = new DynamicJsonProperty[0];
        private bool BooleanValue = false;
        public dynamic Value
        {
            get =>
                Type switch
                {
                    UsedType.Integer => IntegerValue,
                    UsedType.FloatingPoint => FloatingPointValue,
                    UsedType.String => StringValue,
                    UsedType.Array => ArrayValue,
                    UsedType.Boolean => BooleanValue,
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
                        List<DynamicJsonProperty> array = new List<DynamicJsonProperty>();
                        var valarray = value is JsonElement ela ? ela.EnumerateArray() : value;
                        foreach (var elem in valarray)
                        {
                            if (elem is null)
                                continue;
                            if (elem is DynamicJsonProperty sp)
                            {
                                array.Add(sp);
                                continue;
                            }
                            DynamicJsonProperty entry;
                            try
                            {
                                entry = JsonSerializer.Deserialize<DynamicJsonProperty>(elem.ToString());
                            }
                            catch
                            {
                                entry = new DynamicJsonProperty(TypeOfArray ?? UsedType.Integer, elem);
                            }
                            array.Add(entry);
                        }
                        ArrayValue = array.ToArray();
                        break;
                    case UsedType.Boolean:
                        BooleanValue = value is JsonElement elb ? elb.GetBoolean() : Convert.ToBoolean(value);
                        break;
                }
            }
        }
        public static implicit operator string(DynamicJsonProperty prop) => prop.Type == UsedType.String ? prop.Value : throw new InvalidCastException($"DynamicJsonProperty is of type {prop.Type} and thus cannot be converted to string.");
        public static implicit operator int(DynamicJsonProperty prop) => prop.Type == UsedType.Integer ? prop.Value : throw new InvalidCastException($"DynamicJsonProperty is of type {prop.Type} and thus cannot be converted to int.");
        public static implicit operator double(DynamicJsonProperty prop) => prop.Type == UsedType.FloatingPoint ? prop.Value : throw new InvalidCastException($"DynamicJsonProperty is of type {prop.Type} and thus cannot be converted to float.");
        public static implicit operator DynamicJsonProperty[](DynamicJsonProperty prop) => prop.Type == UsedType.Array ? prop.Value : throw new InvalidCastException($"DynamicJsonProperty is of type {prop.Type} and thus cannot be converted to Array.");
        public static implicit operator bool(DynamicJsonProperty prop) => prop.Type == UsedType.Boolean ? prop.Value : throw new InvalidCastException($"DynamicJsonProperty is of type {prop.Type} and thus cannot be converted to Boolean.");
        public static implicit operator DynamicJsonProperty(string value) => new DynamicJsonProperty(UsedType.String, value);
        public static implicit operator DynamicJsonProperty(int value) => new DynamicJsonProperty(UsedType.Integer, value);
        public static implicit operator DynamicJsonProperty(double value) => new DynamicJsonProperty(UsedType.FloatingPoint, value);
        public static implicit operator DynamicJsonProperty(DynamicJsonProperty[] value) => new DynamicJsonProperty(UsedType.Array, value, value.Length > 0 ? value[0].Type : UsedType.Integer);
        public static implicit operator DynamicJsonProperty(bool value) => new DynamicJsonProperty(UsedType.Boolean, value);
        public DynamicJsonProperty()
        {
        }
        public DynamicJsonProperty(UsedType type, dynamic? initialValue = default, UsedType? arrayType = null)
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
        public override bool Equals(object? obj) => obj is DynamicJsonProperty prop ? Value == prop.Value : Value == obj;
        public override int GetHashCode() => Value.GetHashCode();
    }
}