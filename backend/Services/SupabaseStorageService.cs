using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PropertyBill.Api.Services;

// Talks to Supabase Storage over its REST API directly (no official Supabase .NET client
// covers Storage well) using the service_role key — residents authenticate against our own
// JWT, not Supabase Auth, so the backend has to act as a trusted service account here rather
// than uploading on the resident's behalf via RLS.
public class SupabaseStorageService : ISupabaseStorageService
{
    private static bool _bucketEnsured;
    private static readonly SemaphoreSlim BucketLock = new(1, 1);

    private readonly HttpClient _httpClient;
    private readonly string _supabaseUrl;
    private readonly string _bucket;

    public SupabaseStorageService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _supabaseUrl = (configuration["Supabase:Url"] ?? throw new InvalidOperationException("Supabase:Url is not configured."))
            .TrimEnd('/');
        _bucket = configuration["Supabase:StorageBucket"] ?? "payment-proofs";
    }

    public async Task<string> UploadAsync(IFormFile file, int residentId)
    {
        await EnsureBucketExistsAsync();

        var extension = Path.GetExtension(file.FileName);
        var objectPath = $"{residentId}/{Guid.NewGuid():N}{extension}";
        var uploadUrl = $"{_supabaseUrl}/storage/v1/object/{_bucket}/{objectPath}";

        await using var stream = file.OpenReadStream();
        using var content = new StreamContent(stream);
        content.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

        var response = await _httpClient.PostAsync(uploadUrl, content);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Supabase Storage upload failed ({(int)response.StatusCode}): {error}");
        }

        return $"{_supabaseUrl}/storage/v1/object/public/{_bucket}/{objectPath}";
    }

    // Creates the bucket on first use, idempotently — lets the resident app work without
    // requiring the bucket to be created manually in the Supabase dashboard first. Cached
    // per-process (a static flag, not per-request) since this only needs to happen once.
    private async Task EnsureBucketExistsAsync()
    {
        if (_bucketEnsured)
        {
            return;
        }

        await BucketLock.WaitAsync();
        try
        {
            if (_bucketEnsured)
            {
                return;
            }

            var createUrl = $"{_supabaseUrl}/storage/v1/bucket";
            var payload = JsonSerializer.Serialize(new CreateBucketRequest
            {
                Name = _bucket,
                Id = _bucket,
                IsPublic = true,
            });

            using var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(createUrl, content);

            if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.BadRequest)
            {
                // BadRequest here almost always means "Bucket already exists" — Supabase
                // doesn't expose a distinct status code for it, so treat both as success.
                _bucketEnsured = true;
                return;
            }

            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Could not create Supabase Storage bucket '{_bucket}': {error}");
        }
        finally
        {
            BucketLock.Release();
        }
    }

    private class CreateBucketRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("public")]
        public bool IsPublic { get; set; }
    }
}
