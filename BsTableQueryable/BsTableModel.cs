using System.Collections.Generic;

namespace BsTableQueryable
{
    public class BsTableModel
    {
        public class Response<T>
        {
            public int Total { get; set; }
            public int TotalNotFiltered { get; set; }
            public List<T> Rows { get; set; }
        }

        public class Request
        {
            public int PageSize { get; set; } = 10;
            public int PageNumber { get; set; } = 1;
            public string SearchText { get; set; }
            public string SortName { get; set; } = "Id";
            public SortOrder SortOrder { get; set; } = SortOrder.Asc;
            public List<FilterByField> FilterBy { get; set; } = null;
        }

        public class FilterByField
        {
            public string Name { get; set; }
            public string Value { get; set; }
            public bool CaseSensitive { get; set; } = true;
            public Comparator Comparator { get; set; } = Comparator.Equal;
        }

        public enum SortOrder
        {
            Asc,
            Desc
        }

        public enum Comparator
        {
            Equal,
            LessThan,
            LessThanOrEqual,
            GreaterThan,
            GreaterThanOrEqual,
            NotEqual,
            Contains,
            StartsWith,
            EndsWith
        }
    }
}