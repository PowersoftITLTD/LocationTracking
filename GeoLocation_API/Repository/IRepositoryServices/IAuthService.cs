using GeoLocation_API.ResponseModel;

namespace GeoLocation_API.Repository.IRepositoryServices
{
    public interface IAuthService
    {
        Task<string> Authenticate(string? userName, string Password);
        byte[] UserEncryptedReponsone(UserLogin_Model userModel, string keyString);
        string EncryptionObje<T>(T obj, string keyString);

        Task<CommonServicesModel<UserLoginModel>> GetUserDetailsWhenLoginIn(string username, string password);

        string DecryptPassword(string encryptedPassword, string keyString);

        Task<string> VerifyingResponse(string userLogin, string Password);
    }
}
