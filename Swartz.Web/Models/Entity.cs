using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Swartz.Web.Models
{
    public abstract class Entity<TKey>
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public virtual TKey Id { get; set; }
    }
}