namespace POS.UI.Core
{
    public static class AppState
    {
        public static string? CurrentUser { get; set; }
        public static string? CurrentUserName { get; set; }
        public static bool IsAuthenticated { get; set; }
        public static bool IsOnline { get; set; } = true;

        public static void SetUser(string username, string? role = null)
        {
            CurrentUser = username;
            CurrentUserName = username;
            IsAuthenticated = true;
        }

        public static void ClearUser()
        {
            CurrentUser = null;
            CurrentUserName = null;
            IsAuthenticated = false;
        }
    }
}
