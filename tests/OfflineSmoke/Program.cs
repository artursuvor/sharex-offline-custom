using System.Net;
using System.Net.Sockets;
using System.Net.Http;
using ShareX.HelpersLib;

if (!OfflinePolicy.IsOffline || !ShareX.SystemOptions.DisableUpload || !ShareX.SystemOptions.DisableUpdateCheck)
    throw new Exception("Offline build flags are not enforced.");

ShareX.SystemOptions.UpdateSystemOptions();
if (!ShareX.SystemOptions.DisableLogging)
    throw new Exception("Disk logging must stay disabled in this build.");

var listener = new TcpListener(IPAddress.Loopback, 0);
listener.Start();
var port = ((IPEndPoint)listener.LocalEndpoint).Port;
try
{
    using var client = HttpClientFactory.Create();
    try { await client.GetAsync($"http://127.0.0.1:{port}/never-send"); throw new Exception("HTTP request was allowed."); }
    catch (HttpRequestException exception) when (exception.Message.Contains("offline build")) { }
    if (listener.Pending()) throw new Exception("HTTP created a network connection.");
    try { PingHelper.PingHost("127.0.0.1"); throw new Exception("Ping was allowed."); }
    catch (InvalidOperationException) { }
    try { Helpers.SendPing("127.0.0.1"); throw new Exception("Ping helper was allowed."); }
    catch (InvalidOperationException) { }
    Console.WriteLine("PASS: HTTP blocked before connection, ping blocked, uploads and updates forced off.");
}
finally { listener.Stop(); }
