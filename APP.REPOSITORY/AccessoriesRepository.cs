using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using APP.MODELS;
using Microsoft.EntityFrameworkCore;
using Portal.Data;
using System.Linq;

namespace APP.REPOSITORY
{
    public interface IAccessoriesRepository : IRepository<Accessories>
    {
    }
    public class AccessoriesRepository : Repository<Accessories>, IAccessoriesRepository
    {
        readonly private APPDbContext _db;
        public AccessoriesRepository(DbContext context) : base(context)
        {
        }
    }
}
