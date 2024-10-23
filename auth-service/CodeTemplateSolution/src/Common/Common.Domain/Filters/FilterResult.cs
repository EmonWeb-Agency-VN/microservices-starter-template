namespace Common.Domain.Filters
{
    public class FilterResult
    {
        public FilterModel Filters { get; set; }
        public int TotalCount { get; set; }
        public int TotalPageCount { get; set; }
    }

    public class FilterResult<T> : FilterResult where T : class
    {
        public T Result { get; set; }
    }
}
