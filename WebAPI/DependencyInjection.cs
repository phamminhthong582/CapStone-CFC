using BusinessObject.Entities;
using Hangfire;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Repository.Implement;
using Repository.Interface;
using Service.AutoMapper;
using Service.Implement;
using Service.Interface;

namespace WebAPI;

public static class DependencyInjection
{
    public static IServiceCollection AddInfra(this IServiceCollection services, IConfiguration configuration)
    {
        //Unit of work
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        //Authen
        services.AddScoped<IAuthService, AuthService>();
        //Token 
        services.AddScoped<ITokenService, TokenService>();
        //Employee
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IEmployeeService, EmployeeService>();
        //Store
        services.AddScoped<IStoreService, StoreService>();
        // Category
        services.AddScoped<ICategoryService, CategoryService>(); 
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        // Promotion
        services.AddScoped<IPromotionService, PromotionService>(); 
        services.AddScoped<IPromotionRepository, PromotionRepository>();
        // Customer
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ICustomerService, CustomerService>();
        // Comment
        services.AddScoped<ICommentRepository, CommentRepository>();
        services.AddScoped<ICommentService, CommentService>();
        // Feedback
        services.AddScoped<IFeedbackRepository, FeedbackRepository>();
        services.AddScoped<IFeedbackService, FeedbackService>();
        //Flower
        services.AddScoped<IFlowerRepository, FlowerRepository>();
        services.AddScoped<IFlowerService, FlowerService>();
        // FlowerBasket
        services.AddScoped<IFlowerBasketRepository, FlowerBasketRepository>();
        services.AddScoped<IFlowerBasketService, FlowerBasketService>();
        // ProductCustom
        services.AddScoped<IProductCustomRepository, ProductCustomRepository>();
        services.AddScoped<IProductCustomService, ProductCustomService>();
        // FlowerCustom
        services.AddScoped<IFlowerCustomRepository, FlowerCustomRepository>();
        services.AddScoped<IFlowerCustomService, FlowerCustomService>();
        // Role
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IRoleService, RoleService>();
        // Email
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<SendMailWithrawMoneyService>();

        //Product
        services.AddScoped<IProductService, ProductService>();

        //Order
        services.AddScoped<IOrderService, OrderService>();  
        services.AddScoped<IOrderRepository, OrderRepository>();  

        //Delivery
        services.AddScoped<IDeliveryService, DeliveryService>();

        //Cart 
        services.AddScoped<ICartService, CartService>();

        //Wallet
        services.AddScoped<IWalletService, WalletService>();
        //IWithdrawMoney

        services.AddScoped<IWithdrawMoneyService, WithdrawMoneyService>();

        //Refund 
        services.AddScoped<IRefundService, RefundService>();

        //VNpay
        services.AddScoped<IVnPayService,VnPayService>();

        //Payment
        services.AddScoped<IPaymentService, PaymentService>();

        //Style
        services.AddScoped<IStyleService, StyleService>();

        //Style
        services.AddScoped<IAccessoryService, AccessoryService>();

        //Cloudinary
        services.AddSingleton<CloudinaryService>();
        //OpenAI
        services.AddHttpClient<GeminiService>(); // Đăng ký HttpClient
        services.AddScoped<GeminiService>();

        //Revenue
        services.AddScoped<IRevenueService, RevenueService>();
        //ChatRoom
        services.AddScoped<IChatRoomRepository, ChatRoomRepository>();
        services.AddScoped<IChatRoomService, ChatRoomService>();
        //Message
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<IMessageService, MessageService>();
        //Notification
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<INotificationService, NotificationService>();

        //logger
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddDebug();
            // Có thể thêm các provider khác như file logging
            builder.AddAzureWebAppDiagnostics(); // Nếu deploy lên Azure
        });

        services.AddAutoMapper(typeof(MapperConfig).Assembly);

        return services;
    }
}