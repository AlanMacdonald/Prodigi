using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ColorMatcher.Logic.Tests
{
    /// <summary>
    /// Based on https://dev.to/n_develop/mocking-the-httpclient-in-net-core-with-nsubstitute-k4j
    /// NSubstitue cannot mock internal methods and Microsoft made HttpClient hard to mock so we need
    /// a mock HttpMessageHandler that we can control the output from.
    /// </summary>
    public class MockHttpMessageHandler : HttpMessageHandler
    {
        public string StringResponse { get; set; }

        public Stream StreamResponse { get; set; }

        public HttpStatusCode StatusCode { get; set; }

        public string Input { get; private set; }
        public int NumberOfCalls { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            NumberOfCalls++;
            if (request.Content != null) // Could be a GET-request without a body
            {
                Input = await request.Content.ReadAsStringAsync();
            }
            return new HttpResponseMessage
            {
                StatusCode = this.StatusCode,
                Content = (String.IsNullOrEmpty(StringResponse)
                            ? (HttpContent)new StreamContent(this.StreamResponse)
                            : (HttpContent)new StringContent(this.StringResponse)
                            )
            };
        }
    }

}
