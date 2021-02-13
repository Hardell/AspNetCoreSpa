﻿using System.Linq;
using System.Threading.Tasks;
using AspNetCoreSpa.Core.Entities;
using AspNetCoreSpa.Infrastructure;
using AspNetCoreSpa.Infrastructure.OnlineUserManager;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace AspNetCoreSpa.Web.Commands.Implementations
{
    public class LookCommand : Command
    {
        public LookCommand(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, OnlineUserManager onlineUserManager) 
            : base(dbContext, userManager, onlineUserManager)
        {
        }

        public override async Task Execute(Hub hub)
        {
            var username = hub.Context.User.Identity.Name;

            var currentRoomName = onlineUserManager.GetUserRoomName(username);
            var otherUsers = onlineUserManager.GetUsernamesInRoom(currentRoomName);

            var adjacentRoomNames = dbContext.Rooms.Include(r => r.AdjacentRooms).ThenInclude(r => r.AdjacentRoom).Single(r => r.Name == currentRoomName).AdjacentRooms.Select(r => r.AdjacentRoom.Name).ToList();
            await hub.Clients.Caller.SendAsync("send", $"Si na {currentRoomName}. Ludia v miestnosti: {string.Join(", ", otherUsers)} Mozes ist do {JsonConvert.SerializeObject(adjacentRoomNames)}");
        }
    }
}
