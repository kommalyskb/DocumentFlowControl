using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EnterpriseComponent.Helper
{
    public static class JsonSerializeObject
    {
        private static JsonSerializerOptions defaultOptions => new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
        public static string SerializeObject<T>(this T obj)
        {
            return JsonSerializer.Serialize<T>(obj, defaultOptions);
        }
    }
}
