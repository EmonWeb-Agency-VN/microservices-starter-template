namespace Common.Persistence.InitDataHelper
{
    public interface IDataInitService
    {
        int Step { get; }
        string FileName { get; }
        Task<ProcessStatus> ExecuteAsync();
    }
}
