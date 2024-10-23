namespace Common.Persistence.InitDataHelper
{
    public abstract class DataInitService
    {
        protected string SeedFolder { get; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SeedData");
        public bool NeedInit { get; set; } = true;
        public async Task<ProcessStatus> BaseExecute()
        {
            if (NeedInit)
            {
                return await InitSync();
            }

            return ProcessStatus.Pass;
        }

        protected abstract Task<ProcessStatus> InitSync();
    }

    public enum ProcessStatus
    {
        Pass = 0,
        Failed
    }
}
