using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using AceStdio.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AceStdio.Stream
{
    public class CryptoJsonConverter : JsonConverter<AceContent>
    {
        [DllImport("crypto.dll", EntryPoint = "decrypt", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr DllDecrypt(byte[] bytes, long length);

        [DllImport("crypto.dll", EntryPoint = "encrypt", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr DllEncrypt(byte[] bytes, long length);
        
        [DllImport("crypto32.dll", EntryPoint = "decrypt", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr DllDecrypt32(byte[] bytes, long length);

        [DllImport("crypto32.dll", EntryPoint = "encrypt", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr DllEncrypt32(byte[] bytes, long length);
        
        private static string Decrypt(string cipher)
        {
            var bytes = Encoding.UTF8.GetBytes(cipher);
            IntPtr ptr;
            try
            {
                ptr = Environment.Is64BitProcess ? DllDecrypt(bytes, bytes.Length) : DllDecrypt32(bytes, bytes.Length);
            }
            catch
            {
                throw new InvalidDataException("解密失败：输入文件格式错误");
            }
            var len = 0;
            for (; Marshal.ReadByte(ptr, len) != 0; ++len) { }
            var result = new byte[len];
            Marshal.Copy(ptr, result, 0, len);
            Marshal.FreeHGlobal(ptr);
            return Encoding.UTF8.GetString(result);
        }

        private static string Encrypt(string plain)
        {
            var bytes = Encoding.UTF8.GetBytes(plain);
            IntPtr ptr;
            try
            {
                ptr = Environment.Is64BitProcess ? DllEncrypt(bytes, bytes.Length) : DllEncrypt32(bytes, bytes.Length);
            }
            catch
            {
                throw new InvalidDataException("加密失败：输出文件格式错误");
            }

            var result = Marshal.PtrToStringAnsi(ptr);
            Marshal.FreeHGlobal(ptr);
            return result;
        }

        public override void WriteJson(JsonWriter writer, AceContent value, JsonSerializer serializer)
        {
            var plain = JsonConvert.SerializeObject(value);
            var cipher = Encrypt(plain);
            writer.WriteValue(cipher);
        }

        public override AceContent ReadJson(JsonReader reader, Type objectType, AceContent existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            var cipher = reader.Value?.ToString();
            if (cipher == null)
            {
                return null;
            }
            var plain = Decrypt(cipher);
            return JObject.Parse(plain).ToObject<AceContent>();
        }
    }
}
