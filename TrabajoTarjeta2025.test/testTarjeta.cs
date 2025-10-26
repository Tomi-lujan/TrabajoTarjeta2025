using System;
using NUnit.Framework;
using TrabajoTarjeta2025;

namespace testTrabajoTarjeta2025
{
    [TestFixture]
    public class TarjetaTests
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
            Assert.That(resultado, Is.EqualTo(42000)); // recargar no excede MAX_SALDO: mantiene saldo y PendienteAcreditar
            Assert.That(t.verSaldo(), Is.EqualTo(42000));
        }

        [Test]
        public void TestPagar_SaldoSuficiente()
        {
            bool resultado = t.pagar(5000);
            Assert.That(resultado, Is.True);
            Assert.That(t.verSaldo(), Is.EqualTo(5000)); // 10000 - 5000
        }

        [Test]
        public void TestPagar_SaldoInsuficiente_SuperaLimiteMaximo()
        {
            t = new Tarjeta(1000, 40000); // Saldo bajo
            bool resultado = t.pagar(3000); // Intenta pagar más del saldo + límite
            Assert.That(resultado, Is.False);
            Assert.That(t.verSaldo(), Is.EqualTo(1000)); // Saldo no cambia
        }

        [Test]
        public void TestPagar_ExactamenteEnLimiteMaximo()
        {
            t = new Tarjeta(1000, 40000);
            bool resultado = t.pagar(2200); // 1000 - 2200 = -1200 (justo en el límite)
            Assert.That(resultado, Is.True);
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

        [Test]
        public void TestMedioBoleto_Pagar()
        {
            var medioBoleto = new MedioBoleto(10000, 40000);
            bool resultado = medioBoleto.pagar(2000); // Debería cobrar la mitad: 1000
            Assert.That(resultado, Is.True);
            Assert.That(medioBoleto.verSaldo(), Is.EqualTo(9000)); // 10000 - 1000
        }

        [Test]
        public void TestBoletoGratuito_Pagar()
        {
            var boletoGratuito = new BoletoGratuito(10000, 40000);
            bool resultado = boletoGratuito.pagar(5000);
            Assert.That(resultado, Is.True);
            Assert.That(boletoGratuito.verSaldo(), Is.EqualTo(10000)); // Saldo no cambia
        }

        [Test]
        public void TestFranquiciaCompleta_Pagar()
        {
            var franquicia = new FranquiciaCompleta(10000, 40000);
            bool resultado = franquicia.pagar(5000);
            Assert.That(resultado, Is.True);
            Assert.That(franquicia.verSaldo(), Is.EqualTo(10000)); // Saldo no cambia
        }

        [Test]
        public void TestMedioBoleto_PagarConPrecioImpar()
        {
            var medioBoleto = new MedioBoleto(10000, 40000);
            bool resultado = medioBoleto.pagar(3000); // 3000 / 2 = 1500
            Assert.That(resultado, Is.True);
            Assert.That(medioBoleto.verSaldo(), Is.EqualTo(8500)); // 10000 - 1500
        }

        [Test]
        public void TestConstructor_ValoresIniciales()
        {
            var tarjetaNueva = new Tarjeta(5000, 30000);
            Assert.That(tarjetaNueva.verSaldo(), Is.EqualTo(5000));
        }

        // NUEVOS TESTS PARA REGLAS TEMPORALES Y BOLETOS:

        [Test]
        public void TestMedioBoleto_NoPermiteViajarEnMenosDe5Minutos()
        {
            DateTime current = new DateTime(2025, 10, 18, 10, 0, 0);
            Func<DateTime> clock = () => current;
            var medio = new MedioBoleto(10000, 40000, clock);

            bool r1 = medio.pagar(1580);
            Assert.That(r1, Is.True);
            // Avanzamos 3 minutos: menos de 5 => no se permite
            current = current.AddMinutes(3);
            bool r2 = medio.pagar(1580);
            Assert.That(r2, Is.False);
        }

        [Test]
        public void TestMedioBoleto_MaximoDosViajesPorDia_TercerViajeCobradoCompleto()
        {
            DateTime current = new DateTime(2025, 10, 18, 8, 0, 0);
            Func<DateTime> clock = () => current;
            var medio = new MedioBoleto(10000, 40000, clock);

            // Primer viaje
            bool v1 = medio.pagar(1580); // cobra 790
            Assert.That(v1, Is.True);
            current = current.AddMinutes(6); // >5 minutos

            // Segundo viaje
            bool v2 = medio.pagar(1580); // cobra 790
            Assert.That(v2, Is.True);
            current = current.AddMinutes(6); // >5 minutos

            // Tercer viaje del mismo día -> cobra tarifa completa 1580
            bool v3 = medio.pagar(1580);
            Assert.That(v3, Is.True);

            int esperado = 10000 - 790 - 790 - 1580; // 10000 - 3160 = 6840
            Assert.That(medio.verSaldo(), Is.EqualTo(esperado));
        }

        [Test]
        public void TestBoletoGratuito_MaximoDosGratis_PosterioresCobrados()
        {
            DateTime current = new DateTime(2025, 10, 18, 9, 0, 0);
            Func<DateTime> clock = () => current;
            var gratuito = new BoletoGratuito(10000, 40000, clock);

            // Dos viajes gratuitos
            Assert.That(gratuito.pagar(1580), Is.True);
            current = current.AddMinutes(10);
            Assert.That(gratuito.pagar(1580), Is.True);
            current = current.AddMinutes(10);

            // Tercer viaje: debe cobrarse tarifa completa
            bool t3 = gratuito.pagar(1580);
            Assert.That(t3, Is.True);
            Assert.That(gratuito.verSaldo(), Is.EqualTo(10000 - 1580));
        }

        [Test]
        public void TestEmitirBoleto_ContieneDatosEsperados()
        {
            DateTime current = new DateTime(2025, 10, 18, 12, 0, 0);
            Func<DateTime> clock = () => current;
            var tarjeta = new Tarjeta(10000, 40000);
            var colectivo = new Colectivo();

            // Emitir boleto mediante el colectivo (usa tarifa por defecto 1580)
            Boleto? b = colectivo.EmitirBoleto(tarjeta, linea: 123, tarifa: 1580, nowProvider: clock);

            Assert.That(b, Is.Not.Null);
            Assert.That(int.Parse(b!.Linea), Is.EqualTo(123));
            Assert.That(b.PrecioNormal, Is.EqualTo((decimal)1580));
            Assert.That(b.TotalAbonado, Is.EqualTo((decimal)tarjeta.LastPagoAmount));
            Assert.That(b.SaldoRestante, Is.EqualTo((decimal)tarjeta.verSaldo()));
            Assert.That(b.Fecha, Is.EqualTo(current));
            Assert.That(b.TipoTarjeta, Is.EqualTo(tarjeta.Tipo.ToString()));
            Assert.That(int.Parse(b.IdTarjeta), Is.EqualTo(tarjeta.Id));
        }

        [Test]
        public void Recarga_Que_Supera_Maximo_Acredita_Hasta_Maximo_Y_Deja_Pendiente()
        {
            var tarjeta = new Tarjeta(55000, 100000);
            int resultado = tarjeta.recargar(10000);
            Assert.That(resultado, Is.EqualTo(56000));
            Assert.That(tarjeta.verSaldo(), Is.EqualTo(56000));
            Assert.That(tarjeta.PendienteAcreditar, Is.EqualTo(9000));
        }

        [Test]
        public void Al_Usar_Tarjeta_Se_Acredita_Pendiente_Hasta_Llegar_Al_Maximo()
        {
            var tarjeta = new Tarjeta(55000, 100000);
            tarjeta.recargar(10000);

            bool pago = tarjeta.pagar(1000);
            Assert.That(pago, Is.True);
            Assert.That(tarjeta.verSaldo(), Is.EqualTo(56000));
            Assert.That(tarjeta.PendienteAcreditar, Is.EqualTo(8000));
        }
    }
}