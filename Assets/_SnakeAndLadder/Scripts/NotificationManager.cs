using System.Collections.Generic;
using Unity.Notifications.Android;
using UnityEngine;

namespace ArtboxGames
{
    public class NotificationManager : MonoBehaviour
    {
        [SerializeField] private List<string> messageList;

        // Start is called before the first frame update
        void Start()
        {
            var channel = new AndroidNotificationChannel()
            {
                Id = "channel_id",
                Name = "Default Channel",
                Importance = Importance.Default,
                Description = "Generic notifications",
            };
            AndroidNotificationCenter.RegisterNotificationChannel(channel);
        }

        private void OnApplicationFocus(bool focus)
        {
            if (!focus)
            {
                var notification = new AndroidNotification
                {
                    SmallIcon = "icon_0",
                    LargeIcon = "icon_1",
                    Title = "Snake And Ladder: Ludo Game!",
                    Text = messageList[Random.Range(0, 5)],
                    FireTime = System.DateTime.Now.AddDays(1),
                    RepeatInterval = new System.TimeSpan(1, 0, 0, 0)

                    //FireTime = System.DateTime.Now.AddMinutes(1),
                    //RepeatInterval = new System.TimeSpan(0, 0, 1, 0),
                };

                AndroidNotificationCenter.SendNotification(notification, "channel_id");
            }
            else
            {
                AndroidNotificationCenter.CancelAllNotifications();
            }
        }
    }
}