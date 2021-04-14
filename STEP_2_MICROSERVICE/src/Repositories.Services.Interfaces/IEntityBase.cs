using System;
using System.Collections.Generic;
using System.Text;

namespace Repositories.Services.Interfaces
{
    public interface IEntityBase
    {
        public string Id { get; set; }
        public string OwnerId { get; set; }
    }
}
