using System.Net;

namespace Shared.Common.Exceptions;

public class BadRequestException(string message, IReadOnlyCollection<string>? details = null)
    : AppException(message, HttpStatusCode.BadRequest, "BAD_REQUEST", details);

public class UnauthorizedException(string message)
    : AppException(message, HttpStatusCode.Unauthorized, "UNAUTHORIZED");

public class ForbiddenException(string message)
    : AppException(message, HttpStatusCode.Forbidden, "FORBIDDEN");

public class NotFoundException(string message)
    : AppException(message, HttpStatusCode.NotFound, "NOT_FOUND");

public class ConflictException(string message)
    : AppException(message, HttpStatusCode.Conflict, "CONFLICT");