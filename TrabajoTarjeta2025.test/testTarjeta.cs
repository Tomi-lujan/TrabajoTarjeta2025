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

        // ========== TESTS BÁSICOS DE TARJETA NORMAL ==========

        [Test]
        public void TestRecargaValida()
        {
            int saldoEsperado = 40000;
            int resultado = (int)t.recargar(30000);
            Assert.That(resultado, Is.EqualTo(saldoEsperado));
            Assert.That(t.verSaldo(), Is.EqualTo(saldoEsperado));
        }

        [Test]
        public void TestRecargaInvalidaMontoNoPermitido()
        {
            int resultado = (int)t.recargar(1234);
            Assert.That(resultado, Is.EqualTo(0));
            Assert.That(t.verSaldo(), Is.EqualTo(10000));
        }

        [Test]
        public void TestRecargaExcedeLimite()
        {
            t = new Tarjeta(39000, 40000);
            int resultado = (int)t.recargar(3000);
            Assert.That(resultado, Is.EqualTo(42000));
            Assert.That(t.verSaldo(), Is.EqualTo(42000));
        }

        [Test]
        public void TestPagar_SaldoSuficiente()
        {
            bool resultado = t.pagar(5000);
            Assert.That(resultado, Is.True);
            Assert.That(t.verSaldo(), Is.EqualTo(5000));
        }

        [Test]
        public void TestPagar_SaldoInsuficiente_SuperaLimiteMaximo()
        {
            t = new Tarjeta(1000, 40000);
            bool resultado = t.pagar(3000);
            Assert.That(resultado, Is.False);
            Assert.That(t.verSaldo(), Is.EqualTo(1000));
        }

        [Test]
        public void TestPagar_ExactamenteEnLimiteMaximo()
        {
            t = new Tarjeta(1000, 40000);
            bool resultado = t.pagar(2200);
            Assert.That(resultado, Is.True);
            Assert.That(t.verSaldo(), Is.EqualTo(-1200));
        }

        [Test]
        public void TestVerSaldo()
        {
            int saldo = (int)t.verSaldo();
            Assert.That(saldo, Is.EqualTo(10000));
        }

        [Test]
        public void TestRecarga_TodosMontosAceptados()
        {
            decimal[] montosValidos = { 2000m, 3000m, 4000m, 5000m, 8000m, 10000m, 15000m, 20000m, 25000m, 30000m };

            foreach (decimal monto in montosValidos)
            {
                var tarjetaTest = new Tarjeta(0, 50000);
                decimal resultado = tarjetaTest.recargar(monto);
                Assert.That(resultado, Is.EqualTo(monto), $"Fallo con monto: {monto}");
            }
        }

        [Test]
        public void TestRecarga_MontoExactamenteEnLimite()
        {
            t = new Tarjeta(20000, 40000);
            int resultado = (int)t.recargar(20000);
            Assert.That(resultado, Is.EqualTo(40000));
            Assert.That(t.verSaldo(), Is.EqualTo(40000));
        }

        // ========== TESTS DE FRANQUICIAS - FRANJA HORARIA ==========

        [Test]
        public void TestMedioBoleto_NoPermitePagoFueraDeFranjaHoraria_FinDeSemana()
        {
            // Sábado a las 10:00 (fuera de franja)
            var saturday = new DateTime(2024, 1, 6, 10, 0, 0); // Sábado
            var tarjeta = new MedioBoleto(1000m, -1200m, () => saturday);

            bool resultado = tarjeta.pagar(100m);
            Assert.That(resultado, Is.False);
            Assert.That(tarjeta.LastPagoAmount, Is.EqualTo(0m));
        }

        [Test]
        public void TestMedioBoleto_NoPermitePagoFueraDeFranjaHoraria_Noche()
        {
            // Lunes a las 23:00 (fuera de franja)
            var mondayNight = new DateTime(2024, 1, 1, 23, 0, 0); // Lunes
            var tarjeta = new MedioBoleto(1000m, -1200m, () => mondayNight);

            bool resultado = tarjeta.pagar(100m);
            Assert.That(resultado, Is.False);
        }

        [Test]
        public void TestMedioBoleto_PermitePagoDentroDeFranjaHoraria()
        {
            // Martes a las 15:00 (dentro de franja)
            var tuesdayAfternoon = new DateTime(2024, 1, 2, 15, 0, 0); // Martes
            var tarjeta = new MedioBoleto(1000m, -1200m, () => tuesdayAfternoon);

            bool resultado = tarjeta.pagar(100m);
            Assert.That(resultado, Is.True);
            Assert.That(tarjeta.LastPagoAmount, Is.EqualTo(50m)); // Medio boleto
        }

        [Test]
        public void TestBoletoGratuito_NoPermitePagoFueraDeFranjaHoraria_Madrugada()
        {
            // Viernes a las 5:00 (fuera de franja)
            var fridayEarly = new DateTime(2024, 1, 5, 5, 0, 0); // Viernes
            var tarjeta = new BoletoGratuito(1000m, -1200m, () => fridayEarly);

            bool resultado = tarjeta.pagar(100m);
            Assert.That(resultado, Is.False);
        }

        [Test]
        public void TestBoletoGratuito_PermitePagoDentroDeFranjaHoraria()
        {
            // Jueves a las 10:00 (dentro de franja)
            var thursdayMorning = new DateTime(2024, 1, 4, 10, 0, 0); // Jueves
            var tarjeta = new BoletoGratuito(1000m, -1200m, () => thursdayMorning);

            bool resultado = tarjeta.pagar(100m);
            Assert.That(resultado, Is.True);
            Assert.That(tarjeta.LastPagoAmount, Is.EqualTo(0m)); // Gratuito
        }

        [Test]
        public void TestFranquiciaCompleta_NoPermitePagoFueraDeFranjaHoraria_Domingo()
        {
            // Domingo a las 14:00 (fuera de franja)
            var sunday = new DateTime(2024, 1, 7, 14, 0, 0); // Domingo
            var tarjeta = new FranquiciaCompleta(1000m, -1200m, () => sunday);

            bool resultado = tarjeta.pagar(100m);
            Assert.That(resultado, Is.False);
        }

        [Test]
        public void TestFranquiciaCompleta_PermitePagoDentroDeFranjaHoraria()
        {
            // Miércoles a las 18:00 (dentro de franja)
            var wednesdayEvening = new DateTime(2024, 1, 3, 18, 0, 0); // Miércoles
            var tarjeta = new FranquiciaCompleta(1000m, -1200m, () => wednesdayEvening);

            bool resultado = tarjeta.pagar(100m);
            Assert.That(resultado, Is.True);
            Assert.That(tarjeta.LastPagoAmount, Is.EqualTo(0m)); // Gratuito
        }

        [Test]
        public void TestTarjetaNormal_PermitePagoFueraDeFranjaHoraria()
        {
            // Domingo a las 23:00 (fuera de franja para franquicias)
            var sundayNight = new DateTime(2024, 1, 7, 23, 0, 0); // Domingo
            var tarjeta = new Tarjeta(1000m, -1200m);

            bool resultado = tarjeta.pagar(100m);
            Assert.That(resultado, Is.True);
            Assert.That(tarjeta.LastPagoAmount, Is.EqualTo(100m));
        }

        // ========== TESTS DE LÍMITES DE HORARIO ==========

        [Test]
        public void TestFranjaHoraria_LimitesInferiorYSuperior_Incluidos()
        {
            // Test límite inferior (6:00) - debe permitir
            var limiteInferior = new DateTime(2024, 1, 1, 6, 0, 0); // Lunes 6:00
            var tarjeta = new MedioBoleto(1000m, -1200m, () => limiteInferior);
            Assert.That(tarjeta.pagar(100m), Is.True);

            // Test límite superior (22:00) - debe permitir
            var limiteSuperior = new DateTime(2024, 1, 1, 22, 0, 0); // Lunes 22:00
            tarjeta = new MedioBoleto(1000m, -1200m, () => limiteSuperior);
            Assert.That(tarjeta.pagar(100m), Is.True);

            // Test justo antes (5:59) - no debe permitir
            var justoAntes = new DateTime(2024, 1, 1, 5, 59, 59); // Lunes 5:59
            tarjeta = new MedioBoleto(1000m, -1200m, () => justoAntes);
            Assert.That(tarjeta.pagar(100m), Is.False);

            // Test justo después (22:01) - no debe permitir
            var justoDespues = new DateTime(2024, 1, 1, 22, 1, 0); // Lunes 22:01
            tarjeta = new MedioBoleto(1000m, -1200m, () => justoDespues);
            Assert.That(tarjeta.pagar(100m), Is.False);
        }

        // ========== TESTS DE MEDIO BOLETO ==========

        [Test]
        public void TestMedioBoleto_Pagar()
        {
            // Usar horario dentro de franja (Lunes a viernes 6-22)
            DateTime current = new DateTime(2025, 10, 13, 10, 0, 0); // Lunes dentro de franja
            var medioBoleto = new MedioBoleto(10000, 40000, () => current);

            bool resultado = medioBoleto.pagar(2000);
            Assert.That(resultado, Is.True);
            Assert.That(medioBoleto.verSaldo(), Is.EqualTo(9000)); // 10000 - 1000 (mitad de 2000)
            Assert.That(medioBoleto.LastPagoAmount, Is.EqualTo(1000m)); // Verificar que cobró la mitad
        }

        [Test]
        public void TestMedioBoleto_PagarConPrecioImpar()
        {
            DateTime current = new DateTime(2025, 10, 13, 10, 0, 0); // Lunes dentro de franja
            var medioBoleto = new MedioBoleto(10000, 40000, () => current);

            bool resultado = medioBoleto.pagar(3000); // 3000 / 2 = 1500 (floor)
            Assert.That(resultado, Is.True);
            Assert.That(medioBoleto.verSaldo(), Is.EqualTo(8500)); // 10000 - 1500
            Assert.That(medioBoleto.LastPagoAmount, Is.EqualTo(1500m));
        }

        [Test]
        public void TestMedioBoleto_NoPermiteViajarEnMenosDe5Minutos()
        {
            DateTime current = new DateTime(2025, 10, 13, 10, 0, 0); // Lunes dentro de franja
            var medio = new MedioBoleto(10000, 40000, () => current);

            // Primer viaje exitoso
            bool r1 = medio.pagar(1580);
            Assert.That(r1, Is.True);
            Assert.That(medio.LastPagoAmount, Is.EqualTo(790m)); // Mitad

            // Avanzamos 3 minutos: menos de 5 => no se permite
            current = current.AddMinutes(3);
            bool r2 = medio.pagar(1580);
            Assert.That(r2, Is.False, "No debería permitir viajar en menos de 5 minutos");
            Assert.That(medio.verSaldo(), Is.EqualTo(9210m)); // Saldo no cambia
        }

        [Test]
        public void TestMedioBoleto_MaximoDosViajesPorDia_TercerViajeCobradoCompleto()
        {
            DateTime current = new DateTime(2025, 10, 13, 8, 0, 0); // Lunes dentro de franja
            var medio = new MedioBoleto(10000, 40000, () => current);

            // Primer viaje - medio boleto
            bool v1 = medio.pagar(1580);
            Assert.That(v1, Is.True);
            Assert.That(medio.LastPagoAmount, Is.EqualTo(790m));
            current = current.AddMinutes(10);

            // Segundo viaje - medio boleto
            bool v2 = medio.pagar(1580);
            Assert.That(v2, Is.True);
            Assert.That(medio.LastPagoAmount, Is.EqualTo(790m));
            current = current.AddMinutes(10);

            // Tercer viaje del mismo día -> cobra tarifa completa 1580
            bool v3 = medio.pagar(1580);
            Assert.That(v3, Is.True);
            Assert.That(medio.LastPagoAmount, Is.EqualTo(1580m));

            decimal esperado = 10000m - 790m - 790m - 1580m;
            Assert.That(medio.verSaldo(), Is.EqualTo(esperado));
        }

        [Test]
        public void TestMedioBoleto_ContadorDiarioSeReiniciaAlDiaSiguiente()
        {
            DateTime current = new DateTime(2025, 10, 13, 20, 0, 0); // Lunes dentro de franja
            var medio = new MedioBoleto(10000, 40000, () => current);

            // Dos viajes en un día (ambos medio boleto)
            medio.pagar(1580);
            current = current.AddMinutes(10);
            medio.pagar(1580);
            decimal saldoDespuesDosViajes = medio.verSaldo();

            // Tercer viaje mismo día - cobra completo
            current = current.AddMinutes(10);
            medio.pagar(1580);
            decimal saldoDia1 = medio.verSaldo();

            // Siguiente día a las 10:00 (dentro de franja) - contador se reinicia
            current = new DateTime(2025, 10, 14, 10, 0, 0);
            medio.pagar(1580); // Primer viaje del nuevo día - medio boleto
            decimal saldoDespuesNuevoDia = medio.verSaldo();

            // Verificar que se aplicó medio boleto (790 en lugar de 1580)
            Assert.That(saldoDespuesNuevoDia, Is.EqualTo(saldoDia1 - 790m));
            Assert.That(medio.LastPagoAmount, Is.EqualTo(790m));
        }

        // ========== TESTS DE BOLETO GRATUITO ==========

        [Test]
        public void TestBoletoGratuito_Pagar()
        {
            DateTime current = new DateTime(2025, 10, 13, 12, 0, 0); // Lunes dentro de franja
            var gratuito = new BoletoGratuito(10000, 40000, () => current);

            bool resultado = gratuito.pagar(5000);
            Assert.That(resultado, Is.True);
            Assert.That(gratuito.verSaldo(), Is.EqualTo(10000)); // Saldo no cambia
            Assert.That(gratuito.LastPagoAmount, Is.EqualTo(0m)); // No se cobra nada
        }

        [Test]
        public void TestBoletoGratuito_MaximoDosGratis_PosterioresCobrados()
        {
            DateTime current = new DateTime(2025, 10, 13, 9, 0, 0); // Lunes dentro de franja
            var gratuito = new BoletoGratuito(10000, 40000, () => current);

            // Primer viaje gratuito
            Assert.That(gratuito.pagar(1580), Is.True);
            Assert.That(gratuito.LastPagoAmount, Is.EqualTo(0m));
            current = current.AddMinutes(10);

            // Segundo viaje gratuito
            Assert.That(gratuito.pagar(1580), Is.True);
            Assert.That(gratuito.LastPagoAmount, Is.EqualTo(0m));
            current = current.AddMinutes(10);

            // Tercer viaje: debe cobrarse tarifa completa
            bool t3 = gratuito.pagar(1580);
            Assert.That(t3, Is.True);
            Assert.That(gratuito.LastPagoAmount, Is.EqualTo(1580m));
            Assert.That(gratuito.verSaldo(), Is.EqualTo(10000 - 1580));
        }

        [Test]
        public void TestBoletoGratuito_ContadorDiarioSeReinicia()
        {
            DateTime current = new DateTime(2025, 10, 18, 10, 0, 0);
            Func<DateTime> clock = () => current;
            var gratuito = new BoletoGratuito(10000, 40000, clock);

            // Dos viajes gratuitos en un día
            gratuito.pagar(1580);
            current = current.AddMinutes(10);
            gratuito.pagar(1580);
            current = current.AddMinutes(10);

            // Tercer viaje mismo día - paga completo
            gratuito.pagar(1580);
            decimal saldoDia1 = gratuito.verSaldo();

            // Siguiente día - primeros dos viajes son gratuitos otra vez
            current = current.AddDays(1);
            gratuito.pagar(1580); // Debería ser gratuito
            decimal saldoDespuesGratuito = gratuito.verSaldo();

            Assert.That(saldoDespuesGratuito, Is.EqualTo(saldoDia1)); // Saldo no cambia
        }

        // ========== TESTS DE FRANQUICIA COMPLETA ==========

        [Test]
        public void TestFranquiciaCompleta_Pagar()
        {
            DateTime current = new DateTime(2025, 10, 13, 12, 0, 0); // Lunes dentro de franja
            var franquicia = new FranquiciaCompleta(10000, 40000, () => current);

            bool resultado = franquicia.pagar(5000);
            Assert.That(resultado, Is.True);
            Assert.That(franquicia.verSaldo(), Is.EqualTo(10000)); // Saldo no cambia
            Assert.That(franquicia.LastPagoAmount, Is.EqualTo(0m)); // No se cobra nada
        }

        // ========== TESTS DE CONSTRUCTOR Y ESTADO INICIAL ==========

        [Test]
        public void TestConstructor_ValoresIniciales()
        {
            var tarjetaNueva = new Tarjeta(5000, 30000);
            Assert.That(tarjetaNueva.verSaldo(), Is.EqualTo(5000));
            Assert.That(tarjetaNueva.PendienteAcreditar, Is.EqualTo(0m));
            Assert.That(tarjetaNueva.Tipo, Is.EqualTo(TarjetaTipo.Normal));
        }

        [Test]
        public void TestIDsUnicos()
        {
            var tarjeta1 = new Tarjeta(1000, 10000);
            var tarjeta2 = new Tarjeta(1000, 10000);
            var tarjeta3 = new MedioBoleto(1000, 10000);

            Assert.That(tarjeta2.Id, Is.EqualTo(tarjeta1.Id + 1));
            Assert.That(tarjeta3.Id, Is.EqualTo(tarjeta2.Id + 1));
        }

        // ========== TESTS DE ACREDITACIÓN DE CARGA ==========

        [Test]
        public void Recarga_Que_Supera_Maximo_Acredita_Hasta_Maximo_Y_Deja_Pendiente()
        {
            var tarjeta = new Tarjeta(55000, 100000);
            int resultado = (int)tarjeta.recargar(10000);
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

        [Test]
        public void TestAcreditarCarga_SinPendiente()
        {
            var tarjeta = new Tarjeta(10000, 40000);
            decimal acreditado = tarjeta.AcreditarCarga();
            Assert.That(acreditado, Is.EqualTo(0m));
        }

        [Test]
        public void TestAcreditarCarga_ConPendienteYEspacio()
        {
            var tarjeta = new Tarjeta(55000, 100000);

            // Recarga que supera el máximo - usar monto aceptado
            tarjeta.recargar(2000m);
            // 55000 + 1000 = 56000 (máximo), 1000 pendiente
            // Cálculo: 56000 - 55000 = 1000 espacio disponible
            // 2000 - 1000 = 1000 pendiente
            Assert.That(tarjeta.verSaldo(), Is.EqualTo(56000m));
            Assert.That(tarjeta.PendienteAcreditar, Is.EqualTo(1000m));

            // Hacer un pago para liberar espacio
            tarjeta.pagar(1000); // Saldo: 56000 - 1000 = 55000

        }
        // ========== TESTS DE USO FRECUENTE ==========

        [Test]
        public void TestUsoFrecuente_Tramo30_20porciento()
        {
            DateTime now = new DateTime(2025, 10, 1, 9, 0, 0);
            var tarjeta = new Tarjeta(100000m, 200000m);

            // Realizar 29 viajes (viajes 1-29 sin descuento)
            for (int i = 1; i <= 29; i++)
            {
                tarjeta.pagar(1000m, now.AddMinutes(i * 10));
                Assert.That(tarjeta.LastPagoAmount, Is.EqualTo(1000m), $"Viaje {i} debería ser 1000");
            }

            // Viaje 30 => 20% de descuento => 800
            tarjeta.pagar(1000m, now.AddMinutes(300));
            Assert.That(tarjeta.LastPagoAmount, Is.EqualTo(800m), "Viaje 30 debería tener 20% descuento");
        }

        [Test]
        public void TestUsoFrecuente_Tramo60_25porciento()
        {
            DateTime now = new DateTime(2025, 10, 1, 9, 0, 0);
            var tarjeta = new Tarjeta(100000m, 200000m);

            // 59 viajes para llegar al 60 luego
            for (int i = 1; i <= 59; i++)
            {
                tarjeta.pagar(1000m, now.AddMinutes(i * 5));
            }

            // Viaje 60 => 25% de descuento => 750
            tarjeta.pagar(1000m, now.AddMinutes(300));
            Assert.That(tarjeta.LastPagoAmount, Is.EqualTo(750m), "Viaje 60 debería tener 25% descuento");
        }

        [Test]
        public void TestUsoFrecuente_Tramo81_VuelveNormal()
        {
            DateTime now = new DateTime(2025, 10, 1, 9, 0, 0);
            var tarjeta = new Tarjeta(100000m, 200000m);

            // 80 viajes - usar pagar directamente para asegurar el descuento
            for (int i = 1; i <= 80; i++)
            {
                tarjeta.pagar(1000m, now.AddMinutes(i * 5));

                // Verificar que aplica descuentos en los tramos correspondientes
                if (i >= 30 && i <= 59)
                {
                    Assert.That(tarjeta.LastPagoAmount, Is.EqualTo(800m), $"Viaje {i} debería tener 20% descuento");
                }
                else if (i >= 60 && i <= 80)
                {
                    Assert.That(tarjeta.LastPagoAmount, Is.EqualTo(750m), $"Viaje {i} debería tener 25% descuento");
                }
            }

            // viaje 81 => vuelve a tarifa normal => 1000
            tarjeta.pagar(1000m, now.AddMinutes(405));
            Assert.That(tarjeta.LastPagoAmount, Is.EqualTo(1000m), "Viaje 81 debería volver a tarifa normal");
        }

        [Test]
        public void TestUsoFrecuente_ContadorSeReiniciaEnNuevoMes()
        {
            DateTime now = new DateTime(2025, 10, 31, 9, 0, 0); // Último día del mes
            var tarjeta = new Tarjeta(100000m, 200000m);

            // 40 viajes en octubre - usar pagar directamente para mejor control
            for (int i = 1; i <= 40; i++)
            {
                tarjeta.pagar(1000m, now.AddMinutes(i * 10));

                // Verificar que los viajes 30-40 tienen descuento del 20%
                if (i >= 30 && i <= 40)
                {
                    Assert.That(tarjeta.LastPagoAmount, Is.EqualTo(800m),
                        $"Viaje {i} en octubre debería tener 20% descuento (800), pero fue {tarjeta.LastPagoAmount}");
                }
            }

            // El último viaje (40) en octubre debería tener descuento
            Assert.That(tarjeta.LastPagoAmount, Is.EqualTo(800m),
                "Último viaje de octubre debería mantener descuento");

            // Primer día del mes siguiente - contador se reinicia
            DateTime nuevoMes = new DateTime(2025, 11, 1, 9, 0, 0);
            tarjeta.pagar(1000m, nuevoMes);

            // Debería cobrar tarifa normal (primer viaje del mes, sin descuento)
            Assert.That(tarjeta.LastPagoAmount, Is.EqualTo(1000m),
                "Primer viaje de noviembre debería ser tarifa normal (reinicio mensual)");

            // Verificar que el segundo viaje del nuevo mes también es normal
            tarjeta.pagar(1000m, nuevoMes.AddMinutes(10));
            Assert.That(tarjeta.LastPagoAmount, Is.EqualTo(1000m),
                "Segundo viaje de noviembre debería ser tarifa normal");

            // Hacer 29 viajes más en noviembre para llegar al viaje 30
            for (int i = 3; i <= 30; i++)
            {
                tarjeta.pagar(1000m, nuevoMes.AddMinutes(i * 10));

                // Los primeros 29 viajes del mes deberían ser tarifa normal
                if (i < 30)
                {
                    Assert.That(tarjeta.LastPagoAmount, Is.EqualTo(1000m),
                        $"Viaje {i} de noviembre debería ser tarifa normal");
                }
            }

            // Viaje 30 de noviembre debería tener descuento del 20%
            Assert.That(tarjeta.LastPagoAmount, Is.EqualTo(800m),
                "Viaje 30 de noviembre debería tener 20% descuento");
        }

        // ========== TESTS DE TRASBORDO ==========

        [Test]
        public void TestTrasbordo_Libre_EnUnaHora_LineasDistintas_HorarioPermitido()
        {
            DateTime current = new DateTime(2025, 10, 13, 10, 0, 0); // Lunes
            Func<DateTime> clock = () => current;
            var colectivo = new Colectivo();
            var tarjeta = new Tarjeta(100000m, 200000m);

            // primer boleto: linea 1 -> paga
            var b1 = colectivo.EmitirBoleto(tarjeta, linea: 1, tarifa: 1000m, nowProvider: clock);
            Assert.That(b1, Is.Not.Null);
            Assert.That(b1!.TotalAbonado, Is.EqualTo(1000m));

            // avanzar 30 minutos, otra linea distinta -> trasbordo libre
            current = current.AddMinutes(30);
            var b2 = colectivo.EmitirBoleto(tarjeta, linea: 2, tarifa: 1000m, nowProvider: clock);
            Assert.That(b2, Is.Not.Null);
            Assert.That(b2!.IsTrasbordo, Is.True);
            Assert.That(b2.TotalAbonado, Is.EqualTo(0m));
            Assert.That(b2.TrasbordoPagado, Is.False);
        }

        [Test]
        public void TestTrasbordo_MismoLinea_NoEsLibre()
        {
            DateTime current = new DateTime(2025, 10, 13, 11, 0, 0); // Lunes
            Func<DateTime> clock = () => current;
            var colectivo = new Colectivo();
            var tarjeta = new Tarjeta(100000m, 200000m);

            var b1 = colectivo.EmitirBoleto(tarjeta, linea: 5, tarifa: 1000m, nowProvider: clock);
            Assert.That(b1, Is.Not.Null);
            Assert.That(b1!.TotalAbonado, Is.EqualTo(1000m));

            current = current.AddMinutes(20);
            var b2 = colectivo.EmitirBoleto(tarjeta, linea: 5, tarifa: 1000m, nowProvider: clock);
            Assert.That(b2, Is.Not.Null);
            Assert.That(b2.IsTrasbordo, Is.False);
            Assert.That(b2.TotalAbonado, Is.EqualTo(1000m));
            Assert.That(b2.TrasbordoPagado, Is.False);
        }

        [Test]
        public void TestTrasbordo_FueraHorario_NoEsLibre()
        {
            DateTime current = new DateTime(2025, 10, 12, 10, 0, 0); // Domingo
            Func<DateTime> clock = () => current;
            var colectivo = new Colectivo();
            var tarjeta = new Tarjeta(100000m, 200000m);

            var b1 = colectivo.EmitirBoleto(tarjeta, linea: 10, tarifa: 1000m, nowProvider: clock);
            Assert.That(b1, Is.Not.Null);
            Assert.That(b1!.TotalAbonado, Is.EqualTo(1000m));

            current = current.AddMinutes(30);
            var b2 = colectivo.EmitirBoleto(tarjeta, linea: 11, tarifa: 1000m, nowProvider: clock);
            Assert.That(b2, Is.Not.Null);
            Assert.That(b2.IsTrasbordo, Is.False);
            Assert.That(b2.TotalAbonado, Is.EqualTo(1000m));
        }

        [Test]
        public void TestTrasbordo_ExcedeUnaHora_NoEsLibre()
        {
            DateTime current = new DateTime(2025, 10, 13, 10, 0, 0); // Lunes
            Func<DateTime> clock = () => current;
            var colectivo = new Colectivo();
            var tarjeta = new Tarjeta(100000m, 200000m);

            var b1 = colectivo.EmitirBoleto(tarjeta, linea: 1, tarifa: 1000m, nowProvider: clock);
            Assert.That(b1, Is.Not.Null);

            // Avanzar más de 1 hora
            current = current.AddMinutes(61);
            var b2 = colectivo.EmitirBoleto(tarjeta, linea: 2, tarifa: 1000m, nowProvider: clock);
            Assert.That(b2, Is.Not.Null);
            Assert.That(b2.IsTrasbordo, Is.False);
            Assert.That(b2.TotalAbonado, Is.EqualTo(1000m));
        }

        // ========== TESTS DE BOLETO ==========

        [Test]
        public void TestEmitirBoleto_ContieneDatosEsperados()
        {
            DateTime current = new DateTime(2025, 10, 18, 12, 0, 0);
            Func<DateTime> clock = () => current;
            var tarjeta = new Tarjeta(10000, 40000);
            var colectivo = new Colectivo();

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

        // ========== TESTS DE CASOS BORDE ==========

        [Test]
        public void TestPagar_Cero()
        {
            bool resultado = t.pagar(0m);
            Assert.That(resultado, Is.True);
            Assert.That(t.verSaldo(), Is.EqualTo(10000m));
        }

        [Test]
        public void TestRecarga_Cero()
        {
            decimal resultado = t.recargar(0m);
            Assert.That(resultado, Is.EqualTo(0m)); // Monto no aceptado
        }

        [Test]
        public void TestSaldoMaximo_NoPermiteSuperarlo()
        {
            var tarjeta = new Tarjeta(55900m, 100000m); // 100 menos que el máximo (56000)

            // Montos aceptados: el más cercano es 2000, pero probemos con 200 (no aceptado)
            // Para este test necesitamos usar un monto aceptado
            decimal resultado = tarjeta.recargar(2000m); // Intenta cargar 2000

            // 55900 + 2000 = 57900, pero máximo es 56000
            // Solo puede cargar 100 para llegar al máximo (56000 - 55900 = 100)
            // 1900 quedan pendientes
            Assert.That(resultado, Is.EqualTo(56000m)); // Llega exactamente al máximo
            Assert.That(tarjeta.verSaldo(), Is.EqualTo(56000m));
            Assert.That(tarjeta.PendienteAcreditar, Is.EqualTo(1900m)); // 2000 - 100 = 1900 pendientes
        }

        [Test]
        public void TestMultipleRecargas_PendienteSeAcumula()
        {
            var tarjeta = new Tarjeta(55000m, 100000m);
            tarjeta.recargar(5000m); // Llena hasta 56000, pendiente: 4000
            tarjeta.recargar(3000m); // Pendiente adicional: 3000

            Assert.That(tarjeta.verSaldo(), Is.EqualTo(56000m));
            Assert.That(tarjeta.PendienteAcreditar, Is.EqualTo(7000m));
        }
        // ========== TESTS PARA AUMENTAR COVERAGE - CASOS BORDE Y COMBINACIONES ==========

        [Test]
        public void TestTrasbordo_ExactamenteEnLimite60Minutos_DeberiaSerLibre()
        {
            DateTime current = new DateTime(2025, 10, 13, 10, 0, 0); // Lunes
            var colectivo = new Colectivo();
            var tarjeta = new Tarjeta(10000m, 40000m);

            // Primer boleto
            var b1 = colectivo.EmitirBoleto(tarjeta, linea: 1, tarifa: 1000m, () => current);
            Assert.That(b1, Is.Not.Null);

            // Exactamente en el límite de 60 minutos
            current = current.AddMinutes(60);
            var b2 = colectivo.EmitirBoleto(tarjeta, linea: 2, tarifa: 1000m, () => current);

            Assert.That(b2, Is.Not.Null);
            Assert.That(b2!.IsTrasbordo, Is.True, "Debería ser trasbordo en exactamente 60 minutos");
            Assert.That(b2.TotalAbonado, Is.EqualTo(0m));
        }

        [Test]
        public void TestTrasbordo_UnSegundoDespuesDe60Minutos_NoDeberiaSerLibre()
        {
            DateTime current = new DateTime(2025, 10, 13, 10, 0, 0);
            var colectivo = new Colectivo();
            var tarjeta = new Tarjeta(10000m, 40000m);

            var b1 = colectivo.EmitirBoleto(tarjeta, linea: 1, tarifa: 1000m, () => current);
            Assert.That(b1, Is.Not.Null);

            // 60 minutos y 1 segundo - fuera de ventana
            current = current.AddMinutes(60).AddSeconds(1);
            var b2 = colectivo.EmitirBoleto(tarjeta, linea: 2, tarifa: 1000m, () => current);

            Assert.That(b2, Is.Not.Null);
            Assert.That(b2!.IsTrasbordo, Is.False, "No debería ser trasbordo después de 60 minutos");
            Assert.That(b2.TotalAbonado, Is.EqualTo(1000m));
        }

        [Test]
        public void TestFranjaHoraria_LimitesExactos_Inclusivo()
        {
            // Test límite inferior exacto (6:00:00)
            var limiteInferior = new DateTime(2024, 1, 1, 6, 0, 0); // Lunes 6:00 exacto
            var tarjetaMedio = new MedioBoleto(1000m, -1200m, () => limiteInferior);
            Assert.That(tarjetaMedio.pagar(100m), Is.True, "Debería permitir en 6:00:00 exacto");

            // Test límite superior exacto (22:00:00)
            var limiteSuperior = new DateTime(2024, 1, 1, 22, 0, 0); // Lunes 22:00 exacto
            var tarjetaGratuito = new BoletoGratuito(1000m, -1200m, () => limiteSuperior);
            Assert.That(tarjetaGratuito.pagar(100m), Is.True, "Debería permitir en 22:00:00 exacto");
        }

        [Test]
        public void TestUsoFrecuente_Viaje29_SinDescuento_Viaje30_ConDescuento()
        {
            DateTime now = new DateTime(2025, 10, 1, 9, 0, 0);
            var tarjeta = new Tarjeta(100000m, 200000m);

            // Viaje 29 - sin descuento
            for (int i = 1; i <= 28; i++)
            {
                tarjeta.pagar(1000m, now.AddMinutes(i * 10));
            }

            // Viaje 29 - verificar que no tiene descuento
            tarjeta.pagar(1000m, now.AddMinutes(290));
            Assert.That(tarjeta.LastPagoAmount, Is.EqualTo(1000m), "Viaje 29 debería ser sin descuento");

            // Viaje 30 - con descuento del 20%
            tarjeta.pagar(1000m, now.AddMinutes(300));
            Assert.That(tarjeta.LastPagoAmount, Is.EqualTo(800m), "Viaje 30 debería tener 20% descuento");
        }

        [Test]
        public void TestMedioBoleto_IntervaloExacto5Minutos_PermiteViajar()
        {
            DateTime current = new DateTime(2025, 10, 13, 10, 0, 0);
            var medio = new MedioBoleto(10000, 40000, () => current);

            // Primer viaje
            bool r1 = medio.pagar(1580);
            Assert.That(r1, Is.True);

            // Exactamente 5 minutos después - debería permitir
            current = current.AddMinutes(5);
            bool r2 = medio.pagar(1580);

            Assert.That(r2, Is.True, "Debería permitir viajar exactamente a los 5 minutos");
            Assert.That(medio.LastPagoAmount, Is.EqualTo(790m));
        }

        [Test]
        public void TestBoletoGratuito_TercerViajeMismoDia_PagaCompleto_CuartoViajeSiguePagandoCompleto()
        {
            DateTime current = new DateTime(2025, 10, 13, 9, 0, 0);
            var gratuito = new BoletoGratuito(10000, 40000, () => current);

            // Dos viajes gratuitos
            gratuito.pagar(1580);
            current = current.AddMinutes(10);
            gratuito.pagar(1580);

            // Tercer viaje - paga completo
            current = current.AddMinutes(10);
            gratuito.pagar(1580);
            Assert.That(gratuito.LastPagoAmount, Is.EqualTo(1580m), "Tercer viaje debería pagar completo");

            // Cuarto viaje - sigue pagando completo
            current = current.AddMinutes(10);
            gratuito.pagar(1580);
            Assert.That(gratuito.LastPagoAmount, Is.EqualTo(1580m), "Cuarto viaje debería seguir pagando completo");

            decimal saldoEsperado = 10000m - 1580m - 1580m;
            Assert.That(gratuito.verSaldo(), Is.EqualTo(saldoEsperado));
        }

        [Test]
        public void TestColectivo_EmitirBoleto_ConTarjetaSinSaldoSuficiente_DevuelveNull()
        {
            var colectivo = new Colectivo();
            DateTime now = new DateTime(2025, 10, 13, 10, 0, 0);

            // Tarjeta con saldo muy bajo y límite máximo
            var tarjeta = new Tarjeta(1000m, -1200m); // Solo puede pagar hasta 2200

            // Intentar pagar tarifa que supera el límite
            var boleto = colectivo.EmitirBoleto(tarjeta, linea: 123, tarifa: 2500m, () => now);

            Assert.That(boleto, Is.Null, "No debería emitir boleto cuando supera el límite máximo");
            Assert.That(tarjeta.verSaldo(), Is.EqualTo(1000m), "Saldo no debería cambiar");
        }

        [Test]
        public void TestTrasbordo_ConMedioBoleto_DentroFranja_DeberiaFuncionar()
        {
            DateTime current = new DateTime(2025, 10, 13, 10, 0, 0); // Lunes
            var colectivo = new Colectivo();
            var medioBoleto = new MedioBoleto(10000m, 40000m, () => current);

            // Primer boleto - medio boleto
            var b1 = colectivo.EmitirBoleto(medioBoleto, linea: 1, tarifa: 1580m, () => current);
            Assert.That(b1, Is.Not.Null);
            Assert.That(b1!.TotalAbonado, Is.EqualTo(790m)); // Medio boleto

            // Segundo boleto dentro de ventana - trasbordo libre
            current = current.AddMinutes(30);
            var b2 = colectivo.EmitirBoleto(medioBoleto, linea: 2, tarifa: 1580m, () => current);

            Assert.That(b2, Is.Not.Null);
            Assert.That(b2!.IsTrasbordo, Is.True);
            Assert.That(b2.TotalAbonado, Is.EqualTo(0m)); // Libre
        }

        [Test]
        public void TestCalcularTarifaConDescuento_FueraDeTramos_RetornaTarifaNormal()
        {
            DateTime now = new DateTime(2025, 10, 1, 9, 0, 0);
            var tarjeta = new Tarjeta(100000m, 200000m);

            // Viajes 1-29 sin descuento
            for (int i = 1; i <= 29; i++)
            {
                decimal tarifa = tarjeta.CalcularTarifaConDescuento(1000m, now.AddMinutes(i * 10));
                if (i < 30)
                {
                    Assert.That(tarifa, Is.EqualTo(1000m), $"Viaje {i} debería ser tarifa normal");
                }
            }
        }

        [Test]
        public void TestTransferWindow_SeResetea_SiPrimerViajeNoCobra()
        {
            DateTime current = new DateTime(2025, 10, 13, 10, 0, 0);
            var colectivo = new Colectivo();
            var franquicia = new FranquiciaCompleta(10000m, 40000m, () => current);

            // Primer viaje - gratuito (no cobra)
            var b1 = colectivo.EmitirBoleto(franquicia, linea: 1, tarifa: 1580m, () => current);
            Assert.That(b1, Is.Not.Null);
            Assert.That(b1!.TotalAbonado, Is.EqualTo(0m));

            // Como no se cobró, no debería establecer ventana de trasbordo
            Assert.That(franquicia.TransferWindowStart, Is.Null,
                "No debería establecer ventana si no se cobró en el primer viaje");
            Assert.That(franquicia.TransferBaseLine, Is.Null);

            // Segundo viaje - tampoco debería ser trasbordo
            current = current.AddMinutes(30);
            var b2 = colectivo.EmitirBoleto(franquicia, linea: 2, tarifa: 1580m, () => current);

            Assert.That(b2, Is.Not.Null);
            Assert.That(b2!.IsTrasbordo, Is.False, "No debería ser trasbordo si no hubo pago inicial");
            Assert.That(b2.TotalAbonado, Is.EqualTo(0m));
        }

        [Test]
        public void TestRecarga_MontosAceptados_Individualmente()
        {
            decimal[] montosValidos = { 2000m, 3000m, 4000m, 5000m, 8000m, 10000m, 15000m, 20000m, 25000m, 30000m };

            foreach (decimal monto in montosValidos)
            {
                var tarjeta = new Tarjeta(0m, 50000m);
                decimal resultado = tarjeta.recargar(monto);
                Assert.That(resultado, Is.EqualTo(monto), $"Fallo con monto: {monto}");
            }
        }

        [Test]
        public void TestRecarga_MontosNoAceptados_DevuelveCero()
        {
            decimal[] montosInvalidos = { 0m, 100m, 500m, 1000m, 1234m, 6000m, 7000m, 9000m, 12000m };

            foreach (decimal monto in montosInvalidos)
            {
                var tarjeta = new Tarjeta(10000m, 40000m);
                decimal resultado = tarjeta.recargar(monto);
                Assert.That(resultado, Is.EqualTo(0m), $"Debería rechazar monto: {monto}");
            }
        }
    }
}