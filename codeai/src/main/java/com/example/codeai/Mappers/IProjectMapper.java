package com.example.codeai.Mappers;

import java.util.List;
import org.mapstruct.Mapper;
import org.mapstruct.Mapping;
import com.example.codeai.Dtos.ProjectDto;
import com.example.codeai.Entities.Projects;


@Mapper(componentModel = "spring")
public interface IProjectMapper {
    @Mapping(target = "belongs_to", ignore = true)
    Projects toEntity(ProjectDto projectDto);

    @Mapping(target = "belongs_to", source = "belongs_to.u_id")
    ProjectDto toDto(Projects projects);

    List<ProjectDto> toDtos(List<Projects> projects);
}
