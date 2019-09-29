using Newtonsoft.Json;

namespace ShowMustNotGoOn.Core.Model.CallbackQuery
{
    public static class CallbackQueryDataSerializer
    {
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects
        };

        public static string Serialize(CallbackQueryData data)
        {
            return JsonConvert.SerializeObject(data, Formatting.Indented, Settings);
        }

        public static CallbackQueryData Deserialize(string data)
        {
            return JsonConvert.DeserializeObject<CallbackQueryData>(data, Settings);
        }
    }
}
