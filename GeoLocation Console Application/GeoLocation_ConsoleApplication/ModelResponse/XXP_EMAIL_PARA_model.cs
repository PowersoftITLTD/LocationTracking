using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoLocation_ConsoleApplication.ModelResponse
{
    public class XXP_EMAIL_PARA_model
    {
        public decimal MKEY { get; set; }

        public string MAIL_TYPE { get; set; }
        public string MAIL_FROM { get; set; }
        public string MAIL_DISPLAY_NAME { get; set; }
        public string MAIL_PRIORITY { get; set; }

        public string SMTP_PORT { get; set; }
        public string SMTP_HOST { get; set; }
        public string SMTP_PASS { get; set; }
        public string SMTP_ESSL { get; set; }
        public string SMTP_TIMEOUT { get; set; }

        public string MAIL1 { get; set; }
        public string MAIL2 { get; set; }
        public string MAIL3 { get; set; }

        public decimal? CREATED_BY { get; set; }
        public DateTime? CREATION_DATE { get; set; }
        public DateTime? LAST_UPDATE_DATE { get; set; }
        public decimal? LAST_UPDATED_BY { get; set; }

        public string DELETE_FLAG { get; set; }

        public string ATTRIBUTE1 { get; set; }
        public string ATTRIBUTE2 { get; set; }
        public string ATTRIBUTE3 { get; set; }
        public string ATTRIBUTE4 { get; set; }
        public string ATTRIBUTE5 { get; set; }
        public string ATTRIBUTE6 { get; set; }
        public string ATTRIBUTE7 { get; set; }
        public string ATTRIBUTE8 { get; set; }
        public string ATTRIBUTE9 { get; set; }
        public string ATTRIBUTE10 { get; set; }
        public string ATTRIBUTE11 { get; set; }
        public string ATTRIBUTE12 { get; set; }
        public string ATTRIBUTE13 { get; set; }
        public string ATTRIBUTE14 { get; set; }
        public string ATTRIBUTE15 { get; set; }
    }

    public class  Email_InputResponse
    {
        public decimal MKEY { get; set; }
        public string MAIL_TYPE { get; set; }
    }
}
