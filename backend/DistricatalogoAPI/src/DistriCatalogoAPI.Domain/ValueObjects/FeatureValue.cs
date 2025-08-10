using System;
using System.Text.Json;
using DistriCatalogoAPI.Domain.Entities;

namespace DistriCatalogoAPI.Domain.ValueObjects
{
    public class FeatureValue
    {
        public string? RawValue { get; }
        public FeatureValueType Type { get; }
        
        public FeatureValue(string? rawValue, FeatureValueType type)
        {
            RawValue = rawValue;
            Type = type;
        }
        
        public bool GetBoolean()
        {
            if (Type != FeatureValueType.Boolean || string.IsNullOrEmpty(RawValue))
                return false;
                
            return bool.TryParse(RawValue, out var result) && result;
        }
        
        public string? GetString()
        {
            if (Type != FeatureValueType.String)
                return null;
                
            return RawValue;
        }
        
        public decimal? GetNumber()
        {
            if (Type != FeatureValueType.Number || string.IsNullOrEmpty(RawValue))
                return null;
                
            return decimal.TryParse(RawValue, out var result) ? result : null;
        }
        
        public T? GetJson<T>() where T : class
        {
            if (Type != FeatureValueType.Json || string.IsNullOrEmpty(RawValue))
                return null;
                
            try
            {
                return JsonSerializer.Deserialize<T>(RawValue);
            }
            catch
            {
                return null;
            }
        }
        
        public bool IsValid()
        {
            if (string.IsNullOrEmpty(RawValue))
                return true; // Null values are valid
                
            return Type switch
            {
                FeatureValueType.Boolean => bool.TryParse(RawValue, out _),
                FeatureValueType.String => true,
                FeatureValueType.Number => decimal.TryParse(RawValue, out _),
                FeatureValueType.Json => IsValidJson(RawValue),
                _ => false
            };
        }
        
        private static bool IsValidJson(string value)
        {
            try
            {
                JsonDocument.Parse(value);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}