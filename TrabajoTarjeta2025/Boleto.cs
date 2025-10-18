using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrabajoTarjeta2025
{
    public class Boleto
    {
        public DateTime Fecha { get; }
        public string TipoTarjeta { get; }
        public int Linea { get; }
        public int PrecioNormal { get; }
        public int TotalAbonado { get; }
        public int SaldoRestante { get; }
        public int TarjetaId { get; }
        /// <summary>
        /// Monto adicional abonado si por alguna razón se cobró más que el precio normal.
        /// Normalmente 0.
        /// </summary>
        public int MontoExtra { get; }

        public Boleto(DateTime fecha, string tipoTarjeta, int linea, int precioNormal, int totalAbonado, int saldoRestante, int tarjetaId, int montoExtra)
        {
            Fecha = fecha;
            TipoTarjeta = tipoTarjeta;
            Linea = linea;
            PrecioNormal = precioNormal;
            TotalAbonado = totalAbonado;
            SaldoRestante = saldoRestante;
            TarjetaId = tarjetaId;
            MontoExtra = montoExtra;
        }

        // Métodos de acceso (ya proporcionados por las propiedades) permiten conocer:
        // Fecha, TipoTarjeta, Linea, TotalAbonado, SaldoRestante e ID de la tarjeta.
    }
}
