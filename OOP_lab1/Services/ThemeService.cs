using System;
using System.Windows;
using Microsoft.Win32;

namespace OOP_lab1.Services
{
    public static class ThemeService
    {
        private const string LightThemeUri = "pack://application:,,,/ThemeLibrary;component/Themes/LightTheme.xaml";
        private const string DarkThemeUri = "pack://application:,,,/ThemeLibrary;component/Themes/DarkTheme.xaml";

        public static void ApplyTheme(string themeName)
        {
            string uriString = themeName switch
            {
                "Темная (Dark)" => DarkThemeUri,
                "Системная (System)" => IsWindowsDarkTheme() ? DarkThemeUri : LightThemeUri,
                _ => LightThemeUri
            };

            var mergedDictionaries = Application.Current.Resources.MergedDictionaries;
            if (mergedDictionaries.Count > 1)
            {
                mergedDictionaries[1] = new ResourceDictionary
                {
                    Source = new Uri(uriString, UriKind.Absolute)
                };
            }
        }

        private static bool IsWindowsDarkTheme()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
                var value = key?.GetValue("AppsUseLightTheme");
                return value is int intValue && intValue == 0;
            }
            catch
            {
                return false;
            }
        }
    }
}