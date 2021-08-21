using PlantUmlLanguageService.Control;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;

namespace PlantUmlLanguageService.Services
{
    /// <summary>
    ///
    /// </summary>
    public class DiagramService
    {

        /// <summary>
        /// The plant uml URL format
        /// </summary>
        private const string PlantUmlUrlFormat = "{2}{0}/{1}";

        /// <summary>
        /// Gets the image URL for source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="imageFormat">The image format.</param>
        /// <returns></returns>
        public static string GetImageUrlForSource(string source, string imageFormat)
        {
            if (!string.IsNullOrEmpty(source))
            {
                return string.Format(PlantUmlUrlFormat, imageFormat, Encode64(ZipStr(System.Web.HttpUtility.UrlDecode(System.Web.HttpUtility.UrlEncode(source).Replace("+", "%20")))), Global.BaseUrl);
            }
            return Constants.NoImageBase64;
        }

        /// <summary>
        /// Validates the diagram asynchronous.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <returns></returns>
        public static async Task<bool> ValidateDiagramAsync(string uri)
        {
            if (uri != Constants.NoImageBase64)
            {
                var client = new WebClient();
                WebHeaderCollection response;
                try
                {
                    string result = await client.DownloadStringTaskAsync(uri);
                    response = client.ResponseHeaders;
                }
                catch (WebException e)
                {
                    // in this case, the headers are in the exception.Response
                    response = e.Response.Headers;
                }
                try
                {
                    Global.Validator = new Validator(response.Get("X-PlantUML-Diagram-Description"), response.Get("X-PlantUML-Diagram-Error"), response.Get("X-PlantUML-Diagram-Error-Line"));
                }
                catch
                {
                    Global.Validator = new Validator();
                }
                if (Global.Warnings.Count > 0 && Global.Validator.ErrorContext != "error")
                {
                    Global.Validator = new Validator(Global.Validator.Description, Global.Warnings.ToArray());
                }
            } else
            {
                Global.Validator = new Validator() { ErrorContext = "none", Description = "no diagram" };
            }

            if (Global.Validator.ErrorContext == "error")
            {
                Global.DiagramUrl = Constants.NoImageBase64;
            };

            return true;
        }

        /// <summary>
        /// Encodes the specified source text.
        /// </summary>
        /// <param name="sourceText">The source text.</param>
        /// <returns></returns>
        private static string Encode64(byte[] sourceText)
        {
            string result = string.Empty;

            for (int i = 0; i < sourceText.Length; i += 3)
            {
                if (true == ((i + 2) == sourceText.Length))
                {
                    result += Append3bytes(sourceText[i], sourceText[i + 1], 0);
                }
                else if (true == ((i + 1) == sourceText.Length))
                {
                    result += Append3bytes(sourceText[i], 0, 0);
                }
                else
                {
                    result += Append3bytes(sourceText[i], sourceText[i + 1], sourceText[i + 2]);

                }

            }

            return result;

        }

        /// <summary>
        /// Appends the specified bytes.
        /// </summary>
        /// <param name="firstByte">The first byte.</param>
        /// <param name="secondByte">The second byte.</param>
        /// <param name="thirdByte">The third byte.</param>
        /// <returns></returns>
        private static string Append3bytes(int firstByte, int secondByte, int thirdByte)
        {
            string result = string.Empty;
            var firstCursor = firstByte >> 2;
            var secondCursor = ((firstByte & 0x3) << 4) | (secondByte >> 4);
            var thirdCursor = ((secondByte & 0xF) << 2) | (thirdByte >> 6);
            var forthCursor = thirdByte & 0x3F;

            result += Encode6bit(firstCursor & 0x3F);
            result += Encode6bit(secondCursor & 0x3F);
            result += Encode6bit(thirdCursor & 0x3F);
            result += Encode6bit(forthCursor & 0x3F);

            return result;

        }

