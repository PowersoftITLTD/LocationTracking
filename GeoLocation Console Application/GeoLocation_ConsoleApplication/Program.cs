using Dapper;
using GeoLocation_ConsoleApplication.ModelResponse;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GeoLocation_ConsoleApplication
{
    public class Program
    {
        static HttpClient _httpClient = new HttpClient();
        static async Task Main(string[] args)
        {
            try
            {
                string baseUrl = ConfigurationManager.AppSettings["BaseUrl"];
                DateTime startDateDt = DateTime.UtcNow.Date.AddDays(-1); // Yesterday 00:00:00
                DateTime endDateDt = DateTime.UtcNow.Date;               // Today 00:00:00

                string startDate = startDateDt.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                //string startDate = "26/03/2026"; //startDateDt.ToString("yyyy-MM-dd HH:mm:ss");
                string endDate = endDateDt.ToString("dd/MM/yyyy",CultureInfo.InvariantCulture);

                //string endDate = "26/03/2026";

                var request = new ApiRequest
                {
                    session_userId = 13,
                    businessGroupId = 1,
                    startDate = startDate,
                    endDate = endDate ,
                    empCardNo= null
                };

                var emailconfig = new Email_InputResponse { MKEY = 0, MAIL_TYPE = null };
                var Email_Para = await GetEmailConfigAsync(emailconfig.MKEY, emailconfig.MAIL_TYPE);
                string fromEmail = Email_Para.FirstOrDefault()?.MAIL_FROM ?? null;
                string email_pass = Email_Para.FirstOrDefault()?.SMTP_PASS ?? null;
                string email_host = Email_Para.FirstOrDefault()?.SMTP_HOST ?? null;


                var responseResult = await GetUserLocation(request.session_userId, request.businessGroupId, request.startDate, request.endDate);
                if (responseResult.Status != "SUCCESS" || responseResult.Data == null)
                {
                    Console.WriteLine("Error: " + responseResult.Message);
                    //return;
                }

                var result=  await GetRAWiseData(request.startDate ,request.endDate ,request.empCardNo);

                foreach (var user in result)
                {
                    //string body = BuildEmailBody("John Doe Test", user.CheckIn_time.ToString(), user.Selected_location ,user.CheckOut_time.ToString(),user.checkout_location);
                    string body = BuildEmailBody(user.RA1,request.startDate ,request.endDate, user.Employees);
                    List<string> ccList = new List<string> {};  // user.RA1_Email   //,  "narendrakumar.soni@powersoft.in", "hitesh.ghadage@powersoft.in" 
                    List<string> bccList = new List<string> {};
                   // SendEmail(user.RA1_Email, $"HUb Connect Report For {request.startDate}", body, ccList, bccList, null);
                    SendEmail(user.RA1_Email, $"Hub Connect Report For {request.startDate}", body, ccList, bccList, null ,fromEmail ,email_pass ,email_host );
                }

                //"narendrakumar.soni@powersoft.in"

                //Console.ReadLine();
                //using (var client = new HttpClient())
                //{
                //    client.BaseAddress = new Uri(baseUrl);
                //    var json = JsonConvert.SerializeObject(request);
                //    var content = new StringContent(json, Encoding.UTF8, "application/json");
                //    var response = await client.PostAsync(apiUrl, content);
                //    if (response.IsSuccessStatusCode)
                //    {
                //        var result = await response.Content.ReadAsStringAsync();
                //        Console.WriteLine("✅ Success: GeoLocation CheckOut Location Update Successfully");
                //        Console.WriteLine(result);
                //    }
                //    else
                //    {
                //        Console.WriteLine($"❌ Error: {response.StatusCode}");
                //        var error = await response.Content.ReadAsStringAsync();
                //        Console.WriteLine(error);
                //    }

                //}
            }
            catch (Exception ex)
            {

                string folderPath = @"D:\Logs";
                string filePath = Path.Combine(folderPath, $"ErrorLog_{DateTime.Now:yyyyMMdd}.txt");
                // Ensure directory exists
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("--------------------------------------------------");
                sb.AppendLine($"Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($" Error Message exception in Main Method : {ex.Message}");
                sb.AppendLine($"StackTrace: {ex.StackTrace}");
                sb.AppendLine("--------------------------------------------------");

                File.AppendAllText(filePath, sb.ToString());
                Console.WriteLine($"Exception: Due to {ex.Message}");
                Console.WriteLine(ex.Message);
            }
        }


        //public static void SendEmail(string toemail ,string subject, string  body)
        //{
        //    try
        //    {
        //        var fromEmail = "autosupport@powersoft.in";       // Sender Email
        //        var password = " hjdbdb jnittso";           // App Password (Important)
        //        var smtpClient = new SmtpClient("smtp.gmail.com")
        //        {
        //            Port = 587,
        //            Credentials = new NetworkCredential(fromEmail, password),
        //            EnableSsl = true,
        //        };

        //        var mailMessage = new MailMessage
        //        {
        //            From = new MailAddress(fromEmail),
        //            Subject = subject,
        //            Body = body,
        //            IsBodyHtml = true, // set false if plain text
        //        };
        //        mailMessage.To.Add(toemail);
        //        smtpClient.Send(mailMessage);
        //        Console.WriteLine("✅ Email Sent Successfully");
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("❌ Error: " + ex.Message);
        //    }
        //}

        public static async Task SendEmail(string toEmail,string subject,string body,List<string> ccEmails = null,List<string> bccEmails = null,List<string> attachmentPaths = null, string from = null , string Mail_Pass = null ,string SMTP_Host = null )
        {
            try
            {
                var fromEmail = from;       
                var password = Mail_Pass;            
                var smtpHost = SMTP_Host;
                // ── SMTP Client ─────────────────────────────────────────────────
                var smtpClient = new SmtpClient(smtpHost)
                {
                    Port = 587,
                    Credentials = new NetworkCredential(fromEmail, password),
                    EnableSsl = true,
                };

                // ── Mail Message ─────────────────────────────────────────────────
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true,   // Set false if plain text
                };

                // ── To (Required) ────────────────────────────────────────────────
                mailMessage.To.Add(toEmail);

                // ── CC (Optional) — only added when list is non-null and non-empty ──
                if (ccEmails != null && ccEmails.Count > 0)
                {
                    foreach (var cc in ccEmails)
                    {
                        if (!string.IsNullOrWhiteSpace(cc))
                            mailMessage.CC.Add(cc);
                    }
                }

                // ── BCC (Optional) — only added when list is non-null and non-empty ──
                if (bccEmails != null && bccEmails.Count > 0)
                {
                    foreach (var bcc in bccEmails)
                    {
                        if (!string.IsNullOrWhiteSpace(bcc))
                            mailMessage.Bcc.Add(bcc);
                    }
                }

                // ── Attachments (Optional) — only added when list is non-null and non-empty ──
                if (attachmentPaths != null && attachmentPaths.Count > 0)
                {
                    foreach (var filePath in attachmentPaths)
                    {
                        if (!string.IsNullOrWhiteSpace(filePath) && System.IO.File.Exists(filePath))
                        {
                            mailMessage.Attachments.Add(new Attachment(filePath));
                        }
                        else
                        {
                            Console.WriteLine($"⚠️  Attachment skipped (file not found): {filePath}");
                        }
                    }
                }

                // ── Send ─────────────────────────────────────────────────────────
                smtpClient.Send(mailMessage);
                Console.WriteLine("✅ Email Sent Successfully");
            }
            catch (Exception ex)
            {
                string folderPath = @"D:\Logs";
                string filePath = Path.Combine(folderPath, $"ErrorLog_{DateTime.Now:yyyyMMdd}.txt");
                // Ensure directory exists
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("--------------------------------------------------");
                sb.AppendLine($"Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($" Error Message exception in  EMAIL SEND Method : {ex.Message}");
                sb.AppendLine($"StackTrace: {ex.StackTrace}");
                sb.AppendLine("--------------------------------------------------");

                File.AppendAllText(filePath, sb.ToString());
                Console.WriteLine($"Exception: Due to {ex.Message}");
                Console.WriteLine(ex.Message);
                Console.WriteLine("❌ Error: " + ex.Message);
            }
        }

        //public static string BuildEmailBody(string employeeName,string checkInTime,string checkInLocation,string checkOutTime,string checkOutLocation)
        //{
        //    StringBuilder sb = new StringBuilder();

        //    sb.Append("<html>");
        //    sb.Append("<head>");
        //    sb.Append("<style>");
        //    sb.Append("table { border-collapse: collapse; width: 100%; font-family: Arial; }");
        //    sb.Append("th, td { border: 1px solid #ddd; padding: 8px; text-align: center; }");
        //    sb.Append("th { background-color: #4CAF50; color: white; }");
        //    sb.Append("tr:nth-child(even) { background-color: #f2f2f2; }");
        //    sb.Append("</style>");
        //    sb.Append("</head>");
        //    sb.Append("<body>");

        //    sb.Append("<h2>Employee Attendance Details</h2>");

        //    sb.Append("<table>");
        //    sb.Append("<tr>");
        //    sb.Append("<th>Employee Name</th>");
        //    sb.Append("<th>Check-In Time</th>");
        //    sb.Append("<th>Check-In Location</th>");
        //    sb.Append("<th>Check-Out Time</th>");
        //    sb.Append("<th>Check-Out Location</th>");
        //    sb.Append("</tr>");

        //    sb.Append("<tr>");
        //    sb.Append($"<td>{employeeName}</td>");
        //    sb.Append($"<td>{checkInTime}</td>");
        //    sb.Append($"<td>{checkInLocation}</td>");
        //    sb.Append($"<td>{checkOutTime}</td>");
        //    sb.Append($"<td>{checkOutLocation}</td>");
        //    sb.Append("</tr>");

        //    sb.Append("</table>");
        //    sb.Append("</body>");
        //    sb.Append("</html>");

        //    return sb.ToString();
        //}

        public static async Task<List<TempRAGroup_Model>> GetRAWiseData(string fromDate, string toDate, string empCardNo)
        {
            var _connection = ConfigurationManager.ConnectionStrings["defaultConnection"].ConnectionString;
            using (var connection = new SqlConnection(_connection))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@FROM_DATE", fromDate);
                parameters.Add("@TODATE", toDate);
                parameters.Add("@EMPLOYEE_CARD_NO", empCardNo);

                using (var multi = connection.QueryMultiple(
                    "SP_GetRAWise_location",
                    parameters,
                    commandType: CommandType.StoredProcedure))
                {
                    // ✅ First Result Set
                    var raHeaders = multi.Read<RAHeader_Model>().ToList();

                    // ✅ Second Result Set
                    var details = multi.Read<TempRADetails_Model>().ToList();

                    // ✅ Handle NULL safety
                    raHeaders = raHeaders ?? new List<RAHeader_Model>();
                    details = details ?? new List<TempRADetails_Model>();

                    // ✅ Bind Data (Group by RA1)
                    var result = raHeaders.Select(ra => new TempRAGroup_Model
                    {
                        RA1 = ra.RA1 ?? "Unknown",
                        RA1_Email = ra.RA1_Email ?? "",

                        Employees = details
                            .Where(d => d.RA1 == ra.RA1)
                            .ToList()
                    }).ToList();

                    return result;
                }
            }
        }
        public static string BuildEmailBody(string employeeName,string startDate,string endDate , List<TempRADetails_Model> attendanceList)
        {
            string generatedDate = DateTime.Now.ToString("MMMM dd, yyyy");
            StringBuilder sb = new StringBuilder();

            sb.Append("<!DOCTYPE html>");
            sb.Append("<html lang='en'>");
            sb.Append("<head>");
            sb.Append("<meta charset='UTF-8' />");
            sb.Append("<meta name='viewport' content='width=device-width, initial-scale=1.0'/>");
            sb.Append("<meta name='x-apple-disable-message-reformatting'/>");
            sb.Append("<title>Employee Attendance Report</title>");
            sb.Append("</head>");

            // BODY — inline background
            sb.Append("<body style='margin:0;padding:20px;background:#daeaf7;font-family:Arial,Helvetica,sans-serif;color:#3b5068;'>");

            // Outer wrapper table — use TABLE layout, not div, for email clients
            sb.Append("<table width='100%' border='0' cellpadding='0' cellspacing='0' style='max-width:1100px;margin:0 auto;'>");
            sb.Append("<tr><td style='background:#ffffff;border-radius:8px;overflow:hidden;box-shadow:0 4px 20px rgba(27,79,122,0.12);'>");

            // ── HEADER ──
            sb.Append("<table width='100%' border='0' cellpadding='0' cellspacing='0'>");
            sb.Append("<tr>");
            sb.Append("<td style='background:#1b4f7a;padding:24px 28px 18px 28px;border-bottom:3px solid #5bb8f5;'>");
            sb.Append("<div style='font-size:11px;font-weight:700;letter-spacing:3px;text-transform:uppercase;color:#7ecef5;margin-bottom:6px;'>HUB CONNECT</div>");
            sb.Append("<div style='font-size:22px;font-weight:700;color:#ffffff;line-height:1.2;'>&#10022; Attendance Report &#10022;</div>");
            sb.Append($"<div style='margin-top:5px;font-size:11px;color:#b0d6ef;'>Generated &middot; {startDate}</div>");
            sb.Append("</td>");
            sb.Append("</tr>");
            sb.Append("</table>");

            // ── BODY CONTENT ──
            sb.Append("<table width='100%' border='0' cellpadding='0' cellspacing='0'>");
            sb.Append("<tr>");
            sb.Append("<td style='padding:24px 28px;'>");

            // Intro text  
            sb.Append("<div style='font-size:13px;line-height:1.75;color:#4a6680;margin-bottom:20px;border-left:3px solid #2a7ab8;padding:8px 0 8px 14px;background:#f0f7ff;border-radius:0 4px 4px 0;'>");
            sb.Append($@"<div style='margin-bottom:10px;font-size:13px;color:#1b4f7a;'> Dear <strong>{employeeName}</strong> </div>");
            //sb.Append($"Dear {employeeName} <br/>");
            //sb.Append($"Please find below T your reportee(s) attendance report recorded via the Hub Connect application for the period from  <strong>{startDate}</strong> to <strong>{endDate}</strong>.<br/>");
            sb.Append($"Please find below the attendance details of your reportee(s) recorded via the Hub Connect application for <strong>{startDate}</strong>.<br/>");
            //sb.Append("This record reflects verified check-in and check-out data as captured by the Hub Connect App.");
            sb.Append("This record reflects the verified check-in and check-out location as captured by the Hub Connect App.<br/>");
            //sb.Append("You are requested to verify that attendance was marked from the designated work location / official premises.<br/>");
            //sb.Append("Kindly approve if correct, or reject with reasons for any discrepancy on the ERP.<br/>");
            //sb.Append("Please note that the final attendance will be based on your approval, and no backend changes will be made by HR/Admin.");
            sb.Append("</div>");

            // ── ATTENDANCE TABLE — fully inline styled ──
            sb.Append("<div style='width:100%;overflow-x:auto;-webkit-overflow-scrolling:touch;border-radius:6px;border:1px solid #c8dff0;display:block;'>");
            sb.Append("<table width='100%' border='0' cellpadding='0' cellspacing='0' style='border-collapse:collapse;font-size:12px;min-width:580px;'>");

            // Header row — all inline
            sb.Append("<thead>");
            sb.Append("<tr style='background:#1b4f7a;'>");
            sb.Append("<th style='padding:10px 12px;text-align:left;font-size:10px;font-weight:700;letter-spacing:1.5px;text-transform:uppercase;color:#a8d8f5;white-space:nowrap;'>Employee Name</th>");
            sb.Append("<th style='padding:10px 12px;text-align:left;font-size:10px;font-weight:700;letter-spacing:1.5px;text-transform:uppercase;color:#a8d8f5;white-space:nowrap;'>Check-In Time</th>");
            sb.Append("<th style='padding:10px 12px;text-align:left;font-size:10px;font-weight:700;letter-spacing:1.5px;text-transform:uppercase;color:#a8d8f5;white-space:nowrap;'>Check-In Location</th>");
            sb.Append("<th style='padding:10px 12px;text-align:left;font-size:10px;font-weight:700;letter-spacing:1.5px;text-transform:uppercase;color:#a8d8f5;white-space:nowrap;'>Check-Out Time</th>");
            sb.Append("<th style='padding:10px 12px;text-align:left;font-size:10px;font-weight:700;letter-spacing:1.5px;text-transform:uppercase;color:#a8d8f5;white-space:nowrap;'>Check-Out Location</th>");
            sb.Append("<th style='padding:10px 12px;text-align:left;font-size:10px;font-weight:700;letter-spacing:1.5px;text-transform:uppercase;color:#a8d8f5;white-space:nowrap;'>Purpose</th>");
            sb.Append("</tr>");
            sb.Append("</thead>");

            sb.Append("<tbody>");
            int rowIndex = 0;
            foreach (var item in attendanceList)
            {
                // Alternate row background inline
                string rowBg = (rowIndex % 2 == 0) ? "#ffffff" : "#f3f8fd";
                sb.Append($"<tr style='background:{rowBg};border-bottom:1px solid #ddeaf7;'>");

                // Employee Name
                sb.Append($"<td style='padding:9px 12px;color:#1b4f7a;font-size:12px;font-weight:700;vertical-align:middle;'>{item.emp_name} <br/><span style='font-size:12px;color:#7a9ab8;font-weight:500;'><strong>({item.emp_card_no})<strong/></span></td>");

                // Check-In Time
                sb.Append($"<td style='padding:9px 12px;color:#5a7fa0;font-size:12px;vertical-align:middle;white-space:nowrap;'>{item.checkin_Date} {item.checkin_time}</td>");

                // Check-In Location badge — fully inline
                sb.Append($"<td style='padding:9px 12px;vertical-align:middle;'>" +
                          $"<span style='display:inline-block;background:#d0eaf9;border:1px solid #9ac8e8;border-radius:20px;padding:3px 10px;font-size:10.5px;color:#1b6098;'>&#128205; {item.CILocation}</span>" +
                          $"</td>");

                // Check-Out Time
                sb.Append($"<td style='padding:9px 12px;color:#5a7fa0;font-size:12px;vertical-align:middle;white-space:nowrap;'>{item.checkout_Date} {item.checkout_time}</td>");

                // Check-Out Location badge — fully inline
                sb.Append($"<td style='padding:9px 12px;vertical-align:middle;'>" +
                          $"<span style='display:inline-block;background:#d0eaf9;border:1px solid #9ac8e8;border-radius:20px;padding:3px 10px;font-size:10.5px;color:#1b6098;'>&#128205; {item.CoLocation}</span>" +
                          $"</td>");

                sb.Append($"<td style='padding:9px 12px;vertical-align:middle;'>" +
                          $"<span style='display:inline-block;background:#d0eaf9;border:1px solid #9ac8e8;border-radius:20px;padding:3px 10px;font-size:10.5px;color:#1b6098;'> {item.purpose}</span>" +
                          $"</td>");

                sb.Append("</tr>");
                rowIndex++;
            }

            sb.Append("</tbody>");
            sb.Append("</table>");
            sb.Append("</div>"); // scrollable div

            // Divider
            sb.Append("<div style='height:1px;background:#c2d8ed;margin:20px 0;'></div>");

            // Footer note
            sb.Append("<p >");
            //sb.Append("<span style='background:#ddeaf7;border-top:1px solid #c2d8ed;padding:14px 28px;'>Confidential &middot; For Internal Use Only</span>");
            sb.Append("<span style='font-size:11px;color:#7a9ab8;line-height:1.65;margin:0;'>You are requested to verify that attendance was marked from the designated work location / official premises.Kindly approve if correct, or reject with reasons for any discrepancy on the ERP.Please note that the final attendance will be based on your approval, and no backend changes will be made by HR/Admin.</span>");
            sb.Append("</p>");
            //sb.Append("<p >");
            ////sb.Append("<span style='background:#ddeaf7;border-top:1px solid #c2d8ed;padding:14px 28px;'>Confidential &middot; For Internal Use Only</span>");
            //sb.Append("<span style='font-size:11px;color:#7a9ab8;line-height:1.65;margin:0;'>Kindly approve if correct, or reject with reasons for any discrepancy on the ERP.</span>");
            //sb.Append("</p>");
            //sb.Append("<p >");
            ////sb.Append("<span style='background:#ddeaf7;border-top:1px solid #c2d8ed;padding:14px 28px;'>Confidential &middot; For Internal Use Only</span>");
            //sb.Append("<span style='font-size:11px;color:#7a9ab8;line-height:1.65;margin:0;'>Please note that the final attendance will be based on your approval, and no backend changes will be made by HR/Admin.</span>");
            //sb.Append("</p>");


            sb.Append("<p >");
            //sb.Append("<span style='background:#ddeaf7;border-top:1px solid #c2d8ed;padding:14px 28px;'>Confidential &middot; For Internal Use Only</span>");
            sb.Append("<span style='font-size:11px;color:#7a9ab8;line-height:1.65;margin:0;'>Confidential &middot; For Internal Use Only. <br/> This is an automated notification. Please do not reply directly to this message. For discrepancies, contact your HR Team.</span>");
            sb.Append("</p>");
            //sb.Append("<p >");
            ////sb.Append("<span style='background:#ddeaf7;border-top:1px solid #c2d8ed;padding:14px 28px;'>Confidential &middot; For Internal Use Only</span>");
            //sb.Append("<span style='font-size:11px;color:#7a9ab8;line-height:1.65;margin:0;'>This is an automated notification. Please do not reply directly to this message. For discrepancies, contact your HR Team.</span>");
            //sb.Append("</p>");

            sb.Append("</td>");
            sb.Append("</tr>");
            sb.Append("</table>");

            // ── FOOTER ──
            //sb.Append("<table width='100%' border='0' cellpadding='0' cellspacing='0'>");
            //sb.Append("<tr>");
            //sb.Append("<td style='font-size:11px;color:#7a9ab8;line-height:1.65;margin:0;'>");
            //sb.Append("This is an automated notification. Please do not reply directly to this message. For discrepancies, contact your HR Team.");
            //sb.Append("</td>");
            //sb.Append("</tr>");
            //sb.Append("</table>");

            sb.Append("</td></tr>"); // close wrapper table
            sb.Append("</table>");

            sb.Append("</body>");
            sb.Append("</html>");

            return sb.ToString();
        }

        public static DateTime? ParseDate(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            input = input.Replace("{", "").Replace("}", "");

            string[] formats = new[] { "dd-MM-yyyy", "yyyy-MM-dd", "yyyy-MM-dd" };

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

        public static async Task<List<XXP_EMAIL_PARA_model>> GetEmailConfigAsync(decimal? mkey = null, string mailType = null)
        {
            try
            {
                var _connection = ConfigurationManager.ConnectionStrings["defaultConnection"].ConnectionString;

                using (IDbConnection _dapperDbConnection = new SqlConnection(_connection))
                {
                    mkey = mkey == 0 ? null : mkey;
                    var parameters = new DynamicParameters();
                    parameters.Add("@MKEY", mkey, DbType.Decimal);
                    parameters.Add("@MAIL_TYPE", mailType, DbType.Decimal);

                    var result = await _dapperDbConnection.QueryAsync<XXP_EMAIL_PARA_model>("SP_Get_Email_Para", parameters, commandType: CommandType.StoredProcedure);
                    return result.ToList();
                };

            }
            catch (SqlException ex)
            {
                // SQL Error
                Console.WriteLine("SQL Error: " + ex.Message);
                return new List<XXP_EMAIL_PARA_model>();
            }
            catch (Exception ex)
            {
                // General Error
                Console.WriteLine("Error: " + ex.Message);
                return new List<XXP_EMAIL_PARA_model>();
            }
        }

        // Old Code for Email Body (can be removed after confirming new design works fine in all email clients)

        #region
        public static string BuildEmailBodyold(string employeeName, List<UserLocationExportModel> attendanceList)
        {
            string generatedDate = DateTime.Now.ToString("MMMM dd, yyyy");
            StringBuilder sb = new StringBuilder();

            sb.Append("<!DOCTYPE html>");
            sb.Append("<html lang='en'>");
            sb.Append("<head>");
            sb.Append("<meta charset='UTF-8' />");
            sb.Append("<meta name='viewport' content='width=device-width, initial-scale=1.0'/>");
            sb.Append("<meta name='x-apple-disable-message-reformatting'/>");
            sb.Append("<title>Employee Attendance Report</title>");
            sb.Append("</head>");

            // BODY — inline background
            sb.Append("<body style='margin:0;padding:20px;background:#daeaf7;font-family:Arial,Helvetica,sans-serif;color:#3b5068;'>");

            // Outer wrapper table — use TABLE layout, not div, for email clients
            sb.Append("<table width='100%' border='0' cellpadding='0' cellspacing='0' style='max-width:900px;margin:0 auto;'>");
            sb.Append("<tr><td style='background:#ffffff;border-radius:8px;overflow:hidden;box-shadow:0 4px 20px rgba(27,79,122,0.12);'>");

            // ── HEADER ──
            sb.Append("<table width='100%' border='0' cellpadding='0' cellspacing='0'>");
            sb.Append("<tr>");
            sb.Append("<td style='background:#1b4f7a;padding:24px 28px 18px 28px;border-bottom:3px solid #5bb8f5;'>");
            sb.Append("<div style='font-size:11px;font-weight:700;letter-spacing:3px;text-transform:uppercase;color:#7ecef5;margin-bottom:6px;'>HUB CONNECT</div>");
            sb.Append("<div style='font-size:22px;font-weight:700;color:#ffffff;line-height:1.2;'>&#10022; Attendance Report &#10022;</div>");
            sb.Append($"<div style='margin-top:5px;font-size:11px;color:#b0d6ef;'>Generated &middot; {generatedDate}</div>");
            sb.Append("</td>");
            sb.Append("</tr>");
            sb.Append("</table>");

            // ── BODY CONTENT ──
            sb.Append("<table width='100%' border='0' cellpadding='0' cellspacing='0'>");
            sb.Append("<tr>");
            sb.Append("<td style='padding:24px 28px;'>");

            // Intro text
            sb.Append("<div style='font-size:13px;line-height:1.75;color:#4a6680;margin-bottom:20px;border-left:3px solid #2a7ab8;padding:8px 0 8px 14px;background:#f0f7ff;border-radius:0 4px 4px 0;'>");
            sb.Append("Please find below the attendance log for the specified reporting period. ");
            sb.Append("This record reflects verified check-in and check-out data as captured by the Hub Connect App.");
            sb.Append("</div>");

            // ── ATTENDANCE TABLE — fully inline styled ──
            sb.Append("<div style='width:100%;overflow-x:auto;-webkit-overflow-scrolling:touch;border-radius:6px;border:1px solid #c8dff0;display:block;'>");
            sb.Append("<table width='100%' border='0' cellpadding='0' cellspacing='0' style='border-collapse:collapse;font-size:12px;min-width:580px;'>");

            // Header row — all inline
            sb.Append("<thead>");
            sb.Append("<tr style='background:#1b4f7a;'>");
            sb.Append("<th style='padding:10px 12px;text-align:left;font-size:10px;font-weight:700;letter-spacing:1.5px;text-transform:uppercase;color:#a8d8f5;white-space:nowrap;'>Employee Name</th>");
            sb.Append("<th style='padding:10px 12px;text-align:left;font-size:10px;font-weight:700;letter-spacing:1.5px;text-transform:uppercase;color:#a8d8f5;white-space:nowrap;'>Check-In Time</th>");
            sb.Append("<th style='padding:10px 12px;text-align:left;font-size:10px;font-weight:700;letter-spacing:1.5px;text-transform:uppercase;color:#a8d8f5;white-space:nowrap;'>Check-In Location</th>");
            sb.Append("<th style='padding:10px 12px;text-align:left;font-size:10px;font-weight:700;letter-spacing:1.5px;text-transform:uppercase;color:#a8d8f5;white-space:nowrap;'>Check-Out Time</th>");
            sb.Append("<th style='padding:10px 12px;text-align:left;font-size:10px;font-weight:700;letter-spacing:1.5px;text-transform:uppercase;color:#a8d8f5;white-space:nowrap;'>Check-Out Location</th>");
            sb.Append("</tr>");
            sb.Append("</thead>");

            sb.Append("<tbody>");
            int rowIndex = 0;
            foreach (var item in attendanceList)
            {
                // Alternate row background inline
                string rowBg = (rowIndex % 2 == 0) ? "#ffffff" : "#f3f8fd";
                sb.Append($"<tr style='background:{rowBg};border-bottom:1px solid #ddeaf7;'>");

                // Employee Name
                sb.Append($"<td style='padding:9px 12px;color:#1b4f7a;font-size:12px;font-weight:700;vertical-align:middle;'>{employeeName}</td>");

                // Check-In Time
                sb.Append($"<td style='padding:9px 12px;color:#5a7fa0;font-size:12px;vertical-align:middle;white-space:nowrap;'>{item.CheckIn_time}</td>");

                // Check-In Location badge — fully inline
                sb.Append($"<td style='padding:9px 12px;vertical-align:middle;'>" +
                          $"<span style='display:inline-block;background:#d0eaf9;border:1px solid #9ac8e8;border-radius:20px;padding:3px 10px;font-size:10.5px;color:#1b6098;'>&#128205; {item.Selected_location}</span>" +
                          $"</td>");

                // Check-Out Time
                sb.Append($"<td style='padding:9px 12px;color:#5a7fa0;font-size:12px;vertical-align:middle;white-space:nowrap;'>{item.CheckOut_time}</td>");

                // Check-Out Location badge — fully inline
                sb.Append($"<td style='padding:9px 12px;vertical-align:middle;'>" +
                          $"<span style='display:inline-block;background:#d0eaf9;border:1px solid #9ac8e8;border-radius:20px;padding:3px 10px;font-size:10.5px;color:#1b6098;'>&#128205; {item.checkout_location}</span>" +
                          $"</td>");

                sb.Append("</tr>");
                rowIndex++;
            }

            sb.Append("</tbody>");
            sb.Append("</table>");
            sb.Append("</div>"); // scrollable div

            // Divider
            sb.Append("<div style='height:1px;background:#c2d8ed;margin:20px 0;'></div>");

            // Footer note
            sb.Append("<p style='font-size:11px;color:#7a9ab8;line-height:1.65;margin:0;'>");
            sb.Append("This is an automated notification. Please do not reply directly to this message. For discrepancies, contact your HR Team.");
            sb.Append("</p>");

            sb.Append("</td>");
            sb.Append("</tr>");
            sb.Append("</table>");

            // ── FOOTER ──
            sb.Append("<table width='100%' border='0' cellpadding='0' cellspacing='0'>");
            sb.Append("<tr>");
            sb.Append("<td style='background:#ddeaf7;border-top:1px solid #c2d8ed;padding:14px 28px;'>");
            sb.Append("<span style='font-size:10px;color:#7a9ab8;'>Confidential &middot; For Internal Use Only</span>");
            sb.Append("</td>");
            sb.Append("</tr>");
            sb.Append("</table>");

            sb.Append("</td></tr>"); // close wrapper table
            sb.Append("</table>");

            sb.Append("</body>");
            sb.Append("</html>");

            return sb.ToString();
        }

        public static string BuildEmailBody1(string employeeName, List<UserLocationExportModel> attendanceList)   //string checkInTime,string checkInLocation,string checkOutTime,string checkOutLocation 
        {
            string generatedDate = DateTime.Now.ToString("MMMM dd, yyyy");

            StringBuilder sb = new StringBuilder();

            // ── DOCTYPE & HEAD ──────────────────────────────────────────────────
            sb.Append("<!DOCTYPE html>");
            sb.Append("<html lang='en'>");
            sb.Append("<head>");
            sb.Append("<meta charset='UTF-8' />");
            sb.Append("<meta name='viewport' content='width=device-width, initial-scale=1.0'/>");
            sb.Append("<title>Employee Attendance Report</title>");
            sb.Append("<link href='https://fonts.googleapis.com/css2?family=Playfair+Display:wght@600&family=Lato:wght@300;400;700&display=swap' rel='stylesheet'/>");

            // ── STYLES ──────────────────────────────────────────────────────────
            sb.Append("<style>");

            sb.Append("* { margin: 0; padding: 0; box-sizing: border-box; }");

            // ── Keyframe animations ──
            sb.Append("@keyframes shimmer {" +
                      "0%{ background-position:-200% center;}" +
                      "100%{ background-position:200% center;}" +
                      "}");

            sb.Append("@keyframes fadeSlideIn {" +
                      "from{ opacity:0; transform:translateY(10px);}" +
                      "to{ opacity:1; transform:translateY(0);}" +
                      "}");

            sb.Append("@keyframes glowPulse {" +
                      "0%,100%{ box-shadow:0 2px 16px rgba(42,122,184,0.10);}" +
                      "50%{ box-shadow:0 4px 28px rgba(91,184,245,0.30);}" +
                      "}");

            // ── Body ──
            sb.Append("body{" +
                      "background:linear-gradient(145deg,#daeaf7 0%,#eef2f7 60%,#dce8f5 100%);" +
                      "font-family:'Lato',sans-serif;" +
                      "padding:40px 20px;" +
                      "color:#3b5068;" +
                      "}");

            // ── Wrapper ──
            sb.Append(".email-wrapper{" +
                      "max-width:900px;" +
                      "width:100%;" +
                      "margin:0 auto;" +
                      "background:#ffffff;" +
                      "border-radius:12px;" +
                      "overflow:hidden;" +
                      "box-shadow:0 8px 40px rgba(27,79,122,0.14),0 2px 8px rgba(27,79,122,0.08);" +
                      "animation:fadeSlideIn 0.6s ease both;" +
                      "}");

            // ── Header ──
            sb.Append(".email-header{" +
                      "background:linear-gradient(135deg,#0f3460 0%,#1b4f7a 45%,#2a7ab8 100%);" +
                      "padding:30px 40px 22px;" +
                      "border-bottom:3px solid #5bb8f5;" +
                      "position:relative;" +
                      "overflow:hidden;" +
                      "}");

            // Shimmer sweep overlay on header
            sb.Append(".email-header::after{" +
                      "content:'';" +
                      "position:absolute;" +
                      "inset:0;" +
                      "background:linear-gradient(105deg,transparent 30%,rgba(255,255,255,0.09) 50%,transparent 70%);" +
                      "background-size:200% 100%;" +
                      "animation:shimmer 3.5s linear infinite;" +
                      "}");

            // Decorative bubble in header top-right
            sb.Append(".email-header::before{" +
                      "content:'';" +
                      "position:absolute;" +
                      "top:-40px;right:-40px;" +
                      "width:180px;height:180px;" +
                      "border-radius:50%;" +
                      "background:rgba(91,184,245,0.10);" +
                      "pointer-events:none;" +
                      "}");

            sb.Append(".email-header .label{" +
                      "font-weight:700;" +
                      "font-size:8.5px;" +
                      "letter-spacing:3.5px;" +
                      "text-transform:uppercase;" +
                      "color:#7ecef5;" +
                      "margin-bottom:7px;" +
                      "position:relative;z-index:1;" +
                      "}");

            // Shimmer text effect on the h1 title
            sb.Append(".email-header h1{" +
                      "font-family:'Playfair Display',serif;" +
                      "font-size:24px;" +
                      "font-weight:600;" +
                      "line-height:1.2;" +
                      "position:relative;z-index:1;" +
                      "background:linear-gradient(90deg,#ffffff 25%,#a8d8f5 50%,#ffffff 75%);" +
                      "background-size:200% auto;" +
                      "-webkit-background-clip:text;" +
                      "-webkit-text-fill-color:transparent;" +
                      "background-clip:text;" +
                      "animation:shimmer 4s linear infinite;" +
                      "}");

            sb.Append(".email-header .subtitle{" +
                      "margin-top:5px;" +
                      "font-size:11px;" +
                      "color:#b0d6ef;" +
                      "letter-spacing:0.5px;" +
                      "position:relative;z-index:1;" +
                      "}");

            // ── Body content ──
            sb.Append(".email-body{padding:26px 40px;}");

            sb.Append(".intro-text{" +
                      "font-size:13px;" +
                      "line-height:1.75;" +
                      "color:#4a6680;" +
                      "margin-bottom:20px;" +
                      "border-left:3px solid #2a7ab8;" +
                      "padding:6px 0 6px 14px;" +
                      "background:linear-gradient(90deg,#f0f7ff,transparent);" +
                      "border-radius:0 4px 4px 0;" +
                      "}");

            // ── Table wrapper — pulsing glow ──
            sb.Append(".table-wrapper{" +
                      "width:100%;" +
                      "overflow-x:auto;" +
                      "-webkit-overflow-scrolling:touch;" +
                      "border-radius:8px;" +
                      "border:1px solid #c8dff0;" +
                      "animation:glowPulse 4s ease-in-out infinite;" +
                      "}");

            // ── Table ──
            sb.Append("table{" +
                      "width:100%;" +
                      //"min-width:700px;" +
                      "min-width:300px;" +
                      "border-collapse:collapse;" +
                      "font-size:12px;" +
                      "}");

            // Header row — animated shimmer gradient
            sb.Append("thead tr{" +
                      "background:linear-gradient(90deg,#0f3460,#1b4f7a,#2a7ab8,#1b4f7a,#0f3460);" +
                      "background-size:300% 100%;" +
                      "animation:shimmer 5s linear infinite;" +
                      "}");

            // Reduced height: padding 7px 12px
            sb.Append("thead th{" +
                      "padding:7px 12px;" +
                      "text-align:left;" +
                      "font-weight:700;" +
                      "font-size:9px;" +
                      "letter-spacing:1.8px;" +
                      "text-transform:uppercase;" +
                      "color:#a8d8f5;" +
                      "white-space:nowrap;" +
                      "}");

            sb.Append("tbody tr{" +
                      "border-bottom:1px solid #ddeaf7;" +
                      "transition:background 0.25s,box-shadow 0.25s;" +
                      "}");
            sb.Append("tbody tr:last-child{border-bottom:none;}");
            sb.Append("tbody tr:nth-child(even){background:#f3f8fd;}");

            // Row hover — left accent bar + subtle glow
            sb.Append("tbody tr:hover{" +
                      "background:linear-gradient(90deg,#e8f4ff,#f5faff);" +
                      "box-shadow:inset 3px 0 0 #2a7ab8;" +
                      "}");

            // Reduced height: padding 7px 12px
            sb.Append("tbody td{" +
                      "padding:7px 12px;" +
                      "color:#3b5068;" +
                      "font-size:12px;" +
                      "font-weight:400;" +
                      "vertical-align:middle;" +
                      "}");

            sb.Append("tbody td:first-child{" +
                      "font-weight:700;" +
                      "color:#1b4f7a;" +
                      "letter-spacing:0.3px;" +
                      "}");

            sb.Append("tbody td:nth-child(2),tbody td:nth-child(4){" +
                      "font-family:'Courier New',monospace;" +
                      "font-size:11.5px;" +
                      "color:#5a7fa0;" +
                      "}");

            // ── Location badge — gradient pill with pin emoji ──
            sb.Append(".location-badge{" +
                      "display:inline-block;" +
                      "background:linear-gradient(90deg,#d0eaf9,#e8f4ff);" +
                      "border:1px solid #9ac8e8;" +
                      "border-radius:20px;" +
                      "padding:2px 10px;" +
                      "font-size:10.5px;" +
                      "color:#1b6098;" +
                      "letter-spacing:0.2px;" +
                      "box-shadow:0 1px 5px rgba(42,122,184,0.14);" +
                      "}");

            // ── Divider — animated shimmer line ──
            sb.Append(".divider{" +
                      "height:1px;" +
                      "background:linear-gradient(to right,transparent,#5bb8f5,#2a7ab8,#5bb8f5,transparent);" +
                      "background-size:200% 100%;" +
                      "animation:shimmer 3s linear infinite;" +
                      "margin:20px 0;" +
                      "}");

            // ── Footer ──
            sb.Append(".email-footer{" +
                      "background:linear-gradient(90deg,#ddeaf7,#e8f3fb);" +
                      "border-top:1px solid #c2d8ed;" +
                      "padding:16px 40px;" +
                      "display:flex;" +
                      "justify-content:space-between;" +
                      "align-items:center;" +
                      "flex-wrap:wrap;" +
                      "gap:8px;" +
                      "}");

            sb.Append(".email-footer .org{" +
                      "font-family:'Playfair Display',serif;" +
                      "font-size:12.5px;" +
                      "color:#1b4f7a;" +
                      "}");

            sb.Append(".email-footer .note{" +
                      "font-size:10px;" +
                      "color:#7a9ab8;" +
                      "letter-spacing:0.3px;" +
                      "}");

            sb.Append(".footer-note{" +
                      "font-size:11px;" +
                      "color:#7a9ab8;" +
                      "line-height:1.65;" +
                      "}");

            sb.Append("</style>");
            sb.Append("</head>");

            // ── BODY HTML ───────────────────────────────────────────────────────
            sb.Append("<body>");
            sb.Append("<div class='email-wrapper'>");

            // Header
            sb.Append("<div class='email-header'>");
            sb.Append("<div class='label'><h1>&#10022; HUB CONNECT Report &#10022;</h1></div>");
            //sb.Append("<h1>&#10022Employee Attendance Report &#10022;</h1>");
            sb.Append($"<div class='subtitle'>Generated &middot; {generatedDate}</div>");
            sb.Append("</div>");

            // Body
            sb.Append("<div class='email-body'>");
            sb.Append("<p class='intro-text'>" +
                      "Please find below the attendance log for the specified reporting period. " +
                      "This record reflects verified check-in and check-out data as captured by the Hub Connect App." +
                      "</p>");

            // Table
            sb.Append("<div class='table-wrapper'>");
            sb.Append("<table>");
            sb.Append("<thead><tr>");
            sb.Append("<th>Employee Name</th>");
            sb.Append("<th>Check-In Time</th>");
            sb.Append("<th>Check-In Location</th>");
            sb.Append("<th>Check-Out Time</th>");
            sb.Append("<th>Check-Out Location</th>");
            sb.Append("</tr></thead>");
            sb.Append("<tbody><tr>");

            foreach(var item in attendanceList)
            {
                sb.Append("<tr>");
                sb.Append($"<td>{employeeName}</td>");
                sb.Append($"<td>{item.CheckIn_time}</td>");
                sb.Append($"<td><span class='location-badge'>&#128205; {item.Selected_location}</span></td>");
                sb.Append($"<td>{item.CheckOut_time}</td>");
                sb.Append($"<td><span class='location-badge'>&#128205; {item.checkout_location}</span></td>");
                sb.Append("</tr>");
            }
            sb.Append("</tbody>");
            sb.Append("</table>");
            sb.Append("</div>"); // table-wrapper

            sb.Append("<div class='divider'></div>");

            sb.Append("<p class='footer-note'>" +
                      "This is an automated notification. Please do not reply directly to this message. " +
                      "For discrepancies, contact your HR Team." +
                      "</p>");

            sb.Append("</div>"); // email-body

            // Footer
            sb.Append("<div class='email-footer'>");
            //sb.Append("<span class='org'>Human Resources Department</span>");
            sb.Append("<span class='note'>Confidential &middot; For Internal Use Only</span>");
            sb.Append("</div>");

            sb.Append("</div>"); // email-wrapper
            sb.Append("</body>");
            sb.Append("</html>");

            return sb.ToString();
        }
      
        
        //public static async Task<List<UserLocationExportModel>> GetUserLocationList(decimal? sessionUserId, decimal? businessGroupId)
        //{
        //    try
        //    {
        //        var _connection = ConfigurationManager.ConnectionStrings["defaultConnection"].ConnectionString;
        //        DateTime? startDate = ParseDate("08-03-2026 00:00:00.00");  //DateTime.UtcNow;   // null; // Convert.ToDateTime(ConfigurationManager.AppSettings["StartDate"]);
        //        DateTime? endDate = ParseDate("08-03-2026 00:00:00.00");  //DateTime.UtcNow;   // null; // Convert.ToDateTime(ConfigurationManager.AppSettings["StartDate"]);
        //                                                                  //DateTime? endDate = DateTime.UtcNow;   // null; // Convert.ToDateTime(ConfigurationManager.AppSettings["StartDate"]);
        //                                                                  //DateTime? start = ParseDate(startDate);
        //                                                                  //DateTime? end = ParseDate(endDate);
        //        using (IDbConnection _dapperDbConnection = new SqlConnection(_connection))
        //        {
        //            var parameters = new DynamicParameters();
        //            parameters.Add("@Session_UserId", sessionUserId, DbType.Decimal);
        //            parameters.Add("@Business_GroupId", businessGroupId, DbType.Decimal);
        //            parameters.Add("@StartDate", startDate, DbType.DateTime);
        //            parameters.Add("@EndDate", endDate, DbType.DateTime);
        //            var result = await _dapperDbConnection.QueryAsync<UserLocationExportModel>("SP_GET_USER_LOCATION", parameters, commandType: CommandType.StoredProcedure);
        //            return result.ToList();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Error fetching user location data: {ex.Message}");
        //        return new List<UserLocationExportModel>();
        //    }
           
        //}



        public static async Task<AddressModel> GetStructuredAddressAsync(double? lat, double? lng)
        {
            if (lat == null || lng == null)
                throw new ArgumentNullException("Coordinates must not be null.");

            string url = $"https://nominatim.openstreetmap.org/reverse?lat={lat}&lon={lng}&format=json";

            var request = new HttpRequestMessage(HttpMethod.Get, url);

            request.Headers.TryAddWithoutValidation("User-Agent", "GeoLocationApp/1.0 (outsource1@powersoft.in)");
            request.Headers.TryAddWithoutValidation("Referer", "https://myapp.com");
            request.Headers.TryAddWithoutValidation("Accept", "application/json");
            request.Headers.TryAddWithoutValidation("Accept-Language", "en");

            await Task.Delay(2000);

            HttpResponseMessage response = await _httpClient.SendAsync(request);

            if ((int)response.StatusCode == 429)
            {
                await Task.Delay(2000);
                response = await _httpClient.SendAsync(request);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                throw new Exception("403 Forbidden — Nominatim blocked the request.");

            string json = await response.Content.ReadAsStringAsync();

            using (JsonDocument doc = JsonDocument.Parse(json))
            {
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
        }

        private static string GetAddressField(JsonElement addressProp, params string[] keys)
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


        public static async Task<List<UserLocationExportModel>> GetUserLocationList(decimal? sessionUserId, decimal? businessGroupId)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["defaultConnection"].ConnectionString;

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var parameters = new DynamicParameters();
                parameters.Add("@Session_UserId", sessionUserId, DbType.Decimal);
                parameters.Add("@Business_GroupId", businessGroupId, DbType.Decimal);

                var result = await connection.QueryAsync<UserLocationExportModel>(
                    "SP_GET_USER_LOCATION",
                    parameters,   // 🔥 FIXED (you missed this)
                    commandType: CommandType.StoredProcedure
                );

                return result.ToList();
            }
        }

        public static async Task<string> InsertUpdateUserLocation(UserLocation_Model model)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["defaultConnection"].ConnectionString;

            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    await connection.OpenAsync();

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

                    parameters.Add("@Session_UserId", model.Session_UserId, DbType.Decimal);
                    parameters.Add("@Business_GroupId", model.Business_GroupId, DbType.Decimal);

                    // ✅ OUTPUT PARAM
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

        public static async Task<ResponseObject> GetUserLocation(decimal? sessionUserId,decimal? businessGroupId,string startDate,string endDate)
        {
            var response = new ResponseObject();

            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["defaultConnection"].ConnectionString;

                DateTime? start = ParseDate(startDate);
                DateTime? end = ParseDate(endDate);

                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    var parameters = new DynamicParameters();
                    parameters.Add("@Session_UserId", sessionUserId, DbType.Decimal);
                    parameters.Add("@Business_GroupId", businessGroupId, DbType.Decimal);
                    parameters.Add("@StartDate", start);
                    parameters.Add("@EndDate", end);

                    var result = (await connection.QueryAsync<UserLocation_Model>(
                        "SP_GET_USER_LOCATION",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    )).ToList();

                    if (result == null || !result.Any())
                    {
                        response.Status = "Error";
                        response.Message = "No Data Found";
                        return response;
                    }

                    var processedList = new List<UserLocation_Model>();

                    foreach (var item in result)
                    {
                        try
                        {
                            bool isValidCheckoutLocation =
                                item.checkOut_latitude.HasValue &&
                                item.checkOut_longitude.HasValue &&
                                item.checkOut_latitude != 0 &&
                                item.checkOut_longitude != 0;

                            string checkoutAddress = item.checkout_location;

                            if (isValidCheckoutLocation)
                            {
                                var address = await GetStructuredAddressAsync(
                                    item.checkOut_latitude,
                                    item.checkOut_longitude);

                                checkoutAddress = address.FullAddress;
                            }

                            var model = new UserLocation_Model
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
                                checkout_location = checkoutAddress,
                                checkOut_latitude = item.checkOut_latitude,
                                checkOut_longitude = item.checkOut_longitude,
                                Emp_Card_No = item.Emp_Card_No,
                                Tran_Datetime = item.Tran_Datetime,
                                CILocation = item.Selected_location,
                                CoLocation = checkoutAddress
                            };

                            // 🔥 Save back to DB
                            var dbResponse = await InsertUpdateUserLocation(model);

                            if (!dbResponse.Contains("SUCCESS") && !dbResponse.Contains("Updated"))
                            {
                                response.Status = "Error";
                                response.Message = $"Insert Failed for Tran_ID: {item.Tran_ID}";
                                response.Data = item;
                                return response;
                            }

                            processedList.Add(model);
                        }
                        catch (Exception ex)
                        {
                            response.Status = "Error";
                            response.Message = ex.Message;
                            return response;
                        }
                    }

                    response.Status = "SUCCESS";
                    response.Message = "All Data Processed Successfully";
                    response.Data = processedList;
                }
            }
            catch (Exception ex)
            {
                response.Status = "Error";
                response.Message = ex.Message;
            }

            return response;
        }



        #endregion


    }
}
