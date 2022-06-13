using System;
using Newtonsoft.Json;

namespace EcoChemBotVK
{
    class Feedback
    {
        public long UserId { get; set; }
        public int Mark { get; set; }
        public DateTime Datetime { get; set; }

        [JsonConstructor]
        public Feedback(long userId, int mark, DateTime datetime)
        {
            UserId = userId;
            Mark = mark;
            Datetime = datetime;
        }

        public Feedback() { }
    }
}