using System.Text.Json.Serialization;

namespace PaperSite.Application.Common.Responses;

public class BaseResponse<T>
{
    public bool IsSuccess { get; set; }

    [JsonIgnore]
    public bool isSuccess
    {
        get => IsSuccess;
        set => IsSuccess = value;
    }

    public DateTime Time { get; set; } = DateTime.Now;
    public T? Data { get; set; }
    public string Message { get; set; } = string.Empty;
    public IReadOnlyList<string> Errors { get; set; } = Array.Empty<string>();

    public BaseResponse()
    {
    }

    public BaseResponse(bool success, T? data, string message, IReadOnlyList<string>? errors = null)
    {
        IsSuccess = success;
        Data = data;
        Message = message;
        Errors = errors ?? Array.Empty<string>();
        Time = DateTime.Now;
    }

    public static BaseResponse<T> Success(T? data, string message = "Operation completed successfully") => new(true, data, message);

    public static BaseResponse<T> Failure(string message, IReadOnlyList<string>? errors = null) => new(false, default, message, errors);
}
