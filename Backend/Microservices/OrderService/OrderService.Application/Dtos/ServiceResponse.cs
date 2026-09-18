namespace OrderService.Application.Dtos;

public record ServiceResponse(bool Ok, int Status, string? Error)
{
    public static ServiceResponse Success(int status)
    {
        return new ServiceResponse(true, status, null);
    }

    public static ServiceResponse Fail(int status, string error)
    {
        return new ServiceResponse(false, status, error);
    }
};

public record ServiceResponse<T>(
    bool Ok,
    int Status,
    string? Error,
    T? Data)
{
    public static ServiceResponse<T> Success(int status, T? data = default)
    {
        return new ServiceResponse<T>(
            true,
            status,
            null,
            data);
    }

    public static ServiceResponse<T> Fail(int status, string error, T? data = default)
    {
        return new ServiceResponse<T>(
            false,
            status,
            error,
            data);
    }
}