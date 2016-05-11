namespace Swartz.Users.Models
{
    public class SwartzRole<TKey> : ISwartzRole<TKey>
    {
        public virtual TKey Id { get; set; }
        public virtual string Name { get; set; }
    }
}