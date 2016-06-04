using System;

namespace Swartz.Data
{
    public class Puid
    {
        public static ulong NewPuid()
        {
            var guid = GuidGenerator.GenerateTimeBasedGuid();
            return BitConverter.ToUInt64(guid.ToByteArray(), 0);
        }
    }
}