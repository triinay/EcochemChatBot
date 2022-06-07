using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using VkNet;
using VkNet.Model.RequestParams;
using VkNet.Model.Keyboard;
using VkNet.Enums.SafetyEnums;
using VkNet.Model;


namespace EcoChemBotVK
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var api = new VkApi();
            api.Authorize(new ApiAuthParams
            {
                AccessToken = "c4db624efe67503f7b14d139a4d80a48b928b27511387a972461b7b7391c3e8125d7dce1098b6c8a4f8cd"
            });

            string bot_answer = "Здравствуйте! Вас приветствует виртуальный помощник компании Экохим.";

            VKMessageManager manager = new VKMessageManager();

            manager.OnNewMessage += (message, sender) => {
                MessageKeyboard keyboard = DrawMainKeyboard();

                api.Messages.Send(new MessagesSendParams()
                {
                    PeerId = sender.Id,
                    Message = bot_answer,
                    RandomId = new Random().Next(minValue: 0, maxValue: 10000),
                    Keyboard = keyboard
                });
            };

            manager.StartMessagesHandling();
        }


        private static MessageKeyboard DrawMainKeyboard()
        {
            var keyboardBuilder = new KeyboardBuilder();
            MessageKeyboard keyboard = keyboardBuilder.Build();
            keyboard.Buttons = CreateLinkButton();
            return keyboard;
        }


        private static List<List<MessageKeyboardButton>> CreateLinkButton()
        {
            using var sw = new StreamReader("../../Data/settings.json", Encoding.UTF8);
            using var jsonReader = new JsonTextReader(sw);
            var serializer = new JsonSerializer()
            {
                TypeNameHandling = TypeNameHandling.Auto
            };

            List<List<MessageKeyboardButton>> buttons = serializer.Deserialize<List<List<MessageKeyboardButton>>>(jsonReader);

            return buttons;
        }
    }
}