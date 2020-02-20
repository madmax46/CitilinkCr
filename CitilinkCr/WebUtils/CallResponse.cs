using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace WebUtils
{
    public class CallResponse<T>
    {
        public HttpResponseMessage HttpResponse { get; set; }

        public T Response { get; set; }
    }
}
