using System.ComponentModel;

namespace Common.Domain.Entities.Audit
{
    public enum AuditAction : long
    {
        None = 0,
        [Description("[SYSTEM] TOKEN VALIDATION")]
        TokenValidation,

        #region Authentication
        [Description("Đăng nhập hệ thống")]
        SignIn = 1000,
        [Description("Đăng xuất")]
        SignOut,
        #endregion

        #region Users
        [Description("Tạo tài khoản")]
        CreateAccount = 2000,
        [Description("Xóa tài khoản")]
        DeleteAccount,
        [Description("Sửa thông tin tài khoản")]
        EditAccount,
        [Description("Kích hoạt tài khoản")]
        ActivateAccount,
        [Description("Vô hiệu hóa tài khoản")]
        DeactivateAccount,
        [Description("Import danh sách thông tin tài khoản")]
        ImportAccount,
        [Description("Export danh sách tài khoản")]
        ExportAccount,
        [Description("Export toàn bộ tài khoản trong hệ thống")]
        ExportAllAccount,
        [Description("Admin đổi mật khẩu của người dùng")]
        AdminResetPassword,
        [Description("Admin đổi mật khẩu của nhiều người dùng")]
        AdminBatchResetPassword,
        [Description("User đổi mật khẩu đăng nhập")]
        UserChangePassword,
        #endregion
    }
}
