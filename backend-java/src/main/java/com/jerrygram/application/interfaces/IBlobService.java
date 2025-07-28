package com.jerrygram.application.interfaces;

import org.springframework.web.multipart.MultipartFile;

import java.io.InputStream;

public interface IBlobService {
    
    String upload(MultipartFile file, String containerName);
    
    String upload(InputStream fileStream, String fileName, String containerName);
    
    void delete(String blobUrl);
    
    void delete(String blobUrl, String containerName);
    
    boolean exists(String blobUrl);
}