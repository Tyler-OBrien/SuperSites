﻿using System.Text.Json;
using System.Text.Json.Serialization;
using CloudflareSuperSites.Models.Configuration.Storage;

namespace CloudflareSuperSites.Extensions;

// Adapted from:
// https://stackoverflow.com/a/59785679
public class TypeDiscriminatorConverter : JsonConverter<IStorageConfiguration>
{
    private readonly Dictionary<string, Type> _types = new();

    public TypeDiscriminatorConverter()
    {
        var type = typeof(IStorageConfiguration);
        var typeList = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => type.IsAssignableFrom(p) && p.IsClass && !p.IsAbstract)
            .ToList();
        foreach (var derivedType in typeList)
        {
            var getDrivedType = derivedType.GetProperty("Type");

            if (getDrivedType == null)
                throw new InvalidOperationException($"Something went wrong getting property info of \"Type\" of type, {type.FullName}");

            var getTypeName = getDrivedType.GetValue(null) as string;
            _types.Add(getTypeName, derivedType);
        }
    }

    public override IStorageConfiguration Read(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options)
    {
        /*
        if (reader.TokenType == JsonTokenType.String)
        {
            reader.Read();
            /*
            using (var jsonDocument = JsonDocument.ParseValue(ref reader))
            {
                var enumerateObject = jsonDocument.RootElement.EnumerateObject();

                

                var typeProperty = enumerateObject.Current;

                if (typeProperty.Name != "Type")
                {
                    throw new JsonException();
                }

                Type type = null;
                if (_types.TryGetValue(typeProperty.Value.GetString(), out type) == false)
                {
                    throw new JsonException();
                }


                var jsonObject = jsonDocument.RootElement.GetRawText();
                var result = (IStorageConfiguration)JsonSerializer.Deserialize(jsonObject, type, options);

                return result;
            }
            
        }
        */
    if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException($"Tried to read config, expected start of object but got {reader.TokenType} at {reader.TokenStartIndex}, {reader.Position}, {reader.TokenType}");
        }

        using (var jsonDocument = JsonDocument.ParseValue(ref reader))
        {
            if (!jsonDocument.RootElement.TryGetProperty("Type", out var typeProperty))
            {
                  
                throw new JsonException($"HUMAN EXPLANATION: You're probably missing Type on one of your storage configuration blocks Computer Explanation: Missing type element on node at {reader.Position.GetInteger()}, {reader.TokenType} / {reader.TokenStartIndex} / {reader.TokenType}. We got stuck on {jsonDocument.RootElement.ToString()}.");
            }

            Type type;
            var tryGetString = typeProperty.GetString();

            if (tryGetString == null)
                throw new JsonException($"HUMAN EXPLANATION: You're probably missing Type on one of your storage configuration blocks, or messed up the formatting of it.. Computer Explanation: Couldn't read type property at {reader.Position.GetInteger()}, {reader.TokenType} / {reader.TokenStartIndex} / {reader.TokenType}. We got stuck on {jsonDocument.RootElement.ToString()}.");

            if (_types.TryGetValue(tryGetString, out type) == false)
            {
                throw new JsonException($"HUMAN EXPLANATION: You're probably missing Type on one of your storage configuration blocks, or messed up the formatting of it.. Computer Explanation: Couldn't read type property at {reader.Position.GetInteger()}, {reader.TokenType} / {reader.TokenStartIndex} / {reader.TokenType}. We got stuck on {jsonDocument.RootElement.ToString()}.");
            }

            var jsonObject = jsonDocument.RootElement.GetRawText();
            var result = (IStorageConfiguration)JsonSerializer.Deserialize(jsonObject, type, options)!;

            return result;
        }
    }

    public override void Write(Utf8JsonWriter writer, IStorageConfiguration value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, (object)value, options);
    }
}