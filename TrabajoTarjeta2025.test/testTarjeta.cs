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
    }
}