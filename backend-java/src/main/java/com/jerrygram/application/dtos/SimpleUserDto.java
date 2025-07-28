package com.jerrygram.application.dtos;

import com.fasterxml.jackson.databind.annotation.JsonDeserialize;
import com.jerrygram.infrastructure.configuration.UuidDeserializer;
import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.util.UUID;

@Data
@Builder
@NoArgsConstructor
@AllArgsConstructor
public class SimpleUserDto {
    @JsonDeserialize(using = UuidDeserializer.class)
    private UUID id;
    private String username;
    private String profileImageUrl;
}