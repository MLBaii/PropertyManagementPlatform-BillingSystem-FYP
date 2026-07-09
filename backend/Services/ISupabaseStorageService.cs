namespace PropertyBill.Api.Services;

public interface ISupabaseStorageService
{
    // Uploads the file and returns its public URL.
    Task<string> UploadAsync(IFormFile file, int residentId);
}
