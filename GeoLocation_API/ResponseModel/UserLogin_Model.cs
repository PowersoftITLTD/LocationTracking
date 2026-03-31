namespace GeoLocation_API.ResponseModel
{
    public class UserLogin_Model
    {
        public string? UserName { get; set; }
        public string? Password { get; set; }
    }
    public class UserLoginModel
    {
        public int UserId { get; set; }

        public string LoginName { get; set; }

        public string? PasswordHash { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }


        //public bool IsActive { get; set; }

        //public DateTime CreatedDate { get; set; }

        //public string? ModifiedBy { get; set; }

        //public DateTime? ModifiedDate { get; set; }  
    }

    public class CommoninputResponse
    {
        public int? Session_userId { get; set; }
        public int? BusinessGroupId { get; set; }
    }
}
