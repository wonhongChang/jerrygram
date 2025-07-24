using Domain.Constants;
using System.Text.RegularExpressions;

namespace Domain.ValueObjects
{
    public record Username
    {
        public string Value { get; }

        private Username(string value)
        {
            Value = value;
        }

        public static Username Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Username cannot be empty", nameof(value));

            if (value.Length < DomainConstants.MinUsernameLength)
                throw new ArgumentException($"Username must be at least {DomainConstants.MinUsernameLength} characters", nameof(value));

            if (value.Length > DomainConstants.MaxUsernameLength)
                throw new ArgumentException($"Username cannot exceed {DomainConstants.MaxUsernameLength} characters", nameof(value));

            if (!Regex.IsMatch(value, RegexPatterns.Username))
                throw new ArgumentException("Username can only contain letters, numbers, and underscores", nameof(value));

            return new Username(value.ToLowerInvariant());
        }

        public static implicit operator string(Username username) => username.Value;

        public override string ToString() => Value;
    }
}