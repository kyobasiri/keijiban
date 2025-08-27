// ViewModels/CloseWindowMessage.cs
using CommunityToolkit.Mvvm.Messaging.Messages;
using MessageManager.ViewModels;

public class CloseWindowMessage
{
    public ViewModelBase Sender { get; }

    public CloseWindowMessage(ViewModelBase sender)
    {
        Sender = sender;
    }
}
