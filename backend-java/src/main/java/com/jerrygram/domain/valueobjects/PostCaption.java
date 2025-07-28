package com.jerrygram.domain.valueobjects;

import lombok.Getter;

import java.util.List;
import java.util.regex.Pattern;
import java.util.stream.Collectors;

@Getter
public class PostCaption {
    
    private static final Pattern HASHTAG_PATTERN = Pattern.compile("#([a-zA-Z0-9_가-힣]+)");
    private static final Pattern MENTION_PATTERN = Pattern.compile("@([a-zA-Z0-9_\\.]+)");
    private static final int MAX_LENGTH = 2200;
    
    private final String value;
    private final List<String> hashtags;
    private final List<String> mentions;
    
    private PostCaption(String value) {
        this.value = value;
        this.hashtags = extractHashtags(value);
        this.mentions = extractMentions(value);
    }
    
    public static PostCaption create(String caption) {
        if (caption == null) {
            return new PostCaption("");
        }
        
        if (caption.length() > MAX_LENGTH) {
            throw new IllegalArgumentException(
                String.format("Caption exceeds maximum length of %d characters", MAX_LENGTH));
        }
        
        return new PostCaption(caption.trim());
    }
    
    
    private List<String> extractHashtags(String text) {
        if (text == null || text.isEmpty()) {
            return List.of();
        }
        
        return HASHTAG_PATTERN.matcher(text)
                .results()
                .map(matchResult -> matchResult.group(1).toLowerCase())
                .distinct()
                .collect(Collectors.toList());
    }
    
    private List<String> extractMentions(String text) {
        if (text == null || text.isEmpty()) {
            return List.of();
        }
        
        return MENTION_PATTERN.matcher(text)
                .results()
                .map(matchResult -> matchResult.group(1).toLowerCase())
                .distinct()
                .collect(Collectors.toList());
    }
    
    public boolean hasHashtags() {
        return !hashtags.isEmpty();
    }
    
    
    public boolean isEmpty() {
        return value == null || value.trim().isEmpty();
    }
    
    public int length() {
        return value != null ? value.length() : 0;
    }
    
    @Override
    public String toString() {
        return value;
    }
    
    @Override
    public boolean equals(Object obj) {
        if (this == obj) return true;
        if (obj == null || getClass() != obj.getClass()) return false;
        
        PostCaption that = (PostCaption) obj;
        return value != null ? value.equals(that.value) : that.value == null;
    }
    
    @Override
    public int hashCode() {
        return value != null ? value.hashCode() : 0;
    }
}