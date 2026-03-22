package com.example.codeai.Mappers;

import java.util.List;
import org.mapstruct.Mapper;
import org.mapstruct.Mapping;
import com.example.codeai.Dtos.UserDto;
import com.example.codeai.Entities.Users;
import com.example.codeai.Projections.UserProjection;


@Mapper(componentModel = "spring")
public interface IUserMapper {
    @Mapping(target = "u_id", ignore = true)
    Users ToEntity(UserDto userDto);

    List<UserDto> toDtos(List<UserProjection> users);
}
