using System.ComponentModel.DataAnnotations;
using Subman.Models;
using Subman.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;

namespace Subman.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SubscriptionController : BaseController<Subscription> {

    private readonly SubscriptionRepository _subscriptionRepository;
    private readonly ILogger<SubscriptionController> _logger;

    public SubscriptionController(SubscriptionRepository subscriptionRepository, ILogger<SubscriptionController> logger)
        : base(subscriptionRepository) {
        _subscriptionRepository = subscriptionRepository;
        _logger = logger;
    }

    public override async Task<ActionResult<IEnumerable<Subscription>>> GetAll() {
        try {
            var subscriptions = await _subscriptionRepository.GetAllAsync();
            return Ok(subscriptions);
        } catch (Exception ex) {
            _logger.LogError(ex, "Error fetching subscriptions");
            return StatusCode(500, "Error fetching subscriptions");
        }
    }

    public override async Task<ActionResult<Subscription>> GetById(string id) {
        try {
            if (!ObjectId.TryParse(id, out _))
                return BadRequest("Invalid subscription id format");

            var subscription = await _subscriptionRepository.GetByIdAsync(id);
            return subscription == null ? NotFound("Subscription not found") : Ok(subscription);
        } catch (Exception ex) {
            _logger.LogError(ex, $"Error fetching subscription with id {id}");
            return StatusCode(500, $"Couldn't fetch subscription with id {id}");
        }
    }

    public override async Task<ActionResult<Subscription>> Create(Subscription subscription) {
        try {
            ValidateSubscription(subscription);

            await _subscriptionRepository.CreateAsync(subscription);
            return CreatedAtAction(nameof(GetById), new { id = subscription.Id }, subscription);
        } catch (ValidationException ex) {
            return BadRequest(ex.Message);
        } catch (Exception ex) {
            _logger.LogError(ex, "Error creating subscription");
            return StatusCode(500, "Error creating subscription");
        }
    }

    public override async Task<ActionResult<Subscription>> Update(string id, Subscription subscription) {
        try {
            if (GetById(id) == null)
                return BadRequest("Invalid subscription id format");
            ValidateSubscription(subscription);

            var existingSubscription = await _subscriptionRepository.GetByIdAsync(id);
            if (existingSubscription == null)
                return NotFound("Subscription not found");
            
            subscription.Id = id; // update id for update operation
            await _subscriptionRepository.UpdateAsync(id, subscription);
            return NoContent();
        } catch (ValidationException ex) {
            return BadRequest(ex.Message);
        } catch (Exception ex) {
            _logger.LogError(ex, $"Error updating subscription with id {id}");
            return StatusCode(500, $"Couldn't update subscription with id {id}");
        }
    }

    public override async Task<ActionResult<Subscription>> Delete(string id) {
        try {
            var existingSubscription = await _subscriptionRepository.GetByIdAsync(id);
            if (existingSubscription == null)
                return NotFound("Subscription not found");
            
            await _subscriptionRepository.DeleteAsync(id);
            return NoContent();
        } catch (Exception ex) {
            _logger.LogError(ex, $"Error deleting subscription with id {id}");
            return StatusCode(500, $"Couldn't delete subscription with id {id}");
        }
    }

    //validation
    private void ValidateSubscription(Subscription subscription)
    {
        var context = new ValidationContext(subscription);
        var results = new List<ValidationResult>();

        if (!Validator.TryValidateObject(subscription, context, results, true))
        {
            throw new ValidationException(string.Join("; ", results.Select(r => r.ErrorMessage)));
        }
    }
}