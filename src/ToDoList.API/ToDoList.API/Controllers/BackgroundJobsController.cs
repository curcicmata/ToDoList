using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToDoList.Infrastructure.BackgroundJobs;

namespace ToDoList.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class BackgroundJobsController(IBackgroundJobClient backgroundJobClient) : ControllerBase
{
    /// <summary>
    /// Manually trigger overdue task reminders job
    /// </summary>
    [HttpPost("trigger-overdue-reminders")]
    public IActionResult TriggerOverdueReminders()
    {
        var jobId = backgroundJobClient.Enqueue<IBackgroundJobService>(
            service => service.SendOverdueTaskReminders());

        return Ok(new { message = "Overdue reminders job triggered", jobId });
    }

    /// <summary>
    /// Manually trigger cleanup of soft-deleted records
    /// </summary>
    [HttpPost("trigger-cleanup")]
    public IActionResult TriggerCleanup()
    {
        var jobId = backgroundJobClient.Enqueue<IBackgroundJobService>(
            service => service.CleanupSoftDeletedRecords());

        return Ok(new { message = "Cleanup job triggered", jobId });
    }

    /// <summary>
    /// Schedule a delayed job (example)
    /// </summary>
    [HttpPost("schedule-reminder")]
    public IActionResult ScheduleDelayedReminder([FromQuery] int delayMinutes = 5)
    {
        var jobId = backgroundJobClient.Schedule<IBackgroundJobService>(
            service => service.SendOverdueTaskReminders(),
            TimeSpan.FromMinutes(delayMinutes));

        return Ok(new { message = $"Reminder scheduled in {delayMinutes} minutes", jobId });
    }
}
