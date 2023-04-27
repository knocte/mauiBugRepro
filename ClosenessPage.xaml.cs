using DataModel;

namespace Frontend;

public partial class ClosenessPage : ContentPage
{
    private readonly int userID;
    private readonly int friendID;
    private readonly GrpcClient.Instance grpcClient;
    private bool shouldUpdate = false;

    public ClosenessPage(int userID, int friendID, Closeness previousCloseness)
    {
        InitializeComponent();

        this.userID = userID;
        this.friendID = friendID;
        grpcClient = new GrpcClient.Instance();
        SetSwitchesProperly(previousCloseness);
        shouldUpdate = true;
    }

    void SetSwitchesProperly(Closeness closeness)
    {
        // NOTE: toggled = false means that the switch points to the left

        AcquaintanceComradeSwitcher.IsEnabled = true;
        switch (closeness)
        {
            case Closeness.Acquaintance:
                AcquaintanceComradeSwitcher.IsToggled = false;

                FriendFamilySwitcher.IsToggled = false;
                FriendFamilySwitcher.IsEnabled = false;
                FriendLabel.IsEnabled = false;
                FamilyLabel.IsEnabled = false;

                RegularCloseSwitcher.IsToggled = false;
                RegularCloseSwitcher.IsEnabled = false;
                RegularLabel.IsEnabled = false;
                CloseLabel.IsEnabled = false;

                CloseCrushSwitcher.IsToggled = false;
                CloseCrushSwitcher.IsEnabled = false;
                JustCloseLabel.IsEnabled = false;
                CrushLabel.IsEnabled = false;
                break;

            case Closeness.RegularFamily:
                AcquaintanceComradeSwitcher.IsToggled = true;

                FriendFamilySwitcher.IsToggled = true;
                FriendFamilySwitcher.IsEnabled = true;
                FriendLabel.IsEnabled = true;
                FamilyLabel.IsEnabled = true;

                RegularCloseSwitcher.IsToggled = false;
                RegularCloseSwitcher.IsEnabled = true;
                RegularLabel.IsEnabled = true;
                CloseLabel.IsEnabled = true;

                CloseCrushSwitcher.IsToggled = false;
                CloseCrushSwitcher.IsEnabled = false;
                JustCloseLabel.IsEnabled = false;
                CrushLabel.IsEnabled = false;
                break;

            case Closeness.CloseFamily:
                AcquaintanceComradeSwitcher.IsToggled = true;

                FriendFamilySwitcher.IsToggled = true;
                FriendFamilySwitcher.IsEnabled = true;
                FriendLabel.IsEnabled = true;
                FamilyLabel.IsEnabled = true;

                RegularCloseSwitcher.IsToggled = true;
                RegularCloseSwitcher.IsEnabled = true;
                RegularLabel.IsEnabled = true;
                CloseLabel.IsEnabled = true;

                CloseCrushSwitcher.IsToggled = false;
                CloseCrushSwitcher.IsEnabled = false;
                JustCloseLabel.IsEnabled = false;
                CrushLabel.IsEnabled = false;
                break;

            case Closeness.RegularFriend:
                AcquaintanceComradeSwitcher.IsToggled = true;

                FriendFamilySwitcher.IsToggled = false;
                FriendFamilySwitcher.IsEnabled = true;
                FriendLabel.IsEnabled = true;
                FamilyLabel.IsEnabled = true;

                RegularCloseSwitcher.IsToggled = false;
                RegularCloseSwitcher.IsEnabled = true;
                RegularLabel.IsEnabled = true;
                CloseLabel.IsEnabled = true;

                CloseCrushSwitcher.IsToggled = false;
                CloseCrushSwitcher.IsEnabled = false;
                JustCloseLabel.IsEnabled = true;
                CrushLabel.IsEnabled = true;
                break;

            case Closeness.CloseFriend:
                AcquaintanceComradeSwitcher.IsToggled = true;

                FriendFamilySwitcher.IsToggled = false;
                FriendFamilySwitcher.IsEnabled = true;
                FriendLabel.IsEnabled = true;
                FamilyLabel.IsEnabled = true;

                RegularCloseSwitcher.IsToggled = true;
                RegularCloseSwitcher.IsEnabled = true;
                RegularLabel.IsEnabled = true;
                CloseLabel.IsEnabled = true;

                CloseCrushSwitcher.IsToggled = false;
                CloseCrushSwitcher.IsEnabled = true;
                JustCloseLabel.IsEnabled = true;
                CrushLabel.IsEnabled = true;
                break;

            case Closeness.Crush:
                AcquaintanceComradeSwitcher.IsToggled = true;

                FriendFamilySwitcher.IsToggled = false;
                FriendFamilySwitcher.IsEnabled = true;
                FriendLabel.IsEnabled = true;
                FamilyLabel.IsEnabled = true;

                RegularCloseSwitcher.IsToggled = true;
                RegularCloseSwitcher.IsEnabled = true;
                RegularLabel.IsEnabled = true;
                CloseLabel.IsEnabled = true;

                CloseCrushSwitcher.IsToggled = true;
                CloseCrushSwitcher.IsEnabled = true;
                JustCloseLabel.IsEnabled = true;
                CrushLabel.IsEnabled = true;
                break;
        }
    }

    private async Task UpdateCloseness()
    {
        if (!shouldUpdate)
        {
            return;
        }

        var isComrade = AcquaintanceComradeSwitcher.IsToggled;

        Closeness newCloseness;
        if (!isComrade)
        {
            newCloseness = Closeness.Acquaintance;
        }
        else
        {
            var isFamily = FriendFamilySwitcher.IsToggled;
            var isClose = RegularCloseSwitcher.IsToggled;
            if (isFamily)
            {
                if (isClose)
                {
                    newCloseness = Closeness.CloseFamily;
                }
                else
                {
                    newCloseness = Closeness.RegularFamily;
                }
            }
            else
            {
                if (isClose)
                {
                    var isCrush = CloseCrushSwitcher.IsToggled;
                    if (isCrush)
                    {
                        newCloseness = Closeness.Crush;
                    }
                    else
                    {
                        newCloseness = Closeness.CloseFriend;
                    }
                }
                else
                {
                    newCloseness = Closeness.RegularFriend;
                }
            }
        }

        UpdateClosenessRequest updateCloseness = new(userID, friendID, (int)newCloseness);
        await grpcClient.UpdateCloseness(updateCloseness);
        SetSwitchesProperly(newCloseness);
    }

    async void AcquaintanceComradeSwitcherToggled(object sender, ToggledEventArgs e)
    {
        await UpdateCloseness();
    }

    async void FriendFamilySwitcherToggled(object sender, ToggledEventArgs e)
    {
        await UpdateCloseness();
    }

    async void RegularCloseSwitcherToggled(object sender, ToggledEventArgs e)
    {
        await UpdateCloseness();
    }

    async void CloseCrushSwitcherToggled(object sender, ToggledEventArgs e)
    {
        await UpdateCloseness();
    }
}

