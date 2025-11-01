using System;
using System.Text;

namespace TrabajoTarjeta2025
{
    public class Boleto
    {
        public DateTime Fecha { get; }
        public string TipoTarjeta { get; }
        public string Linea { get; } // string para compatibilidad con tests
        public decimal PrecioNormal { get; }
        public decimal TotalAbonado { get; private set; }
        public decimal SaldoPrevio { get; }
        public decimal SaldoRestante { get; private set; }
        public string IdTarjeta { get; }
        /// <summary>
        /// Monto adicional abonado si por alguna razón se cobró más que el precio normal.
        /// Normalmente 0.
        /// </summary>
        public int MontoExtra { get; }

        // Indicadores de trasbordo
        public bool IsTrasbordo { get; }
        public bool TrasbordoPagado { get; }

        // Constructor usado por Colectivo.EmitirBoleto y tests
        public Boleto(DateTime fecha, string tipoTarjeta, string linea, decimal precioNormal, decimal totalAbonado, decimal saldo, decimal saldoRestante, string idTarjeta, int montoExtra, bool esTrasbordo = false, bool trasbordoPagado = false)
        {
            Fecha = fecha;
            TipoTarjeta = tipoTarjeta;
            Linea = linea;
            PrecioNormal = precioNormal;
            TotalAbonado = totalAbonado;
            SaldoPrevio = saldo;
            SaldoRestante = saldoRestante;
            IdTarjeta = idTarjeta;
            MontoExtra = montoExtra;
            IsTrasbordo = esTrasbordo;
            TrasbordoPagado = trasbordoPagado;
        }

        // Constructor auxiliar usado en testBoleto.CreateBoleto (saldoRestante nullable)
        public Boleto(DateTime fecha, string tipoTarjeta, string linea, decimal precioNormal, decimal totalAbonado, decimal saldo, decimal? saldoRestante, string idTarjeta, int montoExtra)
            : this(fecha, tipoTarjeta, linea, precioNormal, totalAbonado, saldo, saldoRestante ?? saldo, idTarjeta, montoExtra)
        { }

        // Método que procesa el pago sobre el boleto según saldo previo
        public void ProcesarPago(decimal tarifa)
        {
            if (tarifa < 0m) throw new ArgumentException("Tarifa negativa no permitida", nameof(tarifa));

            if (SaldoPrevio < 0m)
            {
                // Abonar deuda primero y además tarifa => el total abonado incluye la deuda
                decimal deuda = Math.Abs(SaldoPrevio);
                TotalAbonado = deuda + tarifa;
                SaldoRestante = 0m;
            }
            else
            {
                if (SaldoPrevio >= tarifa)
                {
                    TotalAbonado = tarifa;
                    SaldoRestante = SaldoPrevio - tarifa;
                }
                else
                {
                    // saldo positivo pero menor que tarifa: se abona todo el saldo y queda deuda? según tests se resta
                    TotalAbonado = tarifa;
                    SaldoRestante = SaldoPrevio - tarifa;
                    // En tests se esperan saldoRestante 0 cuando saldo negativo y abona deuda, handled above.
                }
            }
        }

        // Genera un informe textual con los campos esperados por tests
        public string Informe()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Fecha: {Fecha}");
            sb.AppendLine($"TipoTarjeta: {TipoTarjeta}");
            sb.AppendLine($"Linea: {Linea}");
            sb.AppendLine($"IdTarjeta: {IdTarjeta}");
            sb.AppendLine($"Tarifa normal: {PrecioNormal}");
            sb.AppendLine($"Saldo previo: {SaldoPrevio}");
            sb.AppendLine($"Total abonado: {TotalAbonado}");
            sb.AppendLine($"Saldo restante: {SaldoRestante}");

            // AGREGAR ESTA PARTE PARA MOSTRAR INFORMACIÓN DE TRASBORDO
            if (IsTrasbordo)
            {
                if (TrasbordoPagado)
                {
                    sb.AppendLine("TRASBORDO PAGADO");
                }
                else
                {
                    sb.AppendLine("TRASBORDO LIBRE");
                }
            }

            if (SaldoPrevio < 0m)
            {
                sb.AppendLine("Nota: Se abonó deuda previa.");
            }
            return sb.ToString();
        }
    }
}