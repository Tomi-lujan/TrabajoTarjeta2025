using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrabajoTarjeta2025
{
    public class Tarjeta
    {
        public int saldo;
        public int limite;
        public int limiteMaximo = -1200;

        public Tarjeta(int saldo, int limite)
        {
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
                return false;
            }
            saldo -= precio;
            return true;
        }

        public virtual int recargar(int monto)
        {
            int[] montosAceptados = { 2000, 3000, 4000, 5000, 8000, 10000, 15000, 20000, 25000, 30000 };

            if (!montosAceptados.Contains(monto))
            {
                return 0;
            }

            if (saldo + monto <= limite)
            {
                saldo += monto;
                return saldo;
            }
            else
            {
                return 0;
            }
        }
    }

    public class MedioBoleto : Tarjeta
    {
        public MedioBoleto(int saldo, int limite) : base(saldo, limite) { }

        public override bool pagar(int precio)
        {
            int precioMedio = precio / 2;
            return base.pagar(precioMedio);
        }
    }
    public class BoletoGratuito : Tarjeta
    {
        public BoletoGratuito(int saldo, int limite) : base(saldo, limite) { }

        public override bool pagar(int precio)
        {
            return true;
        }
    }

    public class FranquiciaCompleta : Tarjeta
    {
        public FranquiciaCompleta(int saldo, int limite) : base(saldo, limite) { }

        public override bool pagar(int precio)
        {
            return true;
        }
    }
}
