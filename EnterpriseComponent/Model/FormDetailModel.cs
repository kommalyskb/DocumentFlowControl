using HttpClientService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseComponent.Model
{
    public class FormDetailModel
    {
        public string? FormID { get; set; }
        public Type? DetailType { get; set; }
        public APIEndpoint? DetailEndpoint { get; set; }
        public AuthorizeHeader? AuthorizeHeader { get; set; }
    }
}
