using System.Net;

namespace Common.DTOs;

public class ResultDto<T>
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new List<string>();
    public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;

    public static ResultDto<T> Success(T data, string message = "", HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        return new ResultDto<T>
        {
            IsSuccess = true,
            Data = data,
            Message = message,
            StatusCode = statusCode
        };
    }

    public static ResultDto<T> Failure(string error, List<string>? errors = null, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
    {
        return new ResultDto<T>
        {
            IsSuccess = false,
            Message = error,
            Errors = errors ?? new List<string>(),
            StatusCode = statusCode
        };
    }

    public static ResultDto<T> Forbidden(string error = "Access denied")
    {
        return Failure(error, statusCode: HttpStatusCode.Forbidden);
    }

    public static ResultDto<T> NotFound(string error = "Resource not found")
    {
        return Failure(error, statusCode: HttpStatusCode.NotFound);
    }

    public static ResultDto<T> Unauthorized(string error = "Unauthorized")
    {
        return Failure(error, statusCode: HttpStatusCode.Unauthorized);
    }

    public static ResultDto<T> BadRequest(string error = "Bad request")
    {
        return Failure(error, statusCode: HttpStatusCode.BadRequest);
    }
}

public class ResultDto
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new List<string>();
    public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;

    public static ResultDto Success(string message = "", HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        return new ResultDto
        {
            IsSuccess = true,
            Message = message,
            StatusCode = statusCode
        };
    }

    public static ResultDto Failure(string error, List<string>? errors = null, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
    {
        return new ResultDto
        {
            IsSuccess = false,
            Message = error,
            Errors = errors ?? new List<string>(),
            StatusCode = statusCode
        };
    }

    public static ResultDto Forbidden(string error = "Access denied")
    {
        return Failure(error, statusCode: HttpStatusCode.Forbidden);
    }

    public static ResultDto NotFound(string error = "Resource not found")
    {
        return Failure(error, statusCode: HttpStatusCode.NotFound);
    }

    public static ResultDto Unauthorized(string error = "Unauthorized")
    {
        return Failure(error, statusCode: HttpStatusCode.Unauthorized);
    }

    public static ResultDto BadRequest(string error = "Bad request")
    {
        return Failure(error, statusCode: HttpStatusCode.BadRequest);
    }
}
