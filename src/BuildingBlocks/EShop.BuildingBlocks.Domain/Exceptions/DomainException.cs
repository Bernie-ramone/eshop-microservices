namespace EShop.BuildingBlocks.Domain.Exceptions
{
    /// <summary>
    /// Excepción base para violaciones de reglas de negocio del dominio.
    /// 
    /// Diferenciamos errores DE NEGOCIO (DomainException) de errores TÉCNICOS (Exception):
    /// - DomainException → "no puedes confirmar una orden cancelada" → HTTP 400
    /// - Exception        → "la BD se cayó" → HTTP 500
    /// 
    /// El middleware de excepciones inspecciona el TIPO para decidir qué status code devolver.
    /// </summary>
    public class DomainException : Exception
    {
        public DomainException(string message) : base(message) { }

        public DomainException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
