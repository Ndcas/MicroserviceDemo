namespace user.Dtos
{
    public class LogInResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }

        public LogInResponse(string accessToken, string refreshToken)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
        }
    }
}
