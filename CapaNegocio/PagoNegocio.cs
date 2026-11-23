using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using CapaDatos;

namespace CapaNegocio
{
    public class CobroInfo
    {
        public int IdReserva { get; set; }
        public string Cliente { get; set; }
        public string Documento { get; set; }
        public DateTime Entrada { get; set; }
        public DateTime Salida { get; set; }
        public string Habitaciones { get; set; }
        public string Estado { get; set; }
        public decimal Total { get; set; }
        public string Bloqueo { get; set; }
    }

    public class RegistroPagoResultado
    {
        public int IdPago { get; set; }
        public string Error { get; set; }
    }

    public class ComprobanteDetalle
    {
        public int IdPago { get; set; }
        public int IdReserva { get; set; }
        public string Tipo { get; set; }
        public string Serie { get; set; }
        public string Numero { get; set; }
        public DateTime FechaEmision { get; set; }
        public string Cliente { get; set; }
        public string TipoDocumento { get; set; }
        public string Documento { get; set; }
        public DateTime Entrada { get; set; }
        public DateTime Salida { get; set; }
        public int Noches { get; set; }
        public string Metodo { get; set; }
        public decimal Monto { get; set; }
        public DateTime FechaPago { get; set; }
        public List<LineaCotizacion> Lineas { get; set; }

        public ComprobanteDetalle()
        {
            Lineas = new List<LineaCotizacion>();
        }
    }

    public class PagoFila
    {
        public int IdPago { get; set; }
        public int IdReserva { get; set; }
        public DateTime Fecha { get; set; }
        public string Cliente { get; set; }
        public string Metodo { get; set; }
        public string Tipo { get; set; }
        public string Comprobante { get; set; }
        public decimal Monto { get; set; }
    }

    public class HistorialPagos
    {
        public List<PagoFila> Filas { get; set; }
        public decimal Total { get; set; }

        public HistorialPagos()
        {
            Filas = new List<PagoFila>();
        }
    }

    public class PagoNegocio
    {
        public List<ItemCombo> Metodos()
        {
            using (var db = new HotelContext())
            {
                return db.MetodoPago
                    .Where(m => m.Estado)
                    .OrderBy(m => m.IdMetodoPago)
                    .Select(m => new ItemCombo { Id = m.IdMetodoPago, Texto = m.NombreMetodo })
                    .ToList();
            }
        }

        public List<ItemCombo> Tipos()
        {
            using (var db = new HotelContext())
            {
                return db.TipoComprobante
                    .OrderBy(t => t.IdTipoComprobante)
                    .Select(t => new ItemCombo { Id = t.IdTipoComprobante, Texto = t.NombreTipo })
                    .ToList();
            }
        }

        public CobroInfo ParaCobrar(int idReserva)
        {
            using (var db = new HotelContext())
            {
                var r = db.Reserva
                    .Where(x => x.IdReserva == idReserva)
                    .Select(x => new
                    {
                        x.IdReserva,
                        Cliente = x.Cliente.Nombres + " " + x.Cliente.Apellidos,
                        Documento = x.Cliente.NumeroDocumento,
                        x.FechaEntrada,
                        x.FechaSalida,
                        x.MontoTotal,
                        x.IdEstadoReserva,
                        Estado = x.EstadoReserva.NombreEstado,
                        Habs = x.DetalleReserva.Select(d => d.Habitacion.NumeroHabitacion),
                        Pagada = x.Pago.Any()
                    })
                    .FirstOrDefault();

                if (r == null) return null;

                var info = new CobroInfo
                {
                    IdReserva = r.IdReserva,
                    Cliente = r.Cliente,
                    Documento = r.Documento,
                    Entrada = r.FechaEntrada,
                    Salida = r.FechaSalida,
                    Habitaciones = string.Join(", ", r.Habs.OrderBy(h => h)),
                    Estado = r.Estado,
                    Total = r.MontoTotal
                };

                if (r.Pagada)
                    info.Bloqueo = "Esta reserva ya tiene un pago registrado.";
                else if (r.IdEstadoReserva != EstadoReservaId.Confirmada && r.IdEstadoReserva != EstadoReservaId.CheckIn)
                    info.Bloqueo = "Solo se puede cobrar una reserva Confirmada o con check-in. Estado actual: " + r.Estado + ".";

                return info;
            }
        }

