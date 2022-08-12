using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace LastTryTapo.Methods
{
    public class RequestCipher1
    {
        readonly private CipherMode _mode = CipherMode.CBC;
        readonly private PaddingMode _padding = PaddingMode.PKCS7;

        readonly private byte[] _KEY, _IV;

        /*static public void Main2()
        {
            var aes1 = Aes.Create();
            aes1.GenerateKey();
            aes1.GenerateIV();

            RequestCipher aesik = new RequestCipher(aes1.Key, aes1.IV);

            var encrypted = aesik.Encrypt("12345678");
            var decrypted = aesik.Decrypt(encrypted);

            Console.WriteLine(decrypted);
        }*/

        public RequestCipher1(byte[] KEY, byte[] IV)
        {
            _KEY = KEY;
            _IV = IV;
        }

        public string Encrypt(string json)
        {
            var aes = Aes.Create();

            var decryptor = aes.CreateDecryptor(_KEY, _IV);
            var data = Encoding.UTF8.GetBytes(json);

            /*var encryptedBytes = aes.EncryptCbc(Encoding.UTF8.GetBytes(json), _IV, _padding);*/

            var encryptedBytes = decryptor.TransformFinalBlock(data, 0, data.Length);
            string encrypted = Convert.ToBase64String(encryptedBytes);

            return encrypted.Replace("\r\n", "");
        }

        public string Decrypt(string base64)
        {
            var aes = Aes.Create();

            var decryptor = aes.CreateDecryptor(_KEY, _IV);
            var data = Convert.FromBase64String(base64);

            var decryptedBytes = decryptor.TransformFinalBlock(data, 0, data.Length);

            /*var decryptedBytes = aes.DecryptCbc(Convert.FromBase64String(base64), _IV, _padding);*/

            return Encoding.UTF8.GetString(decryptedBytes);
        }
    }
}
