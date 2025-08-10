namespace DistriCatalogoAPI.Application.DTOs
{
    public class AuthResponseDto
    {
        public string Message { get; set; }
        public UserDto User { get; set; }
        public CompanyDto Empresa { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string ExpiresIn { get; set; }
    }
}