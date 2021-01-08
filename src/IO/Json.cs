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
        // Either<Error, T> Deserialize<T>(Byte[] data);
        // Either<Error, T> Deserialize<T>(string str);
        T Deserialize<T>(Byte[] data);
        T Deserialize<T>(string str);
        string Serialize<T>(T obj);
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