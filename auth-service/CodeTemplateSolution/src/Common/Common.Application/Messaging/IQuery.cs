using Common.Domain.Filters;
using MediatR;

namespace Common.Application.Messaging
{
    public interface IQuery<TResponse> : IRequest<Result<TResponse>>
    {
    }
    public interface IQueryFilter<TResponse> : IRequest<Result<FilterResult<List<TResponse>>>>
    {
    }
}
