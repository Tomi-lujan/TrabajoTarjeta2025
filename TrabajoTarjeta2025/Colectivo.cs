using System;

namespace TrabajoTarjeta2025
{
    public class Colectivo
    {
        private const int DEFAULT_TARIFA = 1580;

        // Método antiguo para compatibilidad: devuelve solo si el pago se realizó
        public bool pagarCon(Tarjeta tarjeta)
        {
            return tarjeta.pagar(DEFAULT_TARIFA);
        }

        // Emite un boleto (null si falla el pago). Ahora captura saldo previo y pasa IdTarjeta como string.
        public Boleto? EmitirBoleto(Tarjeta tarjeta, int linea, int tarifa = DEFAULT_TARIFA, Func<DateTime>? nowProvider = null)
        {
            // capturar saldo previo antes del pago
            int saldoPrevio = tarjeta.verSaldo();

            bool pagoExitoso = tarjeta.pagar(tarifa);
            if (!pagoExitoso)
            {
                return null;
            }

            DateTime fecha = (nowProvider ?? (() => DateTime.Now))();
            int totalAbonado = tarjeta.LastPagoAmount;
            int saldoRestante = tarjeta.verSaldo();
            int montoExtra = Math.Max(0, totalAbonado - tarifa);

            return new Boleto(
                fecha: fecha,
                tipoTarjeta: tarjeta.Tipo.ToString(),
                linea: linea.ToString(),
                precioNormal: tarifa,
                totalAbonado: totalAbonado,
                saldo: saldoPrevio,
                saldoRestante: saldoRestante,
                idTarjeta: tarjeta.Id.ToString(),
                montoExtra: montoExtra
            );
        }
    }
}

