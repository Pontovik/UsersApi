namespace UsersApi.Models
{
    public class UserRequestModel
    {
        public string Login { get; set; } = null!;

        public string Password { get; set; } = null!;

        public DateTime? CreatedDate { get; set; }

        public string UserGroup { get; set; }

        public User ConvertToUser()
        {
            var user = new User
            {
                Login = this.Login,
                Password = this.Password,
                CreatedDate = this.CreatedDate,
            };
            return user;
        }
    }
}
