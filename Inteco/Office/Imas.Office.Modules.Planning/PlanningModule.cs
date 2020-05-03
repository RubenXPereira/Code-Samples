using Imas.Mvvm.Controller;
using Ninject.Modules;

namespace Imas.Office.Modules.Lineups
{
    public class PlanningModule : NinjectModule
	{
		public override void Load()
		{
            Bind<IController>().To<PlanningController>().Named("40planning");
		}
	}
}
