using Domain.Constants;
using Domain.Entities;
using Domain.ValueObjects;
using Xunit;

namespace Domain.Tests;

public class PostCaptionTests
{
    [Fact]
    public void Create_NormalizesHashtagsWithoutHashPrefix()
    {
        var caption = PostCaption.Create("Building #Kafka and #kafka trends with #DotNet.");

        Assert.Equal(["kafka", "dotnet"], caption.Hashtags);
        Assert.True(caption.HasHashtags);
    }

    [Fact]
    public void Create_NormalizesMentionsAndRemovesDuplicates()
    {
        var caption = PostCaption.Create("Thanks @Jerry and @jerry for the review.");

        Assert.Equal(["jerry"], caption.Mentions);
        Assert.True(caption.HasMentions);
    }

    [Fact]
    public void Create_TrimsValueButKeepsExtractedTokens()
    {
        var caption = PostCaption.Create("  A clean seed for #Kafka by @Jerry  ");

        Assert.Equal("A clean seed for #Kafka by @Jerry", caption.Value);
        Assert.Equal(["kafka"], caption.Hashtags);
        Assert.Equal(["jerry"], caption.Mentions);
    }

    [Fact]
    public void Create_RejectsOverlongCaption()
    {
        var value = new string('a', DomainConstants.MaxCaptionLength + 1);

        Assert.Throws<ArgumentException>(() => PostCaption.Create(value));
    }

    [Fact]
    public void Post_CaptionSetterClearsNullOrEmptyCaption()
    {
        var post = new Post { Caption = "hello #seed" };

        post.Caption = string.Empty;

        Assert.Null(post.Caption);
        Assert.Empty(post.Hashtags);
    }
}
