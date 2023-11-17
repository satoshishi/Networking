namespace Networking.MQTT.Streaming
{
    using System;
    using Cysharp.Threading.Tasks;
    using MessagePipe;
    using MQTTnet.Client;
    using Networking.MQTT.Client;
    using Networking.Core.Streaming;
    using Networking.MQTT.Payload;
    using UnityEngine;

    /// <summary>
    /// MQTTからのメッセージをストリーミングするための基底クラス
    /// </summary>
    /// <typeparam name="T">ストリーミングするメッセージを表現するクラス</typeparam>
    public class MQTTStreamer<T> : INetworkStreamer<T>, IMQTTMessageListener, IDisposable where T : MQTTPayload
    {
        private IDisposable disposable = null;

        public MQTTStreamer(string topic, ISubscriber<T> subscriber, IPublisher<T> publisher, ISubscriber<MQTTReceivedMessage> mqttMessage)
        {
            this.Topic = topic;
            this.Subscriber = subscriber;
            this.Publisher = publisher;

            DisposableBagBuilder bag = DisposableBag.CreateBuilder();
            mqttMessage.Subscribe(this.OnReceived, this.IsTargetTopic).AddTo(bag);
            this.disposable = bag.Build();
        }

        public string Topic { get; }

        public ISubscriber<T> Subscriber { get; protected set; }

        public IPublisher<T> Publisher { get; protected set; }

        /// <summary>q
        /// payload情報の購読を登録する
        /// </summary>
        /// <param name="listener">リスナー</param>
        /// <returns>購読の破棄を実行するためのdisposable</returns>
        public IDisposable AddListener(Action<T> listener)
        {
            DisposableBagBuilder bag = DisposableBag.CreateBuilder();
            this.Subscriber.Subscribe(listener).AddTo(bag);

            return bag.Build();
        }

        /// <summary>
        /// MQTTからのメッセージの購読を登録する
        /// </summary>
        /// <param name="client">MQTTクライアント</param>
        /// <returns>async</returns>
        [Obsolete]
        public async UniTask SubscribeAsync(IMqttClient client)
        {
            await client.SubscribeAsync(this.Topic.ToTopicFilter());
            Debug.Log($"{this.Topic} Subscribed");
        }

        public void Dispose()
        {
            this.disposable?.Dispose();
        }

        /// <summary>
        /// MQTTからのメッセージが対象としたTopicと紐づいているかどうか
        /// </summary>
        /// <param name="message">メッセージ情報</param>
        /// <returns>真偽</returns>
        private bool IsTargetTopic(MQTTReceivedMessage message)
        {
            return message.Topic.Equals(this.Topic);
        }

        /// <summary>
        /// MQTTからのメッセージをもとにjsonをデシリアライズして購読者にpublishする
        /// </summary>
        /// <param name="message">メッセージ情報</param>
        private void OnReceived(MQTTReceivedMessage message)
        {
            T payload = JsonUtility.FromJson<T>(message.Payload);
            this.Publisher.Publish(payload);
        }
    }
}