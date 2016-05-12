using Swartz.Data;

namespace Swartz.Users.Models
{
    public class SwartzRole<TKey> : ISwartzRole<TKey>
    {
        public virtual TKey Id { get; set; }
        public virtual string Name { get; set; }
    }

    public class SwartzRole : SwartzRole<decimal>
    {
        public SwartzRole()
        {
            Id = Puid.NewPuid();
        }

        public SwartzRole(string roleName) : this()
        {
            Name = roleName;
        }
    }
}