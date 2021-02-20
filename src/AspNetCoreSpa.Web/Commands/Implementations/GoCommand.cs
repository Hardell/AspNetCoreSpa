using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreSpa.Core.Entities;
using AspNetCoreSpa.Infrastructure;
using AspNetCoreSpa.Infrastructure.OnlineUserManager;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace AspNetCoreSpa.Web.Commands.Implementations
{
    public class GoCommand : Command
    {
        private readonly string _destination;

        public GoCommand(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, OnlineUserManager onlineUserManager, string destination) 
            : base(dbContext, userManager, onlineUserManager)
        {
            _destination = destination;
        }

        public override async Task Execute(Hub hub)
        {
            var username = hub.Context.User.Identity.Name;

            if (string.IsNullOrWhiteSpace(_destination))
            {
                var error = new CommandResponse { Command = "Error", Content = "Nevies kam ist?" };
                await hub.Clients.Caller.SendAsync("send", JsonConvert.SerializeObject(error));

                return;
            }

            var newRoom = dbContext.Rooms.FirstOrDefault(r => string.Compare(r.Name, _destination, CultureInfo.CurrentCulture, CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase) == 0);
            var oldRoom = onlineUserManager.GetUserRoomName(username);
            var adjacentRooms = dbContext.RoomEdges.Where(r => r.AdjacentRoomId == newRoom.Id && r.Room.Name == oldRoom);

            if (newRoom != null && adjacentRooms.Any())
            {
                await hub.Clients.Group(oldRoom).SendAsync("send", $"{username} odisiel do {newRoom.Name}.");
                await hub.Groups.RemoveFromGroupAsync(hub.Context.ConnectionId, oldRoom);

                var user = await userManager.FindByNameAsync(username);
                user.RoomId = newRoom.Id;
                await userManager.UpdateAsync(user);
                onlineUserManager.UpdateUserRoom(username, newRoom.Name);

                await hub.Groups.AddToGroupAsync(hub.Context.ConnectionId, newRoom.Name);
                await hub.Clients.OthersInGroup(newRoom.Name).SendAsync("send", $"{username} prisiel do miestnosti.");
                await hub.Clients.Caller.SendAsync("send", $"Ocitol si sa na {newRoom.Name}.");
            }
            else
            {
                var error = new CommandResponse { Command = "Error", Content = "Z tadial sa tam nedostanes." };
                await hub.Clients.Caller.SendAsync("send", JsonConvert.SerializeObject(error));
            }
        }
    }
}
