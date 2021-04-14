using CnAppForAzureDev.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CnAppForAzureDev.Repositories.Specifications
{
    public interface ISpecification<TEntity>
         where TEntity : IEntityBase
    {
        Expression<Func<TEntity, bool>> Criteria { get; }
    }
}
