namespace Frontend.Dtos;

public record ApiResponse(bool Ok, string? Error)
{
    public static ApiResponse Success()
    {
        return new ApiResponse(true, null);
    }

    public static ApiResponse Fail(string error)
    {
        return new ApiResponse(false, error);
    }
}

public record ApiResponse<T>(bool Ok, string? Error, T? Data)
{
    public static ApiResponse<T> Success(T? Data = default)
    {
        return new ApiResponse<T>(true, null, Data);
    }

    public static ApiResponse<T> Fail(string error, T? Data = default)
    {
        return new ApiResponse<T>(false, error, Data);
    }
}
