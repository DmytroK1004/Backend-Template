using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Presentation.Exceptions
{
    public class DefaultBestPracticesException : IOException, IBestPracticesException
    {
        public EventId EventId => new EventId(100, "Default Error");
        public HttpStatusCode? HttpStatusCode { get; set; }
        public int ErrorCode { get; set; }
        public string Message { get; set; }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(new 
            {
                ErrorCode = HttpStatusCode,
                HttpStatusCode = HttpStatusCode,
                Message = Message
            });
        }
    }
}
