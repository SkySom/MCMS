using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatChainCommon.IdentityServerRepository;
using IdentityServer4.Models;
using IdentityServer4.Stores;

namespace ChatChainCommon.IdentityServerStore
{
    public class CustomResourceStore : IResourceStore
    {
        private readonly IRepository _dbRepository;

        public CustomResourceStore(IRepository repository)
        {
            _dbRepository = repository;
        }

        private IEnumerable<ApiResource> GetAllApiResources()
        {
            return _dbRepository.All<ApiResource>();
        }

        private IEnumerable<IdentityResource> GetAllIdentityResources()
        {
            return _dbRepository.All<IdentityResource>();
        }

        public Task<ApiResource> FindApiResourceAsync(string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

            return Task.FromResult(_dbRepository.Single<ApiResource>(a => a.Name == name));
        }
        
        public Task<IEnumerable<ApiResource>> FindApiResourcesByScopeAsync(IEnumerable<string> scopeNames)
        {
            IQueryable<ApiResource> list = _dbRepository.Where<ApiResource>(a => a.Scopes.Any(s => scopeNames.Contains(s.Name)));

            return Task.FromResult(list.AsEnumerable());
        }

        public Task<IEnumerable<IdentityResource>> FindIdentityResourcesByScopeAsync(IEnumerable<string> scopeNames)
        {
            IQueryable<IdentityResource> list = _dbRepository.Where<IdentityResource>(e => scopeNames.Contains(e.Name));

            return Task.FromResult(list.AsEnumerable());
        }

        public Task<Resources> GetAllResourcesAsync()
        {
            Resources result = new Resources(GetAllIdentityResources(), GetAllApiResources());
            return Task.FromResult(result);
        }

        private Func<IdentityResource, bool> BuildPredicate(Func<IdentityResource, bool> predicate)
        {
            return predicate;
        }
    }
}