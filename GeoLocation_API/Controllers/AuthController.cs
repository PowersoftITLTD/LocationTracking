using GeoLocation_API.Repository.IRepositoryServices;
using GeoLocation_API.ResponseModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace GeoLocation_API.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;
        private readonly string keyString;
        private readonly ICommonServices _commonService;

        public AuthController(IAuthService authService, IConfiguration configuration, SqlConnection sqlConnection, ICommonServices commonService)
        {
            _authService = authService;
            _configuration = configuration;
            // _commonService = commonServices;
            keyString = _configuration["EncryptionKey"];
            _commonService = commonService;
        }

        //[HttpPost("login")]
        //public async Task<IActionResult> Login([FromBody] jsonEncryptModel jsonEncrypt)   //UserModel userModel  //jsonEncryptModel jsonEncrypt
        //{
        //    var keyString = _configuration["EncryptionKey"];
        //    var responseObject = new ResponseObject();
        //    // var loginecrypt = _commonService.EncryptionObje<UserLogin_Model>(jsonEncrypt, keyString);
        //    //var loginecrypt = _commonService.EncryptionObje<UserLogin_Model>(userLogin, keyString);
        //    var userModel = _commonService.DecryptObject<UserLogin_Model>(jsonEncrypt.jsonEncrypt, keyString);
        //    //var userModel = _commonService.DecryptObject<UserLogin_Model>(loginecrypt, keyString);

        //    try
        //    {
        //        if (userModel == null || string.IsNullOrEmpty(userModel.UserName) || string.IsNullOrEmpty(userModel.Password))
        //        {
        //            responseObject.Status = "Error";
        //            responseObject.Message = "Please enter a valid username and password.";
        //            return Ok(responseObject);
        //            //return Ok(new { message = "Please Entry Valide User & Password " });

        //        }
        //        var token = await _authService.Authenticate(userModel.UserName, userModel.Password);
        //        var UserEncrypted = _authService.UserEncryptedReponsone(userModel, keyString);
        //        var UserDetails = await _authService.GetUserDetailsWhenLoginIn(userModel.UserName, userModel.Password);
        //        var userEncryptedDetails = _commonService.EncryptionObje<UserLoginModel>(UserDetails.Data, keyString);
        //        var userDeEncryptedDetails = _commonService.DecryptObject<UserLoginModel>(userEncryptedDetails, keyString);
        //        if (token == null || token == "Invalid login name or password")
        //        {
        //            responseObject.Status = "Error";
        //            responseObject.Message = "Invalid username or password.";
        //            return Unauthorized(responseObject);
        //        }
        //        if (UserEncrypted == null)
        //        {
        //            responseObject.Status = "Error";
        //            responseObject.Message = "Invalid user details.";
        //            return Ok(responseObject);
        //        }
        //        //var responseObject = new { status = "Ok", Message = "Token Generate Successfully", Token = token, UserEncryptedDetails = UserEncrypted };
        //        //return Ok(new { Token= token , UserEncryptedDetails = UserEncrypted  ,Status= "OK"});
        //        //return Ok(responseObject);
        //        responseObject.Status = "Ok";
        //        responseObject.Message = "Token generated successfully.";
        //        responseObject.Data = new { Token = token, UserEncryptedDetails = UserEncrypted, User = userEncryptedDetails };
        //        return Ok(responseObject);

        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = "An error occurred", error = ex.Message });
        //    }
        //}

        //[Authorize]
        ////[AllowAnonymous]
        //[HttpGet("UserDecryptedPasswordVerifying")]

        //public async Task<IActionResult> UserDecryptedPasswordVerifying(string Password)
        //{
        //    var responseObject = new ResponseObject();
        //    var userModel = new UserLogin_Model();
        //    try
        //    {
        //        //var Passwordhash = Convert.ToByte(Password);
        //        var keyString = _configuration["EncryptionKey"];
        //        var PassworsHash = _authService.DecryptPassword(Password, keyString);
        //        if (PassworsHash != null)
        //        {
        //            string[] strDatat = PassworsHash.Split(':');
        //            if (strDatat.Length >= 0)
        //            {
        //                userModel = new UserLogin_Model
        //                {
        //                    UserName = strDatat[0],
        //                    Password = strDatat[1]
        //                };
        //            }
        //        }
        //        var result = await _authService.VerifyingResponse(userModel.UserName, userModel.Password);
        //        if (result == "User successfully logged in")
        //        {
        //            responseObject.Status = "Ok";
        //            responseObject.Message = "User successfully Decrypted logged in Credential";
        //            responseObject.Data = userModel;
        //            //return Ok(new { Message = userModel, Status = "Ok" });
        //            return Ok(responseObject);
        //        }
        //        else
        //        {
        //            responseObject.Status = "Error";
        //            responseObject.Message = result;
        //            responseObject.Data = userModel;
        //            //return Ok(new { Message = userModel, Status = "Ok" });
        //            return Ok(responseObject);
        //        }

        //        //return Ok(new { message = result, Status = "Ok" });
        //    }
        //    catch (Exception ex)
        //    {

        //        responseObject.Status = "Error";
        //        responseObject.Message = "An error occurred during login/registration.";
        //        responseObject.Data = new { error = ex.Message };
        //        return StatusCode(500, responseObject);
        //        //throw;
        //        // return StatusCode(500, new { message = "An error occurred", error = ex.Message });
        //    }
        //}
    }
}
