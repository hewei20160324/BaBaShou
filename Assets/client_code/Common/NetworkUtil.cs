using UnityEngine;
using System.Collections;
using System.Net;

namespace CustomUtil
{
    public class NetworkUtil
    {
        public static string GetLocalHostName()
        {
            IPHostEntry localHost = Dns.GetHostEntry(Dns.GetHostName());
            return localHost != null ? localHost.AddressList[0].ToString() : string.Empty;
        }
    }
}

