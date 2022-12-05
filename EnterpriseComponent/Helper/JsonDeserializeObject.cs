using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EnterpriseComponent.Helper
{
    public static class JsonDeserializeObject
    {
        private static JsonSerializerOptions defaultOptions => new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
        public static T DeserializeObject<T>(this string json)
        {
            return JsonSerializer.Deserialize<T>(json, defaultOptions);
        }

        public static object DeserializeObject(this string json)
        {
            return ToObject(JToken.Parse(json));
        }

        private static object ToObject(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    return token.Children<JProperty>()
                                .ToDictionary(prop => prop.Name,
                                              prop => ToObject(prop.Value));

                case JTokenType.Array:
                    return token.Select(ToObject).ToList();

                default:
                    return ((JValue)token).Value;
            }
        }

    }
}
