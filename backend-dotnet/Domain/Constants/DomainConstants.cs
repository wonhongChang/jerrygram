namespace Domain.Constants
{
    public static class DomainConstants
    {
        public const int MaxCaptionLength = 2200;
        public const int MaxCommentLength = 500;
        public const int MaxUsernameLength = 30;
        public const int MinUsernameLength = 3;
        public const int MaxEmailLength = 100;
        public const int MinPasswordLength = 8;
        
        // File upload constraints
        public const long MaxImageSizeBytes = 10 * 1024 * 1024; // 10MB
        public const long MaxAvatarSizeBytes = 5 * 1024 * 1024; // 5MB
    }

    public static class RegexPatterns
    {
        public const string Username = @"^[a-zA-Z0-9_]+$";
        public const string StrongPassword = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]";
        public const string Hashtag = @"#\w+";
        public const string Mention = @"@\w+";
    }

    public static class ContentTypes
    {
        public static readonly string[] AllowedImageTypes = {
            "image/jpeg",
            "image/jpg", 
            "image/png",
            "image/gif",
            "image/webp"
        };
    }
}