using Al_BoomehDAL.Classes;
using Al_BoomehDAL.Interfaces;
using Al_BoomehDAL.Models;
using Al_BoomehServices.Dtos;
using Al_BoomehServices.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_BoomehServices.Services
{
    public class DevicesService: IDevicesService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<DevicesService> _logger;
        private readonly ICurrentUser _user;
        public DevicesService(AppDbContext context, ILogger<DevicesService> logger,ICurrentUser user)
        {
            _context = context;
            _logger = logger;
            _user = user;
        }
        public enum Platform
        {
            Web,
            Android,
            Ios
        }

        public async Task Register(DevicesDto devicesDto)
        {
            if (!_user.UserId.HasValue)
            {
                throw new InvalidOperationException("Current user does not have a valid UserId.");
            }

            var device =await _context.PushDevices
                .Where(d=>d.InstallationId == devicesDto.InstallationId)
                .FirstOrDefaultAsync();
            if (device == null)
            {
                var newDevice = new PushDevice()
                {
                    InstallationId = devicesDto.InstallationId,
                    UserId = _user.UserId.Value.ToString(),
                    Platform=(int)devicesDto.Platform,
                    IsActive=true
                };
                await _context.PushDevices.AddAsync(newDevice);
            }
            else
            {
                device.LastSeenAtUtc = DateTime.UtcNow;
                device.UserId= _user.UserId.Value.ToString();
                device.IsActive = true;
            }
            int rowEffected=await _context.SaveChangesAsync();
            if (rowEffected > 0)
            {
                _logger.LogInformation("Device register successfully with FID {FID}",
                    devicesDto.InstallationId);
                 await Task.CompletedTask;
                return;
            }

            _logger.LogWarning("Device register failed with FID {FID}",
                devicesDto.InstallationId);
            throw new Exception("Failed to register the device");
        }
        public async Task Unregister(string installationId)
        {
            if (!_user.UserId.HasValue)
            {
                throw new InvalidOperationException("Current user does not have a valid UserId.");
            }
            var device=await _context.PushDevices
                .Where(d=>d.InstallationId==installationId )
                .FirstOrDefaultAsync();
            if (device == null)
            {
                throw new NotFoundException("No device found");
            }
            if (device.UserId != _user.UserId.ToString())
            {
                throw new ForbiddenException("You don't have access to this device");
            }
            device.IsActive = true;
            int rowEffected = await _context.SaveChangesAsync();
            if (rowEffected > 0)
            {
                _logger.LogInformation("Device unregister successfully with FID {FID}",
                    installationId);
                await Task.CompletedTask;
                return;
            }
            _logger.LogWarning("Device unregister failed with FID {FID}",
               installationId);
            throw new Exception("Failed to unregister  the device");
        }
    }
}
