using CnAppForAzureDev.Entities;
using CnAppForAzureDev.Repositories.Specifications;
using Repositories.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CnAppForAzureDev.Repositories
{
    public class DbRepository<TEntity> : IRepository<TEntity>
       where TEntity : IEntityBase
    {
        //TODO: Use Table storage instead of In Memory
        public static List<TEntity> Carts { get; set; }
        public Task<TEntity> AddAsync(TEntity entity)
        {
            //TODO : Mock           
            var cart = Carts.Where(t => t.OwnerId == entity.OwnerId)
                        .Select(t => t).FirstOrDefault();
            if (cart == null)
            {
                entity.Id = Guid.NewGuid().ToString();
                Carts.Add(entity);
            }

            return Task.FromResult(entity);
        }

        public Task<TEntity> GetAsync(string id)
        {
            var cart = Carts.Where(t => t.Id == id);
            return Task.FromResult(Carts.FirstOrDefault());
        }

        public Task<List<TEntity>> ListAsync()
        {
            throw new NotImplementedException();
        }

        public Task<List<TEntity>> ListAsync(ISpecification<TEntity> specification)
        {
            if (Carts == null)
            {
                Carts = new List<TEntity>();
            }
            return Task.FromResult(Carts);
            
        }

        public Task RemoveAsync(string id)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateAsync(TEntity entity)
        {
            //throw new NotImplementedException();
            //DO nothing
        }
    }
}
