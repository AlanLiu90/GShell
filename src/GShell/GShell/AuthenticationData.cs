namespace GShell
{
    internal enum AuthenticationType
    {
        None,
        Basic,
        JWT,
    }

    internal class AuthenticationData
    {
        public AuthenticationType Type { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }
    }
}
