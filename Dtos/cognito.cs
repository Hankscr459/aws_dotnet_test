namespace Dtos.cognito
{
    public class SignupInput
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? GivenName { get; set; }
        public string? FamilyName { get; set; }
        public string? Address { get; set; }
    }

    public class SignInInput
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
    }
}