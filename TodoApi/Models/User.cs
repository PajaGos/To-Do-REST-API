namespace TodoApi.Models
{
    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }
        
        public void UpdateFrom(User user)
        {
            if (user == null)
            {
                return;
            }
            
            UserName = user.UserName;
            Email = user.Email;
        }
    }
}