using System;
using System.Windows;

namespace WHampson.Gta3CarGenEditor.Helpers
{
    /// <summary>
    /// Parameters for opening a <see cref="MessageBoxEx"/> from an event.
    /// </summary>
    public class MessageBoxEventArgs : EventArgs
    {
        public MessageBoxEventArgs(Action<MessageBoxResult> resultAction, string text,
            string caption = "",
            MessageBoxButton buttons = MessageBoxButton.OK,
            MessageBoxImage icon = MessageBoxImage.None,
            MessageBoxResult defaultResult = MessageBoxResult.None,
            MessageBoxOptions options = MessageBoxOptions.None)
        {
            ResultAction = resultAction;
            Text = text;
            Caption = caption;
            Buttons = buttons;
            Icon = icon;
            DefaultResult = defaultResult;
            Options = options;
        }

        public string Text
        {
            get;
        }

        public string Caption
        {
            get;
        }

        public MessageBoxButton Buttons
        {
            get;
        }

        public MessageBoxImage Icon
        {
            get;
        }

        public MessageBoxResult DefaultResult
        {
            get;
        }

        public MessageBoxOptions Options
        {
            get;
        }

        public Action<MessageBoxResult> ResultAction
        {
            get;
        }

        public void Show()
        {
            Show(null);
        }

        public void Show(Window w)
        {
            MessageBoxResult result;
            if (w == null) {
                result = MessageBoxEx.Show(Text, Caption, Buttons, Icon, DefaultResult, Options);
            }
            else {
                result = MessageBoxEx.Show(w, Text, Caption, Buttons, Icon, DefaultResult, Options);
            }

            ResultAction?.Invoke(result);
        }
    }
}
