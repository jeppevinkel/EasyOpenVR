using System;
using System.Collections.Generic;
using Valve.VR;

namespace EasyOpenVR;

public partial class EasyOpenVr
{
    public class NotificationMethods(EasyOpenVr evr)
    {
/*
     * Thank you artumino and in extension Marlamin on GitHub for their public code which I referenced for notifications.
     * Also thanks to Valve for finally adding the interface for notifications to the C# header file.
     *
     * In reality, I tried implementing notifications back in April 2016, poked Valve about it in October the same year,
     * pointed out what was missing in May and December 2017, yet again in January 2019 and boom, now we have it!
     */

    private List<uint> _notifications = new List<uint>();

    /*
     * We initialize an overlay to display notifications with.
     * The title will be visible above the notification.
     * Returns the handle used to send notifications, 0 on fail.
     */
    public ulong InitNotificationOverlay(string notificationTitle)
    {
        ulong handle = 0;
        var key = Guid.NewGuid().ToString();
        var error = OpenVR.Overlay.CreateOverlay(key, notificationTitle, ref handle);
        if (evr.DebugLog(error).Success) return handle;
        return 0;
    }

    public uint EnqueueNotification(ulong overlayHandle, string message)
    {
        return EnqueueNotification(overlayHandle, message, new NotificationBitmap_t());
    }

    public uint EnqueueNotification(ulong overlayHandle, string message, NotificationBitmap_t bitmap)
    {
        return EnqueueNotification(overlayHandle, EVRNotificationType.Transient, message,
            EVRNotificationStyle.Application, bitmap);
    }

    /*
     * Will enqueue a notification to be displayed in the headset.
     * Returns ID for this specific notification.
     */
    public uint EnqueueNotification(ulong overlayHandle, EVRNotificationType type, string message,
        EVRNotificationStyle style, NotificationBitmap_t bitmap)
    {
        uint id = 0;
        while (id == 0 || _notifications.Contains(id)) id = (uint)evr._random.Next(); // Not sure why we do this
        var error = OpenVR.Notifications.CreateNotification(overlayHandle, 0, type, message, style, ref bitmap, ref id);
        evr.DebugLog(error);
        _notifications.Add(id);
        return id;
    }

    /*
     * Used to dismiss a persistent notification.
     */
    public EasyOpenVrResult DismissNotification(uint id, out EVRNotificationError error)
    {
        error = OpenVR.Notifications.RemoveNotification(id);
        if (error == EVRNotificationError.OK) _notifications.Remove(id);
        return evr.DebugLog(error);
    }

    public bool EmptyNotificationsQueue()
    {
        var success = true;
        foreach (uint id in _notifications)
        {
            var error = OpenVR.Notifications.RemoveNotification(id);
            var result = evr.DebugLog(error);
            success = result.Success;
        }

        _notifications.Clear();
        return success;
    }
    }
}