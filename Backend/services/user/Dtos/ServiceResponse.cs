namespace user.Dtos
{
    public class ServiceResponse
    {
        public bool Ok { get; set; }
        public int Status { get; set; }
        public string? Message { get; set; }
        public string? Error { get; set; }

        public ServiceResponse() { }

        public ServiceResponse(bool ok, int status, string? message = null, string? error = null)
        {
            Ok = ok;
            Status = status;
            Message = message;
            Error = error;
        }
    }

    public class ServiceResponse<T> : ServiceResponse
    {
        public T? Data { get; set; }

        public ServiceResponse() : base() { }

        public ServiceResponse(bool ok, int status, T? data, string? message = null, string? error = null)
            : base(ok, status, message, error)
        {
            Data = data;
        }
    }
}
