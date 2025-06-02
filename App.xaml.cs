using System;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using LuxuryCarRental.Data;
using LuxuryCarRental.Repositories.Implementations;
using LuxuryCarRental.Repositories.Interfaces;
using LuxuryCarRental.Services.Implementations;
using LuxuryCarRental.Services.Interfaces;
using LuxuryCarRental.Handlers.Implementations;
using LuxuryCarRental.Handlers.Interfaces;
using LuxuryCarRental.ViewModels;
using LuxuryCarRental.Views;
using CommunityToolkit.Mvvm.Messaging;
using LuxuryCarRental.Managers.Implementations;

namespace LuxuryCarRental
{
    public partial class App : Application
    {
        private IServiceProvider? _serviceProvider;
        public IServiceProvider Services => _serviceProvider!;

        // Override the OnStartup method
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();
            services.AddSingleton<IMessenger, WeakReferenceMessenger>();

            services.AddSingleton<UserSessionService>();

            services.AddDbContext<AppDbContext>(opts =>
                opts.UseSqlite("Data Source=LuxuryRental.db"));

            services.AddScoped<IUnitOfWork, EfUnitOfWork>();
            services.AddScoped<IAuthService, AuthService>();

            // Services
            services.AddScoped<IPricingService, PricingService>();
            services.AddScoped<IAvailabilityService, AvailabilityService>();
            services.AddScoped<ICartService, CartService>();
            services.AddScoped<IPaymentService, PaymentService>();

            // Handlers
            services.AddScoped<IRentalHandler, RentalHandler>();
            services.AddScoped<IBasketHandler, BasketHandler>();
            services.AddScoped<ICheckoutHandler, CheckoutHandler>();

            // ViewModels
            services.AddTransient<MainViewModel>();
            services.AddTransient<CatalogViewModel>();
            services.AddTransient<CategoryViewModel>();
            services.AddTransient<CartViewModel>();
            services.AddTransient<CheckoutViewModel>();
            services.AddTransient<ConfirmationViewModel>();
            services.AddTransient<DealsViewModel>();
            services.AddTransient<ProfileViewModel>();
            services.AddTransient<PaymentInfoViewModel>();
            services.AddTransient<LoginViewModel>();
            services.AddTransient<RegisterViewModel>();
            services.AddTransient<VehicleDetailViewModel>();

            // Views
            services.AddTransient<MainWindow>();
            services.AddTransient<CatalogView>();
            services.AddTransient<CategoryView>();
            services.AddTransient<CartView>();
            services.AddTransient<CheckoutView>();
            services.AddTransient<ConfirmationView>();
            services.AddTransient<DealsView>();
            services.AddTransient<ProfileView>();
            services.AddTransient<PaymentInfoView>();
            services.AddTransient<LoginView>();
            services.AddTransient<RegisterView>();
            services.AddTransient<VehicleDetailView>();
            // 
            _serviceProvider = services.BuildServiceProvider();

            // Apply migrations & seed data
            using (var scope = _serviceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.Migrate();
                SeedData.Initialize(scope.ServiceProvider);
            }

            // Show the main window
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
    }
}
