// Controllers/AlertsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserRoles.Data;
using MetroAir.Models;
using Microsoft.EntityFrameworkCore;


[Authorize(Roles = "MonitoringAdmin")]
[Route("api/Alerts/active")]
[ApiController]
public class AlertsController : ControllerBase
{
    private readonly AppDbContext _context;

    public AlertsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActiveAlerts()
    {
        var alerts = await _context.AqiAlerts
            .Include(a => a.Sensor)
            .Where(a => a.IsActive)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();

        return Ok(alerts);
    }

    [HttpGet("notifications")]
    public async Task<IActionResult> GetUnreadNotifications()
    {
        var notifications = await _context.AlertNotifications
            .Where(n => !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        return Ok(notifications);
    }

    [HttpPost("notifications/mark-read")]
    public async Task<IActionResult> MarkNotificationsAsRead([FromBody] List<int> notificationIds)
    {
        var notifications = await _context.AlertNotifications
            .Where(n => notificationIds.Contains(n.Id))
            .ToListAsync();

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
        }

        await _context.SaveChangesAsync();
        return Ok();
    }
}