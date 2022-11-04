using Prism.Events;
using System.Collections.Generic;

namespace AngelDLP.PPC
{
    public class ProjectFactory: IProjectFactory
    {
        private readonly IEventAggregator eventAggregator;

        public ProjectFactory(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            projectors = new List<Projector>();
        }
        private List<Projector> projectors;
        public List<Projector> CreateProjectors(int count)
        {
            if (projectors.Count==count)
            {
                return projectors;
            }
            for (int i = 0; i < count; i++)
            {
                projectors.Add(new Projector(eventAggregator));
            }
            return projectors;
        }

        public List<Projector> GetProjectors()
        {
            return projectors;
        }
    }

    public interface IProjectFactory
    {
        List<Projector> CreateProjectors(int count);
        List<Projector> GetProjectors();
    }
}
