using System.IO;
using System.Threading;
using System.Runtime.Serialization.Json;
using System.Text;

namespace HandBrakeCLIBatchEncode
{
    public static class PresetValidator
    {
        public static bool PresetExists(string presetPath)
        {
            return File.Exists(presetPath);
        }

        public static bool IsJsonText(string jsonText)
        {
            try
            {
                var jsonObject = JSONSerializer.DeSerialize(jsonText);
                return true;
            }
            catch { }

            return false;
        }

        public static bool ValidatePreset(string presetPath, string presetName, out string msg)
        {
            msg = "";
            bool isValid = true;

            BatchEncoder.WriteLineAndRecord("\n\n Validating preset...");

            if (!File.Exists(presetPath))
            {
                msg = "\n Preset file not found. A preset is required.";
                isValid = false;
            }

            string presetText = File.ReadAllText(presetPath);
            Thread.Sleep(200);

            if (string.IsNullOrEmpty(presetText))
            {
                msg = "\n Preset file empty. Not a valid preset file";
                return false;
            }

            if (!IsJsonText(presetText))
            {
                msg = "\n Preset context not valid json.";
                return false;
            }
    
            if (!presetText.Contains("\"" + presetName + "\""))
            {
                msg = "\n Assumed preset name not correct. The correct preset name is required.";
                return false;
            }

            // replace "PictureRotate": "0:0", - If this exists it won't work

            if (presetText.Contains("PictureRotate"))
            {
                try
                {
                    BatchEncoder.WriteLineAndRecord("\n Removing 'PictureRotate' element from preset.");

                    presetText = presetText.Replace("\"PictureRotate\": \"0:0\",\r\n", "");
                    File.WriteAllText(presetPath, presetText);

                    Thread.Sleep(200);
                }
                catch
                {
                    msg = "\n Unable to overwrite with corrected preset";
                    return false;
                }
            }

            return isValid;
        }
    }

    public static class JSONSerializer
    {
        /// <summary>
        /// DeSerializes an object from JSON
        /// </summary>
        public static object DeSerialize(string json)
        {
            using (var stream = new MemoryStream(Encoding.Default.GetBytes(json)))
            {
                var serializer = new DataContractJsonSerializer(typeof(object));
                return serializer.ReadObject(stream);
            }
        }
    }
}
