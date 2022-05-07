using API.Data;
using API.Helpers;
using API.Interfaces;
using API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApllicationServices(this IServiceCollection _services, IConfiguration _config)
        {
            _services.Configure<CloudinarySettings>(_config.GetSection("CloudinarySettings"));
            _services.AddScoped<ITokenService, TokenService>();
            _services.AddScoped<IPhotoService, PhotoService>();
            _services.AddScoped<IUserRepository, UserRepository>();
            _services.AddAutoMapper(typeof(AutoMapperProfiles).Assembly);

            _services.AddDbContext<DataContext>(options =>
            {
                options.UseSqlite(_config.GetConnectionString("DefaultConnection"));
            });


            return _services;
        }
    }
}
