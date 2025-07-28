package com.jerrygram.application.interfaces;

import com.jerrygram.application.dtos.SearchResultDto;

public interface ISearchService {
    SearchResultDto search(String query, String userId);
    SearchResultDto autocomplete(String query);
}