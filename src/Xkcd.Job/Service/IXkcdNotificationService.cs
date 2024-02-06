namespace Xkcd.Job.Service;

public interface IXkcdNotificationService
{
    Task CheckAndNotify();
}