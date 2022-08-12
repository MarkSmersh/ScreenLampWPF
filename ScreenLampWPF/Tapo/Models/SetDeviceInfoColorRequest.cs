using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LastTryTapo.Models
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class SetDeviceInfoColorRequest
    {
        [JsonProperty("method")]
        public string Method { get; set; }

        [JsonProperty("params")]
        public SetDeviceInfoColorRequestParams Params { get; set; }
    }

    public partial class SetDeviceInfoColorRequestParams
    {
        [JsonProperty("hue")]
        public int? Hue { get; set; }

        [JsonProperty("saturation")]
        public int? Saturation { get; set; }

        [JsonProperty("brightness")]
        public int? Brightness { get; set; }

        [JsonProperty("color_temp")]
        public int? ColorTemp { get; set; }
    }
}
