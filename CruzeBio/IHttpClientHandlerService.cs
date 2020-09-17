using System.Net.Http;

namespace CruzeBio
{
    public interface IHttpClientHandlerService
    {
        HttpClientHandler GetInsecureHandler();
    }
}