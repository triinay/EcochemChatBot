using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using VkNet;
using VkNet.Model.RequestParams;
using VkNet.Model.Keyboard;
using VkNet.Model;

namespace EcoChemBotVK
{
    internal class Program
    {
        static VkApi api = new VkApi();
        static List<Feedback> FeedBack;

        static void Main(string[] args)
        {
            
            api.Authorize(new ApiAuthParams
            {
                AccessToken = "c4db624efe67503f7b14d139a4d80a48b928b27511387a972461b7b7391c3e8125d7dce1098b6c8a4f8cd"
            });

            VKMessageManager manager = new VKMessageManager();
            
            manager.OnNewMessage += (message, sender) => {
                switch (message.Payload)
                {
                    case null:
                        MainKeyboard(message, sender);
                        break;

                    case "{\"command\":\"start\"}":
                        MainKeyboard(message, sender);
                        break;

                    case "{\"button\":\"SealantChoice\"}":
                        ChoiceBetweenColorAndScopeButtons(message, sender);
                        break;

                    case "{\"button\":\"ScopeChoice\"}":
                        ScopeChoiceButtons(message, sender);
                        break;

                    case "{\"button\":\"InteriorWorkChoice\"}":
                        InteriorWorkButtons(message, sender);
                        break;

                    case "{\"button\":\"ExteriorWorkChoice\"}":
                        ExteriorWorkButtons(message, sender);
                        break;

                    case "{\"button\":\"FeedBack\"}":
                        FeedBack = Feedback(message, sender);
                        break;

                    case "{\"button\":\"GoodChoice\"}":
                        GoodChoice(message, sender, FeedBack);
                        break;

                    case "{\"button\":\"BadChoice\"}":
                        BadChoice(message, sender, FeedBack);
                        break;

                    case "{\"button\":\"NeutralChoice\"}":
                        NeutralChoice(message, sender, FeedBack);
                        break;

                    case "{\"button\":\"DeliveryChoice\"}":
                        DeliveryChoice(message, sender);
                        break;

                    case "{\"button\":\"15kg\"}":
                        const string bot_answer_15kg = "Доставка будет стоить 500 рублей по Москве.";
                        Delivery(message, sender, bot_answer_15kg);
                        break;

                    case "{\"button\":\"150kg\"}":
                        const string bot_answer_150kg = "Доставка будет осуществляться на легковом " +
                        "автомобиле и будет стоить 1500 рублей по Москве.";
                        Delivery(message, sender, bot_answer_150kg);
                        break;

                    case "{\"button\":\"1t\"}":
                        const string bot_answer_1t = "Доставка будет стоить 15 рублей за каждый километр. " +
                        "Дополнительно взымается 2500 рублей, если доставка занимает менее 5 часов по Москве. " +
                        "После 5 часов за каждый дополнительный час взымается 500 рублей.";
                        Delivery(message, sender, bot_answer_1t);
                        break;
                }
            };

            manager.StartMessagesHandling();
        }

        private static void Delivery(Message message, User sender, string bot_answer)
        {
            api.Messages.Send(new MessagesSendParams()
            {
                PeerId = sender.Id,
                Message = bot_answer,
                RandomId = new Random().Next(minValue: 0, maxValue: 10000),
            });

            TurnBackFromWork(message, sender);
        }

        private static void DeliveryChoice(Message message, User sender)
        {
            const string filename = "../../Data/DeliveryButtons.json";
            const string bot_answer = "Выберете вес Вашего груза:";

            MessageKeyboard keyboard = DrawKeyboard(filename: filename, oneTime: false, inLine: true);

            api.Messages.Send(new MessagesSendParams()
            {
                PeerId = sender.Id,
                Message = bot_answer,
                RandomId = new Random().Next(minValue: 0, maxValue: 10000),
                Keyboard = keyboard
            });
        }

