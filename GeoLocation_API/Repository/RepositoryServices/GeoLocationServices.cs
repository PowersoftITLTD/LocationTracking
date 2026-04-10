using Dapper;
using GeoLocation_API.DB;
using GeoLocation_API.Repository.IRepositoryServices;
using GeoLocation_API.ResponseModel;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GeoLocation_API.Repository.RepositoryServices
{
    public class GeoLocationServices : IGeoLocationServices
    {

        private readonly IConfiguration _configuration;
        private readonly SqlConnection _connection;
        private readonly IDapperDbConnection _dbConnection;
        private readonly FileSettings _fileSettings;
        private readonly HostEnvironment _env;
        private readonly string _api_key;
        public IDapperDbConnection _dapperDbConnection;
        public HttpClient _httpClient;
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        public GeoLocationServices(IConfiguration configuration, SqlConnection sqlConnection, IDapperDbConnection dapperDbConnection, IOptions<FileSettings> fileSettings, IOptions<HostEnvironment> env , HttpClient httpClient)
        {
            _configuration = configuration;
            _connection = sqlConnection;
            _dapperDbConnection = dapperDbConnection;
            _env = env.Value;
            // _settings = options.Value;
            _dapperDbConnection = dapperDbConnection;
            _api_key = _configuration["api_key"];
            _httpClient = httpClient;
        }
        public async Task<ResponseObject> GetUserLocation(decimal? sessionUserId, decimal? businessGroupId , string? startDate,string? endDate)
        {

            DateTime? start = ParseDate(startDate);
            string? strStart = start?.ToString("dd/MM/yyyy" , CultureInfo.InvariantCulture);
            DateTime? end = ParseDate(endDate);
            string? strEnd = end?.ToString("dd/MM/yyyy" , CultureInfo.InvariantCulture);
            var response = new ResponseObject();
            using (var connection = _dapperDbConnection.CreateConnection())
            {
                try
                {
                    var userlocationDetails = new UserLocation_Model();
                    connection.Open();

                    var parameters = new DynamicParameters();
                    parameters.Add("@Session_UserId", sessionUserId, DbType.Decimal);
                    parameters.Add("@Business_GroupId", businessGroupId, DbType.Decimal);
                    parameters.Add("@StartDate", strStart, DbType.DateTime);
                    parameters.Add("@EndDate", strEnd, DbType.DateTime);

                    var result = (await connection.QueryAsync<UserLocation_Model>(
                        "SP_GET_USER_LOCATION",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    )).ToList();

                    foreach (var item in result)
                    {
                        bool isValidCheckoutLocation = item.checkOut_latitude.HasValue && item.checkOut_longitude.HasValue && item.checkOut_latitude != 0 && item.checkOut_longitude != 0;

                        try
                        {
                            if (isValidCheckoutLocation)
                            {

                                var checkoutlocation = await GetStructuredAddressAsync(item.checkOut_latitude, item.checkOut_longitude);
                                userlocationDetails = new UserLocation_Model
                                {
                                    Tran_ID = item.Tran_ID,
                                    Mongo_ID = item.Mongo_ID,
                                    User_id = item.User_id,
                                    Selected_location = item.Selected_location,
                                    Purpose = item.Purpose,
                                    CheckIn_time = item.CheckIn_time,
                                    CheckOut_time = item.CheckOut_time,
                                    Flag = item.Flag,
                                    Latitude = item.Latitude,
                                    Longtude = item.Longtude,
                                    checkout_location = checkoutlocation.FullAddress.ToString(),
                                    checkOut_latitude = item.checkOut_latitude,
                                    checkOut_longitude = item.checkOut_longitude,
                                    Emp_Card_No = item.Emp_Card_No,
                                    Tran_Datetime = item.Tran_Datetime,
                                    CILocation = item.Selected_location,
                                    CoLocation = checkoutlocation.FullAddress.ToString(),
                                    Session_UserId = null,
                                    Business_GroupId = null
                                };
                            }
                            else
                            {
                                userlocationDetails = new UserLocation_Model
                                {
                                    Tran_ID = item.Tran_ID,
                                    Mongo_ID = item.Mongo_ID,
                                    User_id = item.User_id,
                                    Selected_location = item.Selected_location,
                                    Purpose = item.Purpose,
                                    CheckIn_time = item.CheckIn_time,
                                    CheckOut_time = item.CheckOut_time,
                                    Flag = item.Flag,
                                    Latitude = item.Latitude,
                                    Longtude = item.Longtude,
                                    checkout_location = item.checkout_location,
                                    checkOut_latitude = item.checkOut_latitude,
                                    checkOut_longitude = item.checkOut_longitude,
                                    Emp_Card_No = item.Emp_Card_No,
                                    Tran_Datetime = item.Tran_Datetime,
                                    CILocation = item.Selected_location,
                                    CoLocation = item.checkout_location,
                                    Session_UserId = null,
                                    Business_GroupId = null
                                };
                            }
                            //var checkoutlocation = GetAddressFromLatLong(item.checkOut_latitude, item.checkOut_longitude);
                            var resultReponse = await InsertUpdateUserLocation(userlocationDetails);
                            if (resultReponse.Contains("Record Updated Successfully"))
                            {
                                response.Status = "Success";
                                response.Message = "Data Insert successfully";
                                response.Data = result;
                            }
                            else
                            {
                                response.Status = "Error";
                                response.Message = "Insert Failed";
                                response.Data = item;
                                return response;
                            }
                        }
                        catch (Exception ex)
                        {
                            response.Status = "Error";
                            response.Message = ex.Message;
                            response.Data = null;
                            return response;
                        }
                    };
                }
                catch (Exception ex)
                {
                    response.Status = "Error";
                    response.Message = ex.Message;
                    response.Data = null;
                }
                finally
                {
                    connection.Close();
                }
            }
             return response;
        }
        public async Task<string> GetAddressFromLatLong(double? latitude, double? longitude)
        {
            string key = _api_key;
            string url = $"https://maps.googleapis.com/maps/api/geocode/json?latlng={latitude},{longitude}&key={_api_key}";

            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return "Unable to fetch address";

                var json = await response.Content.ReadAsStringAsync();

                var data = JObject.Parse(json);

                var results = data["results"];

                if (results != null && results.HasValues)
                {
                    string address = results[0]["formatted_address"].ToString();
                    return address;
                }

                return "No Address Found";
            }
        }
        public async Task<string> InsertUpdateUserLocation(UserLocation_Model model)
        {
            using (var connection = _dapperDbConnection.CreateConnection())
            {
                try
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@Tran_ID", model.Tran_ID, DbType.Decimal);
                    parameters.Add("@Mongo_ID", model.Mongo_ID, DbType.String);
                    parameters.Add("@User_id", model.User_id, DbType.Decimal);
                    parameters.Add("@Selected_location", model.Selected_location, DbType.String);
                    parameters.Add("@Purpose", model.Purpose, DbType.String);
                    parameters.Add("@CheckIn_time", model.CheckIn_time, DbType.DateTime);
                    parameters.Add("@CheckOut_time", model.CheckOut_time, DbType.DateTime);
                    parameters.Add("@Flag", model.Flag, DbType.String);
                    parameters.Add("@Latitude", model.Latitude, DbType.Double);
                    parameters.Add("@Longtude", model.Longtude, DbType.Double);
                    parameters.Add("@checkout_location", model.checkout_location, DbType.String);
                    parameters.Add("@checkOut_latitude", model.checkOut_latitude, DbType.Double);
                    parameters.Add("@checkOut_longitude", model.checkOut_longitude, DbType.Double);
                    parameters.Add("@Emp_Card_No", model.Emp_Card_No, DbType.Decimal);
                    parameters.Add("@Tran_Datetime", model.Tran_Datetime, DbType.DateTime);
                    parameters.Add("@CILocation", model.CILocation, DbType.String);
                    parameters.Add("@CoLocation", model.CoLocation, DbType.String);

                    // Optional (currently commented in SP, still safe to pass)
                    parameters.Add("@Session_UserId", model.Session_UserId, DbType.Decimal);
                    parameters.Add("@Business_GroupId", model.Business_GroupId, DbType.Decimal);

                    // Output parameter
                    parameters.Add("@ResponseMessage", dbType: DbType.String, size: 500, direction: ParameterDirection.Output);

                    await connection.ExecuteAsync(
                        "sp_InsertUpdate_UserLocation",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    return parameters.Get<string>("@ResponseMessage");
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }
        }
        public async Task<List<UserLocationExportModel>> GetUserLocationList(decimal? sessionUserId, decimal? businessGroupId)
        {
            using (var connection = _dapperDbConnection.CreateConnection())
            {
                connection.Open();
                var parameters = new DynamicParameters();
                parameters.Add("@Session_UserId", sessionUserId, DbType.Decimal);
                parameters.Add("@Business_GroupId", businessGroupId, DbType.Decimal);
                var result = await connection.QueryAsync<UserLocationExportModel>("SP_GET_USER_LOCATION", commandType: CommandType.StoredProcedure);
                return result.ToList();
            }
        }
        public byte[] GenerateExcel(List<UserLocationExportModel> data)
        {
            // Replace this line:
            // ExcelPackage.LicenseContext = System.ComponentModel.LicenseContext.NonCommercial;

            // With this line:
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            //ExcelPackage.LicenseContext = System.ComponentModel.LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("UserLocation");
                // Header
                worksheet.Cells[1, 1].Value = "Tran_ID";
                worksheet.Cells[1, 2].Value = "Mongo_ID";
                worksheet.Cells[1, 3].Value = "User_id";
                worksheet.Cells[1, 4].Value = "Location";
                worksheet.Cells[1, 5].Value = "Purpose";
                worksheet.Cells[1, 6].Value = "CheckInTime";
                worksheet.Cells[1, 7].Value = "CheckOutTime";
                worksheet.Cells[1, 8].Value = "Flag";
                worksheet.Cells[1, 9].Value = "Latitude";
                worksheet.Cells[1, 10].Value = "Longtude";
                worksheet.Cells[1, 11].Value = "checkout_location";
                worksheet.Cells[1, 12].Value = "checkOut_latitude";
                worksheet.Cells[1, 13].Value = "checkOut_longitude";
                worksheet.Cells[1, 14].Value = "Emp_Card_No";
                worksheet.Cells[1, 15].Value = "Tran_Datetime";
                worksheet.Cells[1, 16].Value = "CILocation";
                worksheet.Cells[1, 17].Value = "CoLocation";

                int row = 2;

                foreach (var item in data)
                {
                    worksheet.Cells[row, 1].Value = item.Tran_ID;
                    worksheet.Cells[row, 2].Value = item.Mongo_ID;
                    worksheet.Cells[row, 3].Value = item.User_id;
                    worksheet.Cells[row, 4].Value = item.Selected_location;
                    worksheet.Cells[row, 5].Value = item.Purpose;
                    worksheet.Cells[row, 6].Value = item.CheckIn_time;
                    worksheet.Cells[row, 7].Value = item.CheckOut_time;
                    worksheet.Cells[row, 8].Value = item.Flag;
                    worksheet.Cells[row, 9].Value = item.Latitude;
                    worksheet.Cells[row, 10].Value = item.Longtude;
                    worksheet.Cells[row, 11].Value = item.checkout_location;
                    worksheet.Cells[row, 12].Value = item.checkOut_latitude;
                    worksheet.Cells[row, 13].Value = item.checkOut_longitude;
                    worksheet.Cells[row, 14].Value = item.Emp_Card_No;
                    worksheet.Cells[row, 15].Value = item.Tran_Datetime;
                    worksheet.Cells[row, 16].Value = item.CILocation;
                    worksheet.Cells[row, 17].Value = item.CoLocation;

                    row++;
                }

                worksheet.Cells.AutoFitColumns();

                return package.GetAsByteArray();
            };

            
        
        }
        //public async Task<AddressModel> GetStructuredAddressAsync(double? lat, double? lng)
        //{
        //    string url = $"https://nominatim.openstreetmap.org/reverse?lat={lat}&lon={lng}&format=json";

        //    _httpClient.DefaultRequestHeaders.Add("User-Agent", "MyApp");

        //    var response = await _httpClient.GetAsync(url);
        //    var json = await response.Content.ReadAsStringAsync();

        //    using JsonDocument doc = JsonDocument.Parse(json);
        //    var root = doc.RootElement;

        //    if (!root.TryGetProperty("address", out var addressProp))
        //    {
        //        return new AddressModel { FullAddress = "Address not found" };
        //    }

        //    return new AddressModel
        //    {
        //        FullAddress = root.TryGetProperty("display_name", out var full) ? full.GetString() : "",
        //        City = addressProp.TryGetProperty("city", out var city) ? city.GetString() : "",
        //        State = addressProp.TryGetProperty("state", out var state) ? state.GetString() : ""
        //    };
        //}
        public async Task<AddressModel> GetStructuredAddressAsync(double? lat, double? lng)
        {
            if (lat == null || lng == null)
                throw new ArgumentNullException("Coordinates must not be null.");

            string url = $"https://nominatim.openstreetmap.org/reverse?lat={lat}&lon={lng}&format=json";

            // ✅ Use HttpRequestMessage to manually set headers
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            // ✅ Clear and force set User-Agent
            request.Headers.TryAddWithoutValidation("User-Agent", "GeoLocationApp/1.0 (outsource1@powersoft.in)");
            request.Headers.TryAddWithoutValidation("Referer", "https://myapp.com");
            request.Headers.TryAddWithoutValidation("Accept", "application/json");
            request.Headers.TryAddWithoutValidation("Accept-Language", "en");
            await Task.Delay(2000);
            HttpResponseMessage response = await _httpClient.SendAsync(request);

            // ✅ Handle 429 Too Many Requests
            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                // Wait 2 seconds and retry once
                await Task.Delay(2000);
                response = await _httpClient.SendAsync(request);
            }

            // ✅ Clear error message for 403
            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                throw new Exception("403 Forbidden — Nominatim blocked the request. Check User-Agent header.");

            //response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();

            using JsonDocument doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("error", out var error))
                throw new Exception($"Nominatim error: {error.GetString()}");

            var addressProp = root.GetProperty("address");

            return new AddressModel
            {
                FullAddress = root.TryGetProperty("display_name", out var dn) ? dn.GetString() ?? "" : "",
                Road = GetAddressField(addressProp, "road", "pedestrian", "footway"),
                City = GetAddressField(addressProp, "city", "town", "village", "suburb", "county"),
                State = GetAddressField(addressProp, "state"),
                Country = GetAddressField(addressProp, "country"),
                PostCode = GetAddressField(addressProp, "postcode")
            };
        }
        private string GetAddressField(JsonElement addressProp, params string[] keys)
        {
            foreach (var key in keys)
            {
                if (addressProp.TryGetProperty(key, out var val))
                {
                    var str = val.GetString();
                    if (!string.IsNullOrEmpty(str))
                        return str;
                }
            }
            return "";
        }
        private DateTime? ParseDate(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            input = input.Replace("{", "").Replace("}", "");

            string[] formats = new[]
            {
        "dd-MM-yyyy",
        "yyyy-MM-dd",
        "yyyy-MM-dd"
    };

            if (DateTime.TryParseExact(input, formats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime parsed))
            {
                return parsed;
            }

            // fallback (ISO etc.)
            if (DateTime.TryParse(input, out parsed))
                return parsed;

            throw new Exception("Invalid date format: " + input);
        }
    }
}
