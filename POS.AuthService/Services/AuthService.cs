using POS.AuthService.Entities;
using POS.AuthService.Repositories;

namespace POS.AuthService.Services
{
    public class AuthService
    {
        private readonly AuthRepository _repo;

        public AuthService(AuthRepository repo)
        {
            _repo = repo;
        }

        public bool ValidateUser(string username, string password)
        {
            var user = _repo.GetUserByUsername(username);
            if (user == null) return false;
            return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        }

        public List<string> GetPermissionsWithHierarchy(int roleId)
        {
            var permissions = new List<string>();
            LoadPermissionsRecursive(roleId, permissions);
            return permissions.Distinct().ToList();
        }

        private void LoadPermissionsRecursive(int roleId, List<string> perms)
        {
            perms.AddRange(_repo.GetPermissionsByRole(roleId));
            var role = _repo.GetRole(roleId);
            if (role?.ParentRoleId != null)
                LoadPermissionsRecursive(role.ParentRoleId.Value, perms);
        }

        public User GetUser(string username)
        {
            return _repo.GetUserByUsername(username);
        }
        public bool CheckLicense(string machineId)
        {
            return _repo.IsLicenseValid(machineId);
        }
        public bool CheckLicenseForThisMachine()
        {
            var machineId = MachineHelper.GetMachineId();
            return _repo.IsLicenseValidForMachine(machineId);
        }

        public bool CheckLicenseSecure()
        {
            var lic = _repo.GetLocalLicense();
            if (lic == null) return false;

            var decryptedSig = DataProtector.Decrypt(lic.LicenseSignature);

            var payload = $"{lic.LicenseKey}|{lic.MachineId}|{lic.StoreId}|{lic.ExpiryDate:yyyy-MM-dd}";

            return LicenseCrypto.Verify(payload, decryptedSig);
        }


    }
}
