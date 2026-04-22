using System.Net;

namespace InkWell.Post.Service.External;

public class TaxonomyClient : ITaxonomyClient
{
    private readonly HttpClient _http;

    public TaxonomyClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<bool> CategoryExistsAsync(Guid categoryId, CancellationToken ct = default)
    {
        var res = await _http.GetAsync($"/api/categories/{categoryId}", ct);

        if (res.IsSuccessStatusCode) return true;
        if (res.StatusCode == HttpStatusCode.NotFound) return false;

        throw new HttpRequestException($"Taxonomy service error (categories): {(int)res.StatusCode} {res.ReasonPhrase}");
    }

    public async Task<bool> TagExistsAsync(Guid tagId, CancellationToken ct = default)
    {
        var res = await _http.GetAsync($"/api/tags/{tagId}", ct);

        if (res.IsSuccessStatusCode) return true;
        if (res.StatusCode == HttpStatusCode.NotFound) return false;

        throw new HttpRequestException($"Taxonomy service error (tags): {(int)res.StatusCode} {res.ReasonPhrase}");
    }
}