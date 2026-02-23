namespace ToDoList.Infrastructure.BackgroundJobs;

public interface IBackgroundJobService
{
    void SendOverdueTaskReminders();
    void CleanupSoftDeletedRecords();
    Task SendTaskReminders();
    Task ArchiveOverdueTasks();
}
