using FluentValidation;
using FluentValidation.AspNetCore;
using WebApi.Configurations;
using WebApi.Middleware;
using Application.Commands;
using Application.Commands.Comments;
using Application.Commands.Posts;
using Application.Commands.Users;
using Application.DTOs;
using Application.Interfaces;
using Application.Queries;
using Application.Queries.Comments;
using Application.Queries.Notifications;
using Application.Queries.Posts;
using Application.Queries.Users;
using Domain.Entities;
using Infrastructure.Services;
using Persistence.Data;
using Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Nest;
using System.Text;

namespace WebApi.Extensions
{
    public static class ServiceExtensions
    {
        public static void ConfigureServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            
            // Add FluentValidation
            services.AddFluentValidationAutoValidation();
            services.AddValidatorsFromAssemblyContaining<WebApi.Validators.RegisterDtoValidator>();
            
            // Add CORS
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.WithOrigins(config.GetSection("AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:3000" })
                           .AllowAnyMethod()
                           .AllowAnyHeader()
                           .AllowCredentials();
                });
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Jerrygram API", Version = "v1" });

                var securityScheme = new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Description = "Enter 'Bearer {token}'",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                };

                c.AddSecurityDefinition("Bearer", securityScheme);
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    { securityScheme, new[] { "Bearer" } }
                });
            });

            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(config.GetConnectionString("DefaultConnection")));

            services.Configure<JwtSettings>(config.GetSection("JwtSettings"));
            var jwtSettings = config.GetSection("JwtSettings").Get<JwtSettings>();
            services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    var key = Encoding.UTF8.GetBytes(jwtSettings!.SecretKey);
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwtSettings.Issuer,
                        ValidateAudience = true,
                        ValidAudience = jwtSettings.Audience,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key)
                    };
                });

            services.AddSingleton<IElasticClient>(sp =>
            {
                var settings = new ConnectionSettings(new Uri("http://localhost:9200"))
                    .DefaultMappingFor<Application.Common.PostIndex>(m => m.IndexName("posts"))
                    .DefaultMappingFor<Application.Common.UserIndex>(m => m.IndexName("users"))
                    .EnableDebugMode();

                return new ElasticClient(settings);
            });

            services.AddHttpClient<IRecommendClient, RecommendClient>(client =>
            {
                client.BaseAddress = new Uri("http://localhost:3001");
                client.Timeout = TimeSpan.FromSeconds(3);
            });

            // Register Infrastructure Services
            services.AddScoped<IAuthService, AuthService>();
            services.AddSingleton<IBlobService, BlobService>();
            services.AddScoped<IElasticService, ElasticService>();
            services.AddScoped<ISearchService, SearchService>();
            services.AddScoped<ICacheService, CacheService>();
            services.AddScoped<JwtService>();

            // Add Memory Cache
            services.AddMemoryCache();

            // Register Command Handlers
            services.AddScoped<ICommandHandler<CreatePostCommand, PostListItemDto>, CreatePostCommandHandler>();
            services.AddScoped<ICommandHandler<LikePostCommand>, LikePostCommandHandler>();
            services.AddScoped<ICommandHandler<DeletePostCommand>, DeletePostCommandHandler>();
            services.AddScoped<ICommandHandler<UpdatePostCommand, Post>, UpdatePostCommandHandler>();
            services.AddScoped<ICommandHandler<UnlikePostCommand>, UnlikePostCommandHandler>();
            services.AddScoped<ICommandHandler<FollowUserCommand>, FollowUserCommandHandler>();
            services.AddScoped<ICommandHandler<UnfollowUserCommand>, UnfollowUserCommandHandler>();
            services.AddScoped<ICommandHandler<UploadAvatarCommand, object>, UploadAvatarCommandHandler>();
            services.AddScoped<ICommandHandler<CreateCommentCommand, CommentResponseDto>, CreateCommentCommandHandler>();
            services.AddScoped<ICommandHandler<DeleteCommentCommand>, DeleteCommentCommandHandler>();

            // Register Query Handlers
            services.AddScoped<IQueryHandler<GetPostByIdQuery, PostListItemDto>, GetPostByIdQueryHandler>();
            services.AddScoped<IQueryHandler<GetPublicPostsQuery, PagedResult<PostListItemDto>>, GetPublicPostsQueryHandler>();
            services.AddScoped<IQueryHandler<GetUserFeedQuery, PagedResult<PostListItemDto>>, GetUserFeedQueryHandler>();
            services.AddScoped<IQueryHandler<GetPostLikesQuery, PagedResult<SimpleUserDto>>, GetPostLikesQueryHandler>();
            services.AddScoped<IQueryHandler<GetExplorePostsQuery, List<PostListItemDto>>, GetExplorePostsQueryHandler>();
            services.AddScoped<IQueryHandler<GetCurrentUserQuery, object>, GetCurrentUserQueryHandler>();
            services.AddScoped<IQueryHandler<GetUserProfileQuery, object>, GetUserProfileQueryHandler>();
            services.AddScoped<IQueryHandler<GetFollowersQuery, object>, GetFollowersQueryHandler>();
            services.AddScoped<IQueryHandler<GetFollowingsQuery, object>, GetFollowingsQueryHandler>();
            services.AddScoped<IQueryHandler<GetCommentsQuery, object>, GetCommentsQueryHandler>();
            services.AddScoped<IQueryHandler<GetNotificationsQuery, PagedResult<NotificationResponseDto>>, GetNotificationsQueryHandler>();

            // Register Generic Repository
            services.AddScoped(typeof(Application.Interfaces.IRepository<>), typeof(Repository<>));
            
            // Register Specific Repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ITagRepository, TagRepository>();
            services.AddScoped<IPostRepository, PostRepository>();
            services.AddScoped<IPostLikeRepository, PostLikeRepository>();
            services.AddScoped<IPostTagRepository, PostTagRepository>();
            services.AddScoped<ICommentRepository, CommentRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<IUserFollowRepository, UserFollowRepository>();
        }

        public static void ConfigureMiddleware(this WebApplication app)
        {
            // Security headers (should be first)
            app.UseMiddleware<SecurityHeadersMiddleware>();
            
            // Request logging
            app.UseMiddleware<RequestLoggingMiddleware>();
            
            // Global exception handling
            app.UseMiddleware<GlobalExceptionMiddleware>();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseCors();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Database.Migrate();

            if (!context.Users.Any(u => u.Email == "jerry@gram.com"))
            {
                var user = new User
                {
                    Email = "jerry@gram.com",
                    Username = "jerry",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pa$$w0rd!")
                };
                context.Users.Add(user);
                context.SaveChanges();
            }
        }
    }
}