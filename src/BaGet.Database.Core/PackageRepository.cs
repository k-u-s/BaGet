using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace BaGet.Core
{
    public class PackageRepository : IPackageRepository
    {
        private readonly IContext _context;

        public PackageRepository(IContext context)
        {
            _context = context;
        }
    }
}
