using AngleDLP.Models;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using static AngelDLP.ViewModels.MainWindowViewModel;

namespace AngelDLP.ViewModels
{
    public class UserViewModel : BindableBase
    {
        IEventAggregator _ea;
        private string subViewName = "User";

        public DelegateCommand CloseCommand { get; private set; }

        private UserInfo _userinfo;
        public UserInfo userinfo
        {
            get { return _userinfo; }
            set { SetProperty(ref _userinfo, value); }

        }

        public class UpDatUserUIEvent : PubSubEvent<UserInfo>
        {

        }
        public UserViewModel(IEventAggregator ea)
        {
            _ea = ea;
            CloseCommand = new DelegateCommand(Close);
            _ea.GetEvent<UpDatUserUIEvent>().Subscribe(user =>
            {
                userinfo = user;
            });
        }
        private void Close()
        {
            _ea.GetEvent<ViewSwitchEvent>().Publish(subViewName);
        }
    }
}