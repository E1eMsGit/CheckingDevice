using MaterialDesignThemes.Wpf;
using System;
using System.Threading.Tasks;

namespace TK158.DialogWindows
{
    static class Dialog
    {
        public static async void ShowNotificationDialog(string title, string message)
        {
            var view = new NotificationDialogView
            {
                DataContext = new NotificationDialogViewModel() { Title = title, Message = message }
            };

            // Костыль для того чтобы Dialoghost успел загрузиться в LoadedWindowCommand.
            await Task.Delay(TimeSpan.FromMilliseconds(1));

            await DialogHost.Show(view, "RootDialog");
        }
    }
}
