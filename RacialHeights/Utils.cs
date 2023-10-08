using Newtonsoft.Json;

namespace RacialHeights
{
    public class Utils
    {
        public static void Log(string msg)
        {
            Console.WriteLine(msg);
        }

        public static void LogThrow(Exception e)
        {
            Console.WriteLine(e);
            throw e;
        }

        public static T FromJson<T>(string file)
        {
            var content = File.ReadAllText(file);
            var result = JsonConvert.DeserializeObject<T>(content, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            return result!;
        }
    }
}
