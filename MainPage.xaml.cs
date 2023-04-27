namespace Frontend;

using System;
using System.IO;

using Microsoft.Maui.Devices.Sensors;

#if ANDROID || IOS
using Shiny.Push;
#endif

public partial class MainPage : ContentPage
{
    GrpcClient.Instance grpcClient = new GrpcClient.Instance();
    FileInfo uidFile = new FileInfo(Path.Combine(FileSystem.AppDataDirectory, "userId.txt"));

#if ANDROID || IOS
    IPushManager pushManager;
#endif

    public MainPage()
    {
        InitializeComponent();

        MainThread.BeginInvokeOnMainThread(Setup);
    }

    async void Setup()
    {
        if (!await CheckIfLocationPermissionGranted())
            return;

        try
        {
            await GatherLocation(await GetUserID());
        }
        catch (Microsoft.Maui.ApplicationModel.FeatureNotEnabledException)
        {
            FallbackLabel.Text = Texts.GpsLocationFeatureIsNeededText;
            MainLayout.IsVisible = false;
        }
    }

    private int GetCurrentUserId()
    {
        string userId = File.ReadAllText(uidFile.FullName);
        return int.Parse(userId);
    }

    async Task<int> GetUserID()
    {
        if (!uidFile.Exists)
        {
            int newUserId = await grpcClient.RegisterNewAppInstall();
            File.WriteAllText(uidFile.FullName, newUserId.ToString());
#if ANDROID || IOS
            await SetupPushNotifications(newUserId.ToString());
#endif
        }

        return GetCurrentUserId();
    }

    // workaround for bug https://github.com/dotnet/maui/issues/2451 (it only happened in OnAppearing() tho, not in OnFooClicked)
    private void SafeInvokeInMainThread(Action action)
    {
        if (DeviceInfo.Platform == DevicePlatform.WinUI)
        {
            Application.Current.Dispatcher.Dispatch(action);
        }
        else
        {
            MainThread.BeginInvokeOnMainThread(action);
        }
    }

    protected override void OnAppearing()
    {
        SafeInvokeInMainThread(() => {
            freeOrBusySwitch.IsEnabled = true;
        });
        base.OnAppearing();
    }

    private void OnSwitchToggled(object sender, EventArgs e)
    {
        Switch me = (Switch)sender;
        if (!me.IsToggled)
            return;

        if (DeviceInfo.Platform == DevicePlatform.WinUI)
        {
            SafeInvokeInMainThread(() => {
                //FIXME: maybe we can now that this issue has been fixed: https://github.com/dotnet/maui/issues/8842
                CounterLabel.Text = $"Can't get GPS location because of running in Windows";
            });
        }
    }

    private async Task GatherLocation(int userId)
    {
        MainThread.BeginInvokeOnMainThread(() => {
            CounterLabel.Text =
                $"Our location is...";
            SemanticScreenReader.Announce(CounterLabel.Text);
        });
        var req = new GeolocationRequest(GeolocationAccuracy.Low);
        var location = await Geolocation.GetLocationAsync(req);
        var updateGpsLocationRequest =
            new DataModel.UpdateGpsLocationRequest(
                userId,
                location.Latitude,
                location.Longitude
            );
        await grpcClient.UpdateGpsLocation(updateGpsLocationRequest);
        CounterLabel.Text =
            $"Our location at{Environment.NewLine}{DateTime.Now.ToString()} is:{Environment.NewLine}{location.Latitude},{location.Longitude}";

        SemanticScreenReader.Announce(CounterLabel.Text);
    }

    #region Permissions
    private async Task<PermissionStatus> RequestAndGetLocationPermission()
    {
        PermissionStatus status = await Permissions.CheckStatusAsync<Permissions.LocationAlways>();

        if (status == PermissionStatus.Granted)
            return status;

        if (status == PermissionStatus.Denied && DeviceInfo.Platform == DevicePlatform.iOS)
            return status;

        if (DeviceInfo.Platform == DevicePlatform.Android)
        {
            await DisplayAlert("Warning", Texts.BackgroundLocationPermissionIsNeededText, "Ok");
            await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        }

        status = await Permissions.RequestAsync<Permissions.LocationAlways>();

        return status;
    }

    private async Task<bool> CheckIfLocationPermissionGranted()
    {
        var locationPermissionStatus = await RequestAndGetLocationPermission();
        if (locationPermissionStatus != PermissionStatus.Granted)
        {
            MainLayout.IsVisible = false;
            return false;
        }

        return true;
    }
    #endregion

    #region Navigations
    void NavigateToAddFriendClicked(object sender, EventArgs evArgs)
    {
        Navigation.PushAsync(new AddFriendPage(GetCurrentUserId()));
    }

    void NavigateToFriendshipsClicked(object sender, EventArgs evArgs)
    {
        Navigation.PushAsync(new FriendshipsPage(GetCurrentUserId()));
    }
    #endregion


#if ANDROID || IOS
    async Task SetupPushNotifications(string userID)
    {
        pushManager = this.Handler.MauiContext.Services.GetService<IPushManager>();
        var result = await this.pushManager.RequestAccess();
        if (result.Status == AccessState.Available)
        {
            await pushManager.Tags.AddTag(userID);
        }
    }
#endif
}