        /// <summary>
        /// Encodes the specified bit.
        /// </summary>
        /// <param name="bit">The bit.</param>
        /// <returns></returns>
        private static string Encode6bit(int bit)
        {
            if (bit < 10)
            {
                return Microsoft.VisualBasic.Strings.Chr(48 + bit).ToString();
            }
            bit -= 10;

            if (bit < 26)
            {
                return Microsoft.VisualBasic.Strings.Chr(65 + bit).ToString();
            }
            bit -= 26;

            if (bit < 26)
            {
                return Microsoft.VisualBasic.Strings.Chr(97 + bit).ToString();
            }
            bit -= 26;

            if (bit == 0)
            {
                return "-";
            }

            if (bit == 1)
            {
                return "_";
            }

            return "?";

        }

        /// <summary>
        /// Decodes the specified URL text.
        /// </summary>
        /// <param name="urlText">The URL text.</param>
        /// <returns></returns>
        private static string Decode64(string urlText)
        {
            //int position = 0;

            List<byte> byteList = new List<byte>();
            int firstCursor = 0;
            int secondCursor = 0;
            int thirdCursor = 0;
            int fourthCursor = 0;

            for (int i = 0; i < urlText.Length; i += 4)
            {
                firstCursor = Decode6bit(urlText.Substring(i, 1));
                secondCursor = Decode6bit(urlText.Substring(i + 1, 1));
                thirdCursor = Decode6bit(urlText.Substring(i + 2, 1));
                fourthCursor = Decode6bit(urlText.Substring(i + 3, 1));

                byteList.Add((byte)((firstCursor << 2) | (secondCursor >> 4)));
                byteList.Add((byte)(((secondCursor & 0xF) << 4) | (thirdCursor >> 2)));
                byteList.Add((byte)(((thirdCursor & 0x3) << 6) | fourthCursor));

            }

            return UnZipStr(byteList.ToArray());

        }

        /// <summary>
        /// Decodes the specified character.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <returns></returns>
        private static int Decode6bit(string character)
        {
            if (true == (string.CompareOrdinal(character, "0") >= 0 && string.CompareOrdinal(character, "9") <= 0))
            {
                return Microsoft.VisualBasic.Strings.Asc(character.Substring(0, 1)) - 48;

            }
            else if (true == (string.CompareOrdinal(character, "A") >= 0 && string.CompareOrdinal(character, "Z") <= 0))
            {
                return Microsoft.VisualBasic.Strings.Asc(character.Substring(0, 1)) - 65 + 10;

            }
            else if (true == (string.CompareOrdinal(character, "a") >= 0 && string.CompareOrdinal(character, "z") <= 0))
            {
                return Microsoft.VisualBasic.Strings.Asc(character.Substring(0, 1)) - 97 + 36;

            }
            else if ((true == (character == "-")) || (true == (character == "_")))
            {
                return 63;

            }
            else
            {
                return 0;

            }

        }

        /// <summary>
        /// Zips the string.
        /// </summary>
        /// <param name="stringToZip">The string to zip.</param>
        /// <returns></returns>
        private static byte[] ZipStr(string stringToZip)
        {
            using (MemoryStream zipped = new MemoryStream())
            {
                using (DeflateStream gzip = new DeflateStream(zipped, CompressionMode.Compress))
                {
                    using (StreamWriter writer = new StreamWriter(gzip, System.Text.Encoding.UTF8))
                    {
                        writer.Write(stringToZip);

                    }

                }

                return zipped.ToArray();

            }

        }

        /// <summary>
        /// Unzips string.
        /// </summary>
        /// <param name="bytesToUnZip">The bytes to un zip.</param>
        /// <returns></returns>
        private static string UnZipStr(byte[] bytesToUnZip)
        {
            using (MemoryStream unzipped = new MemoryStream(bytesToUnZip))
            {
                using (DeflateStream gunzip = new DeflateStream(unzipped, CompressionMode.Decompress))
                {

                    using (StreamReader reader = new StreamReader(gunzip, System.Text.Encoding.ASCII))
                    {
                        return reader.ReadToEnd();

                    }

                }
            }

        }

    }

}
