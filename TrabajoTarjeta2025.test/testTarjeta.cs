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
            Assert.That(resultado, Is.EqualTo(39000)); // recargar no excede MAX_SALDO: mantiene saldo y PendienteAcreditar
            Assert.That(t.verSaldo(), Is.EqualTo(39000));
        }

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

        [Test]
        public void TestMedioBoleto_Pagar()
        {
            var medioBoleto = new MedioBoleto(10000, 40000);
            bool resultado = medioBoleto.pagar(2000); // Debería cobrar la mitad: 1000
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

<<<<<<< HEAD
        // NUEVOS TESTS PARA REGLAS TEMPORALES Y BOLETOS:

        [Test]
        public void TestMedioBoleto_NoPermiteViajarEnMenosDe5Minutos()
        {
            DateTime current = new DateTime(2025, 10, 18, 10, 0, 0);
            Func<DateTime> clock = () => current;
            var medio = new MedioBoleto(10000, 40000, clock);

            bool r1 = medio.pagar(1580);
            Assert.IsTrue(r1);
            // Avanzamos 3 minutos: menos de 5 => no se permite
            current = current.AddMinutes(3);
            bool r2 = medio.pagar(1580);
            Assert.IsFalse(r2);
        }

        [Test]
        public void TestMedioBoleto_MaximoDosViajesPorDia_TercerViajeCobradoCompleto()
        {
            DateTime current = new DateTime(2025, 10, 18, 8, 0, 0);
            Func<DateTime> clock = () => current;
            var medio = new MedioBoleto(10000, 40000, clock);

            // Primer viaje
            bool v1 = medio.pagar(1580); // cobra 790
            Assert.IsTrue(v1);
            current = current.AddMinutes(6); // >5 minutos

            // Segundo viaje
            bool v2 = medio.pagar(1580); // cobra 790
            Assert.IsTrue(v2);
            current = current.AddMinutes(6); // >5 minutos

            // Tercer viaje del mismo día -> cobra tarifa completa 1580
            bool v3 = medio.pagar(1580);
            Assert.IsTrue(v3);

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
            Assert.IsTrue(gratuito.pagar(1580));
            current = current.AddMinutes(10);
            Assert.IsTrue(gratuito.pagar(1580));
            current = current.AddMinutes(10);

            // Tercer viaje: debe cobrarse tarifa completa
            bool t3 = gratuito.pagar(1580);
            Assert.IsTrue(t3);
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

            Assert.IsNotNull(b);
            Assert.That(b!.Linea, Is.EqualTo(123));
            Assert.That(b.PrecioNormal, Is.EqualTo(1580));
            Assert.That(b.TotalAbonado, Is.EqualTo(tarjeta.LastPagoAmount));
            Assert.That(b.SaldoRestante, Is.EqualTo(tarjeta.verSaldo()));
            Assert.That(b.Fecha, Is.EqualTo(current));
            Assert.That(b.TipoTarjeta, Is.EqualTo(tarjeta.Tipo.ToString()));
            Assert.That(b.TarjetaId, Is.EqualTo(tarjeta.Id));
=======
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
            Assert.IsTrue(pago);
            Assert.That(tarjeta.verSaldo(), Is.EqualTo(56000)); 
            Assert.That(tarjeta.PendienteAcreditar, Is.EqualTo(8000));
>>>>>>> ac7766337da967922b1f3735c45af0891c579266
        }
    }
}