using System;
using NUnit.Framework;
using TrabajoTarjeta2025;

namespace testTrabajoTarjeta2025
{
    [TestFixture]
    public class BoletoTests
    {
        private static Boleto CreateBoleto(
            DateTime? fecha = null,
            string tipoTarjeta = "Normal",
            string linea = "L1",
            decimal precioNormal = 1580m,
            decimal totalAbonado = 0m,
            decimal saldo = 0m,
            decimal? saldoRestante = null,
            string idTarjeta = "T-1",
            int montoExtra = 0)
        {
            DateTime f = fecha ?? DateTime.Now;
            decimal sr = saldoRestante ?? saldo;
            return new Boleto(
                fecha: f,
                tipoTarjeta: tipoTarjeta,
                linea: linea,
                precioNormal: precioNormal,
                totalAbonado: totalAbonado,
                saldo: saldo,
                saldoRestante: sr,
                idTarjeta: idTarjeta,
                montoExtra: montoExtra
            );
        }

        [Test]
        public void ProcesarPago_SaldoPositivo_RestaCorrectaYTotalAbonado()
        {
            var fecha = DateTime.Now;
            var boleto = CreateBoleto(
                fecha: fecha,
                tipoTarjeta: "Normal",
                linea: "L1",
                precioNormal: 100m,
                totalAbonado: 0m,
                saldo: 50m,
                idTarjeta: "T001"
            );

            boleto.ProcesarPago(30m);

            Assert.That(boleto.TotalAbonado, Is.EqualTo(30m));
            Assert.That(boleto.SaldoRestante, Is.EqualTo(20m));
        }

        [Test]
        public void ProcesarPago_SaldoExacto_ResultadoCeroEnSaldoRestante()
        {
            var fecha = DateTime.Now;
            var boleto = CreateBoleto(
                fecha: fecha,
                tipoTarjeta: "Normal",
                linea: "L1",
                precioNormal: 100m,
                totalAbonado: 0m,
                saldo: 30m,
                idTarjeta: "T002"
            );

            boleto.ProcesarPago(30m);

            Assert.That(boleto.TotalAbonado, Is.EqualTo(30m));
            Assert.That(boleto.SaldoRestante, Is.EqualTo(0m));
        }

        [Test]
        public void ProcesarPago_SaldoNegativo_AbonaDeudaYSobraNotaEnInforme()
        {
            var fecha = DateTime.Now;
            var boleto = CreateBoleto(
                fecha: fecha,
                tipoTarjeta: "Normal",
                linea: "L2",
                precioNormal: 25m,
                totalAbonado: 0m,
                saldo: -10m,           // Deuda de $10
                idTarjeta: "T003"
            );

            boleto.ProcesarPago(25m);  // Tarifa normal: $25

            // Debe pagar deuda ($10) + tarifa ($25) = $35 total
            Assert.That(boleto.TotalAbonado, Is.EqualTo(35m));
            Assert.That(boleto.SaldoRestante, Is.EqualTo(0m));

            var informe = boleto.Informe();
            Assert.That(informe, Does.Contain("Nota:"));
        }

        [Test]
        public void ProcesarPago_TarifaNegativa_LanzaArgumentException()
        {
            var fecha = DateTime.Now;
            var boleto = CreateBoleto(
                fecha: fecha,
                tipoTarjeta: "Normal",
                linea: "L3",
                precioNormal: 50m,
                totalAbonado: 0m,
                saldo: 20m,
                idTarjeta: "T004"
            );

            Assert.Throws<ArgumentException>(() => boleto.ProcesarPago(-5m));
        }

        [Test]
        public void Informe_ContieneCamposEsperados()
        {
            var fecha = new DateTime(2025, 10, 18, 12, 0, 0);
            var boleto = CreateBoleto(
                fecha: fecha,
                tipoTarjeta: "Estudiante",
                linea: "L5",
                precioNormal: 20m,
                totalAbonado: 0m,
                saldo: 50m,
                idTarjeta: "ID-99"
            );

            boleto.ProcesarPago(10m);

            var informe = boleto.Informe();

            Assert.That(informe, Does.Contain("Fecha:"));
            Assert.That(informe, Does.Contain("TipoTarjeta:"));
            Assert.That(informe, Does.Contain("Linea:"));
            Assert.That(informe, Does.Contain("IdTarjeta:"));
            Assert.That(informe, Does.Contain("Tarifa normal:"));
            Assert.That(informe, Does.Contain("Saldo previo:"));
            Assert.That(informe, Does.Contain("Total abonado:"));
            Assert.That(informe, Does.Contain("Saldo restante:"));
        }
    }
}