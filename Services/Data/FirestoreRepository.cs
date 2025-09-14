using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using LinguaLearn.Mobile.Models.Common;

namespace LinguaLearn.Mobile.Services.Data;

public class FirestoreRepository : IFirestoreRepository
{
    private readonly FirestoreDb _firestoreDb;
    private readonly ILogger<FirestoreRepository> _logger;

    public FirestoreRepository(FirestoreDb firestoreDb, ILogger<FirestoreRepository> logger)
    {
        _firestoreDb = firestoreDb;
        _logger = logger;
    }

    public async Task<ServiceResult<T>> GetDocumentAsync<T>(string collection, string documentId, CancellationToken ct = default) where T : class
    {
        try
        {
            var docRef = _firestoreDb.Collection(collection).Document(documentId);
            var snapshot = await docRef.GetSnapshotAsync(ct);

            if (!snapshot.Exists)
            {
                return ServiceResult<T>.Failure($"Document {documentId} not found in collection {collection}");
            }

            var document = snapshot.ConvertTo<T>();
            return ServiceResult<T>.Success(document);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get document {DocumentId} from collection {Collection}", documentId, collection);
            return ServiceResult<T>.Failure($"Failed to retrieve document: {ex.Message}", ex);
        }
    }

    public async Task<ServiceResult<List<T>>> GetCollectionAsync<T>(string collection, CancellationToken ct = default) where T : class
    {
        try
        {
            var collectionRef = _firestoreDb.Collection(collection);
            var snapshot = await collectionRef.GetSnapshotAsync(ct);

            var documents = snapshot.Documents
                .Select(doc => doc.ConvertTo<T>())
                .ToList();

            return ServiceResult<List<T>>.Success(documents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get collection {Collection}", collection);
            return ServiceResult<List<T>>.Failure($"Failed to retrieve collection: {ex.Message}", ex);
        }
    }

    public async Task<ServiceResult<List<T>>> QueryCollectionAsync<T>(string collection, Filter filter, CancellationToken ct = default) where T : class
    {
        try
        {
            var collectionRef = _firestoreDb.Collection(collection);
            var query = collectionRef.Where(filter);
            var snapshot = await query.GetSnapshotAsync(ct);

            var documents = snapshot.Documents
                .Select(doc => doc.ConvertTo<T>())
                .ToList();

            return ServiceResult<List<T>>.Success(documents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to query collection {Collection}", collection);
            return ServiceResult<List<T>>.Failure($"Failed to query collection: {ex.Message}", ex);
        }
    }

    public async Task<ServiceResult<string>> AddDocumentAsync<T>(string collection, T document, CancellationToken ct = default) where T : class
    {
        try
        {
            var collectionRef = _firestoreDb.Collection(collection);
            var docRef = await collectionRef.AddAsync(document, ct);

            return ServiceResult<string>.Success(docRef.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add document to collection {Collection}", collection);
            return ServiceResult<string>.Failure($"Failed to add document: {ex.Message}", ex);
        }
    }

    public async Task<ServiceResult<bool>> SetDocumentAsync<T>(string collection, string documentId, T document, CancellationToken ct = default) where T : class
    {
        try
        {
            var docRef = _firestoreDb.Collection(collection).Document(documentId);
            await docRef.SetAsync(document, cancellationToken: ct);

            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set document {DocumentId} in collection {Collection}", documentId, collection);
            return ServiceResult<bool>.Failure($"Failed to set document: {ex.Message}", ex);
        }
    }

    public async Task<ServiceResult<bool>> UpdateDocumentAsync(string collection, string documentId, Dictionary<string, object> updates, CancellationToken ct = default)
    {
        try
        {
            var docRef = _firestoreDb.Collection(collection).Document(documentId);
            await docRef.UpdateAsync(updates, cancellationToken: ct);

            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update document {DocumentId} in collection {Collection}", documentId, collection);
            return ServiceResult<bool>.Failure($"Failed to update document: {ex.Message}", ex);
        }
    }

    public async Task<ServiceResult<bool>> DeleteDocumentAsync(string collection, string documentId, CancellationToken ct = default)
    {
        try
        {
            var docRef = _firestoreDb.Collection(collection).Document(documentId);
            await docRef.DeleteAsync(cancellationToken: ct);

            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete document {DocumentId} from collection {Collection}", documentId, collection);
            return ServiceResult<bool>.Failure($"Failed to delete document: {ex.Message}", ex);
        }
    }

    public async Task<ServiceResult<bool>> BatchWriteAsync(List<(string collection, string documentId, object document, BatchAction action)> operations, CancellationToken ct = default)
    {
        try
        {
            var batch = _firestoreDb.StartBatch();

            foreach (var (collection, documentId, document, action) in operations)
            {
                var docRef = _firestoreDb.Collection(collection).Document(documentId);

                switch (action)
                {
                    case BatchAction.Set:
                        batch.Set(docRef, document);
                        break;
                    case BatchAction.Update:
                        if (document is Dictionary<string, object> updates)
                        {
                            batch.Update(docRef, updates);
                        }
                        else
                        {
                            throw new ArgumentException($"Update operation requires Dictionary<string, object> but got {document.GetType()}");
                        }
                        break;
                    case BatchAction.Delete:
                        batch.Delete(docRef);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(action), action, "Invalid batch action");
                }
            }

            await batch.CommitAsync(ct);
            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute batch write with {OperationCount} operations", operations.Count);
            return ServiceResult<bool>.Failure($"Failed to execute batch write: {ex.Message}", ex);
        }
    }
}