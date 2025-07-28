package com.jerrygram.application.dtos;

import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.util.List;

@Data
@Builder
@NoArgsConstructor
@AllArgsConstructor
public class PagedResult<T> {
    private int totalCount;
    private int page;
    private int pageSize;
    @Builder.Default
    private List<T> items = List.of();
}