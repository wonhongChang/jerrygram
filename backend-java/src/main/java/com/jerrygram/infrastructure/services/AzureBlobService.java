package com.jerrygram.infrastructure.services;

import com.azure.storage.blob.BlobClient;
import com.azure.storage.blob.BlobContainerClient;
import com.azure.storage.blob.BlobServiceClient;
import com.azure.storage.blob.BlobServiceClientBuilder;
import com.azure.storage.blob.models.PublicAccessType;
import com.jerrygram.application.interfaces.IBlobService;
import lombok.extern.slf4j.Slf4j;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.boot.autoconfigure.condition.ConditionalOnProperty;
import org.springframework.stereotype.Service;
import org.springframework.web.multipart.MultipartFile;

import java.io.IOException;
import java.io.InputStream;
import java.util.UUID;

@Service
@Slf4j
public class AzureBlobService implements IBlobService {

    private final BlobServiceClient blobServiceClient;

    public AzureBlobService(@Value("${azure.blob.connection-string}") String connectionString) {
        this.blobServiceClient = new BlobServiceClientBuilder()
                .connectionString(connectionString)
                .buildClient();
        log.info("Azure Blob Service initialized");
    }

    @Override
    public String upload(MultipartFile file, String containerName) {
        try {
            return upload(file.getInputStream(), file.getOriginalFilename(), containerName);
        } catch (IOException e) {
            log.error("Failed to upload file to Azure Blob Storage: {}", file.getOriginalFilename(), e);
            throw new RuntimeException("Azure Blob upload failed", e);
        }
    }

    @Override
    public String upload(InputStream fileStream, String fileName, String containerName) {
        try {
            BlobContainerClient containerClient = getOrCreateContainer(containerName);
            
            // Generate unique filename
            String fileExtension = getFileExtension(fileName);
            String uniqueFileName = UUID.randomUUID().toString() + fileExtension;
            
            BlobClient blobClient = containerClient.getBlobClient(uniqueFileName);
            
            // Upload file
            blobClient.upload(fileStream, fileStream.available(), true);
            
            String blobUrl = blobClient.getBlobUrl();
            log.info("File uploaded successfully to Azure Blob Storage: {}", blobUrl);
            
            return blobUrl;
        } catch (Exception e) {
            log.error("Failed to upload file to Azure Blob Storage: {}", fileName, e);
            throw new RuntimeException("Azure Blob upload failed", e);
        }
    }

    @Override
    public void delete(String blobUrl) {
        try {
            // Extract container and blob name from URL
            String[] urlParts = extractBlobInfo(blobUrl);
            String containerName = urlParts[0];
            String blobName = urlParts[1];
            
            BlobContainerClient containerClient = blobServiceClient.getBlobContainerClient(containerName);
            BlobClient blobClient = containerClient.getBlobClient(blobName);
            
            if (blobClient.exists()) {
                blobClient.delete();
                log.info("File deleted successfully from Azure Blob Storage: {}", blobUrl);
            } else {
                log.warn("Blob not found for deletion: {}", blobUrl);
            }
        } catch (Exception e) {
            log.error("Failed to delete file from Azure Blob Storage: {}", blobUrl, e);
        }
    }

    @Override
    public void delete(String blobUrl, String containerName) {
        delete(blobUrl);
    }

    @Override
    public boolean exists(String blobUrl) {
        try {
            String[] urlParts = extractBlobInfo(blobUrl);
            String containerName = urlParts[0];
            String blobName = urlParts[1];
            
            BlobContainerClient containerClient = blobServiceClient.getBlobContainerClient(containerName);
            BlobClient blobClient = containerClient.getBlobClient(blobName);
            
            return blobClient.exists();
        } catch (Exception e) {
            log.error("Failed to check blob existence: {}", blobUrl, e);
            return false;
        }
    }

    private BlobContainerClient getOrCreateContainer(String containerName) {
        BlobContainerClient containerClient = blobServiceClient.getBlobContainerClient(containerName);
        
        if (!containerClient.exists()) {
            containerClient.createWithResponse(null, PublicAccessType.BLOB, null, null);
            log.info("Created Azure Blob container: {}", containerName);
        }
        
        return containerClient;
    }

    private String[] extractBlobInfo(String blobUrl) {
        // Example URL: https://storageaccount.blob.core.windows.net/container/blobname
        String[] parts = blobUrl.split("/");
        if (parts.length < 5) {
            throw new IllegalArgumentException("Invalid blob URL format: " + blobUrl);
        }
        
        String containerName = parts[3]; // container name
        String blobName = parts[4]; // blob name
        
        return new String[]{containerName, blobName};
    }

    private String getFileExtension(String fileName) {
        if (fileName == null || fileName.isEmpty()) {
            return "";
        }
        
        int lastDotIndex = fileName.lastIndexOf('.');
        if (lastDotIndex == -1) {
            return "";
        }
        
        return fileName.substring(lastDotIndex);
    }
}