namespace Frontend;

public partial class WelcomePage : ContentPage
{
    bool permissionWasDenied = false;

    public WelcomePage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        if (permissionWasDenied)
            await CheckIfPermissionGrantedInDeviceSettings();
    }

    async void GivePermissionButtonClicked(object sender, System.EventArgs e)
    {
        if (permissionWasDenied)
        {
            AppInfo.Current.ShowSettingsUI();
            return;
        }

        var status = await Permissions.RequestAsync<Permissions.LocationAlways>();
        if (status == PermissionStatus.Granted)
            App.Current.MainPage = new NavigationPage(new MainPage());
        else
            UpdateUIWhenPermissionWasDenied();
    }

    async Task CheckIfPermissionGrantedInDeviceSettings()
    {
        PermissionStatus status = await Permissions.CheckStatusAsync<Permissions.LocationAlways>();
        if (status == PermissionStatus.Granted)
            App.Current.MainPage = new NavigationPage(new MainPage());
    }

    void UpdateUIWhenPermissionWasDenied()
    {
        permissionWasDenied = true;
        GivePermissionButton.Text = "Open settings";
        InfoText.Text = Texts.BackgroundLocationPermissionIsNeededText;
    }

}
