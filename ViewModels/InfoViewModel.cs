using AngleDLP.Models;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using static AngelDLP.ViewModels.MainWindowViewModel;

namespace AngelDLP.ViewModels
{
    public class InfoViewModel : BindableBase
    {
        IEventAggregator _ea;
        private string subViewName = "Info";
        public string AppVersion
        {
            get
            {
                return ClassValue.AppVersion;
            }
            protected set => ClassValue.AppVersion = value;
        }

        public DelegateCommand CloseCommand { get; private set; }
        public InfoViewModel(IEventAggregator ea)
        {
            _ea = ea;
            CloseCommand = new DelegateCommand(Close);
        }
        private void Close()
        {
            _ea.GetEvent<ViewSwitchEvent>().Publish(subViewName);
        }
    }
}