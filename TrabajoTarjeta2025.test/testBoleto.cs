using System;
using NUnit.Framework;
using TrabajoTarjeta2025;

namespace testTrabajoTarjeta2025
{
    public class BoletoTests
    {
        [Test]
        public void ProcesarPago_SaldoPositivo_RestaCorrectaYTotalAbonado()
        {
            var fecha = DateTime.Now;
            var boleto = new Boleto(precio: 100m, fecha: fecha, tipoTarjeta: "Normal", linea: "L1", saldo: 50m, idTarjeta: "T001");

            boleto.ProcesarPago(30m);

            Assert.That(boleto.TotalAbonado, Is.EqualTo(30m));
            Assert.That(boleto.SaldoRestante, Is.EqualTo(20m));
        }

        [Test]
        public void ProcesarPago_SaldoExacto_ResultadoCeroEnSaldoRestante()
        {
            var fecha = DateTime.Now;
            var boleto = new Boleto(precio: 100m, fecha: fecha, tipoTarjeta: "Normal", linea: "L1", saldo: 30m, idTarjeta: "T002");

            boleto.ProcesarPago(30m);

            Assert.That(boleto.TotalAbonado, Is.EqualTo(30m));
            Assert.That(boleto.SaldoRestante, Is.EqualTo(0m));
        }

        [Test]
        public void ProcesarPago_SaldoNegativo_AbonaDeudaYSobraNotaEnInforme()
        {
            var fecha = DateTime.Now;
            // Precio menor que lo que terminará abonado para que aparezca la nota
            var boleto = new Boleto(precio: 25m, fecha: fecha, tipoTarjeta: "Normal", linea: "L2", saldo: -10m, idTarjeta: "T003");

            boleto.ProcesarPago(30m); // deuda = 10 => TotalAbonado = 40

            Assert.That(boleto.TotalAbonado, Is.EqualTo(40m));
            Assert.That(boleto.SaldoRestante, Is.EqualTo(0m));

            var informe = boleto.Informe();
            StringAssert.Contains("Nota:", informe); // debe contener la nota que indica exceso sobre la tarifa
            StringAssert.Contains("Tarifa normal", informe);
            StringAssert.Contains("Total abonado", informe);
        }

        [Test]
        public void ProcesarPago_TarifaNegativa_LanzaArgumentException()
        {
            var fecha = DateTime.Now;
            var boleto = new Boleto(precio: 50m, fecha: fecha, tipoTarjeta: "Normal", linea: "L3", saldo: 20m, idTarjeta: "T004");

            Assert.Throws<ArgumentException>(() => boleto.ProcesarPago(-5m));
        }

        [Test]
        public void Informe_ContieneCamposEsperados()
        {
            var fecha = new DateTime(2025, 10, 18, 12, 0, 0);
            var boleto = new Boleto(precio: 20m, fecha: fecha, tipoTarjeta: "Estudiante", linea: "L5", saldo: 50m, idTarjeta: "ID-99");

            boleto.ProcesarPago(10m);

            var informe = boleto.Informe();

            StringAssert.Contains("Fecha:", informe);
            StringAssert.Contains("TipoTarjeta:", informe);
            StringAssert.Contains("Linea:", informe);
            StringAssert.Contains("IdTarjeta:", informe);
            StringAssert.Contains("Tarifa normal:", informe);
            StringAssert.Contains("Saldo previo:", informe);
            StringAssert.Contains("Total abonado:", informe);
            StringAssert.Contains("Saldo restante:", informe);
        }
    }
}