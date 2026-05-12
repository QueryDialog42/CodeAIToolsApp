package com.example.codeai.Mappers;

import org.mapstruct.Mapper;
import org.mapstruct.Mapping;

import com.example.codeai.Dtos.SettingDto;
import com.example.codeai.Entities.Settings;

@Mapper(componentModel = "spring")
public interface ISettingMapper {
    SettingDto ToDto(Settings settings);
}
