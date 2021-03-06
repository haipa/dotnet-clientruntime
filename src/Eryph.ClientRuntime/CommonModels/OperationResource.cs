// <auto-generated>
// MIT
// </auto-generated>

namespace Eryph.ClientRuntime
{
    using Newtonsoft.Json;
    using System.Linq;

    public partial class OperationResource
    {
        /// <summary>
        /// Initializes a new instance of the OperationResource class.
        /// </summary>
        public OperationResource()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the OperationResource class.
        /// </summary>
        /// <param name="resourceType">Possible values include:
        /// 'Machine'</param>
        public OperationResource(string id = default(string), string resourceId = default(string), string resourceType = default(string))
        {
            Id = id;
            ResourceId = resourceId;
            ResourceType = resourceType;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "resourceId")]
        public string ResourceId { get; set; }

        /// <summary>
        /// Gets or sets possible values include: 'Machine'
        /// </summary>
        [JsonProperty(PropertyName = "resourceType")]
        public string ResourceType { get; set; }

    }
}
