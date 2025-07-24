using Domain.Constants;
using System.Text.RegularExpressions;

namespace Domain.ValueObjects
{
    public record PostCaption
    {
        public string Value { get; }
        public IReadOnlyList<string> Hashtags { get; }
        public IReadOnlyList<string> Mentions { get; }

        private PostCaption(string value, IReadOnlyList<string> hashtags, IReadOnlyList<string> mentions)
        {
            Value = value;
            Hashtags = hashtags;
            Mentions = mentions;
        }

        public static PostCaption Create(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return new PostCaption(string.Empty, Array.Empty<string>(), Array.Empty<string>());

            if (value.Length > DomainConstants.MaxCaptionLength)
                throw new ArgumentException($"Caption cannot exceed {DomainConstants.MaxCaptionLength} characters", nameof(value));

            var hashtags = ExtractHashtags(value);
            var mentions = ExtractMentions(value);

            return new PostCaption(value.Trim(), hashtags, mentions);
        }

        private static IReadOnlyList<string> ExtractHashtags(string caption)
        {
            var matches = Regex.Matches(caption, RegexPatterns.Hashtag, RegexOptions.IgnoreCase);
            return matches.Select(m => m.Value.ToLowerInvariant()).Distinct().ToList();
        }

        private static IReadOnlyList<string> ExtractMentions(string caption)
        {
            var matches = Regex.Matches(caption, RegexPatterns.Mention, RegexOptions.IgnoreCase);
            return matches.Select(m => m.Value.Substring(1).ToLowerInvariant()).Distinct().ToList();
        }

        public bool IsEmpty => string.IsNullOrWhiteSpace(Value);

        public bool HasHashtags => Hashtags.Any();

        public bool HasMentions => Mentions.Any();

        public static implicit operator string(PostCaption caption) => caption.Value;

        public override string ToString() => Value;
    }
}