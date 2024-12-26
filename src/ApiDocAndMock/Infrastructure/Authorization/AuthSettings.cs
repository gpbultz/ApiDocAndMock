using ApiDocAndMock.Shared.Enums;

namespace ApiDocAndMock.Infrastructure.Authorization
{
    public class AuthSettings
    {
        public AuthMode Mode { get; set; } = AuthMode.BearerOnly;
    }

}
