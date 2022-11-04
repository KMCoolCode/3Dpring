using Prism.Commands;

namespace UsingCompositeCommands.Core
{
    public interface IApplicationCommands
    {
        CompositeCommand ViewSwitchCommand { get; }
    }

    public class ApplicationCommands : IApplicationCommands
    {
        private CompositeCommand _viewSwitchCommand = new CompositeCommand();
        public CompositeCommand ViewSwitchCommand
        {
            get { return _viewSwitchCommand; }
        }
    }
}
