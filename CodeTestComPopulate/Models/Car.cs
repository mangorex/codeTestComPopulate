using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace codeTestCom.Models
{
    public class Car
    {
        [JsonProperty("id", PropertyName = "id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "partitionKey")]
        public string PartitionKey { get; set; }

        public string Name { get; set; }
        public string Brand { get; set; }
        public CarType Type { get; set; }
        public bool IsRented { get; set; }


        public Car(string id, string name, string brand, CarType type)
        {
            this.Id = id;
            this.Name = name;
            this.Brand = brand;
            this.Type = type;
            this.IsRented = false;
            this.PartitionKey = brand;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
    public enum CarType
    {
        Premium,
        Suv,
        Small
    }

}

