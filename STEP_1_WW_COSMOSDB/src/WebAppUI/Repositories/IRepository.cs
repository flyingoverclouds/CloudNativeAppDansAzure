using CnAppForAzureDev.Entities;
using CnAppForAzureDev.Repositories.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CnAppForAzureDev.Repositories
{
    public interface IRepository<TEntity>
       where TEntity : IEntityBase
    {
        Task<TEntity> AddAsync(TEntity entity);

        Task<TEntity> GetAsync(string id);

        Task<List<TEntity>> ListAsync();

        Task<List<TEntity>> ListAsync(ISpecification<TEntity> specification);

      

        Task RemoveAsync(string id);

        Task UpdateAsync(TEntity entity);
    }
}
