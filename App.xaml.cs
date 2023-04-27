namespace Frontend;

public partial class App : Application
{
    FileInfo everOpenedFile = new FileInfo(Path.Combine(FileSystem.AppDataDirectory, "zero.ini"));

    public App()
    {
        InitializeComponent();

        Page mainPage = CheckIfAppIsOpenedFirstTime() ? new MainPage() : new WelcomePage();
        MainPage = new NavigationPage(mainPage);
    }

    bool CheckIfAppIsOpenedFirstTime()
    {
        if (everOpenedFile.Exists)
        {
            return true;
        }
        else
        {
            File.WriteAllText(everOpenedFile.FullName, String.Empty);
            return false;
        }
    }
}
