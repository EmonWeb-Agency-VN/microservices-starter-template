namespace Common.Domain.Filters
{
    public class FilterModel
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public Dictionary<string, bool> SortKey { get; set; } = [];
        public bool IsAscending { get; set; }
        public SearchOperand SearchOperand { get; set; } = SearchOperand.Fuzzy;
        public List<string> SearchKeys { get; set; } = new List<string>();
        public string SearchContent { get; set; } = string.Empty;
        public Dictionary<string, List<FilterItem>> Filters { get; set; } = [];
    }

    public class FilterItem
    {
        public string Name { get; set; }
        public int EnumValue { get; set; }
        public bool Checked { get; set; }
    }

    public class SearchCriteria
    {
        public string SearchColumn { get; set; }
        public SearchOperand Operand { get; set; }
    }

    public enum SearchOperand
    {
        Fuzzy = 0,
        Accurate = 1,
        StartsWith = 2,
        EndsWith = 3,
    }
}
