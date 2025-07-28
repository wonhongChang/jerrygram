package com.jerrygram.application.common;

import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Data;
import lombok.NoArgsConstructor;
import org.springframework.data.annotation.Id;
import org.springframework.data.elasticsearch.annotations.Document;
import org.springframework.data.elasticsearch.annotations.Field;
import org.springframework.data.elasticsearch.annotations.FieldType;

import java.time.LocalDateTime;

@Data
@Builder
@NoArgsConstructor
@AllArgsConstructor
@Document(indexName = "users")
public class UserIndex {
    
    @Id
    private String id;
    
    @Field(type = FieldType.Text, analyzer = "standard")
    private String username;
    
    @Field(type = FieldType.Text)
    private String email;
    
    @Field(type = FieldType.Text)
    private String profileImageUrl;
    
    @Field(type = FieldType.Integer)
    private Integer followersCount;
    
    @Field(type = FieldType.Integer)
    private Integer followingCount;
    
    @Field(type = FieldType.Integer)
    private Integer postsCount;
    
    @Field(type = FieldType.Date)
    private LocalDateTime createdAt;
    
    @Field(type = FieldType.Boolean)
    private Boolean isVerified;
}