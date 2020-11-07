﻿using System.Threading.Tasks;
using Skoruba.IdentityServer4.Admin.BusinessLogic.Dtos.Configuration;

namespace Skoruba.IdentityServer4.Admin.BusinessLogic.Services.Interfaces
{
    public interface IApiScopeService
    {
        ApiScopesDto BuildApiScopeViewModel(ApiScopesDto apiScope);

        Task<ApiScopesDto> GetApiScopesAsync(int page = 1, int pageSize = 10);

        Task<ApiScopesDto> GetApiScopeAsync(int apiScopeId);

        Task<int> AddApiScopeAsync(ApiScopesDto apiScope);

        Task<int> UpdateApiScopeAsync(ApiScopesDto apiScope);

        Task<int> DeleteApiScopeAsync(ApiScopesDto apiScope);

        Task<bool> CanInsertApiScopeAsync(ApiScopesDto apiScopes);
    }
}