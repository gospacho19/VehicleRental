using System;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using LuxuryCarRental.Data;
using LuxuryCarRental.Repositories.Implementations;
using LuxuryCarRental.Repositories.Interfaces;
using LuxuryCarRental.ViewModels;
using LuxuryCarRental.Views;
using LuxuryCarRental.Services.Implementations;
using LuxuryCarRental.Services.Interfaces;
using LuxuryCarRental.Handlers.Interfaces;
using LuxuryCarRental.Managers.Implementations;
using LuxuryCarRental.Handlers.Implementations;
using CommunityToolkit.Mvvm.Messaging;

namespace LuxuryCarRental
{
    public partial class App : Application
    {
        private IServiceProvider? _serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 1) Configure services
            var services = new ServiceCollection();

            services.AddSingleton<IMessenger, WeakReferenceMessenger>();

            // EF Core + SQLite
            services.AddDbContext<AppDbContext>(opts =>
                opts.UseSqlite("Data Source=LuxuryRental.db"));

            // Repositories / Unit of Work
            services.AddScoped<IUnitOfWork, EfUnitOfWork>();

            // ViewModels
            services.AddTransient<MainViewModel>();
            services.AddTransient<CatalogViewModel>();
            services.AddTransient<CategoryViewModel>();
            services.AddTransient<CartViewModel>();
            services.AddTransient<CheckoutViewModel>();
            services.AddTransient<ConfirmationViewModel>();
            services.AddTransient<DealsViewModel>();

            // Views
            services.AddTransient<CatalogView>();
            services.AddTransient<CategoryView>();
            services.AddTransient<CartView>();
            services.AddTransient<CheckoutView>();
            services.AddTransient<ConfirmationView>();
            services.AddTransient<DealsView>();
            services.AddTransient<MainWindow>();

            // Services
            services.AddScoped<IPricingService, PricingService>();
            services.AddScoped<IAvailabilityService, AvailabilityService>();
            services.AddScoped<ICartService, CartService>();
            services.AddScoped<IPaymentService, PaymentService>();

            // Handlers
            services.AddScoped<IRentalHandler, RentalHandler>();
            services.AddScoped<IBasketHandler, BasketHandler>();
            services.AddScoped<ICheckoutHandler, CheckoutHandler>();

            // 2) Build the provider
            _serviceProvider = services.BuildServiceProvider();

            // 3) Apply any pending EF Core migrations
            using (var scope = _serviceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.Migrate();
            }

            // 4) Show the main window (it will be constructed via DI)
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
    }
}
