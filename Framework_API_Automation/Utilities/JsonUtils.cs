using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework_API_Automation.Utilities
{

    public static class JsonUtils
    {
        private static readonly string BASE_PATH = "YourBasePathHere"; // Set your base path
        private static readonly TimeSpan TOLERANCE = TimeSpan.FromSeconds(5);
        private static readonly string TIMESTAMP_FORMAT = "yyyy-MM-ddTHH:mm:ss.fffZ";

        public static string PrettyPrintJson(string jsonString)
        {
            var parsedJson = JToken.Parse(jsonString);
            return parsedJson.ToString(Formatting.Indented);
        }

        public static JToken ReadExpectedJsonBodyFromFile(string folderName, string fileName)
        {
            string filePath = Path.Combine(BASE_PATH, folderName, fileName);
            string jsonContent = File.ReadAllText(filePath);
            return JToken.Parse(jsonContent);
        }

        public static bool CompareKeys(string responseBody, JToken expectedJson)
        {
            try
            {
                var responseJson = JToken.Parse(responseBody);
                return CompareJsonKeys(responseJson, expectedJson, "");
            }
            catch (JsonException ex)
            {
                Console.Error.WriteLine(ex.Message);
                return false;
            }
        }

        private static bool CompareJsonKeys(JToken responseJson, JToken expectedJson, string path)
        {
            if (expectedJson.Type != JTokenType.Object || responseJson.Type != JTokenType.Object)
                return false;

            foreach (var property in expectedJson.Children<JProperty>())
            {
                var responseValue = responseJson[property.Name];
                string currentPath = string.IsNullOrEmpty(path) ? property.Name : $"{path}.{property.Name}";

                if (responseValue == null)
                {
                    Console.Error.WriteLine($"Missing field: {currentPath}");
                    return false;
                }

                if (property.Value.Type == JTokenType.Object && responseValue.Type == JTokenType.Object)
                {
                    if (!CompareJsonKeys(responseValue, property.Value, currentPath))
                        return false;
                }
            }
            return true;
        }

        public static bool CompareExpectedKeysAndValues(string responseBody, JToken expectedJson, params string[] fieldsToIgnore)
        {
            var ignoreSet = new HashSet<string>(fieldsToIgnore);
            try
            {
                var responseJson = JToken.Parse(responseBody);
                if (responseJson.Type == JTokenType.Array && expectedJson.Type == JTokenType.Array)
                {
                    return CompareJsonArrays((JArray)responseJson, (JArray)expectedJson, "", ignoreSet);
                }
                else
                {
                    return CompareJsonNodes((JObject)responseJson, (JObject)expectedJson, "", ignoreSet);
                }
            }
            catch (JsonException ex)
            {
                Console.Error.WriteLine(ex.Message);
                return false;
            }
        }

        private static bool CompareJsonNodes(JObject responseJson, JObject expectedJson, string path, HashSet<string> fieldsToIgnore)
        {
            foreach (var property in expectedJson.Properties())
            {
                string currentPath = string.IsNullOrEmpty(path) ? property.Name : $"{path}.{property.Name}";
                if (fieldsToIgnore.Contains(property.Name))
                    continue;

                if (!responseJson.TryGetValue(property.Name, out var responseValue))
                {
                    Console.Error.WriteLine($"Missing field: {currentPath}");
                    return false;
                }

                var expectedValue = property.Value;

                if (expectedValue.Type == JTokenType.Object && responseValue.Type == JTokenType.Object)
                {
                    if (!CompareJsonNodes((JObject)responseValue, (JObject)expectedValue, currentPath, fieldsToIgnore))
                        return false;
                }
                else if (expectedValue.Type == JTokenType.Array && responseValue.Type == JTokenType.Array)
                {
                    if (!CompareJsonArrays((JArray)responseValue, (JArray)expectedValue, currentPath, fieldsToIgnore))
                        return false;
                }
                else if (!JToken.DeepEquals(expectedValue, responseValue))
                {
                    Console.Error.WriteLine($"Field mismatch at {currentPath} - Expected: {expectedValue}, Found: {responseValue}");
                    return false;
                }
            }
            return true;
        }

        private static bool CompareJsonArrays(JArray responseArray, JArray expectedArray, string path, HashSet<string> fieldsToIgnore)
        {
            if (responseArray.Count != expectedArray.Count)
            {
                Console.Error.WriteLine($"Array size mismatch at {path} - Expected: {expectedArray.Count}, Found: {responseArray.Count}");
                return false;
            }

            for (int i = 0; i < responseArray.Count; i++)
            {
                var currentPath = $"{path}[{i}]";
                var responseElement = responseArray[i];
                var expectedElement = expectedArray[i];

                if (responseElement.Type == JTokenType.Object && expectedElement.Type == JTokenType.Object)
                {
                    if (!CompareJsonNodes((JObject)responseElement, (JObject)expectedElement, currentPath, fieldsToIgnore))
                        return false;
                }
                else if (!JToken.DeepEquals(responseElement, expectedElement))
                {
                    Console.Error.WriteLine($"Array element mismatch at {currentPath} - Expected: {expectedElement}, Found: {responseElement}");
                    return false;
                }
            }
            return true;
        }

        private static bool CompareTimestamps(string expected, string actual, string path)
        {
            try
            {
                var expectedTime = DateTime.ParseExact(expected, TIMESTAMP_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
                var actualTime = DateTime.ParseExact(actual, TIMESTAMP_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);

                if (Math.Abs((expectedTime - actualTime).TotalSeconds) > TOLERANCE.TotalSeconds)
                {
                    Console.Error.WriteLine($"Timestamp mismatch at {path} - Expected: {expected}, Found: {actual}");
                    return false;
                }
            }
            catch (FormatException)
            {
                Console.Error.WriteLine($"Failed to parse timestamp at {path} - Expected: {expected}, Found: {actual}");
                return false;
            }
            return true;
        }


        public static bool CompareExpectedResponse(string responseBody, JToken expectedJson, params string[] fieldsToIgnore)
        {
            var ignoreSet = new HashSet<string>(fieldsToIgnore);
            try
            {
                var responseJson = JToken.Parse(responseBody);

                // Handle raw string comparison
                if (expectedJson.Type == JTokenType.String && responseJson.Type == JTokenType.String)
                {
                    string expectedStr = expectedJson.ToString().Trim();
                    string actualStr = responseJson.ToString().Trim();

                    if (!string.Equals(expectedStr, actualStr, StringComparison.OrdinalIgnoreCase))
                    {
                        Console.Error.WriteLine($"String mismatch - Expected: \"{expectedStr}\", Found: \"{actualStr}\"");
                        return false;
                    }
                    return true;
                }

                // Handle array comparison
                if (responseJson.Type == JTokenType.Array && expectedJson.Type == JTokenType.Array)
                {
                    return CompareJsonArrays((JArray)responseJson, (JArray)expectedJson, "", ignoreSet);
                }

                // Handle object comparison
                if (responseJson.Type == JTokenType.Object && expectedJson.Type == JTokenType.Object)
                {
                    return CompareJsonNodes((JObject)responseJson, (JObject)expectedJson, "", ignoreSet);
                }

                // Fallback to deep equality check
                if (!JToken.DeepEquals(expectedJson, responseJson))
                {
                    Console.Error.WriteLine($"Mismatch - Expected: {expectedJson}, Found: {responseJson}");
                    return false;
                }

                return true;
            }
            catch (JsonReaderException)
            {
                // Handle plain text (non-JSON) response
                string expectedStr = expectedJson.ToString().Trim();
                string actualStr = responseBody.Trim();

                if (!string.Equals(expectedStr, actualStr, StringComparison.OrdinalIgnoreCase))
                {
                    Console.Error.WriteLine($"Plain text mismatch - Expected: \"{expectedStr}\", Found: \"{actualStr}\"");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Unexpected error during comparison: {ex.Message}");
                return false;
            }
        }




        public static string ReadFileFromSubfolders(string fileName)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            // Adjusted to match actual folder structure
            string projectRoot = Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\Framework_API_Automation"));
            string baseFolderPath = Path.Combine(projectRoot, "TestData");

            var baseFolder = new DirectoryInfo(baseFolderPath);
            var targetFile = FindFileInSubfolders(baseFolder, fileName);

            if (targetFile == null)
            {
                throw new FileNotFoundException($"File not found: {fileName} in TestData or its subfolders.");
            }

            return File.ReadAllText(targetFile.FullName, Encoding.UTF8);
        }




        public static FileInfo FindFileInSubfolders(DirectoryInfo folder, string fileName)
        {
            FileInfo[] files = folder.GetFiles();
            foreach (var file in files)
            {
                if (file.Name.Equals(fileName, StringComparison.OrdinalIgnoreCase))
                {
                    return file;
                }
            }

            DirectoryInfo[] subDirs = folder.GetDirectories();
            foreach (var subDir in subDirs)
            {
                FileInfo foundFile = FindFileInSubfolders(subDir, fileName);
                if (foundFile != null)
                {
                    return foundFile;
                }
            }

            return null;
        }



        












    }

}
