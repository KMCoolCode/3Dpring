using AngelDLP.PMC;
using AngleDLP.Models;
using AngleDLP.PPC;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Media;
using static AngelDLP.ViewModels.MainWindowViewModel;
using static AngelDLP.ViewModels.PrintControlViewModel;

namespace AngelDLP.ViewModels
{
    public class ProjectorViewModel : BindableBase
    {
        IEventAggregator _ea;

        public int configWidth { get; set; }
        public int configHeight { get; set; }
        public ImageSource P1_Image { get; set; }
        public ProjectorViewModel(IEventAggregator ea, ProjectorConfig projectorConfig)
        {
            configWidth = projectorConfig.projectorPixWidth;
            configHeight = projectorConfig.projectorPixHeight;
            _ea = ea;
            
        }

            


    }
}