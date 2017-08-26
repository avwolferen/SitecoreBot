namespace SugCon.Models.Publishing
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    [Serializable]
    public class PublishingHandle
    {
        [JsonProperty("uniqueValue")]
        public Guid Id { get; set; }

        [JsonProperty("instanceName")]
        public string ProducedByInstanceName { get; set; }
    }
}