        public RegistroPagoResultado Registrar(int idReserva, int idMetodo, int idTipo)
        {
            string serie;
            using (var db = new HotelContext())
            {
                var r = db.Reserva.AsNoTracking().FirstOrDefault(x => x.IdReserva == idReserva);
                if (r == null) return new RegistroPagoResultado { Error = "La reserva no existe." };

                if (r.IdEstadoReserva != EstadoReservaId.Confirmada && r.IdEstadoReserva != EstadoReservaId.CheckIn)
                    return new RegistroPagoResultado { Error = "Solo se puede cobrar una reserva Confirmada o con check-in." };

                if (db.Pago.Any(p => p.IdReserva == idReserva))
                    return new RegistroPagoResultado { Error = "Esta reserva ya tiene un pago registrado." };

                if (r.MontoTotal <= 0)
                    return new RegistroPagoResultado { Error = "La reserva no tiene un monto válido para cobrar." };

                if (!db.MetodoPago.Any(m => m.IdMetodoPago == idMetodo && m.Estado))
                    return new RegistroPagoResultado { Error = "Elige un método de pago válido." };

                var tipo = db.TipoComprobante.FirstOrDefault(t => t.IdTipoComprobante == idTipo);
                if (tipo == null) return new RegistroPagoResultado { Error = "Elige el tipo de comprobante." };

                serie = tipo.NombreTipo == "Factura" ? "F001" : "B001";
            }

            for (int intento = 0; intento < 3; intento++)
            {
                try
                {
                    using (var db = new HotelContext())
                    {
                        string numero = SiguienteNumero(db, serie);

                        db.Database.ExecuteSqlCommand(
                            TransactionalBehavior.DoNotEnsureTransaction,
                            "EXEC sp_RegistrarPago @IdReserva, @IdMetodoPago, @IdTipoComprobante, @Serie, @Numero",
                            new SqlParameter("@IdReserva", idReserva),
                            new SqlParameter("@IdMetodoPago", idMetodo),
                            new SqlParameter("@IdTipoComprobante", idTipo),
                            new SqlParameter("@Serie", SqlDbType.VarChar, 10) { Value = serie },
                            new SqlParameter("@Numero", SqlDbType.VarChar, 15) { Value = numero });

                        int idPago = db.Pago.Where(p => p.IdReserva == idReserva)
                                            .Select(p => p.IdPago).FirstOrDefault();
                        return new RegistroPagoResultado { IdPago = idPago };
                    }
                }
                catch (Exception ex)
                {
                    string msg = MensajeInterno(ex);
                    if (intento < 2 && msg.Contains("UQ_Comprobante_SerieNumero")) continue;
                    return new RegistroPagoResultado { Error = msg };
                }
            }
            return new RegistroPagoResultado { Error = "No se pudo registrar el pago. Inténtalo de nuevo." };
        }

        public HistorialPagos Historial(DateTime? desde, DateTime? hasta, int? idMetodo)
        {
            using (var db = new HotelContext())
            {
                IQueryable<Comprobante> q = db.Comprobante;

                if (desde.HasValue)
                {
                    DateTime d = desde.Value.Date;
                    q = q.Where(c => c.Pago.FechaPago >= d);
                }
                if (hasta.HasValue)
                {
                    DateTime h = hasta.Value.Date.AddDays(1);
                    q = q.Where(c => c.Pago.FechaPago < h);
                }
                if (idMetodo.HasValue)
                    q = q.Where(c => c.Pago.IdMetodoPago == idMetodo.Value);

                var filas = q.OrderByDescending(c => c.Pago.FechaPago)
                    .Select(c => new PagoFila
                    {
                        IdPago = c.IdPago,
                        IdReserva = c.Pago.IdReserva,
                        Fecha = c.Pago.FechaPago,
                        Cliente = c.Pago.Reserva.Cliente.Nombres + " " + c.Pago.Reserva.Cliente.Apellidos,
                        Metodo = c.Pago.MetodoPago.NombreMetodo,
                        Tipo = c.TipoComprobante.NombreTipo,
                        Comprobante = c.Serie + "-" + c.Numero,
                        Monto = c.Pago.Monto
                    })
                    .ToList();

                return new HistorialPagos { Filas = filas, Total = filas.Sum(f => f.Monto) };
            }
        }

