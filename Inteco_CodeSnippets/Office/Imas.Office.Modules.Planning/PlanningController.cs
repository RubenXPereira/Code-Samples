using Imas.Mvvm.Controller;
using Imas.Mvvm.Regions;
using Imas.Office.Modules.Planning.Portal;
using Ninject;
using Prism.Events;
using Prism.Regions;

namespace Imas.Office.Modules.Lineups
{
    public class PlanningController : AppController
    {
        public PlanningController(IEventAggregator eventAggregator, IRegionManager regionManager, IKernel kernel)
            : base(eventAggregator, regionManager, kernel)
        { }

        protected override void DeclareComposition()
        {
            Always()
                .Inject<PlanningView>()
                .Into(RegionNames.MainRegion);
        }
    }
}
