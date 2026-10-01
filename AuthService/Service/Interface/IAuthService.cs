using AuthService.DTOs;

namespace AuthService.Service.Interface
{
    public interface IAuthService
    {
        Task<SignUpResponse> SignUpAsync(SignUpRequest request);
        Task<SignInResponse> SignInAsync(SignInRequest request);
    }
}
