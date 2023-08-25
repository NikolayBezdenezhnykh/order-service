using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Extensions
{
    public static class HttpResponseMessageExtensions
    {
        public static async Task<string> GetTextResponseAsync(this HttpResponseMessage httpResponseMessage)
        {
            string data = null;
            Stream responseStream = null;
            try
            {
                responseStream = await httpResponseMessage.Content.ReadAsStreamAsync();
                using var responseReader = new StreamReader(responseStream);

                data = await responseReader.ReadToEndAsync();
                responseReader.Close();
            }
            finally
            {
                responseStream?.Dispose();
                httpResponseMessage?.Dispose();
            }

            return data;
        }
    }
}
