package com.example.codeai.Controllers;

import com.example.codeai.Dtos.SubscriptionDto;
import com.example.codeai.Dtos.SubscriptionPlanDto;
import com.example.codeai.Dtos.CreateSubscriptionDto;
import com.example.codeai.Services.SubscriptionService;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.util.List;

@RestController
@RequestMapping("/subscription")
public class SubscriptionController {

    private final SubscriptionService subscriptionService;

    public SubscriptionController(SubscriptionService subscriptionService) {
        this.subscriptionService = subscriptionService;
    }

    @GetMapping("/get")
    public ResponseEntity<SubscriptionDto> getUserSubscription(@RequestParam long user_id) {
        SubscriptionDto subscription = subscriptionService.getUserSubscription((int) user_id);
        if (subscription != null) {
            return ResponseEntity.ok(subscription);
        }
        return ResponseEntity.notFound().build();
    }

    @PostMapping("/create")
    public ResponseEntity<String> createSubscription(@RequestBody CreateSubscriptionDto createDto) {
        boolean success = subscriptionService.createSubscription(createDto);
        if (success) {
            return ResponseEntity.ok("Subscription created successfully");
        }
        return ResponseEntity.badRequest().body("Failed to create subscription");
    }

    @PutMapping("/update")
    public ResponseEntity<String> updateSubscription(@RequestParam long user_id, @RequestParam String plan) {
        boolean success = subscriptionService.updateSubscription((int) user_id, plan);
        if (success) {
            return ResponseEntity.ok("Subscription updated successfully");
        }
        return ResponseEntity.badRequest().body("Failed to update subscription");
    }

    @DeleteMapping("/cancel")
    public ResponseEntity<String> cancelSubscription(@RequestParam long user_id) {
        boolean success = subscriptionService.cancelSubscription((int) user_id);
        if (success) {
            return ResponseEntity.ok("Subscription cancelled successfully");
        }
        return ResponseEntity.badRequest().body("Failed to cancel subscription");
    }

    @GetMapping("/plans")
    public ResponseEntity<List<SubscriptionPlanDto>> getAvailablePlans() {
        List<SubscriptionPlanDto> plans = subscriptionService.getAvailablePlans();
        return ResponseEntity.ok(plans);
    }

    @GetMapping("/status")
    public ResponseEntity<Boolean> getSubscriptionStatus(@RequestParam long user_id) {
        boolean isActive = subscriptionService.isSubscriptionActive((int) user_id);
        return ResponseEntity.ok(isActive);
    }
}
