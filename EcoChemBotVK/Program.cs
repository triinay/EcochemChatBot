using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VkBotFramework;
using VkBotFramework.Models;
using VkNet.Model.RequestParams;
using VkNet.Model.Keyboard;
using VkNet.Enums.SafetyEnums;



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

            KeyboardBuilder keyboard = DrawMainKeyboard();

            _bot.Api.Messages.Send(new MessagesSendParams()
            {
                Message = "Здравствуйте! Вас приветствует виртуальный помощник компании Экохим.",
                PeerId = chatId,
                RandomId = Environment.TickCount,
                Keyboard = keyboard.Build()
            });
        }

        private static KeyboardBuilder DrawMainKeyboard()
        {

            var keyboard = new KeyboardBuilder();
            keyboard.AddButton("Какой герметик мне подойдёт?", "");
            keyboard.AddLine();
            keyboard.AddButton("Инструкция по применению", "");
            keyboard.AddLine();
            keyboard.AddButton("Заказать звонок", "");
            keyboard.AddLine();
            keyboard.AddButton("Рассчитать стоимость доставки", "");
            keyboard.AddLine();
            keyboard.AddButton("Найти тех. характеристики герметика", "");
            keyboard.AddLine();
            keyboard.AddButton("Получить скидку", "");
            keyboard.AddLine();
            keyboard.AddButton("Оставить отзыв", "");
            keyboard.Build().Buttons = new List<IEnumerable<MessageKeyboardButton>>(){new List<MessageKeyboardButton>() {CreateLinkButton()}} ;
            
            
            return keyboard;
        }

        private static MessageKeyboardButton CreateLinkButton()
        {

            using (var sw = new StreamReader("../../Data/settings.json", Encoding.UTF8))
            {
                using (var jsonReader = new JsonTextReader(sw))
                {
                    var serializer = new JsonSerializer()
                    {
                        TypeNameHandling = TypeNameHandling.Auto
                    };

                    MessageKeyboardButton linkButton = serializer.Deserialize<MessageKeyboardButton>(jsonReader);
                    return linkButton;
                }
            }

        }


    }
}
