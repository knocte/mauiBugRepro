using Grpc.Core;
using GrpcService;
using GrpcClient;

using ZXing.Net.Maui;
using ZXing.QrCode.Internal;

using DataModel;
using ZXing.Net.Maui.Controls;

namespace Frontend;

public partial class AddFriendPage : ContentPage
{
    int userID;
    GrpcClient.Instance notificationClient = new Instance();

    private const string SuccessFriendshipTitle = "Success!";
    private const string SuccessFriendshipMsg = "You are friends now!";

    private const string FriendshipAlreadyExistsTitle = "Note";
    private const string FriendshipAlreadyExistsMsg = "Friendship already exists!";

    private const string OkButtonText = "Ok";
    CameraBarcodeReaderView cameraBarcodeReaderView = null;

    public AddFriendPage(int userID)
    {
        InitializeComponent();

        this.userID = userID;
        QRCode.Value = userID.ToString();
    }

    protected override async void OnAppearing()
    {
        //TODO: should this be done outside of any page?
        await Task.Run(GetNotifications);
        base.OnAppearing();
    }

    private async Task GetNotifications()
    {
        //TODO: can we move this to GrpcClient.Instance?
        var client = notificationClient.Connect();
        using var call = client.GetNotifications(new GetNotificationRequest { UserId = userID });
        while (await call.ResponseStream.MoveNext())
        {
            var notificationTitle =
                call.ResponseStream.Current.TypeId switch
                {
                    NotificationIdentifiers.AddFriendSuccessNotificationId => SuccessFriendshipTitle,
                    _ => "New notification"
                };

            var notificationText =
                call.ResponseStream.Current.TypeId switch
                {
                    NotificationIdentifiers.AddFriendSuccessNotificationId => SuccessFriendshipMsg,
                    _ => call.ResponseStream.Current.TypeId
                };

            await MainThread.InvokeOnMainThreadAsync(
                () => {
                    DisplayAlert(notificationTitle, notificationText, OkButtonText);
                });
        }
    }

    protected async void BarcodesDetected(object sender, BarcodeDetectionEventArgs e)
    {
        var friendsQRCodeValue = e.Results.FirstOrDefault().Value;
        int friendID = int.Parse(friendsQRCodeValue);
        await AddFriend(friendID);
    }

    async Task AddFriend(int friendID)
    {
        cameraBarcodeReaderView.IsDetecting = false;
        var addFriend = new DataModel.AddFriendRequest(userID, friendID);
        GrpcClient.Instance grpcClient = new GrpcClient.Instance();
        var response = await grpcClient.AddFriend(addFriend);

        if (response.StatusCode == AddFriendStatusCode.FriendedSuccess
            || response.StatusCode == AddFriendStatusCode.FriendedCompleted)
            await MainThread.InvokeOnMainThreadAsync(
                async () => {
                    Switcher.IsToggled = false;
                    await DisplayAlert(SuccessFriendshipTitle, SuccessFriendshipMsg, OkButtonText);
                });
        else if (response.StatusCode == AddFriendStatusCode.AlreadyDone)
            await MainThread.InvokeOnMainThreadAsync(
                async () => {
                    Switcher.IsToggled = false;
                    await DisplayAlert(FriendshipAlreadyExistsTitle, FriendshipAlreadyExistsMsg, OkButtonText);
                });
    }

    private void SwitcherToggled(object sender, ToggledEventArgs _)
    {
        if (Switcher.IsToggled)
        {
            if (cameraBarcodeReaderView == null && DeviceInfo.DeviceType == DeviceType.Physical)
            {
                cameraBarcodeReaderView = new CameraBarcodeReaderView();
                cameraBarcodeReaderView.BarcodesDetected += BarcodesDetected;
                BarcodeContentView.Content = cameraBarcodeReaderView;
            }
            else if (cameraBarcodeReaderView != null)
            {
                cameraBarcodeReaderView.IsDetecting = true;
            }
        }
    }
}
