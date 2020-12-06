using System;
using MicroBootstrap.Commands.Dispatchers;
using MicroBootstrap.Types;
using Microsoft.Extensions.DependencyInjection;

namespace MicroBootstrap.Commands
{
    public static class Extensions
    {
        public static IServiceCollection AddInMemoryCommandDispatcher(this IServiceCollection serviceCollection)
        {
            return serviceCollection.AddSingleton<ICommandDispatcher, InMememoryCommandDispatcher>();
        }
        public static IServiceCollection AddCommandHandlers(this IServiceCollection serviceCollection)
        {
            serviceCollection.Scan(s =>
            s.FromAssemblies(AppDomain.CurrentDomain.GetAssemblies())
                .AddClasses(c => c.AssignableTo(typeof(ICommandHandler<>))
                   .WithoutAttribute(typeof(DecoratorAttribute)))
                .AsImplementedInterfaces()
                .WithTransientLifetime());
            return serviceCollection;
        }
    }
}