package com.jerrygram.application.dtos;

import com.jerrygram.domain.enums.PostVisibility;
import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;
import org.springframework.web.multipart.MultipartFile;

@Data
@NoArgsConstructor
@AllArgsConstructor
public class CreatePostDto {
    private String caption;
    private MultipartFile image;
    private PostVisibility visibility = PostVisibility.Public;
}