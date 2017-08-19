using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;

namespace Utilities
{
	public partial class Conversion
	{
        public static short ToInt16(object obj)
        {
            return ToInt16(obj, 0);
        }

        public static short ToInt16(object obj, short defaultValue)
        {
            short result = defaultValue;
            if (obj != null)
            {
                try
                {
                    result = Convert.ToInt16(obj);
                }
                catch
                { }
            }
            return result;
        }

        public static int ToInt32(object obj)
        {
            return ToInt32(obj, 0);
        }

		public static int ToInt32(object obj, int defaultValue)
		{
            int result = defaultValue;
			if (obj != null)
			{
				try
				{
					result = Convert.ToInt32(obj);
				}
				catch
				{}
			}
			return result;
		}

        public static long ToInt64(object obj)
        {
            return ToInt64(obj, 0);
        }

        public static long ToInt64(object obj, long defaultValue)
        {
            long result = defaultValue;
            if (obj != null)
            {
                try
                {
                    result = Convert.ToInt64(obj);
                }
                catch
                { }
            }
            return result;
        }

        public static ushort ToUInt16(object obj)
        {
            return ToUInt16(obj, 0);
        }

        public static ushort ToUInt16(object obj, ushort defaultValue)
        {
            ushort result = defaultValue;
            if (obj != null)
            {
                try
                {
                    result = Convert.ToUInt16(obj);
                }
                catch
                { }
            }
            return result;
        }

        public static uint ToUInt32(object obj)
        {
            return ToUInt32(obj, 0);
        }

        public static uint ToUInt32(object obj, uint defaultValue)
        {
            uint result = defaultValue;
            if (obj != null)
            {
                try
                {
                    result = Convert.ToUInt32(obj);
                }
                catch
                { }
            }
            return result;
        }

        public static ulong ToUInt64(object obj)
        {
            return ToUInt64(obj, 0);
        }

        public static ulong ToUInt64(object obj, ulong defaultValue)
        {
            ulong result = defaultValue;
            if (obj != null)
            {
                try
                {
                    result = Convert.ToUInt64(obj);
                }
                catch
                { }
            }
            return result;
        }
	}
}
