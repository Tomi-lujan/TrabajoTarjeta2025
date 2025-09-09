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

        public Tarjeta(int saldo, int limite)
        {
            this.saldo = saldo;
            this.limite = limite;
        }

        public int verSaldo()
        {
            return saldo;
        }

        public int pagar(int precio)
        {
            if (precio <= saldo)
            {
                saldo -= precio;
                return saldo;
            }
            else
            {
                return 0; 
            }
        }

        public int recargar(int monto)
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

}
