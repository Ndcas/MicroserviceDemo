namespace UserService.Application.Dtos;

public record ServiceResponse(
    bool Ok,
    int Status,
    string? Message,
    string? Error);

public record ServiceResponse<T>(
    bool Ok,
    int Status,
    string? Message,
    string? Error,
    T? Data) : ServiceResponse(
        Ok, 
        Status, 
        Message, 
        Error);