using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BirthdayApp.Models
{
    [Table("UserTable")]
    public class UserList
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string UserPassword { get; set; } = string.Empty;
        public DateOnly DateOfBirth { get; set; }
    }

    [Table("BirthdayWishes")]
    public class BirthdayWish
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string WishMessage { get; set; } = string.Empty;
        public string FromUserName { get; set; } = string.Empty;
        public int FromUserId { get; set; } // Sender ID
        public string ToUserName { get; set; } = string.Empty;
        public int ToUserId { get; set; } // Receiver ID
        public DateOnly BirthdayDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool EmailSent { get; set; } = false;
    }

    [NotMapped]
    public class LoginModel
    {
        public string UserEmail { get; set; } = string.Empty;
        public string UserPassword { get; set; } = string.Empty;
    }

    [NotMapped]
    public class BirthdayFilterModel
    {
        public DateOnly? SelectedDate { get; set; }
        public List<UserList> UsersWithSameBirthday { get; set; } = new List<UserList>();
        public List<BirthdayWish> BirthdayWishes { get; set; } = new List<BirthdayWish>();
    }

    [NotMapped]
    public class BirthdayWishModel
    {
        public string WishMessage { get; set; } = string.Empty;
        public int ToUserId { get; set; }
        public string ToUserName { get; set; } = string.Empty;
        public DateOnly BirthdayDate { get; set; }
    }

    [NotMapped]
    public class EmailNotificationModel
    {
        public string ToEmail { get; set; } = string.Empty;
        public string ToName { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
        public string WishMessage { get; set; } = string.Empty;
        public DateOnly BirthdayDate { get; set; }
    }
}
