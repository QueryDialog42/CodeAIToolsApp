package com.example.codeai.Mappers;

import java.util.List;
import org.mapstruct.Mapper;
import org.mapstruct.Mapping;
import com.example.codeai.Dtos.DutyDto;
import com.example.codeai.Entities.Duties;

@Mapper(componentModel = "spring")
public interface IDutyMapper {
    @Mapping(target = "worker_id", ignore = true)
    @Mapping(target = "project_id", source = "project_id.p_id")
    DutyDto toDto(Duties duty);

    List<DutyDto> toDtos(List<Duties> duties);
}
