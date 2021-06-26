using MelonLoader;
using System.Reflection;

namespace ModBrowser
{
    public static class Config
    {
        public const string Category = "ModBrowser";

        public static string modDataETag;

        public static void RegisterConfig()
        {
            MelonPreferences.CreateEntry(Category, nameof(modDataETag), "", modDataETag);
            OnModSettingsApplied();
        }

        public static void OnModSettingsApplied()
        {
            foreach (var fieldInfo in typeof(Config).GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                if (fieldInfo.Name == "Category") continue;
                else if (fieldInfo.FieldType == typeof(string)) fieldInfo.SetValue(null, MelonPreferences.GetEntryValue<string>(Category, fieldInfo.Name));
            }
        }

        public static void UpdateValue(string name, object value)
        {
            if (value is string)
            {
                MelonPreferences.SetEntryValue(Category, name, (string)value);
            }
            MelonPreferences.Save();
        }
    }
}
