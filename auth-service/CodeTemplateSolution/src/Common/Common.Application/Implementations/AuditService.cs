//using Common.Domain.Entities.Audit;
//using Common.Domain.Interfaces;
//using Common.SharedKernel.Extensions;
//using Microsoft.AspNetCore.Http;

//namespace Common.Application.Implementations
//{
//    public class AuditService : IAuditService
//    {
//        private readonly IHttpContextAccessor _context;
//        public AuditService(IHttpContextAccessor context)
//        {
//            _context = context;
//        }
//        public void SetAuditObj(AuditModel auditModel)
//        {
//            _context.HttpContext.SetAuditObj(auditModel);
//        }
//    }
//}
