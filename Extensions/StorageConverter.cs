﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using CloudflareWorkerBundler.Models.Configuration;

namespace CloudflareWorkerBundler.Extensions
{
    // Adapted from:
    // https://stackoverflow.com/a/59785679
    public class TypeDiscriminatorConverter : JsonConverter<IStorageConfiguration>
    {
        private readonly Dictionary<string, Type> _types = new Dictionary<string, Type>();

        public TypeDiscriminatorConverter()
        {
            var type = typeof(IStorageConfiguration);
            var typeList = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p) && p.IsClass && !p.IsAbstract)
                .ToList();
            foreach (var derivedType in typeList)
            {
                var getTypeName = derivedType.GetProperty("Type").GetValue(null) as string;
                _types.Add(getTypeName, derivedType);
            }
        }

        public override IStorageConfiguration Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            using (var jsonDocument = JsonDocument.ParseValue(ref reader))
            {
                if (!jsonDocument.RootElement.TryGetProperty("Type", out var typeProperty))
                {
                    throw new JsonException();
                }

                Type type = null;
                if (_types.TryGetValue(typeProperty.GetString(), out type) == false)
                {
                    throw new JsonException();
                }

                var jsonObject = jsonDocument.RootElement.GetRawText();
                var result = (IStorageConfiguration)JsonSerializer.Deserialize(jsonObject, type, options);

                return result;
            }
        }

        public override void Write(Utf8JsonWriter writer, IStorageConfiguration value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, (object)value, options);
        }
    }
}