namespace Networking.MQTT.Client
{
    using MQTTnet;

    public static class MQTTExtentions
    {
        [System.Obsolete]
        public static MqttTopicFilter ToTopicFilter(this string topic)
        {
            return new TopicFilterBuilder().WithTopic(topic).Build();
        }
    }
}
