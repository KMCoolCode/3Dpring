using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System.ComponentModel;
using static AngelDLP.ViewModels.MainWindowViewModel;

namespace AngelDLP.ViewModels
{
    public class InputDialogViewModel : BindableBase
    {
        public string TipMessage { get; set; }
        public string InputText { get; set; } 
        public DelegateCommand EnterCommand { get; private set; }
        public InputDialogViewModel()
        {
            EnterCommand = new DelegateCommand(EnterAction);
        }

        public void EnterAction()
        {
            
        }
    }
}