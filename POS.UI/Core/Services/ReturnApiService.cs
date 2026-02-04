using System.Net.Http;
using System.Net.Http.Json;

namespace POS.UI.Core.Services
{
    public class ReturnApiService : BaseApiService
    {
        public ReturnApiService(HttpClient httpClient) : base(httpClient) { }

        public async Task<bool> CreateReturnAsync(object dto)
        {
            var response = await _http.PostAsJsonAsync("api/returns", dto);
            await EnsureSuccessAsync(response, "CreateReturn");
            return true;
        }
    }
}
