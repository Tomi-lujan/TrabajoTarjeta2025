using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrabajoTarjeta2025
{
    public class Boleto
    {
<<<<<<< HEAD
        public DateTime Fecha { get; }
        public string TipoTarjeta { get; }
        public int Linea { get; }
        public int PrecioNormal { get; }
        public int TotalAbonado { get; }
        public int SaldoRestante { get; }
        public int TarjetaId { get; }
        /// <summary>
        /// Monto adicional abonado si por alguna razón se cobró más que el precio normal.
        /// Normalmente 0.
        /// </summary>
        public int MontoExtra { get; }

        public Boleto(DateTime fecha, string tipoTarjeta, int linea, int precioNormal, int totalAbonado, int saldoRestante, int tarjetaId, int montoExtra)
        {
            Fecha = fecha;
            TipoTarjeta = tipoTarjeta;
            Linea = linea;
            PrecioNormal = precioNormal;
            TotalAbonado = totalAbonado;
            SaldoRestante = saldoRestante;
            TarjetaId = tarjetaId;
            MontoExtra = montoExtra;
=======
        public DateTime Fecha { get; private set; }
        public string TipoTarjeta { get; private set; }
        public string Linea { get; private set; }
        public decimal Precio { get; private set; }           
        public decimal TotalAbonado { get; private set; }     
        public decimal Saldo { get; private set; }           
        public decimal SaldoRestante { get; private set; }    
        public string IdTarjeta { get; private set; }

        public Boleto(decimal precio, DateTime fecha, string tipoTarjeta, string linea, decimal saldo, string idTarjeta)
        {
            this.Precio = precio;
            this.Fecha = fecha;
            this.TipoTarjeta = tipoTarjeta ?? string.Empty;
            this.Linea = linea ?? string.Empty;
            this.Saldo = saldo;
            this.IdTarjeta = idTarjeta ?? string.Empty;
            this.TotalAbonado = 0m;
            this.SaldoRestante = saldo;
        }

        public void ProcesarPago(decimal tarifaNormal)
        {
            decimal tarifa = tarifaNormal;
            if (tarifa < 0) throw new ArgumentException("La tarifa no puede ser negativa.", nameof(tarifaNormal));

            if (this.Saldo < 0m)
            {
                // El pasajero debe abonar la deuda y además la tarifa
                decimal deuda = Math.Abs(this.Saldo);
                this.TotalAbonado = tarifa + deuda;

                this.SaldoRestante = 0m;
            }
            else
            {
                this.TotalAbonado = tarifa;
                this.SaldoRestante = this.Saldo - tarifa;
            }
        }

        // Genera un informe legible del boleto y del pago realizado
        public string Informe()
        {
            var mensaje = $"Fecha: {this.Fecha:G}\n" +
                          $"TipoTarjeta: {this.TipoTarjeta}\n" +
                          $"Linea: {this.Linea}\n" +
                          $"IdTarjeta: {this.IdTarjeta}\n" +
                          $"Tarifa normal: {this.Precio:C}\n" +
                          $"Saldo previo: {this.Saldo:C}\n" +
                          $"Total abonado: {this.TotalAbonado:C}\n" +
                          $"Saldo restante: {this.SaldoRestante:C}";

            if (this.TotalAbonado > this.Precio)
            {
                decimal exceso = this.TotalAbonado - this.Precio;
                mensaje += $"\nNota: El total abonado supera la tarifa por {exceso:C} debido a deuda previa en la tarjeta.";
            }

            return mensaje;
>>>>>>> ac7766337da967922b1f3735c45af0891c579266
        }

        // Métodos de acceso (ya proporcionados por las propiedades) permiten conocer:
        // Fecha, TipoTarjeta, Linea, TotalAbonado, SaldoRestante e ID de la tarjeta.
    }
}
