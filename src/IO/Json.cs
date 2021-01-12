using System;
using System.IO;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;
using Newtonsoft.Json;

namespace LangExtEffSample
{
    public interface JsonIO
    {
        T Deserialize<T>(Byte[] data);
        T Deserialize<T>(string str);
        string Serialize<T>(T obj);
    }

    public interface HasJson<RT>
        where RT : struct
    {
        Eff<RT, JsonIO> EffJson { get; }
    }

    public static class JsonEff<RT>
        where RT : struct, HasJson<RT>
    {
        public static Eff<RT, T> deserialize<T>(Byte[] json) =>
            default(RT).EffJson.Map(j => j.Deserialize<T>(json));
        public static Eff<RT, T> deserialize<T>(string json) =>
            default(RT).EffJson.Map(j => j.Deserialize<T>(json));
        public static Eff<RT, string> serialize<T>(T obj) =>
            default(RT).EffJson.Map(j => j.Serialize<T>(obj));
    }

    public class LiveJsonIO : JsonIO
    {
        public string Serialize<T>(T obj) =>
            JsonConvert.SerializeObject(obj);

        public static string SerializeFormatted<T>(T obj) =>
            JsonConvert.SerializeObject(obj, Formatting.Indented);

        public T Deserialize<T>(Byte[] data)
        {
            // try
            // {
            using (var stream = new MemoryStream(data))
            using (var reader = new StreamReader(stream, System.Text.Encoding.UTF8))
            using (var jsonTextReader = new JsonTextReader(reader))
            {
                // try
                // {
                return JsonSerializer.Create().Deserialize<T>(jsonTextReader);
                // }
                // catch (Exception e)
                // {
                //     return Fail<T>(Error.New(e));
                // }
            }
            // }
            // catch (Exception e)
            // {
            //     return Left<Error, T>(Error.New(e));
            // }
        }

        // public static Either<Error, T> Deserialize<T>(Stream stream)
        // {
        //     try
        //     {
        //         using (var sr = new StreamReader(stream))
        //         using (var jsonTextReader = new JsonTextReader(sr))
        //         {
        //             try
        //             {
        //                 return JsonSerializer.Create().Deserialize<T>(jsonTextReader);
        //             }
        //             catch (Exception e)
        //             {
        //                 return Left<Error, T>(Error.New(e));
        //             }
        //         }
        //     }
        //     catch (Exception e)
        //     {
        //         return Left<Error, T>(Error.New(e));
        //     }
        // }

        public T Deserialize<T>(string str)
        {
            // try
            // {
            return JsonConvert.DeserializeObject<T>(str);
            // }
            // catch (Exception e)
            // {
            //     return Left<Error, T>(Error.New(e));
            // }
        }
    }
}