using Dapper;
using GeoLocation_API.Repository.IRepositoryServices;
using GeoLocation_API.ResponseModel;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace GeoLocation_API.Repository.RepositoryServices
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly SqlConnection _connection;
        //private readonly ZohoOAuthSettings _settings;
        private readonly HttpClient _httpClient;

        public AuthService(IConfiguration configuration, SqlConnection sqlConnection, HttpClient httpClient)  //, IOptions<ZohoOAuthSettings> options
        {
            _configuration = configuration;
            _connection = sqlConnection;
            _httpClient = httpClient;
           // _settings = options.Value;

        }
        public async Task<string> Authenticate(string username, string password)
        {
            var responseMessage = string.Empty;

            // Ensure both username and password are provided
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                return "Please Enter Username and Password";
            }


            var parameters = new DynamicParameters();
            parameters.Add("@pLoginName", username);
            parameters.Add("@pPassword", password);
            parameters.Add("@responseMessage", dbType: DbType.String, direction: ParameterDirection.Output, size: 250); // Output parameter

            try
            {
                await _connection.ExecuteAsync("[dbo].[uspLogin]", parameters, commandType: CommandType.StoredProcedure);

                responseMessage = parameters.Get<string>("@responseMessage");

                if (responseMessage == "User successfully logged in")
                {
                    // Generate the JWT token
                    var claims = new[]
                    {
                new Claim(ClaimTypes.Name, username),  // The user's login name
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // Unique identifier for the JWT
                     };

                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"])); // Secret key from configuration
                    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256); // Signing credentials

                    var token = new JwtSecurityToken(
                        issuer: _configuration["Jwt:Issuer"],  // Issuer of the token
                        audience: _configuration["Jwt:Audience"], // Audience for the token
                        claims: claims, // Claims associated with the token
                        expires: DateTime.Now.AddHours(1), // Token expiration time
                        signingCredentials: creds // Signing credentials
                    );

                    // Return the JWT token
                    return new JwtSecurityTokenHandler().WriteToken(token);
                }
                else
                {
                    // If authentication failed, return the failure message
                    return "Invalid login name or password"; // Authentication failed
                }
            }
            catch (Exception ex)
            {
                // Return any errors that occurred during the process
                return $"An error occurred: {ex.Message}";
            }
        }
        public byte[] UserEncryptedReponsone(UserLogin_Model userModel, string keyString)
        {
            try
            {
                string combined = userModel.UserName + ":" + userModel.Password; // Combine username and password with a separator (e.g., colon)

                byte[] key = GetKey(keyString, 32); // AES-256 requires a 32-byte key

                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = key;
                    aesAlg.IV = new byte[16]; // Zeroed IV (not recommended for production)

                    ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                            {
                                swEncrypt.Write(combined); // Write combined username and password to be encrypted
                            }
                        }
                        return msEncrypt.ToArray(); // Return the encrypted data as byte array
                    }
                }

            }
            catch (Exception ex)
            {
                throw;
            }
        }
        private static byte[] GetKey(string keyString, int requiredLength)
        {
            // Truncate or pad the key to the required length (AES-128 = 16 bytes, AES-192 = 24 bytes, AES-256 = 32 bytes)
            byte[] key = Encoding.UTF8.GetBytes(keyString);

            if (key.Length < requiredLength)
            {
                Array.Resize(ref key, requiredLength); // Pad with zeros if it's too short
            }
            else if (key.Length > requiredLength)
            {
                Array.Resize(ref key, requiredLength); // Truncate if it's too long
            }

            return key;
        }
        public string EncryptionObje<T>(T obj, string keyString)
        {
            string json = System.Text.Json.JsonSerializer.Serialize(obj);
            byte[] key = GetKey(keyString, 32);
            byte[] iv = new byte[16];

            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                byte[] plainBytes = Encoding.UTF8.GetBytes(json);
                byte[] cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
                return Convert.ToBase64String(cipherBytes);
            }
        }
        public async Task<CommonServicesModel<UserLoginModel>> GetUserDetailsWhenLoginIn(string? username, string password)
        {
            var responseMessage = string.Empty;
            CommonServicesModel<UserLoginModel> commonServices = new CommonServicesModel<UserLoginModel>();


            // Ensure both username and password are provided
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                commonServices.Message = "Please Enter Username and Password";
                commonServices.Status = "Error";
                commonServices.Data = null;
                return commonServices;
            }

            var parameters = new DynamicParameters();
            parameters.Add("@pLoginName", username);
            parameters.Add("@pPassword", password);
            //parameters.Add("@responseMessage", dbType: DbType.String, direction: ParameterDirection.Output, size: 250); // Output parameter
            try
            {
                var userDetails = await _connection.QueryFirstOrDefaultAsync<UserLoginModel>("[dbo].[uspLoginDetail]", parameters, commandType: CommandType.StoredProcedure);
                //await _connection.ExecuteAsync("[dbo].[uspLogin]", parameters, commandType: CommandType.StoredProcedure);
                if (userDetails.UserId > 0)
                {
                    commonServices.Status = "Ok";
                    commonServices.Message = "User Details Fetched Successfully";
                    commonServices.Data = userDetails;
                    return commonServices;
                }
                else
                {
                    commonServices.Message = "Invalid login name or password";
                    commonServices.Status = "Error";
                    commonServices.Data = null;
                    return commonServices;
                }
            }
            catch (Exception ex)
            {
                commonServices.Message = $"Exception Error + {ex.Message}";
                commonServices.Status = "Error";
                commonServices.Data = null;
                return commonServices;
            }
        }
        public string DecryptPassword(string encryptedPassword, string keyString)
        {
            byte[] key = GetKey(keyString, 32); // Ensure the key is 32 bytes (AES-256)

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key; // Set the AES key
                aesAlg.IV = new byte[16]; // Initialization Vector, set to 0 for simplicity (NOT recommended for production)

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(encryptedPassword)))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd(); // Return the decrypted string
                        }
                    }
                }
            }
        }
        public async Task<string> VerifyingResponse(string userLogin, string Password)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@pLoginName", userLogin);
                parameters.Add("@pPassword", Password);
                parameters.Add("@responseMessage", dbType: DbType.String, direction: ParameterDirection.Output, size: 250); // Output parameter

                await _connection.ExecuteAsync("[dbo].[uspLogin]", parameters, commandType: CommandType.StoredProcedure);

                string responseMessage = parameters.Get<string>("@responseMessage");

                if (responseMessage == "User successfully logged in")
                {
                    return responseMessage;
                }
                else
                {
                    return responseMessage;
                }

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        //public byte[] UserEncryptedReponsone(UserLogin_Model userModel, string keyString)
        //{
        //    throw new NotImplementedException();
        //}

        //Task<CommonServicesModel<UserLoginModel>> IAuthService.GetUserDetailsWhenLoginIn(string username, string password)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
