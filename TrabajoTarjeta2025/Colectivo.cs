using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        // Nuevo método: intenta emitir un boleto y devuelve el objeto Boleto (null si falla el pago)
        public Boleto? EmitirBoleto(Tarjeta tarjeta, int linea, int tarifa = DEFAULT_TARIFA, Func<DateTime>? nowProvider = null)
        {
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
                linea: linea,
                precioNormal: tarifa,
                totalAbonado: totalAbonado,
                saldoRestante: saldoRestante,
                tarjetaId: tarjeta.Id,
                montoExtra: montoExtra
            );
        }
    }
}

