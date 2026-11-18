using System;
using System.Collections.Generic;

namespace CGJ2026
{
    /// <summary>
    /// Global string-keyed event bus for cross-system messages. Event ids live in <see cref="E"/>.
    /// Subscribe in OnEnable and unsubscribe in OnDisable.
    /// </summary>
    public static class EventBus
    {
        static readonly Dictionary<string, Action<object>> events = new();

        public static void On(string id, Action<object> handler)
        {
            if (events.TryGetValue(id, out var existing))
                events[id] = existing + handler;
            else
                events[id] = handler;
        }

        public static void Off(string id, Action<object> handler)
        {
            if (events.TryGetValue(id, out var existing))
            {
                existing -= handler;
                if (existing == null)
                    events.Remove(id);
                else
                    events[id] = existing;
            }
        }

        public static void Emit(string id, object data = null)
        {
            if (events.TryGetValue(id, out var handler))
                handler.Invoke(data);
        }
    }
}
