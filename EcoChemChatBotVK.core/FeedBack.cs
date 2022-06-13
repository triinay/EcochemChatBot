using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace EcoChemChatBotVK.core
{
    public class FeedBack
    {
        public long UserId { get; set; }
        public int Mark { get; set; }
        public DateTime Datetime { get; set; }

        [JsonConstructor]
        public FeedBack(long userId, int mark, DateTime datetime)
        {
            UserId = userId;
            Mark = mark;
            Datetime = datetime;
        }

        public FeedBack() { }
    }
}
