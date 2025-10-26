using System;

namespace TrabajoTarjeta2025
{
    public class Boleto
    {
        public DateTime Fecha { get; private set; }
        public string TipoTarjeta { get; private set; }
        public string Linea { get; private set; }
        public decimal Precio { get; private set; }
        public decimal PrecioNormal { get; private set; }
        public decimal TotalAbonado { get; private set; }
        public decimal Saldo { get; private set; }
        public decimal SaldoRestante { get; private set; }
        public string IdTarjeta { get; private set; }

        public Boleto(DateTime fecha, string tipoTarjeta, string linea, decimal precioNormal, decimal totalAbonado, decimal saldo, decimal saldoRestante, string idTarjeta, int montoExtra)
        {
            Fecha = fecha;
            TipoTarjeta = tipoTarjeta;
            Linea = linea;
            PrecioNormal = precioNormal;
            TotalAbonado = totalAbonado;
            Saldo = saldo;
            SaldoRestante = saldoRestante;
            IdTarjeta = idTarjeta;
            Precio = precioNormal;
        }

        public void ProcesarPago(decimal tarifaNormal)
        {
            if (tarifaNormal < 0)
                throw new ArgumentException("La tarifa no puede ser negativa.");

            decimal deuda = Saldo < 0 ? Math.Abs(Saldo) : 0;
            TotalAbonado = tarifaNormal + deuda;
            SaldoRestante = Saldo + deuda - tarifaNormal;
            if (SaldoRestante < 0) SaldoRestante = 0;
        }

        public string Informe()
        {
            string informe = $"Fecha: {Fecha}\nTipoTarjeta: {TipoTarjeta}\nLinea: {Linea}\nIdTarjeta: {IdTarjeta}\nTarifa normal: {PrecioNormal}\nSaldo previo: {Saldo}\nTotal abonado: {TotalAbonado}\nSaldo restante: {SaldoRestante}";
            if (Saldo < 0)
            {
                informe += "\nNota: Se abonó deuda previa.";
            }
            return informe;
        }
    }
}
