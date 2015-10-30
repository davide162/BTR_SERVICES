using BTR_Services.Models;
using BTR_Services.Persistencia;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace BTR_Services.Api
{
    /// <summary>
    /// Controlador encargado de gestionar el registro de una institucion a un evento en particular
    /// </summary>
    public class EventoParticipanteController : ApiController
    {
        private readonly IRepositorio _repositorio;

        public EventoParticipanteController(Repositorio repository)
        {
            _repositorio = repository;
        }

        /// <summary>
        /// Obtiene el listado de todos los participantes y los eventos registrados en el sistema.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IQueryable<UtilsJson.AEventoParticipante>> getEventoParticipanteAll()
        {
            IQueryable<UtilsJson.AEventoParticipante> listado = null;
            SqlParameter[] param = new SqlParameter[3];
            param[0] = new SqlParameter("id_institucion", "0");
            param[1] = new SqlParameter("id_publicacion", "0");
            param[2] = new SqlParameter("id_evento_institucion", "0");

            listado = _repositorio.executeStored<UtilsJson.AEventoParticipante>("getListingInstitucionEvento", param).Cast<UtilsJson.AEventoParticipante>().AsQueryable<UtilsJson.AEventoParticipante>();

            return listado;
        }

        /// <summary>
        /// Obtiene el evento_participante filtrado por id
        /// </summary>
        /// <param name="id_evento_participante">The id_evento_participante.</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IQueryable<UtilsJson.AEventoParticipante>> getEventoParticipanteId(string id_evento_participante)
        {
            IQueryable<UtilsJson.AEventoParticipante> listado = null;
            SqlParameter[] param = new SqlParameter[3];
            param[0] = new SqlParameter("id_institucion", "0");
            param[1] = new SqlParameter("id_publicacion", "0");
            param[2] = new SqlParameter("id_evento_institucion", id_evento_participante);

            listado = _repositorio.executeStored<UtilsJson.AEventoParticipante>("getListingInstitucionEvento", param).Cast<UtilsJson.AEventoParticipante>().AsQueryable<UtilsJson.AEventoParticipante>();

            return listado;
        }

        /// <summary>
        /// Obtiene todos los participantes registrados a un evento espeficicado por id.
        /// </summary>
        /// <param name="id_evento">The id_evento.</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IQueryable<UtilsJson.AEventoParticipante>> getEventoId(string id_evento)
        {
            IQueryable<UtilsJson.AEventoParticipante> listado = null;
            SqlParameter[] param = new SqlParameter[3];
            param[0] = new SqlParameter("id_institucion", "0");
            param[1] = new SqlParameter("id_publicacion", id_evento);
            param[2] = new SqlParameter("id_evento_institucion", "0");

            listado = _repositorio.executeStored<UtilsJson.AEventoParticipante>("getListingInstitucionEvento", param).Cast<UtilsJson.AEventoParticipante>().AsQueryable<UtilsJson.AEventoParticipante>();

            return listado;
        }

        /// <summary>
        /// Obtiene todos eventos en los que participa una institucion especificada por el id
        /// </summary>
        /// <param name="id_institucion">The id_institucion.</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IQueryable<UtilsJson.AEventoParticipante>> getInstitucionId(string id_institucion)
        {
            IQueryable<UtilsJson.AEventoParticipante> listado = null;
            SqlParameter[] param = new SqlParameter[3];
            param[0] = new SqlParameter("id_institucion", id_institucion);
            param[1] = new SqlParameter("id_publicacion", "0");
            param[2] = new SqlParameter("id_evento_institucion", "0");

            listado = _repositorio.executeStored<UtilsJson.AEventoParticipante>("getListingInstitucionEvento", param).Cast<UtilsJson.AEventoParticipante>().AsQueryable<UtilsJson.AEventoParticipante>();

            return listado;
        }

        /// <summary>
        /// Registra un nuevo participante a un evento especifico
        /// </summary>
        /// <param name="eventoParticipante">Objeto json con los parametros del evento y el participante.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPost]
        public Mensaje createEventoParticipante([FromBody]UtilsJson.AEventoParticipante eventoParticipante)
        {
            Mensaje mensaje = null;

            try
            {
                if (eventoParticipante != null)
                {
                    if (!string.IsNullOrWhiteSpace(eventoParticipante.token_string))
                    {
                        if (AutenticacionToken.validateToken(eventoParticipante.token_string) == 1)
                        {
                            long id_inst = eventoParticipante.id_institucion;
                            long id_pub = eventoParticipante.id_evento;
                            Institucion institucion = _repositorio.Get<Institucion>(id_inst);
                            Publicacion publicacion = _repositorio.Get<Publicacion>(id_pub);
                            Expression<Func<EventoParticipante, bool>> query = (u => u.institucion.id == id_inst && u.evento.id==id_pub && u.estado == true);
                            List<EventoParticipante> result = _repositorio.Filter<EventoParticipante>(query);
                            if (institucion != null && publicacion !=null && result.Count ==0)
                            {
                                if (AutenticacionToken.validateUserToken(eventoParticipante.token_string, institucion.logueo.correo_electronico))
                                {
                                    //datos institucion y publicacion
                                    EventoParticipante newRegistro = new EventoParticipante
                                    {
                                        institucion=institucion,
                                        evento=publicacion,
                                        fecha_ult_modificacion=DateTime.Now,
                                        estado=eventoParticipante.estado
                                    };
                                    //Almaceno o actualizo la salaMesa
                                    _repositorio.SaveOrUpdate<EventoParticipante>(newRegistro);
                                    mensaje = new Mensaje(EnumTipoMensaje.Notificacion, "Notificación", "Institucion registrada al evento exitosamente.");
                                    
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
        /// Edita el registro de un participante asociado a un evento especificado por id.
        /// </summary>
        /// <param name="eventoParticipante">Objeto json con los parametros del evento y del participante.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPost]
        public Mensaje editAEventoParticipante([FromBody]UtilsJson.AEventoParticipante eventoParticipante)
        {
            Mensaje mensaje = null;

            try
            {
                if (eventoParticipante != null)
                {
                    if (!string.IsNullOrWhiteSpace(eventoParticipante.token_string))
                    {
                        if (AutenticacionToken.validateToken(eventoParticipante.token_string) == 1)
                        {
                            long id_institucion_c = eventoParticipante.id_institucion;
                            long id_publicacion = eventoParticipante.id_evento;
                            Institucion institucion = _repositorio.Get<Institucion>(id_institucion_c);
                            Publicacion publicacion = _repositorio.Get<Publicacion>(id_publicacion);
                            EventoParticipante registro = _repositorio.Get<EventoParticipante>(eventoParticipante.id_evento_institucion);
                            if (institucion != null && publicacion != null && registro != null)
                            {
                                if (AutenticacionToken.validateUserToken(eventoParticipante.token_string, institucion.logueo.correo_electronico))
                                {
                                        registro.institucion=institucion;
                                        registro.evento=publicacion;
                                        registro.fecha_ult_modificacion=DateTime.Now;
                                        registro.estado = eventoParticipante.estado;
                                        //Almaceno o actualizo el registro
                                        _repositorio.SaveOrUpdate<EventoParticipante>(registro);

                                        mensaje = new Mensaje(EnumTipoMensaje.Notificacion, "Notificación", "Participante a evento editado exitosamente.");                                    
                                }
                                else
                                {
                                    mensaje = new Mensaje(EnumTipoMensaje.Error, "Error", "No cuenta con los privilegios suficientes");
                                }
                            }
                            else
                            {
                                mensaje = new Mensaje(EnumTipoMensaje.Error, "Error", "No se encontro el registro solicitada o esta asociada a otra institucion.");
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
        /// Elimina el registro de un participante a un evento especificado por id.
        /// </summary>
        /// <param name="eventoParticipante">The evento participante.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public Mensaje deleteEventoParticipante([FromBody]UtilsJson.AEventoParticipante eventoParticipante)
        {
            Mensaje mensaje = null;

            try
            {
                if (eventoParticipante != null)
                {
                    if (!string.IsNullOrWhiteSpace(eventoParticipante.token_string))
                    {
                        if (AutenticacionToken.validateToken(eventoParticipante.token_string) == 1)
                        {
                            Institucion institucion = _repositorio.Get<Institucion>(eventoParticipante.id_institucion);
                            EventoParticipante eventoParticipanteDB = _repositorio.Get<EventoParticipante>(eventoParticipante.id_evento_institucion);
                            if (institucion != null && eventoParticipanteDB != null)
                            {
                                if (AutenticacionToken.validateUserToken(eventoParticipante.token_string, institucion.logueo.correo_electronico))
                                {
                                    _repositorio.Delete<Conferencista>(eventoParticipanteDB.id);
                                    mensaje = new Mensaje(EnumTipoMensaje.Notificacion, "Notificación", "Registro a evento eliminada exitosamente.");
                                }
                                else
                                {
                                    mensaje = new Mensaje(EnumTipoMensaje.Error, "Error", "No cuenta con los privilegios suficientes");
                                }
                            }
                            else
                            {
                                mensaje = new Mensaje(EnumTipoMensaje.Error, "Error", "No se encontro el registro solicitada o esta asociada a otra institucion.");
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
