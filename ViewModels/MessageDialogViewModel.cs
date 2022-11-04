using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System.ComponentModel;
using System.Windows;
using static AngelDLP.ViewModels.MainWindowViewModel;

namespace AngelDLP.ViewModels
{
    public class MessageDialogViewModel : BindableBase
    {
        public string TipTittle { get; set; }
        public string TipMessage { get; set; }


        public Visibility YesButtonEnable { get; set; } = Visibility.Visible;

        public Visibility NoButtonEnable { get; set; } = Visibility.Visible;
        public MessageDialogViewModel()
        {

        }

    }
}