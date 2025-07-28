package com.jerrygram.application.common;

import java.util.ArrayList;
import java.util.HashSet;
import java.util.List;
import java.util.Set;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

public class HashtagExtractor {
    
    private static final Pattern HASHTAG_PATTERN = Pattern.compile("#([a-zA-Z0-9_가-힣]+)");
    
    public static List<String> extractHashtags(String content) {
        if (content == null || content.trim().isEmpty()) {
            return new ArrayList<>();
        }
        
        Set<String> hashtags = new HashSet<>();
        Matcher matcher = HASHTAG_PATTERN.matcher(content);
        
        while (matcher.find()) {
            String hashtag = matcher.group(1).toLowerCase();
            if (!hashtag.isEmpty() && hashtag.length() <= 50) {
                hashtags.add(hashtag);
            }
        }
        
        return new ArrayList<>(hashtags);
    }
    
    public static boolean containsHashtag(String content) {
        return content != null && HASHTAG_PATTERN.matcher(content).find();
    }
    
    public static String normalizeHashtag(String hashtag) {
        if (hashtag == null) {
            return "";
        }
        
        // Remove # if present and convert to lowercase
        String normalized = hashtag.startsWith("#") ? hashtag.substring(1) : hashtag;
        return normalized.toLowerCase().trim();
    }
}