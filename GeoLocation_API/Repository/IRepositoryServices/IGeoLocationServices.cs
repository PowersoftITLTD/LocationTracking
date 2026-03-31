using GeoLocation_API.ResponseModel;

namespace GeoLocation_API.Repository.IRepositoryServices
{
    public interface IGeoLocationServices
    {
        Task<ResponseObject> GetUserLocation(decimal? sessionUserId, decimal? businessGroupId, string? startDate, string? endDate);
        Task<List<UserLocationExportModel>> GetUserLocationList(decimal? sessionUserId, decimal? businessGroupId);
        byte[] GenerateExcel(List<UserLocationExportModel> data);
        Task<AddressModel> GetStructuredAddressAsync(double? lat, double? lng);
    }
}
