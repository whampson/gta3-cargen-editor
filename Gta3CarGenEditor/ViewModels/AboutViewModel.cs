using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Input;
using System.Windows.Navigation;
using WHampson.Gta3CarGenEditor.Events;
using WHampson.Gta3CarGenEditor.Helpers;
using WHampson.Gta3CarGenEditor.Resources;

namespace WHampson.Gta3CarGenEditor.ViewModels
{
    public class AboutViewModel : ObservableObject
    {
        public string AppVersion
        {
            get {
                return GetAppVersionString();
            }
        }

        private string GetAppVersionString()
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            if (asm == null) {
                return Strings.TextVersionError;
            }

            FileVersionInfo vInfo = FileVersionInfo.GetVersionInfo(asm.Location);
            return string.Format(Strings.TextVersionFormat,
                vInfo.ProductVersion, vInfo.FilePrivatePart);
        }

        public ICommand DonateCommand
        {
            get {
                return new RelayCommand(
                    () => OnNavigationRequested(
                        new RequestNavigateEventArgs(new Uri(Strings.UrlDonate, UriKind.Absolute), null)));
            }
        }

        public ICommand CloseCommand
        {
            get { return new RelayCommand(() => OnDialogCloseRequested(new DialogCloseEventArgs(false))); }
        }

        public event EventHandler<RequestNavigateEventArgs> NavigationRequested;
        protected void OnNavigationRequested(RequestNavigateEventArgs e)
        {
            NavigationRequested?.Invoke(this, e);
        }

        public event EventHandler<DialogCloseEventArgs> DialogCloseRequested;
        protected void OnDialogCloseRequested(DialogCloseEventArgs e)
        {
            DialogCloseRequested?.Invoke(this, e);
        }
    }
}
