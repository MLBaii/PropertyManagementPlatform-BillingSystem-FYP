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
    private readonly ILogger<SupabaseStorageService> _logger;

    public SupabaseStorageService(HttpClient httpClient, IConfiguration configuration, ILogger<SupabaseStorageService> logger)
    {
        _httpClient = httpClient;
        _supabaseUrl = (configuration["Supabase:Url"] ?? throw new InvalidOperationException("Supabase:Url is not configured."))
            .TrimEnd('/');
        _bucket = configuration["Supabase:StorageBucket"] ?? "payment-proofs";
        _logger = logger;
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
            _logger.LogError(
                "Supabase Storage upload failed for object {ObjectPath}: {StatusCode} {Body}",
                objectPath, (int)response.StatusCode, error);
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

            if (response.IsSuccessStatusCode)
            {
                _bucketEnsured = true;
                return;
            }

            var error = await response.Content.ReadAsStringAsync();

            // Supabase reports "bucket already exists" as an HTTP 400 with this specific body
            // rather than a dedicated status code — but a 400 can just as easily mean something
            // else went wrong, so only THIS body counts as success. Treating every 400 as
            // "already exists" previously meant a genuine failure (e.g. a cold-starting Supabase
            // project timing out) got swallowed, _bucketEnsured got stuck true for the rest of
            // the process's life, and every upload after it failed with "Bucket not found".
            if (response.StatusCode == HttpStatusCode.BadRequest && IsBucketAlreadyExistsError(error))
            {
                _bucketEnsured = true;
                return;
            }

            _logger.LogError(
                "Could not create Supabase Storage bucket '{Bucket}': {StatusCode} {Body}",
                _bucket, (int)response.StatusCode, error);
            throw new InvalidOperationException($"Could not create Supabase Storage bucket '{_bucket}': {error}");
        }
        finally
        {
            BucketLock.Release();
        }
    }

    // Matches Supabase's actual "already exists" response body:
    // {"statusCode":"409","error":"Duplicate","message":"The resource already exists"}
    private static bool IsBucketAlreadyExistsError(string responseBody)
    {
        try
        {
            using var doc = JsonDocument.Parse(responseBody);
            return doc.RootElement.TryGetProperty("error", out var errorProp)
                && errorProp.GetString() == "Duplicate";
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private class CreateBucketRequest
    {
        // Supabase's Storage API requires these lowercase — System.Text.Json's default
        // serializer emits the C# PascalCase names verbatim, which Supabase silently
        // rejects with "body must have required property 'name'" (still an HTTP 400,
        // easy to mistake for the "bucket already exists" 400).
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("public")]
        public bool IsPublic { get; set; }
    }
}
