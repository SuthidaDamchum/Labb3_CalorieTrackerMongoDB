namespace Labb3_CalorieTrackerMongoDB.Services
{
    public static class AppEvents
    {
        public static event Func<Task>? DailyLogChanged;

        public static async Task RaiseDailyLogChangedAsync()
        {
            var handlers = DailyLogChanged;
            if (handlers == null) return;

            foreach (Func<Task> handler in handlers.GetInvocationList())
                await handler();
        }
    }
}   