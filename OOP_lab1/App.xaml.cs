using System.Text.Json;
using System.Windows;
using CodingSeb.Localization;
using CodingSeb.Localization.Loaders;

namespace OOP_lab1
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        internal const string DefaultLanguage = "ru";

        protected override void OnStartup(StartupEventArgs e)
        {
            var translationUri = new Uri("/ThemeLibrary;component/Localization/Translations.json", UriKind.Relative);
            using var resource = GetResourceStream(translationUri).Stream;
            using var translations = JsonDocument.Parse(resource);

            foreach (var text in translations.RootElement.EnumerateObject())
            {
                foreach (var translation in text.Value.EnumerateObject())
                {
                    LocalizationLoader.Instance.AddTranslation(text.Name, translation.Name, translation.Value.GetString());
                }
            }

            Loc.Instance.CurrentLanguage = DefaultLanguage;
            base.OnStartup(e);
        }
    }

}
