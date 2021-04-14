using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Repositories.Services.Interfaces
{
    public interface ISpecification<TEntity>
        where TEntity : IEntityBase
    {
        Expression<Func<TEntity, bool>> Criteria { get; }
    }
}
