namespace OrderService.Domain.Constants;

internal static class OrderStatuses
{
    public const string Processing = "PROCESSING";
    public const string Unpaid = "UNPAID";
    public const string Confirmed = "CONFIRMED";
    public const string Finished = "FINISHED";
    public const string Canceled = "CANCELED";
}