        private static void NeutralChoice(Message message, User sender, List<Feedback> FeedBack)
        {
            api.Messages.Send(new MessagesSendParams()
            {
                PeerId = sender.Id,
                Message = "Спасибо за Ваш отзыв! Будем стараться стать лучше для Вас!",
                RandomId = new Random().Next(minValue: 0, maxValue: 10000),
            });

            FeedBack[FeedBack.Count - 1].Mark = 2;
            FeedBack[FeedBack.Count - 1].Datetime = DateTime.Now;

            SaveFeedback(FeedBack);
            TurnBackFromWork(message, sender);
        }

        private static void BadChoice(Message message, User sender, List<Feedback> FeedBack)
        {
            api.Messages.Send(new MessagesSendParams()
            {
                PeerId = sender.Id,
                Message = "Спасибо за Ваш отзыв! С Вами свяжутся для выяснения причин оценки.",
                RandomId = new Random().Next(minValue: 0, maxValue: 10000),
            });

            FeedBack[FeedBack.Count - 1].Mark = 1;
            FeedBack[FeedBack.Count - 1].Datetime = DateTime.Now;

            SaveFeedback(FeedBack);
            TurnBackFromWork(message, sender);
        }

        private static void GoodChoice(Message message, User sender, List<Feedback> FeedBack)
        {
            api.Messages.Send(new MessagesSendParams()
            {
                PeerId = sender.Id,
                Message = "Спасибо за Ваш отзыв! Рады стараться для Вас!",
                RandomId = new Random().Next(minValue: 0, maxValue: 10000),
            });

            FeedBack[FeedBack.Count - 1].Mark = 3;
            FeedBack[FeedBack.Count - 1].Datetime = DateTime.Now;

            SaveFeedback(FeedBack);
            TurnBackFromWork(message, sender);
        }

        private static List<Feedback> Feedback(Message message, User sender)
        {
            const string filename = "../../Data/FeedbackButtons.json";
            MessageKeyboard keyboard = DrawKeyboard(filename: filename, oneTime: false, inLine: true);

            api.Messages.Send(new MessagesSendParams()
            {
                PeerId = sender.Id,
                Message = "Чтобы поделиться отзывом, нажмите на кнопку:",
                RandomId = new Random().Next(minValue: 0, maxValue: 10000),
                Keyboard = keyboard
            });

            List<Feedback> FeedBack = ReadFeedback();
            Feedback feedback = new Feedback();
            feedback.UserId = sender.Id;
            FeedBack.Add(feedback);

            return FeedBack;
        }

        private static void TurnBackFromWork(Message message, User sender)
        {
            const string newFilename = "../../Data/StartMenuButtons.json";
            const string newBot_answer = "Спасибо за интерес к нашей продукции!";

            MessageKeyboard newKeyboard = DrawKeyboard(filename: newFilename, oneTime: true, inLine: false);

            api.Messages.Send(new MessagesSendParams()
            {
                PeerId = sender.Id,
                Message = newBot_answer,
                RandomId = new Random().Next(minValue: 0, maxValue: 10000),
                Keyboard = newKeyboard
            });
        }

        private static void ExteriorWorkButtons(Message message, User sender)
        {
            const string filename = "../../Data/ExteriorWorkButtons.json";
            const string bot_answer = "Что Вам нужно обработать?";

            MessageKeyboard keyboard = DrawKeyboard(filename: filename, oneTime: false, inLine: true);
            api.Messages.Send(new MessagesSendParams()
            {
                PeerId = sender.Id,
                Message = bot_answer,
                RandomId = new Random().Next(minValue: 0, maxValue: 10000),
                Keyboard = keyboard
            });

            TurnBackFromWork(message, sender);
        }

        private static void InteriorWorkButtons(Message message, User sender)
        {
            const string filename = "../../Data/InteriorWorkButtons.json";
            const string bot_answer = "Что Вам нужно обработать?";

            MessageKeyboard keyboard = DrawKeyboard(filename: filename, oneTime: false, inLine: true);
            api.Messages.Send(new MessagesSendParams()
            {
                PeerId = sender.Id,
                Message = bot_answer,
                RandomId = new Random().Next(minValue: 0, maxValue: 10000),
                Keyboard = keyboard
            });

            TurnBackFromWork(message, sender);
        }

