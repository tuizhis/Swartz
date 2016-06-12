using System.Web.Security;
using Microsoft.Owin.Security.DataProtection;

namespace Swartz.Users
{
    public class MachineKeyProtector : IDataProtector
    {
        public byte[] Protect(byte[] userData)
        {
            return MachineKey.Protect(userData);
        }

        public byte[] Unprotect(byte[] protectedData)
        {
            return MachineKey.Unprotect(protectedData);
        }
    }
}