using System;

namespace Btc.Common.Extensions
{
    public static class ULongExtensions
    {
        public static DateTime ToDateTime(this ulong source)
        {
            return DateTime.UnixEpoch.AddMilliseconds(source * 1000);
        }
    }
}