using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Search;
using MailKit.Security;
using MimeKit;

Console.OutputEncoding = System.Text.Encoding.UTF8;

Console.WriteLine("          EMAIL CLIENT");

Console.Write("Gmail address: ");
string email = Console.ReadLine() ?? "";

Console.Write("App Password: ");
string password = ReadPassword().Replace(" ", "");

if (string.IsNullOrWhiteSpace(email) ||
    string.IsNullOrWhiteSpace(password))
{
    Console.WriteLine("Email and password are required.");
    return;
}

while (true)
{
    Console.WriteLine();
    Console.WriteLine(" MENU ");
    Console.WriteLine("1. Send email");
    Console.WriteLine("2. View inbox");
    Console.WriteLine("0. Exit");
    Console.Write("Choose: ");

    string choice = Console.ReadLine() ?? "";

    try
    {
        switch (choice)
        {
            case "1":
                await SendEmailAsync(email, password);
                break;

            case "2":
                await ViewInboxAsync(email, password);
                break;

            case "0":
                Console.WriteLine("Goodbye!");
                return;

            default:
                Console.WriteLine("Invalid choice.");
                break;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
}


async Task SendEmailAsync(string senderEmail, string senderPassword)
{
    Console.Write("Recipient email: ");
    string recipient = Console.ReadLine() ?? "";

    Console.Write("Subject: ");
    string subject = Console.ReadLine() ?? "";

    Console.WriteLine("Message text:");
    string body = Console.ReadLine() ?? "";

    var message = new MimeMessage();

    message.From.Add(MailboxAddress.Parse(senderEmail));
    message.To.Add(MailboxAddress.Parse(recipient));
    message.Subject = subject;

    message.Body = new TextPart("plain")
    {
        Text = body
    };

    using var smtpClient = new SmtpClient();

    Console.WriteLine("Connecting to Gmail SMTP...");

    await smtpClient.ConnectAsync(
        "smtp.gmail.com",
        587,
        SecureSocketOptions.StartTls
    );

    await smtpClient.AuthenticateAsync(
        senderEmail,
        senderPassword
    );

    await smtpClient.SendAsync(message);

    await smtpClient.DisconnectAsync(true);

    Console.WriteLine("Email sent successfully!");
}


async Task ViewInboxAsync(string accountEmail, string accountPassword)
{
    using var imapClient = new ImapClient();

    Console.WriteLine("Connecting to Gmail IMAP...");

    await imapClient.ConnectAsync(
        "imap.gmail.com",
        993,
        SecureSocketOptions.SslOnConnect
    );

    await imapClient.AuthenticateAsync(
        accountEmail,
        accountPassword
    );

    IMailFolder inbox = imapClient.Inbox;

    await inbox.OpenAsync(FolderAccess.ReadWrite);

    IList<UniqueId> uids = await inbox.SearchAsync(SearchQuery.All);

    if (uids.Count == 0)
    {
        Console.WriteLine("Inbox is empty.");
        await imapClient.DisconnectAsync(true);
        return;
    }

    IList<UniqueId> latestUids = uids
        .Reverse()
        .Take(20)
        .ToList();

    var summaries = await inbox.FetchAsync(
        latestUids,
        MessageSummaryItems.Envelope |
        MessageSummaryItems.InternalDate
    );

    Console.WriteLine();
    Console.WriteLine(" INBOX ");

    for (int i = 0; i < summaries.Count; i++)
    {
        var summary = summaries[i];

        string sender = summary.Envelope.From.ToString();
        string subject = summary.Envelope.Subject ?? "(No subject)";

        string date = summary.InternalDate?.ToLocalTime().ToString("g")
            ?? summary.Envelope.Date?.ToLocalTime().ToString("g")
            ?? "Unknown";

        Console.WriteLine();
        Console.WriteLine($"{i + 1}. From: {sender}");
        Console.WriteLine($"   Subject: {subject}");
        Console.WriteLine($"   Date: {date}");
    }

    Console.WriteLine();
    Console.WriteLine("Enter message number to open it.");
    Console.WriteLine("Enter 0 to return.");
    Console.Write("Choose: ");

    if (!int.TryParse(Console.ReadLine(), out int choice) ||
        choice < 0 || choice > summaries.Count)
    {
        Console.WriteLine("Invalid choice.");
        await imapClient.DisconnectAsync(true);
        return;
    }

    if (choice == 0)
    {
        await imapClient.DisconnectAsync(true);
        return;
    }

    var selectedSummary = summaries[choice - 1];

    UniqueId selectedUid = selectedSummary.UniqueId;

    var selectedMessage = await inbox.GetMessageAsync(selectedUid);

    Console.WriteLine();
    Console.WriteLine(" MESSAGE ");
    Console.WriteLine($"From: {selectedMessage.From}");
    Console.WriteLine($"To: {selectedMessage.To}");
    Console.WriteLine($"Subject: {selectedMessage.Subject}");
    Console.WriteLine($"Date: {selectedMessage.Date}");
    Console.WriteLine();
    Console.WriteLine("Message:");

    string messageBody =
        selectedMessage.TextBody ??
        selectedMessage.HtmlBody ??
        "(No message body)";

    Console.WriteLine(messageBody);

    Console.WriteLine();
    Console.WriteLine("1. Delete message");
    Console.WriteLine("0. Return");
    Console.Write("Choose: ");

    string action = Console.ReadLine() ?? "";

    if (action == "1")
    {
        await inbox.AddFlagsAsync(
            selectedUid,
            MessageFlags.Deleted,
            true
        );

        await inbox.ExpungeAsync();

        Console.WriteLine("Message deleted.");
    }

    await imapClient.DisconnectAsync(true);
}


string ReadPassword()
{
    var password = new System.Text.StringBuilder();

    while (true)
    {
        ConsoleKeyInfo key = Console.ReadKey(true);

        if (key.Key == ConsoleKey.Enter)
        {
            Console.WriteLine();
            break;
        }

        if (key.Key == ConsoleKey.Backspace)
        {
            if (password.Length > 0)
            {
                password.Length--;
            }
        }
        else if (!char.IsControl(key.KeyChar))
        {
            password.Append(key.KeyChar);
        }
    }

    return password.ToString();
}
