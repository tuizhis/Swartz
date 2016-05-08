using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Swartz.Users.Models
{
    public class SwartzUserRole<TKey>
    {
        [Key, Column(Order = 0)]
        public virtual TKey UserId { get; set; }

        [Key, Column(Order = 1)]
        public virtual TKey RoleId { get; set; }
    }
}