using System.Web.Mvc;

namespace Swartz.Mvc.AntiForgery
{
    public class ValidateAntiForgeryTokenSwartzAttribute : FilterAttribute
    {
        public ValidateAntiForgeryTokenSwartzAttribute() : this(true)
        {
        }

        public ValidateAntiForgeryTokenSwartzAttribute(bool enabled)
        {
            Enabled = enabled;
        }

        public bool Enabled { get; }
    }
}