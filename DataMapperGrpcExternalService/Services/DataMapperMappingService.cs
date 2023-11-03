using System.Collections.Concurrent;
using System.Text.Json;

namespace DataMapperGrpcExternalService.Services
{
    public class DataMapperMappingService
    {
        private ConcurrentDictionary<string, string> MappingDictionary = new ConcurrentDictionary<string, string>(16, 1);
        public DataMapperMappingService(IConfiguration configuration) 
        {
            string solutionPath = configuration["SolutionPath"];
            LoadMappings(solutionPath);
        }

        public void LoadMappings(string directoryPath)
        {
            try
            {
                // Check if the directory exists
                if (!Directory.Exists(directoryPath))
                {
                    throw new Exception("Directory does not exist.");
                }

                // Get all JSON files in the specified directory
                string[] jsonFiles = Directory.GetFiles(directoryPath, "*.json");

                foreach (string jsonFile in jsonFiles)
                {
                    try
                    {
                        // Read the contents of each JSON file
                        string jsonContent = File.ReadAllText(jsonFile);

                        // Extract the file name (without extension) as the key
                        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(jsonFile);

                        // Add the JSON content to the ConcurrentDictionary
                        MappingDictionary.TryAdd(fileNameWithoutExtension, jsonContent);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Error loading {jsonFile}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading mapping files: {ex.Message}");
            }
        }

        public string GetMapping(string key)
        {
            if (MappingDictionary.ContainsKey(key))
            {
                if (MappingDictionary.TryGetValue(key, out string mappingString))
                {
                    return mappingString;
                }
                else
                {
                    throw new Exception($"Failed to retrieve mapping string for key: {key}.");
                }
            }
            else
            {
                throw new Exception($"Key not found: {key}.");
            }
        }
    
        public string GetReturnType(string dataMapperName)
        {
            string jsonMapping = GetMapping(dataMapperName);
            var jsonDoc = JsonDocument.Parse(jsonMapping);

            // Access the "dataSource" array and get the second item
            var dataSourceArray = jsonDoc.RootElement.GetProperty("dataSource");
            var secondItem = dataSourceArray.EnumerateArray().ElementAt(1);

            // Get the value of "$type" from the second item
            string typeValue = secondItem.GetProperty("$type").GetString();
            bool containsXmlModule = typeValue.Contains("XmlModule");

            string returnType = "application/json";
            if (containsXmlModule)
            {
                returnType = "application/xml";
            }
            return returnType;
        }
    
    }
}
