using GeoLocation_API.Repository.IRepositoryServices;
using GeoLocation_API.ResponseModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace GeoLocation_API.Controllers
{
    public class GeoLocationControllerr : Controller
    {
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;
        private readonly IGeoLocationServices _geolocation;
        private readonly string keyString;
        //private readonly ICommonServices _commonService;
        //private readonly IBrokerSupportService _brokerSupportService;

        public GeoLocationControllerr(IAuthService authService, IConfiguration configuration, SqlConnection sqlConnection , IGeoLocationServices geolocation)
        {
            _authService = authService;
            _configuration = configuration;
            // _commonService = commonServices;
            keyString = _configuration["EncryptionKey"];
            _geolocation = geolocation;
        }

        //[HttpGet("Get-Geolocation")]
        //public IActionResult Geolocation([FromBody] User_LocationDetails user_Location)  // [FromBody] CommoninputResponse commoninput
        //{
        //    var responseObject = new ResponseObject();
        //    try
        //    {
        //        //var loginecrypt = _commonService.EncryptionObje<CommoninputResponse>(commoninput, keyString);
        //        //var userModel = _geolocation.DecryptObject<CommoninputResponse>(jsonEncrypt.jsonEncrypt, keyString);   //jsonEncrypt.jsonEncrypt
        //        //int? session_user_id = userModel.UserId > 0 ? userModel.UserId : null;
        //        //int? business_group_id = userModel.BusinessGroupId > 0 ? userModel.BusinessGroupId : null;
        //        CommoninputResponse commoninput = new CommoninputResponse
        //        {
        //            Session_userId = user_Location.Session_userId > 0 ? user_Location.Session_userId : 2693, // Replace with actual user ID
        //            BusinessGroupId = user_Location.BusinessGroupId > 0 ? user_Location.BusinessGroupId : 1, // Replace with actual business group ID
        //        };
        //        var response =  _geolocation.GetUserLocation(commoninput.Session_userId, commoninput.BusinessGroupId, user_Location.StartDate, user_Location.EndDate);
        //        if (response == null)
        //        {
        //            responseObject.Status = "Ok";
        //            responseObject.Message = "No Data Found";
        //            responseObject.Data = response.Result.Data;
        //        }
        //        else
        //        {
        //            //var userEncryptedDetails = _commonService.EncryptionObje<IEnumerable<TblProject_DD_Model>>(response, keyString);
        //           // var userDeEncryptedDetails = _commonService.DecryptObject<IEnumerable<TblProject_DD_Model>>(userEncryptedDetails, keyString);
        //            responseObject.Status = "Ok";
        //            responseObject.Message = "Data Found Successfully";
        //            responseObject.Data = response.Result.Data;
        //        }
        //        return Ok(responseObject);
        //    }
        //    catch (Exception ex)
        //    {
        //        responseObject.Status = "Error";
        //        responseObject.Message = $"An error occurred during {ex.Message}";
        //        responseObject.Data = new { error = ex.Message };
        //        return StatusCode(500, responseObject);
        //    }
        //}

  [HttpPost("Update_UserLocation")]
        public async Task<IActionResult> PostUser_LocationDetails([FromBody] User_LocationDetails user_Location)
        {
            var responseObject = new ResponseObject();
            try
            {
                CommoninputResponse commoninput = new CommoninputResponse
                {
                    Session_userId = user_Location.Session_userId > 0 ? user_Location.Session_userId : 2693, // Replace with actual user ID
                    BusinessGroupId = user_Location.BusinessGroupId > 0 ? user_Location.BusinessGroupId : 1, // Replace with actual business group ID
                };
                var response = await _geolocation.GetUserLocation(commoninput.Session_userId, commoninput.BusinessGroupId ,user_Location.StartDate ,user_Location.EndDate);
                if (response == null)
                {
                    responseObject.Status = "Ok";
                    responseObject.Message = "No Data Found";
                    responseObject.Data = response;
                }
                else
                {
                    //var userEncryptedDetails = _commonService.EncryptionObje<IEnumerable<TblProject_DD_Model>>(response, keyString);
                    // var userDeEncryptedDetails = _commonService.DecryptObject<IEnumerable<TblProject_DD_Model>>(userEncryptedDetails, keyString);
                    responseObject.Status = "Ok";
                    responseObject.Message = "Data Save Successfully";
                    responseObject.Data = response;
                }
                var data = _geolocation.GetUserLocationList(commoninput.Session_userId ,commoninput.BusinessGroupId);
                //foreach(var item in data.Result)
                //{
                //    double lat = item.checkOut_latitude ?? 0.0;
                //    double lng = item.checkOut_longitude ?? 0.0;
                //    var outlocation = await _geolocation.GetStructuredAddressAsync(lat , lng);
                //}
                
                var excelBytes = _geolocation.GenerateExcel(data.Result);
               
                
                return File(
                excelBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "UserLocation.xlsx"
            );
            }
            catch(Exception ex)
            {
                responseObject.Status = "Error";
                responseObject.Message = $"An error occurred during {ex.Message}";
                responseObject.Data = new { error = ex.Message };
                return StatusCode(500, responseObject);
            }
        }
    }
}
