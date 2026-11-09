using CampusBooking.Infrastructure.DTOs;
using CampusBooking.Infrastructure.Entities;

namespace CampusBooking.Infrastructure.Services;

/// <summary>
/// Interface for mock payment processing and subscription-based booking confirmation.
/// </summary>
public interface IPlataService
{
    /// <summary>
    /// Processes a mock single-session payment: transitions a <see cref="TEMPORAR_BLOCATA"/> reservation to
    /// <see cref="CONFIRMATA"/> and creates a <see cref="Plata"/> record with status <c>SUCCES</c>.
    /// </summary>
    Task<(bool Success, PaymentResponse? Response, string? ErrorMessage)> ProcessSingleSessionPaymentAsync(
        PaymentRequest req, CancellationToken ct = default);

    /// <summary>
    /// Processes a mock subscription-based payment: verifies an active subscription with remaining sessions,
    /// decrements <c>sedinte_ramase</c>, and confirms the reservation instantly.
    /// </summary>
    Task<(bool Success, PaymentResponse? Response, string? ErrorMessage)> ProcessSubscriptionPaymentAsync(
        SubscriptionPaymentRequest req, CancellationToken ct = default);
}
