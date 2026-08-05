namespace SchoolCRM.Shared.Models;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public int StatusCode { get; set; }
    public List<string> Errors { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponse<T> SuccessResponse(T data, string message = "Success", int statusCode = 200)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            StatusCode = statusCode,
            Timestamp = DateTime.UtcNow
        };
    }

    public static ApiResponse<T> FailResponse(string message, int statusCode = 400, List<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            StatusCode = statusCode,
            Errors = errors ?? new List<string>(),
            Timestamp = DateTime.UtcNow
        };
    }

    public static ApiResponse<T> NotFoundResponse(string message = "Resource not found")
    {
        return FailResponse(message, 404);
    }

    public static ApiResponse<T> UnauthorizedResponse(string message = "Unauthorized")
    {
        return FailResponse(message, 401);
    }

    public static ApiResponse<T> ForbiddenResponse(string message = "Forbidden")
    {
        return FailResponse(message, 403);
    }
}

public class ApiResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public List<string> Errors { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponse SuccessResponse(string message = "Success", int statusCode = 200)
    {
        return new ApiResponse
        {
            Success = true,
            Message = message,
            StatusCode = statusCode,
            Timestamp = DateTime.UtcNow
        };
    }

    public static ApiResponse FailResponse(string message, int statusCode = 400, List<string>? errors = null)
    {
        return new ApiResponse
        {
            Success = false,
            Message = message,
            StatusCode = statusCode,
            Errors = errors ?? new List<string>(),
            Timestamp = DateTime.UtcNow
        };
    }
}
