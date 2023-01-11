﻿using StardewArchipelago.Archipelago;
using StardewArchipelago.Stardew;

namespace StardewArchipelago.Items.Mail
{
    public class LetterItemAttachment : LetterAttachment
    {
        public StardewItem ItemAttachment { get; private set; }
        public int AttachmentAmount { get; private set; }

        public LetterItemAttachment(ReceivedItem apItem, StardewItem itemAttachment, int attachmentAmount = 1) : base(apItem)
        {
            ItemAttachment = itemAttachment;
            AttachmentAmount = attachmentAmount;
        }

        public override string GetEmbedString()
        {
            return $"%item {ItemAttachment.Id} {AttachmentAmount} %%";
        }

        public override void SendToPlayer(Mailman _mailman)
        {
            _mailman.SendArchipelagoMail(ArchipelagoItem.ItemName, ArchipelagoItem.PlayerName, ArchipelagoItem.LocationName, GetEmbedString());
        }
    }
}
