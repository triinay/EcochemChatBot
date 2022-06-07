using System;
using System.Linq;
using System.Threading;
using VkNet;
using VkNet.Model;
using VkNet.Model.RequestParams;

namespace EcoChemChatBotVK.core
{
    public class VKMessageManager
    {
        private VkApi _api = new VkApi();
        private ulong ts;
        private ulong? pts;
        public event Action<Message, User> OnNewMessage;

        public VKMessageManager()
        {
            _api.Authorize(new ApiAuthParams()
            {
                AccessToken = "c4db624efe67503f7b14d139a4d80a48b928b27511387a972461b7b7391c3e8125d7dce1098b6c8a4f8cd"
            });
        }


        public void StartMessagesHandling()
        {
            LongPollServerResponse longPoolServerResponse = _api.Messages.GetLongPollServer(needPts: true);
            ts = Convert.ToUInt64(longPoolServerResponse.Ts);
            pts = longPoolServerResponse.Pts;

            new Thread(LongPollEventLoop).Start();
        }


        public void LongPollEventLoop()
        {
            while (true)
            {
                LongPollHistoryResponse longPollResponse = _api.Messages.GetLongPollHistory(new MessagesGetLongPollHistoryParams()
                {
                    Ts = ts,
                    Pts = pts
                });

                pts = longPollResponse.NewPts;

                for (int i = 0; i < longPollResponse.Messages.Count; i++)
                {
                    switch (longPollResponse.History[i][0])
                    {
                        //Код 4 - новое сообщение
                        case 4:
                            OnNewMessage?.Invoke(
                                longPollResponse.Messages[i],
                                longPollResponse.Profiles
                                 .Where(u => u.Id == longPollResponse.Messages[i].FromId)
                                 .FirstOrDefault()
                            );
                            break;

                        case 12:
                            OnNewMessage?.Invoke(
                                longPollResponse.Messages[i],
                                longPollResponse.Profiles
                                 .Where(u => u.Id == longPollResponse.Messages[i].FromId)
                                 .FirstOrDefault()
                            );
                            break;
                    }
                }
            }
        }

    }
}
