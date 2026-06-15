using System;

namespace NotificationFacadeExample
{

    public class SmsService
    {
        public void SendSms(string phone, string text)
        {
            Console.WriteLine($"SMS sent to {phone}: {text}");
        }

        public bool CheckDelivery(string messageId)
        {
            Console.WriteLine($"Checking SMS delivery: {messageId}");
            return true;
        }
    }

    public class EmailService
    {
        public void SendEmail(string email, string subject, string body)
        {
            Console.WriteLine($"Email sent to {email}");
            Console.WriteLine($"Subject: {subject}");
            Console.WriteLine($"Body: {body}");
        }

        public void AddAttachment(string messageId, byte[] file)
        {
            Console.WriteLine($"Attachment added to email {messageId}");
        }
    }

    public class PushService
    {
        public void SendPush(string deviceToken, string title, string body)
        {
            Console.WriteLine($"Push sent to {deviceToken}");
            Console.WriteLine($"Title: {title}");
            Console.WriteLine($"Message: {body}");
        }

        public void SetBadge(string deviceToken, int count)
        {
            Console.WriteLine($"Badge set to {count} for {deviceToken}");
        }
    }

    public class NotificationFacade
    {
        private readonly SmsService smsService;
        private readonly EmailService emailService;
        private readonly PushService pushService;

        public NotificationFacade()
        {
            smsService = new SmsService();
            emailService = new EmailService();
            pushService = new PushService();
        }

        public void SendSimpleMessage(string phone, string text)
        {
            Console.WriteLine("\n[Simple Message]");
            smsService.SendSms(phone, text);
        }

        public void SendEmailMessage(
            string email,
            string subject,
            string body)
        {
            Console.WriteLine("\n[Email Message]");
            emailService.SendEmail(email, subject, body);
        }

        public void SendPushMessage(
            string deviceToken,
            string title,
            string body)
        {
            Console.WriteLine("\n[Push Message]");
            pushService.SendPush(deviceToken, title, body);
        }

        public void SendUrgentNotification(
            string deviceToken,
            string phone,
            string title,
            string message)
        {
            Console.WriteLine("\n[Urgent Notification]");

            pushService.SendPush(deviceToken, title, message);

            smsService.SendSms(
                phone,
                "URGENT: " + message);
        }

        public bool GetDeliveryStatus(
            string channel,
            string messageId)
        {
            Console.WriteLine("\n[Delivery Status]");

            switch (channel.ToLower())
            {
                case "sms":
                    return smsService.CheckDelivery(messageId);

                case "email":
                    Console.WriteLine(
                        $"Email {messageId} delivered.");
                    return true;

                case "push":
                    Console.WriteLine(
                        $"Push {messageId} delivered.");
                    return true;

                default:
                    Console.WriteLine(
                        "Unknown channel.");
                    return false;
            }
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            NotificationFacade facade =
                new NotificationFacade();

            facade.SendSimpleMessage(
                "+380991112233",
                "Your order has been received.");

            facade.SendEmailMessage(
                "client@gmail.com",
                "Order Confirmation",
                "Your order #1234 has been confirmed.");

            facade.SendPushMessage(
                "DEVICE_TOKEN_123",
                "Food Delivery",
                "Courier is on the way.");

            facade.SendUrgentNotification(
                "DEVICE_TOKEN_123",
                "+380991112233",
                "Urgent Update",
                "Courier cannot reach you.");

            bool status =
                facade.GetDeliveryStatus(
                    "sms",
                    "MSG001");

            Console.WriteLine(
                $"Delivered: {status}");

            Console.ReadKey();
        }
    }
}