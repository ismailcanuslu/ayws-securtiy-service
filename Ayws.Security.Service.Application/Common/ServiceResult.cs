using System.Net;

namespace Ayws.Security.Service.Application.Common;

public class ServiceResult<T>
{
    public T? Data { get; private set; }
    public bool IsSuccess { get; private set; }
    public List<string> ErrorMessages { get; private set; } = new();
    public HttpStatusCode StatusCode { get; private set; }

    private ServiceResult() { }

    public static ServiceResult<T> SuccessAsOk(T data) => new()
    {
        Data = data,
        IsSuccess = true,
        StatusCode = HttpStatusCode.OK
    };

    public static ServiceResult<T> SuccessAsCreated(T data) => new()
    {
        Data = data,
        IsSuccess = true,
        StatusCode = HttpStatusCode.Created
    };

    public static ServiceResult<T> Fail(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest) => new()
    {
        IsSuccess = false,
        StatusCode = statusCode,
        ErrorMessages = [message]
    };

    public static ServiceResult<T> Fail(List<string> messages, HttpStatusCode statusCode = HttpStatusCode.BadRequest) => new()
    {
        IsSuccess = false,
        StatusCode = statusCode,
        ErrorMessages = messages
    };
}

public class ServiceResult
{
    public bool IsSuccess { get; private set; }
    public List<string> ErrorMessages { get; private set; } = new();
    public HttpStatusCode StatusCode { get; private set; }

    private ServiceResult() { }

    public static ServiceResult SuccessAsNoContent() => new()
    {
        IsSuccess = true,
        StatusCode = HttpStatusCode.NoContent
    };

    public static ServiceResult Fail(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest) => new()
    {
        IsSuccess = false,
        StatusCode = statusCode,
        ErrorMessages = [message]
    };

    public static ServiceResult Fail(List<string> messages, HttpStatusCode statusCode = HttpStatusCode.BadRequest) => new()
    {
        IsSuccess = false,
        StatusCode = statusCode,
        ErrorMessages = messages
    };
}
