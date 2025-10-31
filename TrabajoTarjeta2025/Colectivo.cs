using System;

namespace TrabajoTarjeta2025
{
    public class Colectivo
    {
        private const decimal DEFAULT_TARIFA = 1580m;

        // Método antiguo para compatibilidad: devuelve solo si el pago se realizó
        public bool pagarCon(Tarjeta tarjeta)
        {
            return tarjeta.pagar(DEFAULT_TARIFA);
        }

        // Nuevo método: intenta emitir un boleto y devuelve el objeto Boleto (null si falla el pago)
        public Boleto? EmitirBoleto(Tarjeta tarjeta, int linea, decimal tarifa = DEFAULT_TARIFA, Func<DateTime>? nowProvider = null)
        {
            DateTime now = (nowProvider ?? (() => DateTime.Now))();

            bool esTrasbordo = false;
            bool trasbordoPagado = false;
            decimal montoCobrado = 0m;

            // Verificar si existe ventana de trasbordo activa para la tarjeta
            bool ventanaActiva = tarjeta.TransferWindowStart.HasValue &&
                                 (now - tarjeta.TransferWindowStart.Value).TotalHours <= 1.0;

            bool horarioPermitido = IsHorarioTrasbordoPermitido(now);

            if (ventanaActiva && horarioPermitido && tarjeta.TransferBaseLine.HasValue && tarjeta.TransferBaseLine.Value != linea)
            {
                // Trasbordo libre: monto 0
                esTrasbordo = true;
                // Aún así respetamos reglas de la tarjeta (por ejemplo medio boleto 5 minutos)
                bool res = tarjeta.pagar(0m);
                if (!res)
                {
                    return null;
                }
                montoCobrado = tarjeta.LastPagoAmount;
                trasbordoPagado = montoCobrado > 0m;
            }
            else
            {
                // No es trasbordo libre: calcular monto según tipo y beneficios de uso frecuente (solo tarjetas normales)
                if (tarjeta.Tipo == TarjetaTipo.Normal)
                {
                    decimal montoConDescuento = tarjeta.CalcularTarifaConDescuento(tarifa, now);
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
                if (montoCobrado > 0m)
                {
                    tarjeta.TransferWindowStart = now;
                    tarjeta.TransferBaseLine = linea;
                }
            }

            decimal montoExtra = Math.Max(0m, montoCobrado - tarifa);

            // Construir boleto con la info pedida (Linea como string para compatibilidad con tests de Boleto)
            var boleto = new Boleto(
                fecha: now,
                tipoTarjeta: tarjeta.Tipo.ToString(),
                linea: linea.ToString(),
                precioNormal: tarifa,
                totalAbonado: montoCobrado,
                saldo: 0m, // saldo previo no requerido aquí
                saldoRestante: tarjeta.verSaldo(),
                idTarjeta: tarjeta.Id.ToString(),
                montoExtra: (int)montoExtra,
                esTrasbordo: esTrasbordo,
                trasbordoPagado: trasbordoPagado
            );

            return boleto;
        }

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
