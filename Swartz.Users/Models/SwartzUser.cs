using Swartz.Data;

namespace Swartz.Users.Models
{
    public class SwartzUser<TKey> : ISwartzUser<TKey>
    {
        public virtual string Email { get; set; }

        public virtual string Phone { get; set; }

        public virtual string Password { get; set; }

        public virtual bool EmailConfirmed { get; set; }

        public virtual bool PhoneNumberConfirmed { get; set; }

        public virtual string SecurityStamp { get; set; }
        public virtual TKey Id { get; set; }
        public virtual string UserName { get; set; }
    }

    public class SwartzUser : SwartzUser<decimal>
    {
        public SwartzUser()
        {
            Id = Puid.NewPuid();
        }

        public SwartzUser(string userName) : this()
        {
            UserName = userName;
        }
    }
}