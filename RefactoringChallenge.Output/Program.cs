using RefactoringChallenge.Output;
using RefactoringChallenge.Repositories;
using RefactoringChallenge.Factories;
using RefactoringChallenge.Orchestration.Factories;
using RefactoringChallenge.Orchestration.Repositories;
using RefactoringChallenge.Services;
using RefactoringChallenge.Orchestration.Services;

var builder = Host.CreateApplicationBuilder(args);

// Add configuration
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

// Register services
builder.Services.AddSingleton<IDatabaseConnectionFactory, SqlConnectionFactory>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<IDiscountService, DiscountService>();
builder.Services.AddScoped<IOrderProcessingService, OrderProcessingService>();
builder.Services.AddScoped<New_CustomerOrderProcessor>();

// Add the worker services
builder.Services.AddHostedService<Worker>();

// Example: To use the New_CustomerOrderProcessor, uncomment the following line
// and comment out the line above
// builder.Services.AddHostedService<WorkerWithNewProcessor>();

var host = builder.Build();
host.Run();
