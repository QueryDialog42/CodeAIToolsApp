package com.example.codeai.Services;

import com.example.codeai.Dtos.SubscriptionDto;
import com.example.codeai.Dtos.SubscriptionPlanDto;
import com.example.codeai.Dtos.CreateSubscriptionDto;
import com.example.codeai.Entities.Subs;
import com.example.codeai.Entities.Users;
import com.example.codeai.Repositories.ISubRepository;
import com.example.codeai.Repositories.IUserRepository;
import com.example.codeai.Mappers.SubscriptionMapper;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.sql.Timestamp;
import java.time.Instant;
import java.time.temporal.ChronoUnit;
import java.util.List;
import java.util.Optional;
import java.util.stream.Collectors;

@Service
@Transactional
public class SubscriptionService {

    private final ISubRepository subscriptionRepository;
    private final IUserRepository userRepository;

    public SubscriptionService(ISubRepository subscriptionRepository, IUserRepository userRepository) {
        this.subscriptionRepository = subscriptionRepository;
        this.userRepository = userRepository;
    }

    public SubscriptionDto getUserSubscription(Integer userId) {
        Optional<Subs> subscription = subscriptionRepository.findByUser_U_id(userId);
        return subscription.map(SubscriptionMapper.INSTANCE::toDto).orElse(null);
    }

    public boolean createSubscription(CreateSubscriptionDto createDto) {
        try {
            // Validate user exists
            Optional<Users> userOpt = userRepository.findById(createDto.getU_id());
            if (userOpt.isEmpty()) {
                return false;
            }

            Users user = userOpt.get();
            
            // Check if user already has a subscription
            Optional<Subs> existingSubscription = subscriptionRepository.findByUser(user);
            if (existingSubscription.isPresent()) {
                // Update existing subscription
                Subs subscription = existingSubscription.get();
                updateSubscriptionDetails(subscription, createDto.getS_plan());
                subscriptionRepository.save(subscription);
                return true;
            }

            // Create new subscription
            Subs newSubscription = new Subs();
            newSubscription.setUser(user);
            updateSubscriptionDetails(newSubscription, createDto.getS_plan());
            
            subscriptionRepository.save(newSubscription);
            return true;
        } catch (Exception e) {
            return false;
        }
    }

    public boolean cancelSubscription(Integer userId) {
        try {
            Optional<Subs> subscriptionOpt = subscriptionRepository.findByUser_U_id(userId);
            if (subscriptionOpt.isEmpty()) {
                return false;
            }

            Subs subscription = subscriptionOpt.get();
            subscription.setS_is_active(false);
            subscriptionRepository.save(subscription);
            return true;
        } catch (Exception e) {
            return false;
        }
    }

    public boolean updateSubscription(Integer userId, String newPlan) {
        try {
            Optional<Subs> subscriptionOpt = subscriptionRepository.findByUser_U_id(userId);
            if (subscriptionOpt.isEmpty()) {
                return false;
            }

            Subs subscription = subscriptionOpt.get();
            updateSubscriptionDetails(subscription, newPlan);
            subscriptionRepository.save(subscription);
            return true;
        } catch (Exception e) {
            return false;
        }
    }

    public List<SubscriptionPlanDto> getAvailablePlans() {
        return List.of(
            new SubscriptionPlanDto("free", 0.0, "monthly", 
                new String[]{"Basic AI assistance", "5 projects max", "Community support"}),
            new SubscriptionPlanDto("premium", 29.99, "monthly", 
                new String[]{"Advanced AI assistance", "Unlimited projects", "Priority support", "GitHub integration"}),
            new SubscriptionPlanDto("pro", 99.99, "monthly", 
                new String[]{"Premium AI assistance", "Unlimited projects", "24/7 support", "Advanced GitHub features", "Team collaboration"})
        );
    }

    private void updateSubscriptionDetails(Subs subscription, String plan) {
        subscription.setS_plan(plan);
        subscription.setS_is_active(true);
        subscription.setS_start_date(Timestamp.from(Instant.now()));
        
        // Set end date based on plan (simplified - 1 month for paid plans, 1 year for free)
        if ("free".equals(plan)) {
            subscription.setS_end_date(Timestamp.from(Instant.now().plus(365, ChronoUnit.DAYS)));
        } else {
            subscription.setS_end_date(Timestamp.from(Instant.now().plus(30, ChronoUnit.DAYS)));
        }
    }

    public boolean isSubscriptionActive(Integer userId) {
        Optional<Subs> subscription = subscriptionRepository.findByUser_U_id(userId);
        if (subscription.isEmpty()) {
            return false;
        }
        
        Subs sub = subscription.get();
        return sub.getS_is_active() && 
               sub.getS_end_date() != null && 
               sub.getS_end_date().after(Timestamp.from(Instant.now()));
    }
}
