using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        private int limiteMaximo = -1200;

        private const int MAX_SALDO = 56000;

        public int PendienteAcreditar { get; private set; } = 0;

        public TarjetaTipo Tipo { get; protected set; } = TarjetaTipo.Normal;

        /// <summary>
        /// Importe cobrado en la última operación de pago. 0 si fue gratuito o no se cobró.
        /// </summary>
        public int LastPagoAmount { get; protected set; } = 0;

        public Tarjeta(int saldo, int limite)
        {
            Id = _nextId++;
            this.saldo = saldo;
            this.limite = limite;
        }

        public virtual int verSaldo()
        {
            return saldo;
        }

        public virtual bool pagar(int precio)
        {
            if (saldo - precio < limiteMaximo)
            {
                LastPagoAmount = 0;
                return false;
            }
            saldo -= precio;

            LastPagoAmount = precio;

            AcreditarCarga();

            return true;
        }

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
            fechaActual = (nowProvider ?? (() => DateTime.Now))().Date;
            _now = nowProvider ?? (() => DateTime.Now);
        }

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
            return base.pagar(precioMedio);
        }
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
            fechaActual = (nowProvider ?? (() => DateTime.Now))().Date;
            _now = nowProvider ?? (() => DateTime.Now);
        }

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
            return true;
        }
    }

    public class FranquiciaCompleta : Tarjeta
    {
        public FranquiciaCompleta(int saldo, int limite) : base(saldo, limite)
        {
            Tipo = TarjetaTipo.FranquiciaCompleta;
        }

        public override bool pagar(int precio)
        {
            LastPagoAmount = 0;
            return true;
        }
    }
}
