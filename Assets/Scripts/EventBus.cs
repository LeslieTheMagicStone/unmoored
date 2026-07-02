using System;
using System.Collections.Generic;

namespace CGJ2026
{
    public static class EventBus
    {
        static readonly Dictionary<string, Action<object>> events = new();

        public static void On(string id, Action<object> handler) => events[id] = handler;

        public static void Off(string id) => events.Remove(id);

        public static void Emit(string id, object data = null)
        {
            if (events.TryGetValue(id, out var handler))
                handler?.Invoke(data);
        }
    }
}
