namespace BlogPlatform.Domain.Common;

public class ResultModel<T>
{
    public bool IsSuccess { get; private set; }
    public T? Data { get; private set; }
    public string? Error { get; private set; }
    public int StatusCode { get; private set; }

    private ResultModel(bool isSuccess, T? data, string? error, int statusCode)
    {
        IsSuccess = isSuccess;
        Data = data;
        Error = error;
        StatusCode = statusCode;
    }

    public static ResultModel<T> Ok(T data) =>
        new(true, data, null, 200);

    public static ResultModel<T> Created(T data) =>
        new(true, data, null, 201);

    public static ResultModel<T> NotFound(string error) =>
        new(false, default, error, 404);

    public static ResultModel<T> BadRequest(string error) =>
        new(false, default, error, 400);

    public static ResultModel<T> Unauthorized(string error) =>
        new(false, default, error, 401);

    public static ResultModel<T> Forbidden(string error) =>
        new(false, default, error, 403);

    public static ResultModel<T> Conflict(string error) =>
        new(false, default, error, 409);

    public static ResultModel<T> ServerError(string error) =>
        new(false, default, error, 500);
}

public class ResultModel
{
    public bool IsSuccess { get; private set; }
    public string? Error { get; private set; }
    public int StatusCode { get; private set; }

    private ResultModel(bool isSuccess, string? error, int statusCode)
    {
        IsSuccess = isSuccess;
        Error = error;
        StatusCode = statusCode;
    }

    public static ResultModel NoContent() =>
        new(true, null, 204);

    public static ResultModel NotFound(string error) =>
        new(false, error, 404);

    public static ResultModel BadRequest(string error) =>
        new(false, error, 400);

    public static ResultModel Unauthorized(string error) =>
        new(false, error, 401);

    public static ResultModel Forbidden(string error) =>
        new(false, error, 403);

    public static ResultModel Conflict(string error) =>
        new(false, error, 409);

    public static ResultModel ServerError(string error) =>
        new(false, error, 500);
}
