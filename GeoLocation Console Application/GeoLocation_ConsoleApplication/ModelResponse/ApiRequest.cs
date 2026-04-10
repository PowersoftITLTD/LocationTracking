using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoLocation_ConsoleApplication.ModelResponse
{
    public class ApiRequest
    {
        public int session_userId { get; set; }
        public int businessGroupId { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }
        public string empCardNo { get; set; }
    }
}
