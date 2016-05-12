using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Swartz.Data
{
    public class Puid
    {
        [DllImport("rpcrt4.dll", SetLastError = true)]
        private static extern int UuidCreateSequential(out Guid guid);

        public static ulong NewPuid()
        {
            Guid guid;
            var result = UuidCreateSequential(out guid);
            if (result != 0)
            {
                throw new Win32Exception(result);
            }

            var guidArray = guid.ToByteArray();
            var baseDate = new DateTime(1900, 1, 1);
            var now = DateTime.Now;

            // Get the days and milliseconds which will be used to build    
            // the byte string
            var days = new TimeSpan(now.Ticks - baseDate.Ticks);
            var msecs = now.TimeOfDay;

            // Convert to a byte array        
            // Note that SQL Server is accurate to 1/300th of a    
            // millisecond so we divide by 3.333333
            var daysArray = BitConverter.GetBytes(days.Days);
            var msecsArray = BitConverter.GetBytes((long) msecs.TotalMilliseconds/3.333333);

            // Reverse the bytes to match SQL Servers ordering
            Array.Reverse(daysArray);
            Array.Reverse(msecsArray);

            // Copy the bytes into the guid 
            Array.Copy(daysArray, daysArray.Length - 2, guidArray, guidArray.Length - 6, 2);
            Array.Copy(msecsArray, msecsArray.Length - 4, guidArray, guidArray.Length - 4, 4);

            var newGuid = new Guid(guidArray);
            return BitConverter.ToUInt64(newGuid.ToByteArray(), 0);
        }
    }
}