using Ninject;
using Imas.Business;

namespace Shop.Business
{
    public class BusinessFactory : IBusinessFactory
    {
        private readonly IKernel kernel;

        public BusinessFactory(IKernel kernel)
        {
            this.kernel = kernel;
        }

        public TCommand Command<TCommand>() where TCommand : ICommand
        {
            return kernel.Get<TCommand>();
        }

        public TCommand NamedCommand<TCommand>(string named) where TCommand : ICommand
        {
            return kernel.Get<TCommand>(named);
        }

        public TQuery Query<TQuery>() where TQuery : IQuery
        {
            return kernel.Get<TQuery>();
        }
    }
}
