using BTR_Services.Models;
using BTR_Services.Persistencia;
using BTR_Services.Persistencia;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace BTR_Services.Api
{
    public class CronogramaController : ApiController
    {
        private readonly IRepositorio _repositorio;

        public CronogramaController(Repositorio repository)
        {
            _repositorio = repository;
        }

        /// <summary>
        /// Obtiene todos los eventos existentes en el sistema.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IQueryable<UtilsJson.ACronograma>> getCronogramaAll()
        {
            IQueryable<UtilsJson.ACronograma> listado = null;
            SqlParameter[] param = new SqlParameter[4];
            param[0] = new SqlParameter("id_institucion", "0");
            param[1] = new SqlParameter("id_evento", "0");
            param[2] = new SqlParameter("id_cronograma", "0");
            param[3] = new SqlParameter("id_conferencista", "0");

            listado = _repositorio.executeStored<UtilsJson.ACronograma>("getListingCronograma", param).Cast<UtilsJson.ACronograma>().AsQueryable<UtilsJson.ACronograma>();

            return listado;
        }

        /// <summary>
        /// Obtiene los eventos asociados a un conferencista.
        /// </summary>
        /// <param name="id_conferencista">id_conferencista.</param>
        /// <returns></returns>
        public async Task<IQueryable<UtilsJson.ACronograma>> getConferencistaId(string id_conferencista)
        {
            IQueryable<UtilsJson.ACronograma> listado = null;
            SqlParameter[] param = new SqlParameter[4];
            param[0] = new SqlParameter("id_institucion", "0");
            param[1] = new SqlParameter("id_evento", "0");
            param[2] = new SqlParameter("id_cronograma", "0");
            param[3] = new SqlParameter("id_conferencista", id_conferencista);

            listado = _repositorio.executeStored<UtilsJson.ACronograma>("getListingCronograma", param).Cast<UtilsJson.ACronograma>().AsQueryable<UtilsJson.ACronograma>();

            return listado;
        }

        /// <summary>
        /// Obtiene todos los eventos asociados a una publicacion tipo evento.
        /// </summary>
        /// <param name="id_evento">id de la publicacion.</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IQueryable<UtilsJson.ACronograma>> getEventoId(string id_evento)
        {
            IQueryable<UtilsJson.ACronograma> listado = null;
            SqlParameter[] param = new SqlParameter[4];
            param[0] = new SqlParameter("id_institucion", "0");
            param[1] = new SqlParameter("id_evento", id_evento);
            param[2] = new SqlParameter("id_cronograma", "0");
            param[3] = new SqlParameter("id_conferencista", "0");

            listado = _repositorio.executeStored<UtilsJson.ACronograma>("getListingCronograma", param).Cast<UtilsJson.ACronograma>().AsQueryable<UtilsJson.ACronograma>();

            return listado;
        }

        /// <summary>
        /// Obtiene el evento asociado a un id especifico.
        /// </summary>
        /// <param name="id_cronograma">id_cronograma.</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IQueryable<UtilsJson.ACronograma>> getCronograId(string id_cronograma)
        {
            IQueryable<UtilsJson.ACronograma> listado = null;
            SqlParameter[] param = new SqlParameter[4];
            param[0] = new SqlParameter("id_institucion", "0");
            param[1] = new SqlParameter("id_evento", "0");
            param[2] = new SqlParameter("id_cronograma", id_cronograma);
            param[3] = new SqlParameter("id_conferencista", "0");

            listado = _repositorio.executeStored<UtilsJson.ACronograma>("getListingCronograma", param).Cast<UtilsJson.ACronograma>().AsQueryable<UtilsJson.ACronograma>();

            return listado;
        }

        /// <summary>
        /// Obtiene el listado de eventos asociados a una institucion.
        /// </summary>
        /// <param name="id_institucion">The id_institucion.</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IQueryable<UtilsJson.ACronograma>> getInstitucionId(string id_institucion)
        {
            IQueryable<UtilsJson.ACronograma> listado = null;
            SqlParameter[] param = new SqlParameter[4];
            param[0] = new SqlParameter("id_institucion", id_institucion);
            param[1] = new SqlParameter("id_evento", "0");
            param[2] = new SqlParameter("id_cronograma", "0");
            param[3] = new SqlParameter("id_conferencista", "0");

            listado = _repositorio.executeStored<UtilsJson.ACronograma>("getListingCronograma", param).Cast<UtilsJson.ACronograma>().AsQueryable<UtilsJson.ACronograma>();

            return listado;
        }

        /// <summary>
        /// Crea un nuevo evento asociado a una publicacion tipo evento.
        /// </summary>
        /// <param name="cronograma">Objeto json con los parametros de un cronograma cronograma.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPost]
        public Mensaje createCronograma([FromBody]UtilsJson.ACronograma cronograma)
        {
            Mensaje mensaje = null;

            try
            {
                if (cronograma != null)
                {
                    if (!string.IsNullOrWhiteSpace(cronograma.token_string))
                    {
                        if (AutenticacionToken.validateToken(cronograma.token_string) == 1)
                        {
                            Institucion institucion = _repositorio.Get<Institucion>(cronograma.institucionId);
                            if (institucion != null)
                            {
                                if (AutenticacionToken.validateUserToken(cronograma.token_string, institucion.logueo.correo_electronico))
                                {
                                    //datos personales
                                    string nombre = cronograma.nombre;
                                    string tema = cronograma.tema;
                                    string descripcion = cronograma.descripcion;
                                    string color = cronograma.color;
                                    long id_sala = cronograma.salaMesaId;
                                    bool estado = cronograma.estado;
                                    long id_evento = cronograma.eventoId;

                                    Publicacion publicacion = _repositorio.Get<Publicacion>(id_evento);
                                    SalaMesa salaMesa = _repositorio.Get<SalaMesa>(id_sala);

                                    if (!string.IsNullOrWhiteSpace(cronograma.hora_inicio) && !string.IsNullOrWhiteSpace(cronograma.hora_fin))
                                    {
                                        DateTime hora_inicio = Convert.ToDateTime(cronograma.hora_inicio);
                                        DateTime hora_fin = Convert.ToDateTime(cronograma.hora_fin);
                                        if (DateTime.Compare(hora_inicio, hora_fin) < 0)
                                        {
                                            if (publicacion != null && salaMesa != null)
                                            {

                                                Cronograma cronogramaDB = new Cronograma
                                                {
                                                    evento = publicacion,
                                                    sala = salaMesa,
                                                    nombre = nombre,
                                                    tema = tema,
                                                    descripcion = descripcion,
                                                    hora_inicio = hora_inicio,
                                                    hora_fin = hora_fin,
                                                    estado = estado,
                                                    fecha_ult_modificacion=DateTime.Now
                                                };
                                                //Almaceno o actualizo la salaMesa
                                                _repositorio.SaveOrUpdate<Cronograma>(cronogramaDB);
                                                mensaje = new Mensaje(EnumTipoMensaje.Notificacion, "Notificación", "Cronograma registrada exitosamente.");
                                            }
                                            else
                                            {
                                                mensaje = new Mensaje(EnumTipoMensaje.Error, "Error", "El evento o la sala solicitada no existe. verifique");
                                            }
                                        }
                                        else
                                        {
                                            mensaje = new Mensaje(EnumTipoMensaje.Error, "Error", "La hora de inicio es posterior a la hora de finalizacion. verifique");
                                        }
                                    }
                                    else
                                    {
                                        mensaje = new Mensaje(EnumTipoMensaje.Error, "Error", "La hora de inicio y fin del evento son requeridas. verifique");
                                    }
                                }
                                else
                                {
                                    mensaje = new Mensaje(EnumTipoMensaje.Error, "Error", "No cuenta con los privilegios suficientes");
                                }
                            }
                            else
                            {
                                mensaje = new Mensaje(EnumTipoMensaje.Error, "Error", "No cuenta con los privilegios suficientes para modificar estos campos.");
                            }
                        }
                        else
                        {
                            mensaje = new Mensaje(EnumTipoMensaje.Expiracion, "Error", "La sesion actual ha expirado. Inicie sesion");
                        }
                    }
                    else
                    {
                        mensaje = new Mensaje(EnumTipoMensaje.Error, "Error", "No cuenta con los privilegios suficientes");
                    }
                }
                else
                {
                    mensaje = new Mensaje(EnumTipoMensaje.Error, "Error", "No se puede insertar un objeto nulo");
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                var sb = new System.Text.StringBuilder();
                foreach (var failure in ex.EntityValidationErrors)
                {
                    sb.AppendFormat("{0} failed validation", failure.Entry.Entity.GetType());
                    foreach (var error in failure.ValidationErrors)
                    {
                        sb.AppendFormat("- {0} : {1}", error.PropertyName, error.ErrorMessage);
                        sb.AppendLine();
                    }
                }
                mensaje = new Mensaje(EnumTipoMensaje.Error, "Error", sb.ToString());
                SystemLog log = new SystemLog();
                log.ErrorLog(sb.ToString());
                throw new Exception(sb.ToString());
            }
            return mensaje;

        }

        /// <summary>
        /// Edita un evento asociado a una publicacion.
        /// </summary>
        /// <param name="cronograma">Objeto json con los parametros de un cronograma cronograma.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPost]
        public Mensaje editCronograma([FromBody]UtilsJson.ACronograma cronograma)
        {
            Mensaje mensaje = null;

            try
            {
                if (cronograma != null)
                {
                    if (!string.IsNullOrWhiteSpace(cronograma.token_string))
                    {
                        if (AutenticacionToken.validateToken(cronograma.token_string) == 1)
                        {
                            Institucion institucion = _repositorio.Get<Institucion>(cronograma.institucionId);
                            Cronograma cronogramaDB = _repositorio.Get<Cronograma>(cronograma.id);
                            if (institucion != null && cronogramaDB != null)
                            {
                                if (AutenticacionToken.validateUserToken(cronograma.token_string, institucion.logueo.correo_electronico))
                                {
                                    string nombre = cronograma.nombre;
                                    string tema = cronograma.tema;
                                    string descripcion = cronograma.descripcion;
                                    string color = cronograma.color;
                                    long id_sala = cronograma.salaMesaId;
                                    bool estado = cronograma.estado;
                                    long id_evento = cronograma.eventoId;
                                    if (!string.IsNullOrWhiteSpace(cronograma.hora_inicio) && !string.IsNullOrWhiteSpace(cronograma.hora_fin))
                                    {
                                        DateTime hora_inicio = Convert.ToDateTime(cronograma.hora_inicio);
                                        DateTime hora_fin = Convert.ToDateTime(cronograma.hora_fin);
                                        Publicacion publicacion = _repositorio.Get<Publicacion>(id_evento);
                                        SalaMesa salaMesa = _repositorio.Get<SalaMesa>(id_sala);
                                        if (DateTime.Compare(hora_inicio, hora_fin) < 0)
                                        {
                                            if (publicacion != null && salaMesa != null)
                                            {
                                                cronogramaDB.evento = publicacion;
                                                cronogramaDB.sala = salaMesa;
                                                cronogramaDB.nombre = nombre;
                                                cronogramaDB.tema = tema;
                                                cronogramaDB.descripcion = descripcion;
                                                cronogramaDB.hora_inicio = hora_inicio;
                                                cronogramaDB.hora_fin = hora_fin;
                                                cronogramaDB.estado = estado;
                                                cronogramaDB.fecha_ult_modificacion = DateTime.Now;
                                                _repositorio.SaveOrUpdate<Cronograma>(cronogramaDB);
                                                mensaje = new Mensaje(EnumTipoMensaje.Notificacion, "Notificación", "Cronograma editado exitosamente.");
                                            }
                                            else
                                            {
                                                mensaje = new Mensaje(EnumTipoMensaje.Error, "Error", "El evento o la sala solicitada no existe. verifique");
                                            }
                                        }
                                        else
                                        {
                                            mensaje = new Mensaje(EnumTipoMensaje.Error, "Error", "La hora de inicio es posterior a la hora de finalizacion. verifique");
                                        }
                                    }
                                    else
                                    {
                                        mensaje = new Mensaje(EnumTipoMensaje.Error, "Error", "La hora de inicio es posterior a la hora de finalizacion. verifique");
                                    }

                                }
                                else
                                {
                                    mensaje = new Mensaje(EnumTipoMensaje.Error, "Error", "No cuenta con los privilegios suficientes");
                                }
                            }
                            else
                            {
                                mensaje = new Mensaje(EnumTipoMensaje.Error, "Error", "No se encontro la sala solicitada o esta asociada a otra institucion.");
                            }
                        }
                        else
                        {
                            mensaje = new Mensaje(EnumTipoMensaje.Expiracion, "Error", "La sesion actual ha expirado. Inicie sesion");
                        }
                    }
                    else
                    {
                        mensaje = new Mensaje(EnumTipoMensaje.Error, "Error", "No cuenta con los privilegios suficientes");
                    }
                }
                else
                {
                    mensaje = new Mensaje(EnumTipoMensaje.Error, "Error", "No se puede insertar un objeto nulo");
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                var sb = new System.Text.StringBuilder();
                foreach (var failure in ex.EntityValidationErrors)
                {
                    sb.AppendFormat("{0} failed validation", failure.Entry.Entity.GetType());
                    foreach (var error in failure.ValidationErrors)
                    {
                        sb.AppendFormat("- {0} : {1}", error.PropertyName, error.ErrorMessage);
                        sb.AppendLine();
                    }
                }
                mensaje = new Mensaje(EnumTipoMensaje.Error, "Error", sb.ToString());
                SystemLog log = new SystemLog();
                log.ErrorLog(sb.ToString());
                throw new Exception(sb.ToString());
            }
            return mensaje;

        }

        /// <summary>
        /// Elimina del sistema un evento asociado a una publicacion tipo evento.
        /// </summary>
        /// <param name="cronograma">Objeto json con los parametros de un cronograma cronograma.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPost]
        public Mensaje deleteCronograma([FromBody]UtilsJson.ACronograma cronograma)
        {
            Mensaje mensaje = null;

            try
            {
                if (cronograma != null)
                {
                    if (!string.IsNullOrWhiteSpace(cronograma.token_string))
                    {
                        if (AutenticacionToken.validateToken(cronograma.token_string) == 1)
                        {
                            Institucion institucion = _repositorio.Get<Institucion>(cronograma.institucionId);
                            Cronograma cronogramaDB = _repositorio.Get<Cronograma>(cronograma.id);
                            if (institucion != null && cronogramaDB != null)
                            {
                                if (AutenticacionToken.validateUserToken(cronograma.token_string, institucion.logueo.correo_electronico))
                                {
                                    _repositorio.Delete<Conferencista>(cronogramaDB.id);
                                    mensaje = new Mensaje(EnumTipoMensaje.Notificacion, "Notificación", "Cronograma fue eliminada exitosamente.");
                                }
                                else
                                {
                                    mensaje = new Mensaje(EnumTipoMensaje.Error, "Error", "No cuenta con los privilegios suficientes");
                                }
                            }
                            else
                            {
                                mensaje = new Mensaje(EnumTipoMensaje.Error, "Error", "No se encontro el evento solicitada o esta asociada a otra institucion.");
                            }
                        }
                        else
                        {
                            mensaje = new Mensaje(EnumTipoMensaje.Expiracion, "Error", "La sesion actual ha expirado. Inicie sesion");
                        }
                    }
                    else
                    {
                        mensaje = new Mensaje(EnumTipoMensaje.Error, "Error", "No cuenta con los privilegios suficientes");
                    }
                }
                else
                {
                    mensaje = new Mensaje(EnumTipoMensaje.Error, "Error", "No se puede eliminar un objeto nulo");
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                var sb = new System.Text.StringBuilder();
                foreach (var failure in ex.EntityValidationErrors)
                {
                    sb.AppendFormat("{0} failed validation", failure.Entry.Entity.GetType());
                    foreach (var error in failure.ValidationErrors)
                    {
                        sb.AppendFormat("- {0} : {1}", error.PropertyName, error.ErrorMessage);
                        sb.AppendLine();
                    }
                }
                mensaje = new Mensaje(EnumTipoMensaje.Error, "Error", sb.ToString());
                SystemLog log = new SystemLog();
                log.ErrorLog(sb.ToString());
                throw new Exception(sb.ToString());
            }
            return mensaje;

        }
    
    }
}