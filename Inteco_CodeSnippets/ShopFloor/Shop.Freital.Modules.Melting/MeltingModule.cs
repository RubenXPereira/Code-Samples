using Ninject.Modules;
using Imas.Mvvm.Controller;

namespace Shop.Freital.Modules.Melting
{
    public class MeltingModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IController>().To<MeltingController>().Named("40melting");
        }
    }
}
