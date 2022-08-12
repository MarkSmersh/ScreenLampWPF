using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LastTryTapo.Models;
using LastTryTapo.Methods;
using System.Net;
using System.Security.Cryptography;
using System.Drawing;
using LastTryTapo.Helpers;
using System.Net.Http;

namespace LastTryTapo
{
    public class TapoConnection
    {
        readonly private string _email, _password, _deviceIP;

        private HttpClient _client = new HttpClient();
        private RequestCipher _requestCipher;
        private KeyPair _keyPair;

        public TapoConnection(string email, string password, string deviceIP)
        {
            _email = email;
            _password = password;
            _deviceIP = deviceIP;
        }

        private async Task<string> SecurePasstrough(string request, string cookie, string token = "")
        {
            object passtroughRequest = new BasicRequest
            {
                Method = "securePassthrough",
                Params = new BasicRequestParams
                {
                    Request = _requestCipher.Encrypt(request)
                }
            };

            Console.WriteLine(JsonConvert.SerializeObject(passtroughRequest));

            var response =
                await RequestWithHeader(
                    $"http://{_deviceIP}/app?token={token}",
                    JsonConvert.SerializeObject(passtroughRequest),
                    cookie);

            string decryptedData = _requestCipher.Decrypt(response);

            return decryptedData;
        }

        public async Task<DeviceInfo> LoginWithIP()
        {
            DeviceInfo deviceInfo = await Handshake();

            _requestCipher = new RequestCipher(deviceInfo.Key, deviceInfo.IV);

            object loginDeviceRequest = new LoginDeviceRequest
            {
                Method = "login_device",
                RequestTimeMils = 0,
                Params = new LoginDeviceRequestParams
                {
                    Username = Convert.ToBase64String(Encoding.UTF8.GetBytes(ShaDigest(_email))),
                    Password = Convert.ToBase64String(Encoding.UTF8.GetBytes(_password)),
                }
            };

            Console.WriteLine(JsonConvert.SerializeObject(loginDeviceRequest));

            string responseJson = await SecurePasstrough(
                JsonConvert.SerializeObject(loginDeviceRequest),
                deviceInfo.SessionId);

            Console.WriteLine(responseJson);

            LoginDeviceResponseDecrypted response = JsonConvert.DeserializeObject<LoginDeviceResponseDecrypted>(responseJson);

            Console.WriteLine(response.Result.Token);

            deviceInfo.Token = response.Result.Token;

            return deviceInfo;
        }

        public async Task setColour(string hex, DeviceInfo deviceInfo)
        {
            var colorRGB = ColorTranslator.FromHtml(hex); // hex

            Console.WriteLine($"{colorRGB.R} {colorRGB.G} {colorRGB.B}");

            var colorHSL = ColorsToHSL.FromRGBToHSL(colorRGB.R, colorRGB.G, colorRGB.B);

            Console.WriteLine(colorHSL.Hue);
            Console.WriteLine(colorHSL.Saturation);
            Console.WriteLine(colorHSL.Lightness);

            var hue = Convert.ToInt16(colorHSL.Hue);
            var saturation = Convert.ToInt16(colorHSL.Saturation * 100);
            var brightness = Convert.ToInt16(colorHSL.Lightness * 100);

            Console.WriteLine($"{hue} {saturation} {brightness}");

            SetDeviceInfoColorRequest request;

            Console.WriteLine(saturation < 50);
            Console.WriteLine(saturation < 50);

            if (hue < 5 && saturation < 20)
            {
                request = new SetDeviceInfoColorRequest
                {
                    Method = "set_device_info",
                    Params = new SetDeviceInfoColorRequestParams
                    {
                        Brightness = brightness,
                        ColorTemp = 4500
                    }
                };
            } else
            {
                request = new SetDeviceInfoColorRequest
                {
                    Method = "set_device_info",
                    Params = new SetDeviceInfoColorRequestParams
                    {
                        Hue = hue,
                        Saturation = saturation,
                        Brightness = brightness,
                        ColorTemp = 0
                    }
                };
            }

            string json = JsonConvert.SerializeObject(request, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            Console.WriteLine(json);

            var responseJson = await SecurePasstrough(
                json,
                deviceInfo.SessionId,
                deviceInfo.Token);
        }

        private string ShaDigest(string data)
        {
            using var sha1 = SHA1.Create();
            return Convert.ToHexString(sha1.ComputeHash(Encoding.UTF8.GetBytes(data)));
        }

        private async Task<DeviceInfo> Handshake()
        {
            _keyPair = new KeyPair();

            object handshake = new HandshakeRequest
            {
                Method = "handshake",
                Params = new HandshakeRequestParams
                {
                    Key = _keyPair.GetPublicKeyPem()
                },
                RequestTimeMils = 0
            };
            //Console.WriteLine();

            var httpContent = new StringContent(JsonConvert.SerializeObject(handshake), Encoding.UTF8, "application/json");
            Uri uri = new Uri($"http://{_deviceIP}/app");

            HttpResponseMessage response = await _client.PostAsync($"http://{_deviceIP}/app", httpContent);

            CookieContainer cookies = new CookieContainer();

            foreach (var cookieHeader in response.Headers.GetValues("Set-Cookie"))
            {
                cookies.SetCookies(uri, cookieHeader);
            }
            string cookieValue = cookies.GetCookies(uri).FirstOrDefault(c => c.Name == "TP_SESSIONID")?.Value;

            Console.WriteLine("Data: " + await response.Content.ReadAsStringAsync());
            Console.WriteLine("TP_SESSIONID: " + cookieValue);

            var dataResponse = JsonConvert.DeserializeObject<HandshakeResponse>(
                await response.Content.ReadAsStringAsync());

            byte[] deviceKeyIvBytes = _keyPair.Decrypt(dataResponse.Result.Key); /*dataResponse.Result.Key*/

            byte[] KeyArray = new byte[16];
            byte[] IVArray = new byte[16];

            Console.WriteLine(Encoding.UTF8.GetString(deviceKeyIvBytes));

            Array.Copy(deviceKeyIvBytes, 0, KeyArray, 0, 16);
            Array.Copy(deviceKeyIvBytes, 16, IVArray, 0, 16);

            Console.WriteLine("Key: " + Convert.ToHexString(KeyArray));
            Console.WriteLine("IV: " + Convert.ToHexString(IVArray));

            Console.WriteLine(KeyArray.Length.ToString() + IVArray.Length.ToString());


            DeviceInfo deviceInfo = new DeviceInfo
            {
                Key = KeyArray,
                IV = IVArray,
                SessionId = cookieValue
            };

            return deviceInfo;
        }

        private async Task<string> RequestWithHeader(string uri, string json, string cookie)
        {
            string responseJson;

            var handler = new HttpClientHandler();

            handler.AutomaticDecompression = ~DecompressionMethods.None;

            using (var httpClient = new HttpClient(handler))
            {
                using (var request = new HttpRequestMessage(new HttpMethod("POST"), uri))
                {
                    request.Headers.TryAddWithoutValidation("Cookie", $"TP_SESSIONID={cookie}");

                    request.Content = new StringContent(json);

                    var responseRaw = await httpClient.SendAsync(request);
                    responseJson = await responseRaw.Content.ReadAsStringAsync(); //pohuy chto 2 raza await
                }
            }

            Console.WriteLine(responseJson);

            LoginDeviceResponse response = JsonConvert.DeserializeObject<LoginDeviceResponse>(responseJson);

            return response.Result.Response;
        }
    }
}
