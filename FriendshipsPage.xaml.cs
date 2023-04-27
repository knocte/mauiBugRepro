using DataModel;

namespace Frontend;

public partial class FriendshipsPage : ContentPage
{
    Dictionary<int, int> friends;
    int userID;

    public FriendshipsPage(int userID)
    {
        InitializeComponent();
        this.userID = userID;
    }

    protected override async void OnAppearing()
    {
        if (friends is null || friends.Count == 0)
        {
            friends = await GetFriends();
            ListOfFriends.ItemsSource = friends.Keys;
        }
    }

    async Task<Dictionary<int, int>> GetFriends()
    {
        var instance = new GrpcClient.Instance();
        var response = await instance.GetFriends(new GetFriendsRequest(userID));
        return response.Friends;
    }

    async void ListOfFriendsItemSelected(object sender, TappedEventArgs e)
    {
        int friendID = (int)e.Parameter;

        if (!friends.ContainsKey(friendID))
            return;

        await Navigation.PushAsync(new ClosenessPage(userID, friendID, (Closeness)friends[friendID]));
    }

}
