using AspNet.Security.OpenIdConnect.Primitives;
using AspNetCoreSpa.Core;
using AspNetCoreSpa.Core.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenIddict.Core;
using OpenIddict.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCoreSpa.Infrastructure
{
    public interface IDatabaseInitializer
    {
        Task SeedAsync(IConfiguration configuration);
    }

    public class DatabaseInitializer : IDatabaseInitializer
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly OpenIddictApplicationManager<OpenIddictApplication> _openIddictApplicationManager;
        private readonly ILogger _logger;
        private readonly IHostingEnvironment _hostingEnvironment;

        public DatabaseInitializer(
            ApplicationDbContext context,
            ILogger<DatabaseInitializer> logger,
            OpenIddictApplicationManager<OpenIddictApplication> openIddictApplicationManager,
            RoleManager<ApplicationRole> roleManager,
            UserManager<ApplicationUser> userManager,
            IHostingEnvironment hostingEnvironment
            )
        {
            _context = context;
            _logger = logger;
            _openIddictApplicationManager = openIddictApplicationManager;
            _roleManager = roleManager;
            _userManager = userManager;
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task SeedAsync(IConfiguration configuration)
        {
            await _context.Database.EnsureDeletedAsync();
            await _context.Database.MigrateAsync();

            CreateRooms();
            CreateRoomEdges();
            CreateRoles();
            CreateUsers();
            AddLocalisedData();
            await AddOpenIdConnectOptions(configuration);
        }
        
        private void CreateRooms()
        {
            if (!_context.Rooms.Any()) // todo: load this from some json resource instead
            {
                var namestie = new Room { Name = "Námestie", Description = "Nieco velmi zaujiamve." };
                var ulica = new Room { Name = "Ulica", Description = "Nieco velmi zaujiamve." };
                var mestskaBrana = new Room { Name = "Mestska Brana", Description = "Nieco velmi zaujiamve." };
                var tesco = new Room { Name = "Tesco", Description = "Nieco velmi zaujiamve." };
                _context.Rooms.AddRange(namestie, ulica, mestskaBrana, tesco);
                _context.SaveChanges();
            }
        }

        private void CreateRoomEdges()
        {
            if (!_context.RoomEdges.Any()) // todo: load this from some json resource instead
            {
                var rooms = _context.Rooms.ToList();
                this.CreateRoomEdge(rooms[0], rooms[1]);
                this.CreateRoomEdge(rooms[0], rooms[2]);
                this.CreateRoomEdge(rooms[1], rooms[2]);
                this.CreateRoomEdge(rooms[1], rooms[3]);
                this.CreateRoomEdge(rooms[1], rooms[0]);
                this.CreateRoomEdge(rooms[2], rooms[0]);
                this.CreateRoomEdge(rooms[3], rooms[0]);

                _context.SaveChanges();
            }
        }

        private void CreateRoomEdge(Room from, Room to)
        {
            var edge = new RoomEdge { Room = from, AdjacentRoom = to, RoomId = from.Id, AdjacentRoomId = to.Id };
            _context.RoomEdges.Add(edge);
        }

        private void CreateRoles()
        {
            var rolesToAdd = new List<ApplicationRole>(){
                new ApplicationRole { Name= "Admin", Description = "Full rights role"},
                new ApplicationRole { Name= "User", Description = "Limited rights role"}
            };
            foreach (var role in rolesToAdd)
            {
                if (!_roleManager.RoleExistsAsync(role.Name).Result)
                {
                    _roleManager.CreateAsync(role).Result.ToString();
                }
            }
        }

        private void CreateUsers()
        {
            var defaultRoom = _context.Rooms.Find(1);
            if (!_context.ApplicationUsers.Any())
            {
                var adminUser = new ApplicationUser { UserName = "Admin123", FirstName = "Admin first", LastName = "Admin last", Email = "admin@admin.com", Mobile = "0123456789", EmailConfirmed = true, CreatedDate = DateTime.Now, IsEnabled = true, RoomId = defaultRoom.Id };
                _userManager.CreateAsync(adminUser, "Admin123").Result.ToString();
                _userManager.AddClaimAsync(adminUser, new Claim(OpenIdConnectConstants.Claims.PhoneNumber, adminUser.Mobile.ToString(), ClaimValueTypes.Integer)).Result.ToString();
                _userManager.AddToRoleAsync(_userManager.FindByNameAsync("Admin123").GetAwaiter().GetResult(), "Admin").Result.ToString();
                var normalUser = new ApplicationUser { UserName = "User123", FirstName = "First", LastName = "Last", Email = "user@user.com", Mobile = "0123456789", EmailConfirmed = true, CreatedDate = DateTime.Now, IsEnabled = true, RoomId = defaultRoom.Id };
                _userManager.CreateAsync(normalUser, "User123").Result.ToString();
                _userManager.AddClaimAsync(normalUser, new Claim(OpenIdConnectConstants.Claims.PhoneNumber, normalUser.Mobile.ToString(), ClaimValueTypes.Integer)).Result.ToString();
                _userManager.AddToRoleAsync(_userManager.FindByNameAsync("User123").GetAwaiter().GetResult(), "User").Result.ToString();
            }
        }

        private void AddLocalisedData()
        {
            if (!_context.Cultures.Any())
            {
                var translations = _hostingEnvironment.GetTranslationFile();

                var locales = translations.First().Split(",").Skip(1).ToList();

                var currentLocale = 0;

                locales.ForEach(locale =>
                {
                    currentLocale += 1;

                    var culture = new Culture
                    {
                        Name = locale
                    };
                    var resources = new List<Resource>();
                    translations.Skip(1).ToList().ForEach(t =>
                    {
                        var line = t.Split(",");
                        resources.Add(new Resource
                        {
                            Culture = culture,
                            Key = line[0],
                            Value = line[currentLocale]
                        });
                    });

                    culture.Resources = resources;

                    _context.Cultures.Add(culture);

                    _context.SaveChanges();
                });
            }

        }

        private async Task AddOpenIdConnectOptions(IConfiguration configuration)
        {
            if (await _openIddictApplicationManager.FindByClientIdAsync("aspnetcorespa") == null)
            {
                var host = configuration["HostUrl"].ToString();

                var descriptor = new OpenIddictApplicationDescriptor
                {
                    ClientId = "aspnetcorespa",
                    DisplayName = "AspnetCoreSpa",
                    PostLogoutRedirectUris = { new Uri($"{host}signout-oidc") },
                    RedirectUris = { new Uri(host) },
                    Permissions =
                    {
                        OpenIddictConstants.Permissions.Endpoints.Authorization,
                        OpenIddictConstants.Permissions.Endpoints.Token,
                        OpenIddictConstants.Permissions.GrantTypes.Implicit,
                        OpenIddictConstants.Permissions.GrantTypes.Password,
                        OpenIddictConstants.Permissions.GrantTypes.RefreshToken
                    }
                };

                await _openIddictApplicationManager.CreateAsync(descriptor);
            }

        }
    }
}
