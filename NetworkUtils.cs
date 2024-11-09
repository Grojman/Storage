using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

public static class NetworkUtils {

    private static readonly string messagePattern = "(?<=[}])";

    public const int BUFFER_SIZE = 1024;

    public static int SERVER_PORT = 9000;


    public static IPEndPoint GetEndPoint()
    {
        return GetEndPointFromFile() ?? new(LocalIpAddres(), SERVER_PORT);
    }

    private static IPEndPoint? GetEndPointFromFile()
    {
        try
        {
            return new IPEndPoint(IPAddress.Parse(
                TFGUserControls.Properties.Settings.Default.GameServerHost),
                TFGUserControls.Properties.Settings.Default.GameServerPort);
        }
        catch (Exception e) { Trace.WriteLine(e); return null; }
    }
    public static IPAddress LocalIpAddres()
    {
        foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            var addr = ni.GetIPProperties().GatewayAddresses.FirstOrDefault();
            if (addr != null && !addr.Address.ToString().Equals("0.0.0.0"))
            {
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            return ip.Address;
                        }
                    }
                }
            }
        }
        throw new Exception("No network adapters with an IPv4 address in the system!");
    }


    
    public static byte[] ConvertToByteArray(Message message) 
    {
        Trace.WriteLine(message.ToString());
        return UTF8Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
    }

    public static List<Message> ConvertToMessage(Byte[] buffer, int length) 
    {
        List<Message> messages = new();

        Array.Resize(ref buffer, length);
        string json = UTF8Encoding.UTF8.GetString(buffer);
        Trace.WriteLine(json);
        
        foreach(string s in Regex.Split(json, messagePattern).Where(n => n.Length != 0))
        {
            messages.Add(JsonConvert.DeserializeObject<Message>(s));
        }

        return messages;
    }
}