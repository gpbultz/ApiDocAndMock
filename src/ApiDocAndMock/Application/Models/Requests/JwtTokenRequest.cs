namespace ApiDocAndMock.Application.Models.Requests
{
    public class JwtTokenRequest
    {
        public List<string> Roles { get; set; } = new List<string>();
    }
}
