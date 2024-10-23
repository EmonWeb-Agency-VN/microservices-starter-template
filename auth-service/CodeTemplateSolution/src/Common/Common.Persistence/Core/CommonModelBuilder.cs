using Microsoft.EntityFrameworkCore;

namespace Common.Persistence.Core
{
    public sealed class CommonModelBuilder
    {
        public static void AppendConfiguration(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(Common.Persistence.AssemblyReference.Assembly);
        }
    }
}
