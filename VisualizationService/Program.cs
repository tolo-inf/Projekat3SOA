using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using MQTTnet.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using VisualizationService;

var port = 1883;
var address = "broker.hivemq.com";
var edgexTopic = "edgex/sensor_value";
int i = 1;

var mqttService = MqttService.Instance();

await mqttService.ConnectAsync(address, port);
var client = new InfluxDBClient(url: "http://influxdb:8086", "admin", "adminadmin");
Console.WriteLine($"CLIENT is null {client == null}");
await mqttService.SubsribeToTopicsAsync(new List<string> { edgexTopic });

async Task ApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
{
    try
    {
        string payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
        var data = (JObject)JsonConvert.DeserializeObject(payload);
        var device = data.SelectToken("device").Value<String>();

        if (device != "SensorValueCluster2") return;
        var reading = JArray.Parse(data.SelectToken("readings").ToString())[0];
        var tempValue = reading.SelectToken("value").Value<Int32>();
        await WriteToDatabase(tempValue);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }
}
async Task WriteToDatabase(int temp)
{
    var point = PointData
        .Measurement("temperature")
        .Field("celsius_degrees", temp)
        .Timestamp(DateTime.UtcNow, WritePrecision.Ns);

    await client.GetWriteApiAsync().WritePointAsync(point, "iot3", "organization");
    Console.WriteLine($"Write in InfluxDb: temperature{i}");
    i++;
}

mqttService.AddApplicationMessageReceived(ApplicationMessageReceivedAsync);

while (true) ;