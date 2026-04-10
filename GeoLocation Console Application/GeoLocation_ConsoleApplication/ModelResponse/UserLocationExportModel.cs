using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoLocation_ConsoleApplication.ModelResponse
{
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

    public class TempRADetails_Model
    {
        public string RA1 { get; set; }
        public string RA1_Email { get; set; }

        public decimal Tran_ID { get; set; }
        public decimal emp_card_no { get; set; }

        public string emp_name { get; set; }
        public string email_id { get; set; }

        public string selected_location { get; set; }
        public string purpose { get; set; }

        public string checkin_Date { get; set; }
        public string checkin_time { get; set; }
        public string CILocation { get; set; }

        public string checkout_Date { get; set; }
        public string checkout_time { get; set; }
        public string CoLocation { get; set; }

        public double? Latitude { get; set; }
        public double? Longtude { get; set; }

        public double? checkOut_latitude { get; set; }
        public double? checkOut_longitude { get; set; }
    }

    public class RAHeader_Model
    {
        public string RA1 { get; set; }
        public string RA1_Email { get; set; }
    }

    public class TempRAGroup_Model
    {
        public string RA1 { get; set; }
        public string RA1_Email { get; set; }

        public List<TempRADetails_Model> Employees { get; set; }
    }
}
