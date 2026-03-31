namespace GeoLocation_API.ResponseModel
{
    public class CommonServicesModel<T>
    {
        public string? Status { get; set; }
        public string? Message { get; set; }
        public T Data { get; set; }
    }
    public class ExcelFileSettings
    {
        public string BaseFolder { get; set; }
    }
    public class HostEnvironment
    {
        public string env { get; set; }
    }
    public class FileSettings
    {
        public string FilePath { get; set; }
    }

    public class jsonEncryptModel
    {
        public string jsonEncrypt { get; set; }
    }

    public class ResponseObject
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public object Data { get; set; } // You can make Data generic if needed
    }


    public class Subcategory_inputResponse
    {
        public int? UserId { get; set; }
        public int? BusinessGroupId { get; set; }
        public int? categoryId { get; set; }
    }


    public class User_LocationDetails
    {
        public int? Session_userId { get; set;}
        public int? BusinessGroupId { get; set;}

        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
    }
}
