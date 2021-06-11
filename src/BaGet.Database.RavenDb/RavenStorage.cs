using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BaGet.Core;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;

namespace BaGet.Database.RavenDb
{
    public class RavenStorage : IStorageService
    {
        private readonly IAsyncDocumentSession _session;

        public RavenStorage(
            IAsyncDocumentSession session)
        {
            _session = session;
        }

        public async Task<Stream> GetAsync(Blob blob, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var attachmentResult = await _session.Advanced.Attachments.GetAsync(
                blob.PackageId, blob.Name, cancellationToken);
            return attachmentResult.Stream;
        }

        public async Task<StoragePutResult> PutAsync(
            Blob blob,
            Stream content,
            string contentType,
            CancellationToken cancellationToken = default)
        {
            if (content == null)
                throw new ArgumentNullException(nameof(content));
            if (string.IsNullOrEmpty(contentType))
                throw new ArgumentException("Content type is required", nameof(contentType));

            cancellationToken.ThrowIfCancellationRequested();

            var packageEntity = await _session.LoadAsync<Package>(blob.PackageId, cancellationToken);
            if (packageEntity is null)
                throw new ArgumentNullException(nameof(packageEntity));

            var exists = await _session.Advanced.Attachments.ExistsAsync(
                blob.PackageId, blob.Name, cancellationToken);
            if (exists)
                return StoragePutResult.Conflict;

            _session.Advanced.Attachments.Store(packageEntity, blob.Name, content, contentType);
            await _session.SaveChangesAsync(cancellationToken);
            return StoragePutResult.Success;
        }

        public async Task DeleteAsync(Blob blob, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _session.Advanced.Attachments.Delete(blob.PackageId, blob.Name);
        }
    }
}
