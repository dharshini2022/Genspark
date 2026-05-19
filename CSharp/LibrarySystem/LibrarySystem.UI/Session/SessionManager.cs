using LibrarySystem.Models;
namespace LibrarySystem.UI.Session
{
    public static class SessionManager
    {
        public static Member? SessionMember;
        
        public static void Login(Member member)
        {
            SessionMember = member;
        }

        public static bool IsLoggedIn()
        {
            return SessionMember != null;
        }
        public static bool LogOut()
        {
            SessionMember = null;
            return true;
        }
    }
}