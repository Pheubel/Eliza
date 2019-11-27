using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.ComponentModel;
using DiscordAbstraction.Interfaces;

#if DEBUG
using System.Reflection;
#endif

namespace ElizaBot
{
    public class AppSettingsLoader : IAppSettingsLoader
    {
        public T LoadSettings<T>() where T : class, new()
        {
            const string APPLICATION_FILE = "appsettings.json";

            if (!File.Exists(APPLICATION_FILE))
            {
                using (StreamWriter writer = new StreamWriter(APPLICATION_FILE))
                {
                    writer.Write(JsonSerializer.Serialize(new T(), new JsonSerializerOptions() { WriteIndented = true }));
                }

                Console.WriteLine($"The application has been terminated, please fill \"{APPLICATION_FILE}\" with the appropriate settings.");
                Environment.Exit(0);
            }

            T settings = JsonSerializer.Deserialize<T>(File.ReadAllText(APPLICATION_FILE), new JsonSerializerOptions() { ReadCommentHandling = JsonCommentHandling.Skip });

#if DEBUG
            ModifyWithDebugSettings(settings);
#endif

            return settings;
        }


#if DEBUG
        private void ModifyWithDebugSettings<T>(T settings) where T : class
        {
            const string DEVELOPMENT_APPLICATION_FILE = "dev.appsettings.json";

            if (File.Exists(DEVELOPMENT_APPLICATION_FILE))
            {
                var type = typeof(T);
                var propertyInfo = type.GetProperties();

                using var developmentSettingsDocument = JsonDocument.Parse(File.ReadAllText(DEVELOPMENT_APPLICATION_FILE));

                var rootElement = developmentSettingsDocument.RootElement;

                for (int i = 0; i < propertyInfo.Length; i++)
                {
                    if (rootElement.TryGetProperty(propertyInfo[i].Name, out var element))
                    {
                        var propertyType = propertyInfo[i].PropertyType;

                        var elementText = element.GetRawText();

                        if (elementText.StartsWith('{'))
                        {
                            propertyInfo[i].SetValue(settings, JsonSerializer.Deserialize(elementText, propertyType));
                        }
                        else if (elementText.StartsWith('"'))
                        {

                            propertyInfo[i].SetValue(settings, elementText[1..^1]);
                        }
                        else
                        {
                            var converter = TypeDescriptor.GetConverter(propertyType);
                            if (converter == null)
                                throw new Exception($"No converter for {propertyType.Name}.");
                            if (!converter.CanConvertFrom(typeof(string)))
                                throw new Exception($"Cannot convert {propertyType.Name} from string.");

                            propertyInfo[i].SetValue(settings, converter.ConvertFromString(elementText));
                        }
                    }
                }
            }
        }
#endif
    }
}
