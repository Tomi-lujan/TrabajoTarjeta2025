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

        // EmitirBoleto con tarifa entera
        public Boleto? EmitirBoleto(Tarjeta tarjeta, int linea, int tarifa = DEFAULT_TARIFA, Func<DateTime>? nowProvider = null)
        {
            DateTime now = (nowProvider ?? (() => DateTime.Now))();

            bool esTrasbordo = false;
            bool trasbordoPagado = false;
            int montoCobrado = 0;

            // Verificar si existe ventana de trasbordo activa para la tarjeta
            bool ventanaActiva = tarjeta.TransferWindowStart.HasValue &&
                                 (now - tarjeta.TransferWindowStart.Value).TotalHours <= 1.0;

            bool horarioPermitido = IsHorarioTrasbordoPermitido(now);

            if (ventanaActiva && horarioPermitido && tarjeta.TransferBaseLine.HasValue && tarjeta.TransferBaseLine.Value != linea)
            {
                // Trasbordo libre: monto 0
                esTrasbordo = true;
                // Aún así respetamos reglas de la tarjeta (por ejemplo medio boleto 5 minutos)
                bool res = tarjeta.pagar(0);
                if (!res)
                {
                    return null;
                }
                montoCobrado = tarjeta.LastPagoAmount;
                trasbordoPagado = montoCobrado > 0;
            }
            else
            {
                // No es trasbordo libre: calcular monto según tipo y beneficios de uso frecuente (solo tarjetas normales)
                if (tarjeta.Tipo == TarjetaTipo.Normal)
                {
                    int montoConDescuento = tarjeta.CalcularTarifaConDescuento(tarifa, now);
                    bool pagoOk = tarjeta.pagar(montoConDescuento);
                    if (!pagoOk)
                    {
                        return null;
                    }
                    montoCobrado = tarjeta.LastPagoAmount;
                }
                else
                {
                    // Para otras tarjetas delegamos en su pagar (MedioBoleto, Gratuito, FranquiciaCompleta)
                    bool pagoOk = tarjeta.pagar(tarifa);
                    if (!pagoOk)
                    {
                        return null;
                    }
                    montoCobrado = tarjeta.LastPagoAmount;
                }

                // Abrir o reiniciar ventana de trasbordo desde este boleto si se pagó (montoCobrado > 0)
                if (montoCobrado > 0)
                {
                    tarjeta.TransferWindowStart = now;
                    tarjeta.TransferBaseLine = linea;
                }
            }

            int montoExtra = Math.Max(0, montoCobrado - tarifa);

            // Construir boleto con la info pedida (Linea e Id como strings para compatibilidad con tests)
            var boleto = new Boleto(
                fecha: now,
                tipoTarjeta: tarjeta.Tipo.ToString(),
                linea: linea.ToString(),
                precioNormal: tarifa,
                totalAbonado: montoCobrado,
                saldoPrevio: 0, // no requerido aquí
                saldoRestante: tarjeta.verSaldo(),
                idTarjeta: tarjeta.Id.ToString(),
                montoExtra: montoExtra,
                esTrasbordo: esTrasbordo,
                trasbordoPagado: trasbordoPagado
            );

            return boleto;
        }

        // Sobrecarga para aceptar decimal tarifa en tests si fuese llamado así
        public Boleto? EmitirBoleto(Tarjeta tarjeta, int linea, decimal tarifa, Func<DateTime>? nowProvider = null)
            => EmitirBoleto(tarjeta, linea, (int)tarifa, nowProvider);

        private bool IsHorarioTrasbordoPermitido(DateTime now)
        {
            // Trasbordos se pueden realizar de lunes a sábado de 7:00 a 22:00.
            DayOfWeek d = now.DayOfWeek;
            bool diaValido = d >= DayOfWeek.Monday && d <= DayOfWeek.Saturday;
            TimeSpan t = now.TimeOfDay;
            bool horaValida = t >= TimeSpan.FromHours(7) && t < TimeSpan.FromHours(22);
            return diaValido && horaValida;
        }
    }
}
