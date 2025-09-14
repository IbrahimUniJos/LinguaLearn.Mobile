using Google.Cloud.Firestore;
using LinguaLearn.Mobile.Models.Common;

namespace LinguaLearn.Mobile.Services.Data;

public interface IFirestoreRepository
{
    Task<ServiceResult<T>> GetDocumentAsync<T>(string collection, string documentId, CancellationToken ct = default) where T : class;
    Task<ServiceResult<List<T>>> GetCollectionAsync<T>(string collection, CancellationToken ct = default) where T : class;
    Task<ServiceResult<List<T>>> QueryCollectionAsync<T>(string collection, Filter filter, CancellationToken ct = default) where T : class;
    Task<ServiceResult<string>> AddDocumentAsync<T>(string collection, T document, CancellationToken ct = default) where T : class;
    Task<ServiceResult<bool>> SetDocumentAsync<T>(string collection, string documentId, T document, CancellationToken ct = default) where T : class;
    Task<ServiceResult<bool>> UpdateDocumentAsync(string collection, string documentId, Dictionary<string, object> updates, CancellationToken ct = default);
    Task<ServiceResult<bool>> DeleteDocumentAsync(string collection, string documentId, CancellationToken ct = default);
    Task<ServiceResult<bool>> BatchWriteAsync(List<(string collection, string documentId, object document, BatchAction action)> operations, CancellationToken ct = default);
}

public enum BatchAction
{
    Set,
    Update,
    Delete
}