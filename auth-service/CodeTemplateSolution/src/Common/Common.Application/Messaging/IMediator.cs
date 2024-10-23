using MediatR;

namespace Common.Application.Messaging
{
    public interface IMediator : ISender, IPublisher
    {
    }
}
