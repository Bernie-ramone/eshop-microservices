using System;
using System.Collections.Generic;
using System.Text;

namespace EShop.BuildingBlocks.Infrastructure.Outbox
{

    /// <summary>
    /// Mensaje persistido en la base de datos del servicio, pendiente de ser
    /// publicado al broker (RabbitMQ).
    ///
    /// Este es el corazón del Outbox Pattern: el mensaje se guarda en la MISMA
    /// transacción que el cambio de negocio, garantizando atomicidad.
    ///
    /// Un proceso background (OutboxProcessor) lee esta tabla periódicamente,
    /// publica los mensajes pendientes, y los marca como procesados.
    /// </summary>
    public sealed class OutboxMessage
    {
        /// <summary>
        /// Constructor privado para EF Core.
        /// </summary>
        private OutboxMessage() { }

        /// <summary>
        /// Constructor del dominio: crea un nuevo mensaje pendiente.
        /// </summary>
        public OutboxMessage(string type, string content)
        {
            Id = Guid.NewGuid();
            OccurredOnUtc = DateTime.UtcNow;
            Type = type;
            Content = content;
        }

        /// <summary>
        /// ID del mensaje. NO es el Id del evento integration original.
        /// Este Id es del registro outbox; el evento serializado dentro tiene SU propio Id.
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Cuándo se generó el evento. Útil para ordering y troubleshooting.
        /// </summary>
        public DateTime OccurredOnUtc { get; private set; }

        /// <summary>
        /// Nombre completo del tipo CLR del evento (ej: "EShop.Catalog.IntegrationEvents.ProductCreated").
        /// El processor usa esto para deserializar correctamente.
        /// </summary>
        public string Type { get; private set; } = string.Empty;

        /// <summary>
        /// JSON serializado del evento.
        /// Lo guardamos como string para flexibilidad: si cambia el esquema del evento,
        /// los mensajes viejos siguen siendo deserializables (con cuidado).
        /// </summary>
        public string Content { get; private set; } = string.Empty;

        /// <summary>
        /// Cuándo se publicó exitosamente al broker.
        /// NULL = pendiente. Si tiene valor, ya se publicó.
        /// </summary>
        public DateTime? ProcessedOnUtc { get; private set; }

        /// <summary>
        /// Si el procesamiento falló, aquí guardamos el último error.
        /// Útil para debugging y para implementar reintentos con backoff.
        /// </summary>
        public string? Error { get; private set; }

        /// <summary>
        /// Marca el mensaje como procesado exitosamente.
        /// Lo invoca el OutboxProcessor después de publicar al broker.
        /// </summary>
        public void MarkAsProcessed()
        {
            ProcessedOnUtc = DateTime.UtcNow;
            Error = null;
        }

        /// <summary>
        /// Marca el mensaje como fallido (publicación al broker no funcionó).
        /// El processor lo reintentará en la siguiente corrida.
        /// </summary>
        public void MarkAsFailed(string error)
        {
            Error = error;
        }
    }
}
