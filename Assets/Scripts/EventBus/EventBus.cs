using System;
using System.Collections.Generic;
using UnityEngine;

public static class EventBus
{
    private static readonly Dictionary<Type, Delegate> s_event = new Dictionary<Type, Delegate>();
    private static readonly object s_lock = new object();

    // Map nhớ "wrapper" tương ứng với handler gốc cho UnsubscribeForSource
    private static readonly Dictionary<(Type type, Delegate orig, int source), Delegate> s_wrappers
        = new Dictionary<(Type, Delegate, int), Delegate>();

    // =========================
    // API gốc 
    // =========================
    public static void Subscribe<T>(Action<T> handler)
    {
        if (handler == null) throw new ArgumentNullException(nameof(handler));
        lock (s_lock)
        {
            var t = typeof(T);
            if (s_event.TryGetValue(t, out var ex))
                s_event[t] = Delegate.Combine(ex, handler);
            else
                s_event[t] = handler;
        }
    }

    public static void Unsubscribe<T>(Action<T> handler)
    {
        if (handler == null) throw new ArgumentNullException(nameof(handler));
        lock (s_lock)
        {
            var t = typeof(T);
            if (s_event.TryGetValue(t, out var ex))
            {
                var nd = Delegate.Remove(ex, handler);
                if (nd != null) s_event[t] = nd;
                else s_event.Remove(t);
            }
        }
    }

    public static void Publish<T>(T eventData)
    {
        Delegate handlers;
        lock (s_lock)
        {
            if (!s_event.TryGetValue(typeof(T), out handlers)) return;
        }

        if (handlers is Action<T> action)
        {
            foreach (Action<T> single in action.GetInvocationList())
            {
                try { single.Invoke(eventData); }
                catch (Exception ex) { Debug.LogError($"Error in event handler: {ex.Message}"); }
            }
        }
    }

    // =========================
    // API theo "nguồn" (sourceId)
    // =========================
    /// <summary>
    /// Đăng ký handler CHỈ nhận event có SourceId == sourceId.
    /// </summary>
    public static void SubscribeForSource<T>(int sourceId, Action<T> handler) where T : IHasSourceId
    {
        if (handler == null) throw new ArgumentNullException(nameof(handler));
        // Tạo wrapper lọc theo SourceId
        Action<T> wrapper = (e) =>
        {
            if (e != null && e.SourceId == sourceId)
                handler(e);
        };

        lock (s_lock)
        {
            // Lưu mapping để gỡ đúng wrapper khi UnsubscribeForSource
            s_wrappers[(typeof(T), handler, sourceId)] = wrapper;
        }

        // Đăng ký wrapper như handler thường
        Subscribe(wrapper);
    }

    /// <summary>
    /// Hủy đăng ký handler đã đăng ký bằng SubscribeForSource.
    /// </summary>
    public static void UnsubscribeForSource<T>(int sourceId, Action<T> handler) where T : IHasSourceId
    {
        if (handler == null) throw new ArgumentNullException(nameof(handler));

        Delegate wrapper;
        lock (s_lock)
        {
            if (!s_wrappers.TryGetValue((typeof(T), handler, sourceId), out wrapper))
                return; // không tìm thấy wrapper -> có thể chưa đăng ký hoặc đã gỡ
            s_wrappers.Remove((typeof(T), handler, sourceId));
        }

        // Gỡ wrapper ra khỏi bus
        Unsubscribe((Action<T>)wrapper);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetBus()
    {
        lock (s_lock)
        {
            s_event.Clear();
            s_wrappers.Clear();
        }
    }
}