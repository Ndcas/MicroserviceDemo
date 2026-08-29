namespace user.Dtos
{
    public class RefreshResponse
    {
        public string AccessToken { get; set; }

        public RefreshResponse(string accessToken)
        {
            AccessToken = accessToken;
        }
    }
}