        private static void ScopeChoiceButtons(Message message, User sender)
        {
            const string filename = "../../Data/MainScopeButtons.json";
            const string bot_answer = "Выберете тип работ:";

            MessageKeyboard keyboard = DrawKeyboard(filename: filename, oneTime: false, inLine: true);
            api.Messages.Send(new MessagesSendParams()
            {
                PeerId = sender.Id,
                Message = bot_answer,
                RandomId = new Random().Next(minValue: 0, maxValue: 10000),
                Keyboard = keyboard
            });
        }

        private static void ChoiceBetweenColorAndScopeButtons(Message message, User sender)
        {
            const string filename = "../../Data/ChoiceBetweenColorAndScopeButtons.json";
            const string bot_answer = "Пожалуйста, выберете подходящую опцию.";

            MessageKeyboard keyboard = DrawKeyboard(filename, oneTime: true, inLine: false);
            api.Messages.Send(new MessagesSendParams()
            {
                PeerId = sender.Id,
                Message = bot_answer,
                RandomId = new Random().Next(minValue: 0, maxValue: 10000),
                Keyboard = keyboard
            });
        }

        private static void MainKeyboard(Message message, User sender)
        {
            const string filename = "../../Data/StartMenuButtons.json";
            const string bot_answer = "Здравствуйте! Вас приветствует виртуальный помощник компании Экохим.";

            MessageKeyboard keyboard = DrawKeyboard(filename: filename, oneTime: true, inLine: false);
            api.Messages.Send(new MessagesSendParams()
            {
                PeerId = sender.Id,
                Message = bot_answer,
                RandomId = new Random().Next(minValue: 0, maxValue: 10000),
                Keyboard = keyboard
            });
        }

        private static MessageKeyboard DrawKeyboard(string filename, bool oneTime, bool inLine)
        {
            var keyboardBuilder = new KeyboardBuilder();
            if (inLine)
                keyboardBuilder.SetInline();
            if (oneTime)
                keyboardBuilder.SetOneTime();
            MessageKeyboard keyboard = keyboardBuilder.Build();
            keyboard.Buttons = CreateButtons(filename);
            
            return keyboard;
        }

        private static List<List<MessageKeyboardButton>> CreateButtons(string filename)
        {
            using var sw = new StreamReader(filename, Encoding.UTF8);
            using var jsonReader = new JsonTextReader(sw);
            var serializer = new JsonSerializer()
            {
                TypeNameHandling = TypeNameHandling.Auto
            };

            List<List<MessageKeyboardButton>> buttons = serializer.Deserialize<List<List<MessageKeyboardButton>>>(jsonReader);

            return buttons;
        }

        public static List<Feedback> ReadFeedback()
        {
            List<Feedback> FeedBack;

            try
            {
                using (var sw = new StreamReader("../../Data/Feedback.json", Encoding.UTF8))
                {
                    using (var jsonReader = new JsonTextReader(sw))
                    {
                        var serializer = new JsonSerializer()
                        {
                            TypeNameHandling = TypeNameHandling.Auto
                        };

                        FeedBack = serializer.Deserialize<List<Feedback>>(jsonReader);
                    }
                }
            }
            catch
            {
                FeedBack = new List<Feedback>();
            }
            return FeedBack;
        }

        public static void SaveFeedback(List<Feedback> FeedBack)
        {
            using (var sw = new StreamWriter("../../Data/Feedback.json"))
            {
                using (var jsonWriter = new JsonTextWriter(sw))
                {
                    jsonWriter.Formatting = Formatting.Indented;

                    var serializer = new JsonSerializer
                    {
                        TypeNameHandling = TypeNameHandling.Auto
                    };

                    serializer.Serialize(jsonWriter, FeedBack);
                }
            }
        }
    }
}