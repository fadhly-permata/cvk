using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BsTableQueryable
{
    public class BsTableDataProvider
    {
        public static async Task<BsTableModel.Response<T>> BsTableDataGenerator<T>(IQueryable<T> data, BsTableModel.Request request, CancellationToken cancellationToken = default)
        {
            var ttl = data.Count();
            var ttlNF = ttl;
            var qry = data;

            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                qry = qry.GlobalSearch(request.SearchText);
                ttl = qry.Count();
            }

            if (request.FilterBy != null && request.FilterBy.Count > 0)
            {
                qry = qry.SpecificSearch(request.FilterBy);
                ttl = qry.Count();
            }

            return new BsTableModel.Response<T>
            {
                Total = ttl,
                TotalNotFiltered = ttlNF,
                Rows = await qry
                    .Sort(request.SortName, request.SortOrder)
                    .Paging(request.PageNumber, request.PageSize)
                    .ToListAsync(cancellationToken)
            };
        }
    }
}