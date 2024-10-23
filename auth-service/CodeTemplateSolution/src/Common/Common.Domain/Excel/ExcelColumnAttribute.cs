namespace Common.Domain.Excel
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ExcelColumnAttribute : Attribute
    {
        public string Name { get; set; }
        public bool IsRequired { get; set; }
        public ExcelColumnAttribute(string name, bool isRequired)
        {
            this.Name = name;
            this.IsRequired = isRequired;
        }
    }
}
