package com.jerrygram.application.interfaces;

import com.jerrygram.application.common.PostIndex;
import com.jerrygram.application.common.TagIndex;
import com.jerrygram.application.common.UserIndex;

import java.util.List;

public interface IElasticService {
    
    void indexUser(UserIndex userIndex);
    
    void indexPost(PostIndex postIndex);
    
    void indexTag(TagIndex tagIndex);
    
    void updateUser(UserIndex userIndex);
    
    void updatePost(PostIndex postIndex);
    
    void deletePost(String postId);
    
    List<PostIndex> searchPosts(String query);
    
    List<UserIndex> searchUsers(String query, int size);
    
    List<TagIndex> searchTags(String query, int size);
}