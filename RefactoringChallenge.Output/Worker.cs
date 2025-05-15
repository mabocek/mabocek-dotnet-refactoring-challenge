using RefactoringChallenge.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using LanguageExt;
using LanguageExt.Common;

namespace RefactoringChallenge.Output;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IOrderProcessingService _orderProcessingService;

    public Worker(ILogger<Worker> logger, IOrderProcessingService orderProcessingService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _orderProcessingService = orderProcessingService ?? throw new ArgumentNullException(nameof(orderProcessingService));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker started at: {time}", DateTimeOffset.Now);

        try
        {
            // Example usage - process orders for customer ID 1
            var processedOrders = await _orderProcessingService.ProcessCustomerOrdersAsync(1);

            _logger.LogInformation("Processed {OrderCount} orders for customer ID: 1", processedOrders.Count);

            foreach (var order in processedOrders)
            {
                _logger.LogInformation("Order {OrderId} Status: {Status}, Total Amount: {Amount}, Discount: {Discount}%",
                    order.Id, order.Status, order.TotalAmount, order.DiscountPercent);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing orders");
        }

        // Exit the application after processing
        _logger.LogInformation("Worker completed at: {time}", DateTimeOffset.Now);
    }
}
