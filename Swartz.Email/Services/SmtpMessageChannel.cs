using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Configuration;
using System.Text;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;
using Swartz.Email.Models;
using Swartz.Logging;

namespace Swartz.Email.Services
{
    public class SmtpMessageChannel : Component, ISmtpChannel
    {
        public async Task ProcessAsync(IDictionary<string, object> parameters)
        {
            var emailMessage = new EmailMessage
            {
                Body = Read(parameters, "Body"),
                Subject = Read(parameters, "Subject"),
                Recipients = Read(parameters, "Recipients"),
                ReplyTo = Read(parameters, "ReplyTo"),
                From = Read(parameters, "From"),
                Bcc = Read(parameters, "Bcc"),
                Cc = Read(parameters, "CC")
            };

            if (emailMessage.Recipients.Length == 0)
            {
                Logger.Error("Email message doesn\'t have any recipient");
                return;
            }

            var section = (SmtpSection) ConfigurationManager.GetSection("system.net/mailSettings/smtp");
            if (section == null)
            {
                Logger.Error("You must config 'system.net/mailSettings/smtp' section");
                return;
            }

            var message = new MimeMessage();
            var html = new TextPart("html")
            {
                Text = emailMessage.Body
            };
            message.Body = html;
            message.Subject = emailMessage.Subject;
            message.From.Add(!string.IsNullOrWhiteSpace(section.From)
                ? new MailboxAddress(Encoding.UTF8,
                    section.From.Substring(0,
                        section.From.IndexOf("@", StringComparison.Ordinal)), section.Network.UserName)
                : new MailboxAddress(Encoding.UTF8,
                    emailMessage.From.Substring(0, emailMessage.From.IndexOf("@", StringComparison.Ordinal)),
                    emailMessage.From));

            try
            {
                foreach (var recipient in ParseRecipients(emailMessage.Recipients))
                {
                    message.To.Add(new MailboxAddress(Encoding.UTF8,
                        recipient.Substring(0, recipient.IndexOf("@", StringComparison.Ordinal)), recipient));
                }

                if (!string.IsNullOrWhiteSpace(emailMessage.Cc))
                {
                    foreach (var recipient in ParseRecipients(emailMessage.Cc))
                    {
                        message.Cc.Add(new MailboxAddress(Encoding.UTF8,
                            recipient.Substring(0, recipient.IndexOf("@", StringComparison.Ordinal)), recipient));
                    }
                }

                if (!string.IsNullOrWhiteSpace(emailMessage.Bcc))
                {
                    foreach (var recipient in ParseRecipients(emailMessage.Bcc))
                    {
                        message.Bcc.Add(new MailboxAddress(Encoding.UTF8,
                            recipient.Substring(0, recipient.IndexOf("@", StringComparison.Ordinal)), recipient));
                    }
                }

                if (!string.IsNullOrWhiteSpace(emailMessage.ReplyTo))
                {
                    foreach (var recipient in ParseRecipients(emailMessage.ReplyTo))
                    {
                        message.ReplyTo.Add(new MailboxAddress(Encoding.UTF8,
                            recipient.Substring(0, recipient.IndexOf("@", StringComparison.Ordinal)), recipient));
                    }
                }

                using (var client = await CreateSmtpClient(section))
                {
                    await client.SendAsync(message);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Could not send email");
            }
        }

        private async Task<SmtpClient> CreateSmtpClient(SmtpSection section)
        {
            if (section == null)
            {
                throw new ArgumentNullException(nameof(section));
            }

            if (string.IsNullOrWhiteSpace(section.Network.Host))
            {
                return new SmtpClient();
            }

            var smtpClient = new SmtpClient();
            await smtpClient.ConnectAsync(section.Network.Host, section.Network.Port, section.Network.EnableSsl);

            smtpClient.AuthenticationMechanisms.Remove("XOAUTH2");
            if (!string.IsNullOrWhiteSpace(section.Network.UserName))
            {
                await
                    smtpClient.AuthenticateAsync(Encoding.UTF8,
                        new NetworkCredential(section.Network.UserName, section.Network.Password));
            }

            return smtpClient;
        }

        private string Read(IDictionary<string, object> dictionary, string key)
        {
            return dictionary.ContainsKey(key) ? dictionary[key] as string : null;
        }

        private IEnumerable<string> ParseRecipients(string recipients)
        {
            return recipients.Split(new[] {',', ';'}, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}