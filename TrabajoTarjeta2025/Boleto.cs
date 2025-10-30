using System;
using System.Text;

namespace TrabajoTarjeta2025
{
    public class Boleto
    {
        public DateTime Fecha { get; }
        public string TipoTarjeta { get; }
        public string Linea { get; } // string para compatibilidad con tests que hacen int.Parse
        public int PrecioNormal { get; }
        public int TotalAbonado { get; private set; }
        public int SaldoPrevio { get; }
        public int SaldoRestante { get; private set; }
        public string IdTarjeta { get; }
        public int MontoExtra { get; }

        // Indicadores de trasbordo
        public bool IsTrasbordo { get; }
        public bool TrasbordoPagado { get; }

        public Boleto(DateTime fecha, string tipoTarjeta, string linea, int precioNormal, int totalAbonado, int saldoPrevio, int saldoRestante, string idTarjeta, int montoExtra, bool esTrasbordo = false, bool trasbordoPagado = false)
        {
            Fecha = fecha;
            TipoTarjeta = tipoTarjeta;
            Linea = linea;
            PrecioNormal = precioNormal;
            TotalAbonado = totalAbonado;
            SaldoPrevio = saldoPrevio;
            SaldoRestante = saldoRestante;
            IdTarjeta = idTarjeta;
            MontoExtra = montoExtra;
            IsTrasbordo = esTrasbordo;
            TrasbordoPagado = trasbordoPagado;
        }

        // Método que procesa el pago sobre el boleto según saldo previo (usado por tests de Boleto independiente)
        public void ProcesarPago(int tarifa)
        {
            if (tarifa < 0) throw new ArgumentException("Tarifa negativa no permitida", nameof(tarifa));

            if (SaldoPrevio < 0)
            {
                int deuda = Math.Abs(SaldoPrevio);
                TotalAbonado = deuda + tarifa;
                SaldoRestante = 0;
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
                    // si saldo positivo menor que tarifa: se abona tarifa completa y queda saldoRestante negativo
                    TotalAbonado = tarifa;
                    SaldoRestante = SaldoPrevio - tarifa;
                }
            }
        }

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
            if (SaldoPrevio < 0)
            {
                sb.AppendLine("Nota: Se abonó deuda previa.");
            }
            return sb.ToString();
        }
    }
}