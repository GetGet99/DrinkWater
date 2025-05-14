using Microsoft.UI.Xaml;
using Microsoft.Win32;
using Microsoft.Windows.AppLifecycle;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using System;
using System.Threading;
using Windows.ApplicationModel;
namespace DrinkWater;

public partial class App : Application
{
    public App() => InitializeComponent();

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var activatedArgs = Microsoft.Windows.AppLifecycle.AppInstance.GetCurrent().GetActivatedEventArgs();
        var activationKind = activatedArgs.Kind;
        if (activationKind is ExtendedActivationKind.AppNotification)
        {
            // I don't care, just terminate the app.
            Environment.Exit(0);
        }
        SetStartup();
        // To ensure all Notification handling happens in this process instance, register for
        // NotificationInvoked before calling Register(). Without this a new process will
        // be launched to handle the notification.
        AppNotificationManager notificationManager = AppNotificationManager.Default;
        notificationManager.NotificationInvoked += (o, e) =>
        {
            // I don't care
        };
        notificationManager.Register();
        var builder = new AppNotificationBuilder()
            .AddText("Drink Water app is successfully activated.")
            .AddButton(new AppNotificationButton("Dismiss").AddArgument("action", "dismiss"));
        notificationManager.Show(builder.BuildNotification());
        var t = new Thread((ThreadStart)delegate
        {
            while (true)
            {
                Thread.Sleep((int)Math.Ceiling(GetTimeUntilNextHourMark().TotalMilliseconds));
                var builder = new AppNotificationBuilder()
                    .AddText("Reminder to drink some water!")
                    .AddButton(new AppNotificationButton("Dismiss").AddArgument("action", "dismiss"));
                builder.SetDuration(AppNotificationDuration.Long);

                notificationManager.Show(builder.BuildNotification());
            }
        });
        t.SetApartmentState(ApartmentState.STA);
        t.Start();
    }
    static TimeSpan GetTimeUntilNextHourMark()
    {
        var now = DateTime.Now;
        var nextHour = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0).AddHours(1);
        return nextHour - now;
    }

    private static async void SetStartup()
    {
        var startupTask = await StartupTask.GetAsync("DrinkWater");
        if (startupTask.State == StartupTaskState.Disabled)
        {
            await startupTask.RequestEnableAsync();
        }
    }
}
