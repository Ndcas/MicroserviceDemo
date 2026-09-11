namespace LogService.Application.Dtos;

public record LogMessage(
    string Level,
    string Source,
    string CorrelationId,
    string Ip,
    string Time,
    string Content);

