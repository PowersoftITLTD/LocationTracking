using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoLocation_ConsoleApplication.ModelResponse
{
    public class AddressModel
    {
        public string FullAddress { get; set; }
        public string Road { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string PostCode { get; set; }
    }

    public class UserLocation_Model
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
        public Decimal? Session_UserId { get; set; }
        public Decimal? Business_GroupId { get; set; }
    }

    public class ResponseObject
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public object Data { get; set; } // You can make Data generic if needed
    }
}
