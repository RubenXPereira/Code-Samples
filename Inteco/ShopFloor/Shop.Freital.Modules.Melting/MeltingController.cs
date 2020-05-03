using Ninject;
using Prism.Events;
using Prism.Regions;
using Imas.Mvvm.Regions;
using Imas.Mvvm.Controller;
using Shop.Freital.Modules.Melting.Portal;

namespace Shop.Freital.Modules.Melting
{
    public class MeltingController : AppController
    {
        public MeltingController(IEventAggregator eventAggregator, IRegionManager regionManager, IKernel kernel)
            : base(eventAggregator, regionManager, kernel)
        { }

        protected override void DeclareComposition()
        {
            Always()
                .Inject<MeltingView>()
                .Into(RegionNames.MainRegion);
        }
    }
}
