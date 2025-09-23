using System;
using NUnit.Framework;
using TrabajoTarjeta2025;

namespace testTrabajoTarjeta2025
{
    public class Tests
    {
        public Tarjeta t;

        [SetUp]
        public void Setup()
        {
            t = new Tarjeta(10000, 40000);
        }

        [Test]
        public void TestRecargaValida()
        {
            int saldoEsperado = 40000;
            int resultado = t.recargar(30000);
            Assert.That(resultado, Is.EqualTo(saldoEsperado));
            Assert.That(t.verSaldo(), Is.EqualTo(saldoEsperado));
        }

        [Test]
        public void TestRecargaInvalidaMontoNoPermitido()
        {
            int resultado = t.recargar(1234);
            Assert.That(resultado, Is.EqualTo(0));
            Assert.That(t.verSaldo(), Is.EqualTo(10000));
        }

        [Test]
        public void TestRecargaExcedeLimite()
        {
            t = new Tarjeta(39000, 40000);
            int resultado = t.recargar(3000);
            Assert.That(resultado, Is.EqualTo(0));
            Assert.That(t.verSaldo(), Is.EqualTo(39000));
        }

        [Test]
        public void TestRecargaValida()
        {
            int saldoEsperado = 40000;
            int resultado = t.recargar(30000);
            Assert.That(resultado, Is.EqualTo(saldoEsperado));
            Assert.That(t.verSaldo(), Is.EqualTo(saldoEsperado));
        }

        [Test]
        public void TestRecargaInvalidaMontoNoPermitido()
        {
            int resultado = t.recargar(1234);
            Assert.That(resultado, Is.EqualTo(0));
            Assert.That(t.verSaldo(), Is.EqualTo(10000));
        }

        [Test]
        public void TestRecargaExcedeLimite()
        {
            t = new Tarjeta(39000, 40000);
            int resultado = t.recargar(3000);
            Assert.That(resultado, Is.EqualTo(0));
            Assert.That(t.verSaldo(), Is.EqualTo(39000));
        }

        // NUEVOS TESTS PARA AUMENTAR COBERTURA:

        [Test]
        public void TestPagar_SaldoSuficiente()
        {
            bool resultado = t.pagar(5000);
            Assert.IsTrue(resultado);
            Assert.That(t.verSaldo(), Is.EqualTo(5000)); // 10000 - 5000
        }

        [Test]
        public void TestPagar_SaldoInsuficiente_SuperaLimiteMaximo()
        {
            t = new Tarjeta(1000, 40000); // Saldo bajo
            bool resultado = t.pagar(3000); // Intenta pagar más del saldo + límite
            Assert.IsFalse(resultado);
            Assert.That(t.verSaldo(), Is.EqualTo(1000)); // Saldo no cambia
        }

        [Test]
        public void TestPagar_ExactamenteEnLimiteMaximo()
        {
            t = new Tarjeta(1000, 40000);
            bool resultado = t.pagar(2200); // 1000 - 2200 = -1200 (justo en el límite)
            Assert.IsTrue(resultado);
            Assert.That(t.verSaldo(), Is.EqualTo(-1200));
        }

        [Test]
        public void TestVerSaldo()
        {
            int saldo = t.verSaldo();
            Assert.That(saldo, Is.EqualTo(10000));
        }

        [Test]
        public void TestRecarga_TodosMontosAceptados()
        {
            int[] montosValidos = { 2000, 3000, 4000, 5000, 8000, 10000, 15000, 20000, 25000, 30000 };

            foreach (int monto in montosValidos)
            {
                var tarjetaTest = new Tarjeta(0, 50000);
                int resultado = tarjetaTest.recargar(monto);
                Assert.That(resultado, Is.EqualTo(monto), $"Fallo con monto: {monto}");
            }
        }

        [Test]
        public void TestRecarga_MontoExactamenteEnLimite()
        {
            t = new Tarjeta(20000, 40000);
            int resultado = t.recargar(20000); // 20000 + 20000 = 40000 (justo en el límite)
            Assert.That(resultado, Is.EqualTo(40000));
            Assert.That(t.verSaldo(), Is.EqualTo(40000));
        }

        // TESTS PARA LAS SUBCLASES:

        [Test]
        public void TestMedioBoleto_Pagar()
        {
            var medioBoleto = new MedioBoleto(10000, 40000);
            bool resultado = medioBoleto.pagar(2000); // Debería pagar 1000 (la mitad)
            Assert.IsTrue(resultado);
            Assert.That(medioBoleto.verSaldo(), Is.EqualTo(9000)); // 10000 - 1000
        }

        [Test]
        public void TestBoletoGratuito_Pagar()
        {
            var boletoGratuito = new BoletoGratuito(10000, 40000);
            bool resultado = boletoGratuito.pagar(5000);
            Assert.IsTrue(resultado);
            Assert.That(boletoGratuito.verSaldo(), Is.EqualTo(10000)); // Saldo no cambia
        }

        [Test]
        public void TestFranquiciaCompleta_Pagar()
        {
            var franquicia = new FranquiciaCompleta(10000, 40000);
            bool resultado = franquicia.pagar(5000);
            Assert.IsTrue(resultado);
            Assert.That(franquicia.verSaldo(), Is.EqualTo(10000)); // Saldo no cambia
        }

        [Test]
        public void TestMedioBoleto_PagarConPrecioImpar()
        {
            var medioBoleto = new MedioBoleto(10000, 40000);
            bool resultado = medioBoleto.pagar(3000); // 3000 / 2 = 1500
            Assert.IsTrue(resultado);
            Assert.That(medioBoleto.verSaldo(), Is.EqualTo(8500)); // 10000 - 1500
        }

        [Test]
        public void TestConstructor_ValoresIniciales()
        {
            var tarjetaNueva = new Tarjeta(5000, 30000);
            Assert.That(tarjetaNueva.verSaldo(), Is.EqualTo(5000));
        }

    }
}