        public ComprobanteDetalle Obtener(int idPago, int? soloCliente)
        {
            using (var db = new HotelContext())
            {
                IQueryable<Comprobante> q = db.Comprobante.Where(c => c.IdPago == idPago);
                if (soloCliente.HasValue)
                    q = q.Where(c => c.Pago.Reserva.IdCliente == soloCliente.Value);

                var c0 = q.Select(c => new
                {
                    c.IdPago,
                    IdReserva = c.Pago.IdReserva,
                    Tipo = c.TipoComprobante.NombreTipo,
                    c.Serie,
                    c.Numero,
                    c.FechaEmision,
                    Cliente = c.Pago.Reserva.Cliente.Nombres + " " + c.Pago.Reserva.Cliente.Apellidos,
                    TipoDoc = c.Pago.Reserva.Cliente.TipoDocumento,
                    Doc = c.Pago.Reserva.Cliente.NumeroDocumento,
                    Entrada = c.Pago.Reserva.FechaEntrada,
                    Salida = c.Pago.Reserva.FechaSalida,
                    Metodo = c.Pago.MetodoPago.NombreMetodo,
                    c.Pago.Monto,
                    c.Pago.FechaPago
                }).FirstOrDefault();

                if (c0 == null) return null;

                int noches = (c0.Salida - c0.Entrada).Days;
                var det = new ComprobanteDetalle
                {
                    IdPago = c0.IdPago,
                    IdReserva = c0.IdReserva,
                    Tipo = c0.Tipo,
                    Serie = c0.Serie,
                    Numero = c0.Numero,
                    FechaEmision = c0.FechaEmision,
                    Cliente = c0.Cliente,
                    TipoDocumento = c0.TipoDoc,
                    Documento = c0.Doc,
                    Entrada = c0.Entrada,
                    Salida = c0.Salida,
                    Noches = noches,
                    Metodo = c0.Metodo,
                    Monto = c0.Monto,
                    FechaPago = c0.FechaPago
                };

                var lineas = db.DetalleReserva
                    .Where(d => d.IdReserva == c0.IdReserva)
                    .Select(d => new
                    {
                        d.IdHabitacion,
                        d.Habitacion.NumeroHabitacion,
                        d.Habitacion.TipoHabitacion.NombreTipo,
                        d.PrecioNocheAplicado
                    })
                    .ToList();

                foreach (var l in lineas.OrderBy(x => x.NumeroHabitacion))
                {
                    det.Lineas.Add(new LineaCotizacion
                    {
                        IdHabitacion = l.IdHabitacion,
                        NumeroHabitacion = l.NumeroHabitacion,
                        NombreTipo = l.NombreTipo,
                        PrecioNoche = l.PrecioNocheAplicado,
                        Subtotal = l.PrecioNocheAplicado * noches
                    });
                }
                return det;
            }
        }

        public ComprobanteDetalle ObtenerPorReserva(int idReserva, int idCliente)
        {
            using (var db = new HotelContext())
            {
                int idPago = db.Pago
                    .Where(p => p.IdReserva == idReserva && p.Reserva.IdCliente == idCliente)
                    .Select(p => p.IdPago)
                    .FirstOrDefault();
                if (idPago == 0) return null;
                return Obtener(idPago, idCliente);
            }
        }


        private static string SiguienteNumero(HotelContext db, string serie)
        {
            var existentes = db.Comprobante.Where(c => c.Serie == serie).Select(c => c.Numero).ToList();
            int max = 0;
            foreach (var n in existentes)
            {
                int v;
                if (int.TryParse(n, out v) && v > max) max = v;
            }
            return (max + 1).ToString("D8");
        }

        private static string MensajeInterno(Exception ex)
        {
            while (ex.InnerException != null) ex = ex.InnerException;
            return ex.Message;
        }
    }
}
