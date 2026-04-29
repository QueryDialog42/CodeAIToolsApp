package com.example.codeai.Mappers;

import com.example.codeai.Dtos.SubscriptionDto;
import com.example.codeai.Entities.Subs;
import org.mapstruct.Mapper;
import org.mapstruct.Mapping;
import org.mapstruct.factory.Mappers;

@Mapper
public interface SubscriptionMapper {
    SubscriptionMapper INSTANCE = Mappers.getMapper(SubscriptionMapper.class);

    @Mapping(source = "user.u_id", target = "u_id")
    SubscriptionDto toDto(Subs subscription);

    @Mapping(target = "user", ignore = true)
    Subs toEntity(SubscriptionDto subscriptionDto);
}
