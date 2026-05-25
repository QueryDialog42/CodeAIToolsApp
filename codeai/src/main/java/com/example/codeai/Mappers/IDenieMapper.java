package com.example.codeai.Mappers;

import org.mapstruct.Mapper;
import org.mapstruct.Mapping;

import com.example.codeai.Dtos.DenieDto;
import com.example.codeai.Entities.Denies;

@Mapper(componentModel = "spring")
public interface IDenieMapper {
    @Mapping(target = "id", ignore = true)
    Denies toEntity(DenieDto denieDto);
}
