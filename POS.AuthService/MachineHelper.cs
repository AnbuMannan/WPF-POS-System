namespace POS.AuthService
{
    public static class MachineHelper
    {
        public static string GetMachineId()
        {
            return Environment.MachineName;
        }
    }

}
