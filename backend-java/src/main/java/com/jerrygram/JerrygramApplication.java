package com.jerrygram;

import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.cache.annotation.EnableCaching;
import org.springframework.data.jpa.repository.config.EnableJpaRepositories;
import org.springframework.data.elasticsearch.repository.config.EnableElasticsearchRepositories;

@SpringBootApplication
@EnableCaching
@EnableJpaRepositories(basePackages = "com.jerrygram.infrastructure.repositories")
@EnableElasticsearchRepositories(basePackages = "com.jerrygram.infrastructure.elasticsearch")
public class JerrygramApplication {

    public static void main(String[] args) {
        SpringApplication.run(JerrygramApplication.class, args);
    }
}