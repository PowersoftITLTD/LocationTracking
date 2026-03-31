namespace GeoLocation_API.ResponseModel
{
    public class UserLocation_Model
    {
        public decimal Tran_ID { get; set; }

        public string? Mongo_ID { get; set; }

        public decimal User_id { get; set; }

        public string? Selected_location { get; set; }

        public string? Purpose { get; set; }

        public DateTime? CheckIn_time { get; set; }

        public DateTime? CheckOut_time { get; set; }

        public string? Flag { get; set; }

        public double? Latitude { get; set; }

        public double? Longtude { get; set; }

        public string? checkout_location { get; set; }

        public double? checkOut_latitude { get; set; }

        public double? checkOut_longitude { get; set; }

        public decimal Emp_Card_No { get; set; }

        public DateTime Tran_Datetime { get; set; }

        public string? CILocation { get; set; }

        public string? CoLocation { get; set; }
        public Decimal? Session_UserId { get; set; }
        public Decimal? Business_GroupId { get; set; }
    }

    public class UserLocationExportModel
    {
        public decimal Tran_ID { get; set; }
        public string Mongo_ID { get; set; }
        public decimal User_id { get; set; }
        public string Selected_location { get; set; }
        public string Purpose { get; set; }
        public DateTime? CheckIn_time { get; set; }
        public DateTime? CheckOut_time { get; set; }
        public string Flag { get; set; }
        public double? Latitude { get; set; }
        public double? Longtude { get; set; }
        public string checkout_location { get; set; }
        public double? checkOut_latitude { get; set; }
        public double? checkOut_longitude { get; set; }
        public decimal Emp_Card_No { get; set; }
        public DateTime Tran_Datetime { get; set; }
        public string CILocation { get; set; }
        public string CoLocation { get; set; }
    }
}
