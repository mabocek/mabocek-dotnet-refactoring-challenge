using Microsoft.Extensions.Logging;
using RefactoringChallenge.Models;
using RefactoringChallenge.Repositories;
using RefactoringChallenge.Orchestration.Resources;
using System.Globalization;

namespace RefactoringChallenge.Services;

public class OrderProcessingService : IOrderProcessingService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IDiscountService _discountService;
    private readonly ILogger<OrderProcessingService> _logger;

    public OrderProcessingService(
        ICustomerRepository customerRepository,
        IOrderRepository orderRepository,
        IInventoryRepository inventoryRepository,
        IDiscountService discountService,
        ILogger<OrderProcessingService> logger)
    {
        _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _inventoryRepository = inventoryRepository ?? throw new ArgumentNullException(nameof(inventoryRepository));
        _discountService = discountService ?? throw new ArgumentNullException(nameof(discountService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<Order>> ProcessCustomerOrdersAsync(int customerId)
    {
        _logger.LogInformation(OrderProcessingMessages.StartingProcessCustomerOrders, customerId);

        if (customerId <= 0)
        {
            _logger.LogError(OrderProcessingMessages.InvalidCustomerId, customerId);
            throw new ArgumentException(OrderProcessingMessages.CustomerIdMustBePositive, nameof(customerId));
        }

        // Get customer
        var customer = await _customerRepository.GetCustomerByIdAsync(customerId);
        if (customer == null)
        {
            _logger.LogError(OrderProcessingMessages.CustomerNotFoundError, customerId);
            throw new InvalidOperationException(string.Format(OrderProcessingMessages.CustomerNotFoundException, customerId));
        }

        _logger.LogInformation(OrderProcessingMessages.CustomerFound, customer.Name, customer.IsVip);

        // Get pending orders
        var pendingOrders = await _orderRepository.GetPendingOrdersByCustomerIdAsync(customerId);
        _logger.LogInformation(OrderProcessingMessages.PendingOrdersFound, pendingOrders.Count, customerId);

        var processedOrders = new List<Order>();

        foreach (var order in pendingOrders)
        {
            await ProcessOrderAsync(customer, order);
            processedOrders.Add(order);
        }

        _logger.LogInformation(OrderProcessingMessages.OrdersProcessingCompleted, processedOrders.Count, customerId);
        return processedOrders;
    }

    private async Task ProcessOrderAsync(Customer customer, Order order)
    {
        _logger.LogInformation(OrderProcessingMessages.ProcessingOrderId, order.Id);

        // Calculate raw total amount
        decimal totalAmount = order.Items.Sum(item => item.Quantity * item.UnitPrice);
        _logger.LogDebug(OrderProcessingMessages.OrderRawTotal, order.Id, totalAmount);

        // Calculate discount
        decimal discountPercent = _discountService.CalculateDiscountPercentage(customer, totalAmount);
        decimal discountAmount = totalAmount * (discountPercent / 100);
        decimal finalAmount = totalAmount - discountAmount;

        // Update order
        order.DiscountPercent = discountPercent;
        order.DiscountAmount = discountAmount;
        order.TotalAmount = finalAmount;
        order.Status = "Processed";

        _logger.LogDebug(OrderProcessingMessages.OrderProcessedWithDiscount,
            order.Id, order.DiscountPercent, order.TotalAmount);

        await _orderRepository.UpdateOrderAsync(order);
        _logger.LogInformation(OrderProcessingMessages.OrderUpdated, order.Id);

        // Check inventory and update status
        bool allProductsAvailable = await CheckAndUpdateInventoryAsync(order);

        if (allProductsAvailable)
        {
            order.Status = "Ready";
            await _orderRepository.UpdateOrderStatusAsync(order.Id, "Ready");

            string logMessage = string.Format(OrderProcessingMessages.OrderCompletedLog, order.DiscountPercent, order.TotalAmount);
            await _orderRepository.AddOrderLogAsync(order.Id, logMessage);
            _logger.LogInformation(OrderProcessingMessages.OrderReady, order.Id);
        }
        else
        {
            order.Status = "OnHold";
            await _orderRepository.UpdateOrderStatusAsync(order.Id, "OnHold");

            await _orderRepository.AddOrderLogAsync(order.Id, OrderProcessingMessages.OrderOnHoldLog);
            _logger.LogWarning(OrderProcessingMessages.OrderOnHold, order.Id);
        }
    }

    private async Task<bool> CheckAndUpdateInventoryAsync(Order order)
    {
        // Check if all products are available in the inventory
        foreach (var item in order.Items)
        {
            var stockQuantity = await _inventoryRepository.GetStockQuantityByProductIdAsync(item.ProductId);
            if (stockQuantity == null || stockQuantity < item.Quantity)
            {
                _logger.LogWarning(OrderProcessingMessages.InsufficientInventory,
                    item.ProductId, item.Quantity, stockQuantity);
                return false;
            }
        }

        // Update inventory for all items
        foreach (var item in order.Items)
        {
            await _inventoryRepository.UpdateStockQuantityAsync(item.ProductId, item.Quantity);
            _logger.LogDebug(OrderProcessingMessages.InventoryUpdated,
                item.ProductId, item.Quantity);
        }

        return true;
    }
}
