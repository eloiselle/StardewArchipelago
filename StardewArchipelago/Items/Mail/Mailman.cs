﻿using System;
using System.Collections.Generic;
using StardewArchipelago.Stardew;
using StardewValley;

namespace StardewArchipelago.Items.Mail
{
    public class Mailman
    {
        private static readonly Random _random = new Random();
        private static readonly string[] ApMailStrings = {
            "Hey @, I was at {2}, minding my own business, and there I found {0}. I thought you would would make better use of it than I ever could.   -{1}{3}[#]Archipelago Item"
        };

        public void SendVanillaMail(string mailTitle, bool noLetter)
        {
            if (Game1.player.mailReceived.Contains(mailTitle))
            {
                return;
            }

            Game1.player.mailForTomorrow.Add(mailTitle + (noLetter ? "%&NL&%" : ""));
        }

        public void SendArchipelagoInvisibleMail(string apItemName, string findingPlayer, string locationName)
        {
            var mailKey = GetMailKey(apItemName, findingPlayer, locationName);

            if (Game1.player.hasOrWillReceiveMail(apItemName))
            {
                return;
            }

            var mailData = Game1.content.Load<Dictionary<string, string>>("Data\\mail");
            var embedString = "";
            mailData[mailKey] = string.Format(GetRandomApMailString(), apItemName, findingPlayer, locationName, embedString);
            Game1.player.mailForTomorrow.Add(mailKey + "%&NL&%");
        }

        public void SendArchipelagoMail(string apItemName, string findingPlayer, string locationName, string attachmentEmbedString)
        {
            var mailKey = GetMailKey(apItemName, findingPlayer, locationName);

            if (Game1.player.hasOrWillReceiveMail(apItemName))
            {
                return;
            }

            var mailData = Game1.content.Load<Dictionary<string, string>>("Data\\mail");

            mailData[mailKey] = string.Format(GetRandomApMailString(), apItemName, findingPlayer, locationName, attachmentEmbedString);

            Game1.player.mailForTomorrow.Add(mailKey);
        }

        public int OpenedMailsStartingWithKey(string apItemName)
        {
            var numberReceived = 0;
            foreach (var mail in Game1.player.mailReceived)
            {
                if (!mail.StartsWith(apItemName))
                {
                    continue;
                }

                numberReceived++;
            }

            return numberReceived;
        }

        private string GetMailKey(string apItemName, string findingPlayer, string locationName)
        {
            return (apItemName + findingPlayer + locationName);
        }

        private string GetRandomApMailString()
        {
            return ApMailStrings[_random.Next(0, ApMailStrings.Length)];
        }
    }
}
