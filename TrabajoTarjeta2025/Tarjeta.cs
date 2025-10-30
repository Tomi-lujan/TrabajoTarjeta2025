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
        protected int saldo;
        public int limite;
        private readonly int limiteMaximo = -1200;

        private const int MAX_SALDO = 56000;

        public int PendienteAcreditar { get; private set; } = 0;

        public TarjetaTipo Tipo { get; protected set; } = TarjetaTipo.Normal;

        /// <summary>
        /// Importe cobrado en la última operación de pago. 0 si fue gratuito o no se cobró.
        /// </summary>
        public int LastPagoAmount { get; protected set; } = 0;

        public DateTime? LastPagoTime { get; protected set; } = null;

        // Contadores para beneficio de uso frecuente (solo para tarjetas normales).
        private int viajesMesActual = 0;
        private int mesRef = 0;
        private int anioRef = 0;

        // Trasbordo: ventana iniciada (desde primer boleto pagado) y línea base
        public DateTime? TransferWindowStart { get; internal set; } = null;
        public int? TransferBaseLine { get; internal set; } = null;

        public Tarjeta(int saldo, int limite)
        {
            Id = _nextId++;
            this.saldo = saldo;
            this.limite = limite;
            var now = DateTime.Now;
            mesRef = now.Month;
            anioRef = now.Year;
        }

        // Permite construirse pasando decimales desde tests que usan sufijo m
        public Tarjeta(decimal saldo, decimal limite) : this((int)saldo, (int)limite) { }

        public virtual int verSaldo()
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
        /// Devuelve entero (pesos).
        /// </summary>
        public int CalcularTarifaConDescuento(int tarifa, DateTime now)
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
                return (tarifa * 80) / 100;
            }
            else if (nroViaje >= 60 && nroViaje <= 80)
            {
                // 25% de descuento
                return (tarifa * 75) / 100;
            }
            else
            {
                // fuera de tramos -> tarifa normal
                return tarifa;
            }
        }

        // Sobrecarga que recibe decimal desde tests si ocurriera
        public int CalcularTarifaConDescuento(decimal tarifa, DateTime now) => CalcularTarifaConDescuento((int)tarifa, now);

        public virtual bool pagar(int precio)
        {
            if (saldo - precio < limiteMaximo)
            {
                LastPagoAmount = 0;
                return false;
            }

            saldo -= precio;
            LastPagoAmount = precio;
            LastPagoTime = DateTime.Now;

            AcreditarCarga();

            return true;
        }

        // Sobrecarga para aceptar decimal en llamadas (casts al entero)
        public bool pagar(decimal precio) => pagar((int)precio);

        public virtual int recargar(int monto)
        {
            int[] montosAceptados = { 2000, 3000, 4000, 5000, 8000, 10000, 15000, 20000, 25000, 30000 };

            if (!montosAceptados.Contains(monto))
            {
                return 0;
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
                    int espacio = MAX_SALDO - saldo;
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

        // Sobrecarga decimal
        public int recargar(decimal monto) => recargar((int)monto);

        public int AcreditarCarga()
        {
            if (PendienteAcreditar <= 0)
            {
                return 0;
            }

            if (saldo >= MAX_SALDO)
            {
                return 0;
            }

            int espacio = MAX_SALDO - saldo;
            int toAcredit = Math.Min(espacio, PendienteAcreditar);

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

        public MedioBoleto(int saldo, int limite, Func<DateTime>? nowProvider = null) : base(saldo, limite)
        {
            Tipo = TarjetaTipo.Medio;
            ultimoUso = DateTime.MinValue;
            viajesDiarios = 0;
            _now = nowProvider ?? (() => DateTime.Now);
            fechaActual = _now().Date;
        }

        // Constructor decimal-friendly
        public MedioBoleto(decimal saldo, decimal limite, Func<DateTime>? nowProvider = null) : this((int)saldo, (int)limite, nowProvider) { }

        public override bool pagar(int precio)
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
                LastPagoAmount = 0;
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
            int precioMedio = precio / 2;
            bool r = base.pagar(precioMedio);
            return r;
        }

        public override bool pagar(decimal precio) => pagar((int)precio);
    }

    public class BoletoGratuito : Tarjeta
    {
        private int viajesGratuitosDiarios;
        private DateTime fechaActual;
        private readonly Func<DateTime> _now;

        public BoletoGratuito(int saldo, int limite, Func<DateTime>? nowProvider = null) : base(saldo, limite)
        {
            Tipo = TarjetaTipo.Educativo;
            viajesGratuitosDiarios = 0;
            _now = nowProvider ?? (() => DateTime.Now);
            fechaActual = _now().Date;
        }

        public BoletoGratuito(decimal saldo, decimal limite, Func<DateTime>? nowProvider = null) : this((int)saldo, (int)limite, nowProvider) { }

        public override bool pagar(int precio)
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
            LastPagoAmount = 0;
            LastPagoTime = now;
            return true;
        }

        public override bool pagar(decimal precio) => pagar((int)precio);
    }

    public class FranquiciaCompleta : Tarjeta
    {
        public FranquiciaCompleta(int saldo, int limite) : base(saldo, limite)
        {
            Tipo = TarjetaTipo.FranquiciaCompleta;
        }

        public FranquiciaCompleta(decimal saldo, decimal limite) : this((int)saldo, (int)limite) { }

        public override bool pagar(int precio)
        {
            LastPagoAmount = 0;
            LastPagoTime = DateTime.Now;
            return true;
        }

        public override bool pagar(decimal precio) => pagar((int)precio);
    }
}