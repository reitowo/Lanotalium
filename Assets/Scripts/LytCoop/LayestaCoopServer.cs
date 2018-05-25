using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.NetworkInformation;

public class LayestaDevice
{
    public string Message;
    public IPEndPoint IpEndPoint;
    public DateTime LastActiveTime;
    public void Update(string message, IPEndPoint remote)
    {
        Message = message;
        IpEndPoint = remote;
        LastActiveTime = DateTime.Now;
    }
}
public class LayestaDeviceList
{
    private object deviceListLock = new object();
    private List<LayestaDevice> devices = new List<LayestaDevice>();
    public List<LayestaDevice> Devices
    {
        get
        {
            lock (deviceListLock)
            {
                return devices;
            }
        }
    }

    private LayestaDevice GetDevice(IPEndPoint remote)
    {
        foreach (LayestaDevice d in devices)
        {
            if (d.IpEndPoint.Equals(remote)) return d;
        }
        LayestaDevice n = new LayestaDevice();
        devices.Add(n);
        return n;
    }
    public void RemoveDeadDevices()
    {
        lock (deviceListLock)
        {
            devices = devices.Where((b) => (DateTime.Now - b.LastActiveTime).Seconds < 3).ToList();
        }
    }
    public void HandleBroadcastMessage(string message, IPEndPoint remote)
    {
        lock (deviceListLock)
        {
            if (!message.StartsWith("Layesta")) return;
            LayestaDevice device = GetDevice(remote);
            IPEndPoint end = new IPEndPoint(remote.Address, remote.Port);
            device.Update(message.Remove(0, 7), end);
            RemoveDeadDevices();
        }
    }
}

public class LayestaCoopServer : MonoBehaviour
{
    public const int MulticastingPort = 11451;
    public const int ProxyPort = 41919;
    public const string MulticastingAddress = "225.114.51.4";

    private static LayestaCoopServer instance;
    public static LayestaCoopServer Instance
    {
        get
        {
            return instance;
        }
    }
    private void Start()
    {
        instance = this;
        Initialize();
    }
    private void OnDestroy()
    {
        CleanUp();
    }

    public List<LayestaDevice> Devices
    {
        get
        {
            return deviceList.Devices;
        }
    }

    private volatile bool working;
    private LayestaDeviceList deviceList;
    private Socket detectLayestaSocket;
    private Thread detectLayestaThread, activeSelfThread;

    public void ResetServer()
    {
        CleanUp();
        Initialize();
        LimLayestaManager.Instance.ResetSend();
    }
    private void Initialize()
    {
        working = true;
        deviceList = new LayestaDeviceList();
        detectLayestaSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        detectLayestaSocket.Bind(new IPEndPoint(IPAddress.Parse(GetLocalIPAddress()), MulticastingPort));
        detectLayestaSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(IPAddress.Parse(MulticastingAddress), IPAddress.Parse(GetLocalIPAddress())));
        detectLayestaSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 200);

        NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
        foreach (NetworkInterface adapter in nics)
        {
            if (!adapter.GetIPProperties().MulticastAddresses.Any())
                continue; // most of VPN adapters will be skipped
            if (!adapter.SupportsMulticast)
                continue; // multicast is meaningless for this type of connection
            if (OperationalStatus.Up != adapter.OperationalStatus)
                continue; // this adapter is off or not connected
            IPv4InterfaceProperties p = adapter.GetIPProperties().GetIPv4Properties();
            if (null == p)
                continue; // IPv4 is not configured on this adapter
            detectLayestaSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastInterface, IPAddress.HostToNetworkOrder(p.Index));
            break;
        }

        detectLayestaThread = new Thread(DetectLayestaThread);
        detectLayestaThread.Start();
        activeSelfThread = new Thread(ActiveSelfThread);
        activeSelfThread.Start();
    }
    private void CleanUp()
    {
        detectLayestaSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.DropMembership, new MulticastOption(IPAddress.Parse(MulticastingAddress), IPAddress.Parse(GetLocalIPAddress())));
        detectLayestaSocket.Close();
        working = false;
    }
    public string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        throw new Exception("No network adapters with an IPv4 address in the system!");
    }
    public void DetectLayestaThread()
    {
        EndPoint remote = new IPEndPoint(IPAddress.Parse(MulticastingAddress), 0);
        while (working)
        {
            try
            {
                deviceList.RemoveDeadDevices();
                if (detectLayestaSocket.Available > 0)
                {
                    byte[] buf = new byte[128];
                    detectLayestaSocket.ReceiveFrom(buf, ref remote);
                    string message = Encoding.UTF8.GetString(buf);
                    deviceList.HandleBroadcastMessage(message, remote as IPEndPoint);
                }
                else Thread.Sleep(200);
            }
            catch (Exception Ex)
            {
                Debug.LogException(Ex);
            }
        }
    }
    public void ActiveSelfThread()
    {
        while (working)
        {
            try
            {
                detectLayestaSocket.SendTo(Encoding.UTF8.GetBytes("Active"), new IPEndPoint(IPAddress.Parse(MulticastingAddress), MulticastingPort));
                Thread.Sleep(1000);
            }
            catch (Exception)
            {

            }
        }
    }
}
