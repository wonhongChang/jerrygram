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
@Document(indexName = "tags")
public class TagIndex {
    
    @Id
    private String id;
    
    @Field(type = FieldType.Text, analyzer = "keyword")
    private String name;
    
    @Field(type = FieldType.Integer)
    private Integer usageCount;
    
    @Field(type = FieldType.Date)
    private LocalDateTime lastUsed;
    
    @Field(type = FieldType.Boolean)
    private Boolean isActive;
}