using System;
using System.Linq;

namespace TrabajoTarjeta2025
{
    public enum TarjetaTipo
    {
        Normal,
        Medio,
        Educativo,
        FranquiciaCompleta
    }

    public class Tarjeta
    {
        private static int _nextId = 1;

        public int Id { get; }
        protected decimal saldo;
        public decimal limite;
        private readonly decimal limiteMaximo = -1200m;

        private const decimal MAX_SALDO = 56000m;

        public decimal PendienteAcreditar { get; private set; } = 0m;

        public TarjetaTipo Tipo { get; protected set; } = TarjetaTipo.Normal;

        /// <summary>
        /// Importe cobrado en la última operación de pago. 0 si fue gratuito o no se cobró.
        /// </summary>
        public decimal LastPagoAmount { get; protected set; } = 0m;

        public DateTime? LastPagoTime { get; protected set; } = null;

        // Contadores para beneficio de uso frecuente (solo para tarjetas normales).
        private int viajesMesActual = 0;
        private int mesRef = 0;
        private int anioRef = 0;

        // Trasbordo: ventana iniciada (desde primer boleto pagado) y línea base
        public DateTime? TransferWindowStart { get; internal set; } = null;
        public int? TransferBaseLine { get; internal set; } = null;

        public Tarjeta(decimal saldo, decimal limite)
        {
            Id = _nextId++;
            this.saldo = saldo;
            this.limite = limite;
            var now = DateTime.Now;
            mesRef = now.Month;
            anioRef = now.Year;
        }

        public virtual decimal verSaldo()
        {
            return saldo;
        }

        protected void ActualizarMesSiCorresponde(DateTime now)
        {
            if (now.Month != mesRef || now.Year != anioRef)
            {
                viajesMesActual = 0;
                mesRef = now.Month;
                anioRef = now.Year;
            }
        }

        /// <summary>
        /// Calcula la tarifa aplicando el descuento de uso frecuente si corresponde (solo tarjetas normales).
        /// Incrementa el contador mensual de viajes (el descuento se basa en el número de viaje).
        /// </summary>
        public decimal CalcularTarifaConDescuento(decimal tarifa, DateTime now)
        {
            if (Tipo != TarjetaTipo.Normal)
            {
                // No aplica beneficio: no contamos viajes para este beneficio
                return tarifa;
            }

            ActualizarMesSiCorresponde(now);

            // Se considera el número de viaje actual (incrementamos antes de calcular la franja)
            viajesMesActual++;
            int nroViaje = viajesMesActual;

            if (nroViaje >= 30 && nroViaje <= 59)
            {
                // 20% de descuento
                return Math.Floor((tarifa * 80m) / 100m);
            }
            else if (nroViaje >= 60 && nroViaje <= 80)
            {
                // 25% de descuento
                return Math.Floor((tarifa * 75m) / 100m);
            }
            else
            {
                // fuera de tramos -> tarifa normal
                return tarifa;
            }
        }

        public virtual bool pagar(decimal precio)
        {
            if (saldo - precio < limiteMaximo)
            {
                LastPagoAmount = 0m;
                return false;
            }

            saldo -= precio;
            LastPagoAmount = precio;
            LastPagoTime = DateTime.Now;

            AcreditarCarga();

            return true;
        }

        public virtual decimal recargar(decimal monto)
        {
            decimal[] montosAceptados = { 2000m, 3000m, 4000m, 5000m, 8000m, 10000m, 15000m, 20000m, 25000m, 30000m };

            if (!montosAceptados.Contains(monto))
            {
                return 0m;
            }

            if (saldo + monto <= MAX_SALDO)
            {
                saldo += monto;
                return saldo;
            }
            else
            {
                if (saldo < MAX_SALDO)
                {
                    decimal espacio = MAX_SALDO - saldo;
                    saldo += espacio;
                    PendienteAcreditar += monto - espacio;
                }
                else
                {
                    PendienteAcreditar += monto;
                }

                return saldo;
            }
        }

        public decimal AcreditarCarga()
        {
            if (PendienteAcreditar <= 0m)
            {
                return 0m;
            }

            if (saldo >= MAX_SALDO)
            {
                return 0m;
            }

            decimal espacio = MAX_SALDO - saldo;
            decimal toAcredit = Math.Min(espacio, PendienteAcreditar);

            saldo += toAcredit;
            PendienteAcreditar -= toAcredit;

            return toAcredit;
        }
    }

    public class MedioBoleto : Tarjeta
    {
        private DateTime ultimoUso;
        private int viajesDiarios;
        private DateTime fechaActual;
        private readonly Func<DateTime> _now;

        public MedioBoleto(decimal saldo, decimal limite, Func<DateTime>? nowProvider = null) : base(saldo, limite)
        {
            Tipo = TarjetaTipo.Medio;
            ultimoUso = DateTime.MinValue;
            viajesDiarios = 0;
            _now = nowProvider ?? (() => DateTime.Now);
            fechaActual = _now().Date;
        }

        public override bool pagar(decimal precio)
        {
            DateTime now = _now();

            // Resetear contador si es un nuevo día
            if (now.Date != fechaActual.Date)
            {
                viajesDiarios = 0;
                fechaActual = now.Date;
            }

            // Verificar el intervalo de 5 minutos
            if (ultimoUso != DateTime.MinValue && (now - ultimoUso).TotalMinutes < 5)
            {
                LastPagoAmount = 0m;
                return false;
            }

            // Verificar cantidad de viajes diarios
            if (viajesDiarios >= 2)
            {
                // Cobrar tarifa completa después de dos viajes
                bool resFull = base.pagar(precio);
                if (resFull)
                {
                    ultimoUso = now;
                }
                return resFull;
            }

            viajesDiarios++;
            ultimoUso = now;
            decimal precioMedio = Math.Floor(precio / 2m);
            bool r = base.pagar(precioMedio);
            return r;
        }
    }

    public class BoletoGratuito : Tarjeta
    {
        private int viajesGratuitosDiarios;
        private DateTime fechaActual;
        private readonly Func<DateTime> _now;

        public BoletoGratuito(decimal saldo, decimal limite, Func<DateTime>? nowProvider = null) : base(saldo, limite)
        {
            Tipo = TarjetaTipo.Educativo;
            viajesGratuitosDiarios = 0;
            _now = nowProvider ?? (() => DateTime.Now);
            fechaActual = _now().Date;
        }

        public override bool pagar(decimal precio)
        {
            DateTime now = _now();

            // Resetear contador si es un nuevo día
            if (now.Date != fechaActual.Date)
            {
                viajesGratuitosDiarios = 0;
                fechaActual = now.Date;
            }

            if (viajesGratuitosDiarios >= 2)
            {
                // Cobrar tarifa completa después de dos viajes gratuitos
                return base.pagar(precio);
            }

            viajesGratuitosDiarios++;
            LastPagoAmount = 0m;
            LastPagoTime = now;
            return true;
        }
    }

    public class FranquiciaCompleta : Tarjeta
    {
        public FranquiciaCompleta(decimal saldo, decimal limite) : base(saldo, limite)
        {
            Tipo = TarjetaTipo.FranquiciaCompleta;
        }

        public override bool pagar(decimal precio)
        {
            LastPagoAmount = 0m;
            LastPagoTime = DateTime.Now;
            return true;
        }
    }
}