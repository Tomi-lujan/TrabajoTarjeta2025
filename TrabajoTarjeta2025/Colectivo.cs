using System;
using System.Collections.Generic;

namespace TrabajoTarjeta2025
{
    public class Colectivo
    {
        private const decimal DEFAULT_TARIFA = 1580m;
        private const decimal INTERURBAN_TARIFA = 3000m;

        private readonly HashSet<int> _interurbanLines = new HashSet<int>();

        // Constructor opcional que permite registrar líneas interurbanas desde el inicio
        public Colectivo(IEnumerable<int>? interurbanLines = null)
        {
            if (interurbanLines != null)
            {
                foreach (var l in interurbanLines)
                {
                    _interurbanLines.Add(l);
                }
            }
        }

        // Permite añadir o quitar líneas interurbanas en tiempo de ejecución (útil para tests)
        public void AddInterurbanLine(int linea) => _interurbanLines.Add(linea);
        public bool RemoveInterurbanLine(int linea) => _interurbanLines.Remove(linea);
        public bool IsInterurbanLine(int linea) => _interurbanLines.Contains(linea);

        private const decimal UNUSED = 0m;

        // Método antiguo para compatibilidad: devuelve solo si el pago se realizó
        public bool pagarCon(Tarjeta tarjeta)
        {
            return tarjeta.pagar(DEFAULT_TARIFA);
        }

        // Nuevo método: intenta emitir un boleto y devuelve el objeto Boleto (null si falla el pago)
        public Boleto? EmitirBoleto(Tarjeta tarjeta, int linea, decimal tarifa = DEFAULT_TARIFA, Func<DateTime>? nowProvider = null)
        {
            DateTime now = (nowProvider ?? (() => DateTime.Now))();

            // Guardar saldo previo antes de intentar el pago
            decimal saldoPrevio = tarjeta.verSaldo();

            bool esTrasbordo = false;
            bool trasbordoPagado = false;
            decimal montoCobrado = 0m;

            // Ventana de trasbordo activa (<= 1 hora desde TransferWindowStart)
            bool ventanaActiva = tarjeta.TransferWindowStart.HasValue &&
                                 (now - tarjeta.TransferWindowStart.Value).TotalHours <= 1.0;

            bool horarioPermitido = IsHorarioTrasbordoPermitido(now);

            bool isInterurban = IsInterurbanLine(linea);
            decimal tarifaEf = isInterurban ? INTERURBAN_TARIFA : tarifa;

            if (ventanaActiva && horarioPermitido && tarjeta.TransferBaseLine.HasValue && tarjeta.TransferBaseLine.Value != linea)
            {
                // Trasbordo libre: monto 0 (pero respetar reglas de la tarjeta)
                esTrasbordo = true;
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
                // No es trasbordo libre: calcular monto según tipo y beneficios
                if (tarjeta.Tipo == TarjetaTipo.Normal)
                {
                    decimal montoConDescuento = tarjeta.CalcularTarifaConDescuento(tarifaEf, now);
                    bool pagoOk = tarjeta.pagar(montoConDescuento);
                    if (!pagoOk) return null;
                    montoCobrado = tarjeta.LastPagoAmount;
                }
                else
                {
                    bool pagoOk = tarjeta.pagar(tarifaEf);
                    if (!pagoOk) return null;
                    montoCobrado = tarjeta.LastPagoAmount;
                }

                // Abrir/reiniciar ventana de trasbordo si se cobró
                if (montoCobrado > 0m)
                {
                    tarjeta.TransferWindowStart = now;
                    tarjeta.TransferBaseLine = linea;
                }
            }

            int montoExtra = 0;
            var boleto = new Boleto(
                now,
                tarjeta.Tipo.ToString(),
                linea.ToString(),
                tarifaEf,                   // Precio normal (usa tarifa interurbana si corresponde)
                montoCobrado,
                saldoPrevio,
                tarjeta.verSaldo(),
                tarjeta.Id.ToString(),
                montoExtra,
                esTrasbordo,
                trasbordoPagado
            );

            return boleto;
        }

        // Horario permitido para trasbordo (mismo criterio que franquicias)
        private static bool IsHorarioTrasbordoPermitido(DateTime now)
        {
            var dayOk = now.DayOfWeek >= DayOfWeek.Monday && now.DayOfWeek <= DayOfWeek.Friday;
            var time = now.TimeOfDay;
            var start = TimeSpan.FromHours(6);
            var end = TimeSpan.FromHours(22);
            return dayOk && time >= start && time <= end;
        }
    }
}
