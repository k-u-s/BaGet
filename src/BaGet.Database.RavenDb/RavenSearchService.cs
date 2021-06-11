using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BaGet.Core;
using BaGet.Protocol.Models;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;

namespace BaGet.Database.RavenDb
{
    public class RavenSearchService: ISearchService
    {
        private readonly IContext _context;
        private readonly IFrameworkCompatibilityService _frameworks;
        private readonly IUrlGenerator _url;

        public RavenSearchService(IContext context, IFrameworkCompatibilityService frameworks, IUrlGenerator url)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _frameworks = frameworks ?? throw new ArgumentNullException(nameof(frameworks));
            _url = url ?? throw new ArgumentNullException(nameof(url));
        }

        public async Task<SearchResponse> SearchAsync(
            SearchRequest request,
            CancellationToken cancellationToken)
        {
            var result = new List<SearchResult>();
            var packages = await SearchImplAsync(
                request,
                cancellationToken);

            foreach (var package in packages)
            {
                var versions = package.OrderByDescending(p => p.Version).ToList();
                var latest = versions.First();
                var iconUrl = latest.HasEmbeddedIcon
                    ? _url.GetPackageIconDownloadUrl(latest.Identifier, latest.Version)
                    : latest.IconUrlString;

                result.Add(new SearchResult
                {
                    PackageId = latest.Identifier,
                    Version = latest.Version.ToFullString(),
                    Description = latest.Description,
                    Authors = latest.Authors,
                    IconUrl = iconUrl,
                    LicenseUrl = latest.LicenseUrlString,
                    ProjectUrl = latest.ProjectUrlString,
                    RegistrationIndexUrl = _url.GetRegistrationIndexUrl(latest.Identifier),
                    Summary = latest.Summary,
                    Tags = latest.Tags,
                    Title = latest.Title,
                    TotalDownloads = versions.Sum(p => p.Downloads),
                    Versions = versions
                        .Select(p => new SearchResultVersion
                        {
                            RegistrationLeafUrl = _url.GetRegistrationLeafUrl(p.Identifier, p.Version),
                            Version = p.Version.ToFullString(),
                            Downloads = p.Downloads,
                        })
                        .ToList()
                });
            }

            return new SearchResponse
            {
                TotalHits = result.Count,
                Data = result,
                Context = SearchContext.Default(_url.GetPackageMetadataResourceUrl())
            };
        }

        public async Task<AutocompleteResponse> AutocompleteAsync(
            AutocompleteRequest request,
            CancellationToken cancellationToken)
        {
            IQueryable<Package> search = _context.PackagesQueryable;

            if (!string.IsNullOrEmpty(request.Query))
            {
                var queryText = request.Query.ToLower();
                search = search
                    .Search(p => p.Identifier, queryText)
                    ;
            }

            search = AddSearchFilters(
                search,
                request.IncludePrerelease,
                request.IncludeSemVer2,
                request.PackageType,
                frameworks: null);

            var query = search
                .OrderByDescending(p => p.Downloads)
                .Distinct()
                .Skip(request.Skip)
                .Take(request.Take)
                .Select(p => p.Identifier);

            var results = await _context.ToListAsync(query, cancellationToken);

            return new AutocompleteResponse
            {
                TotalHits = results.Count,
                Data = results,
                Context = AutocompleteContext.Default
            };
        }

        public async Task<AutocompleteResponse> ListPackageVersionsAsync(
            VersionsRequest request,
            CancellationToken cancellationToken)
        {
            var packageId = request.PackageId.ToLower();
            IQueryable<Package> search = _context
                    .PackagesQueryable
                    .Search(p => p.Identifier, packageId)
                ;

            search = AddSearchFilters(
                search,
                request.IncludePrerelease,
                request.IncludeSemVer2,
                packageType: null,
                frameworks: null);

            var query = search
                .Select(p => p.NormalizedVersionString);
            var results = await _context.ToListAsync(query, cancellationToken);

            return new AutocompleteResponse
            {
                TotalHits = results.Count,
                Data = results,
                Context = AutocompleteContext.Default
            };
        }

        public async Task<DependentsResponse> FindDependentsAsync(string packageId, CancellationToken cancellationToken)
        {
            var query = _context
                .PackagesQueryable
                .Where(p => p.Listed)
                .OrderByDescending(p => p.Downloads)
                .Where(p => p.Dependencies.Any(d => d.Id == packageId))
                .Take(20)
                .Select(r => new DependentResult
                {
                    Id = r.Identifier,
                    Description = r.Description,
                    TotalDownloads = r.Downloads
                })
                .Distinct();
            var results = await _context.ToListAsync(query, cancellationToken);

            return new DependentsResponse
            {
                TotalHits = results.Count,
                Data = results
            };
        }

        private async Task<List<IGrouping<string, Package>>> SearchImplAsync(
            SearchRequest request,
            CancellationToken cancellationToken)
        {
            var frameworks = GetCompatibleFrameworksOrNull(request.Framework);
            IQueryable<Package> search = _context.PackagesQueryable;

            search = AddSearchFilters(
                search,
                request.IncludePrerelease,
                request.IncludeSemVer2,
                request.PackageType,
                frameworks);

            if (!string.IsNullOrEmpty(request.Query))
            {
                var query = request.Query.ToLower();
                search = search
                    .Search(p => p.Identifier, query)
                    ;
            }

            var packageIds = search
                .OrderBy(p => p.Identifier)
                .Select(p => p.Identifier)
                .Distinct()
                .Skip(request.Skip)
                .Take(request.Take);

            // This query MUST fetch all versions for each package that matches the search,
            // otherwise the results for a package's latest version may be incorrect.
            // If possible, we'll find all these packages in a single query by matching
            // the package IDs in a subquery. Otherwise, run two queries:
            //   1. Find the package IDs that match the search
            //   2. Find all package versions for these package IDs
            if (_context.SupportsLimitInSubqueries)
            {
                search = _context.PackagesQueryable.Where(p => p.Identifier.In(packageIds));
            }
            else
            {
                var packageIdResults = await _context.ToListAsync(packageIds, cancellationToken);

                search = _context.PackagesQueryable.Where(p => p.Identifier.In(packageIdResults));
            }

            search = AddSearchFilters(
                search,
                request.IncludePrerelease,
                request.IncludeSemVer2,
                request.PackageType,
                frameworks);

            var results = await _context.ToListAsync(search, cancellationToken);

            return results.GroupBy(p => p.Identifier).ToList();
        }

        private IQueryable<Package> AddSearchFilters(
            IQueryable<Package> query,
            bool includePrerelease,
            bool includeSemVer2,
            string packageType,
            IReadOnlyList<string> frameworks)
        {
            if (!includePrerelease)
            {
                query = query.Where(p => !p.IsPrerelease);
            }

            if (!includeSemVer2)
            {
                query = query.Where(p => p.SemVerLevel != SemVerLevel.SemVer2);
            }

            if (!string.IsNullOrEmpty(packageType))
            {
                query = query.Where(p => p.PackageTypes.Any(t => t.Name == packageType));
            }

            if (frameworks != null)
            {
                query = query.Where(p => p.TargetFrameworks.Any(f => f.Moniker.In(frameworks)));
            }

            query = query.Where(p => p.Listed);

            return query;
        }

        private IReadOnlyList<string> GetCompatibleFrameworksOrNull(string framework)
        {
            if (framework == null) return null;

            return _frameworks.FindAllCompatibleFrameworks(framework);
        }
    }
}
