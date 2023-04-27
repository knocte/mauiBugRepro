using System;
namespace Frontend
{
    public static class Texts
    {
        private static string AppName = "DG";

        // XAML's EOL
        private static string EOL = Environment.NewLine;

        public static string WelcomeText = $"To use {AppName}, access to your GPS location is required at all times, to save the location of each entry!";
        public static string GpsLocationFeatureIsNeededText = $"{AppName} needs your GPS location feature enabled in order to run.";
        public static string BackgroundLocationPermissionIsNeededText = $"{AppName} needs permission to access GPS location to run.{EOL}Go to settings on your device and enable it.";
    }
}
