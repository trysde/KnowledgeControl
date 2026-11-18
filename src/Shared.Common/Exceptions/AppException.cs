using System.Net;

namespace Shared.Common.Exceptions;

public abstract class AppException(
    string message,
    HttpStatusCode statusCode,
    string errorCode,
    IReadOnlyCollection<string>? details = null) : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
    public string ErrorCode { get; } = errorCode;
    public IReadOnlyCollection<string> Details { get; } = details ?? Array.Empty<string>();
}