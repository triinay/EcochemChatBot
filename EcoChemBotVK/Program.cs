using System;
using EcoChemChatBotVK.core;
using VkBotFramework;
using VkBotFramework.Models;
using VkNet.Model.RequestParams;

namespace EcoChemBotVK
{
    internal class Program
    {
        private static string AccessToken = "c4db624efe67503f7b14d139a4d80a48b928b27511387a972461b7b7391c3e8125d7dce1098b6c8a4f8cd";
        private static string GroupURL = "https://vk.com/club213632604";
        private static VkBot _bot;

        static void Main(string[] args)
        {
            _bot = new VkBot(AccessToken, GroupURL);
            _bot.OnMessageReceived += BotOnMessageReceived;
            _bot.Start();
            Console.ReadLine();
        }

        private static void BotOnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            var chatId = e.Message.PeerId;
            _bot.Api.Messages.Send(new MessagesSendParams()
            {
                Message = "Здравствуйте! Вас приветствует виртуальный помощник компании Экохим.",
                PeerId = chatId,
                RandomId = Environment.TickCount
            });
        }
    }
}
