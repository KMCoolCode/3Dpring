using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngelDLP
{
    public class PartModelFactory: IPartModelFactory
    {
        private ObservableCollection<PartModel> partModels;
        private readonly IEventAggregator _ea;
        public PartModelFactory(IEventAggregator ea)
        {
            _ea = ea;
            partModels = new ObservableCollection<PartModel>();
        }
        public ObservableCollection<PartModel> Add(PartModel partModel)
        {
            partModels.Add(partModel);
            return partModels;
        }
        public ObservableCollection<PartModel> GetPartModels()
        {
            return partModels;
        }
    }
    public interface IPartModelFactory
    {
        ObservableCollection<PartModel> Add(PartModel partModel);
        ObservableCollection<PartModel> GetPartModels();
    }
}
