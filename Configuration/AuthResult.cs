using System.Collections.Generic;

namespace TodoApp.Configuration
{
    public class AuthResult
    {
        public string Token { get; set; }
        public bool Success { get; set; }
        public Dictionary<string, List<string>> Errors { get; set; }
    }
}