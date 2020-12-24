using System.Collections.Generic;

namespace MicroBootstrap.MicroBootstrap.MessageBrokers.RabbitMQ.Context
{
    public interface IContextProvider
    {
        string HeaderName { get; }
        object Get(IDictionary<string, object> headers);
    }
}