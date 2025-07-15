namespace MeetApp.DataEntities.Common;

public class Result
{
    public bool IsSuccess { get; protected set; }
    public string? Error { get; protected set; }
    public bool IsError => !IsSuccess;
    public static Result Success() => new Result { IsSuccess = true };
    public static Result Failure(string Error) => new Result { IsSuccess = false, Error = Error }; 
}

public class Result<T> : Result
{
    public T? Data { get; private set; }
    public static Result<T> Success(T Data) => new Result<T> { IsSuccess = true, Data = Data };
    public static Result<T> Failure(string Error) => new Result<T> { IsSuccess = false, Error = Error };

}