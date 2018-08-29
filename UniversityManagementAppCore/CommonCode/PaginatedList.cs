using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UniversityManagementAppCore.CommonCode
{
    public class PaginatedList<T> : List<T>
    {
        public int PageIndex { get; }
        public int TotalPages { get; }
        public long TotalItems { get; }
        public int PaginationButtonNumber { get; } = 5;
        public long PageItemsStartsAt { get; }
        public long PageItemsEndsAt { get; }

        public PaginatedList(List<T> items, long count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            TotalItems = count;
            PageItemsStartsAt = count > 0 ? ((pageIndex - 1) * pageSize) + 1 : 0;

            PageItemsEndsAt = 0;
            if (count > 0)
            {
                if (pageIndex * pageSize > count)
                {
                    PageItemsEndsAt = count;
                }
                else
                {
                    PageItemsEndsAt = pageIndex * pageSize;
                }
            }

            this.AddRange(items);
        }

        public bool HasPreviousPage => (PageIndex > 1);

        public bool HasNextPage => (PageIndex < TotalPages);

        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
        {
            long count = await source.CountAsync();
            var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }
    }
}
