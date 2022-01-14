namespace PQS.MessageBusFun.Messaging.Contracts
{
    /// <summary>
    /// Configuracion para conectar a RabbitMQ
    /// </summary>
    public class RabbitMQConfig
    {
        public const string SECTION_NAME = "RabbitMQ";
        /// <summary>
        /// The URI host address of the RabbitMQ host (rabbitmq://host:port/vhost)
        /// </summary>
        public string Host { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}