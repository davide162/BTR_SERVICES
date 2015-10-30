using BTR_Services.Models;
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
    /// <summary>
    /// Gestiona los conferencistas registrado por una institucion en el sistema
    /// </summary>
    public class ConferencistaController : ApiController
    {
        private readonly IRepositorio _repositorio;

        public ConferencistaController(Repositorio repository)
        {
            _repositorio = repository;
        }

        /// <summary>
        /// Obtiene el listado de todos los conferencista existentes en el sistema.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IQueryable<UtilsJson.APersona>> getConferencistaAll()
        {
            IQueryable<UtilsJson.APersona> listado = null;
            SqlParameter[] param = new SqlParameter[4];
            param[0] = new SqlParameter("id_institucion", "0");
            param[1] = new SqlParameter("id_evento", "0");
            param[2] = new SqlParameter("id_cronograma", "0");
            param[3] = new SqlParameter("id_conferencista", "0");

            listado = _repositorio.executeStored<UtilsJson.APersona>("getListingConferencista", param).Cast<UtilsJson.APersona>().AsQueryable<UtilsJson.APersona>();

            return listado;
        }


        /// <summary>
        /// Obtiene informacion de un conferencista en particular filtrada por id.
        /// </summary>
        /// <param name="id_conferencista">id_conferencista.</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IQueryable<UtilsJson.APersona>> getConferencistaId(string id_conferencista)
        {
            IQueryable<UtilsJson.APersona> listado = null;
            SqlParameter[] param = new SqlParameter[4];
            param[0] = new SqlParameter("id_institucion", "0");
            param[1] = new SqlParameter("id_evento", "0");
            param[2] = new SqlParameter("id_cronograma", "0");
            param[3] = new SqlParameter("id_conferencista", id_conferencista);

            listado = _repositorio.executeStored<UtilsJson.APersona>("getListingConferencista", param).Cast<UtilsJson.APersona>().AsQueryable<UtilsJson.APersona>();

            return listado;
        }

        /// <summary>
        /// Obtiene listado de conferencistas asociados a un evento.
        /// </summary>
        /// <param name="id_evento">id_evento.</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IQueryable<UtilsJson.APersona>> getEventoConferencistaId(string id_evento)
        {
            IQueryable<UtilsJson.APersona> listado = null;
            SqlParameter[] param = new SqlParameter[4];
            param[0] = new SqlParameter("id_institucion", "0");
            param[1] = new SqlParameter("id_evento", id_evento);
            param[2] = new SqlParameter("id_cronograma", "0");
            param[3] = new SqlParameter("id_conferencista", "0");

            listado = _repositorio.executeStored<UtilsJson.APersona>("getListingConferencista", param).Cast<UtilsJson.APersona>().AsQueryable<UtilsJson.APersona>();

            return listado;
        }

        /// <summary>
        /// Obtiene listado de conferencistas asociados a un cronograma en particula.
        /// </summary>
        /// <param name="id_cronograma">id_cronograma.</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IQueryable<UtilsJson.APersona>> getCronograConferencistaId(string id_cronograma)
        {
            IQueryable<UtilsJson.APersona> listado = null;
            SqlParameter[] param = new SqlParameter[4];
            param[0] = new SqlParameter("id_institucion", "0");
            param[1] = new SqlParameter("id_evento", "0");
            param[2] = new SqlParameter("id_cronograma", id_cronograma);
            param[3] = new SqlParameter("id_conferencista", "0");

            listado = _repositorio.executeStored<UtilsJson.APersona>("getListingConferencista", param).Cast<UtilsJson.APersona>().AsQueryable<UtilsJson.APersona>();

            return listado;
        }

        /// <summary>
        /// Obtiene el listado de los conferencistas asociados a una institucion.
        /// </summary>
        /// <param name="id_institucion">The id_institucion.</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IQueryable<UtilsJson.APersona>> getInstitucionConferencistaId(string id_institucion)
        {
            IQueryable<UtilsJson.APersona> listado = null;
            SqlParameter[] param = new SqlParameter[4];
            param[0] = new SqlParameter("id_institucion", id_institucion);
            param[1] = new SqlParameter("id_evento", "0");
            param[2] = new SqlParameter("id_cronograma", "0");
            param[3] = new SqlParameter("id_conferencista", "0");

            listado = _repositorio.executeStored<UtilsJson.APersona>("getListingConferencista", param).Cast<UtilsJson.APersona>().AsQueryable<UtilsJson.APersona>();

            return listado;
        }

        /// <summary>
        /// Crea en el sistema un nuevo conferencista asociado a una institucion.
        /// </summary>
        /// <param name="conferencista">Objeto json con los atributos de un conferencista.</param>
        /// <returns></returns>
        [HttpPost]
        public Mensaje createConferencista([FromBody]UtilsJson.APersona conferencista)
        {
            Mensaje mensaje = null;

            try
            {
                if (conferencista != null)
                {
                    if (!string.IsNullOrWhiteSpace(conferencista.token))
                    {
                        if (AutenticacionToken.validateToken(conferencista.token) == 1)
                        {
                            long id_institucion_c=(!string.IsNullOrWhiteSpace(conferencista.Ainstitucion)) ? long.Parse(conferencista.Ainstitucion) : 0;
                            Institucion institucion = _repositorio.Get<Institucion>(id_institucion_c);
                            if (institucion != null)
                            {
                                if (AutenticacionToken.validateUserToken(conferencista.token, institucion.logueo.correo_electronico))
                                {
                                    //datos personales
                                    string tipo_identificacion = validarTipoIdentificacion(conferencista.tipo_identificacion);
                                    string identificacion = conferencista.identificacion;
                                    string nombre_persona = conferencista.nombre;
                                    string apellido_persona = conferencista.apellido;
                                    string correo_persona = conferencista.correo_electronico;
                                    string urlCvlac = conferencista.urlCvlac;
                                    string perfil=conferencista.perfil_profesional;
                                    string foto=conferencista.foto;
                                    if (tipo_identificacion != null)
                                    {

                                        Imagen newImagen = null;
                                        if (!string.IsNullOrWhiteSpace(foto))
                                        {
                                            newImagen = new Imagen { imagenBase64 = foto };
                                        }
                                        Conferencista newConferencista = new Conferencista
                                        {
                                            institucion = institucion,
                                            persona = new Persona
                                            {
                                                nombre = nombre_persona,
                                                apellido = apellido_persona,
                                                tipo_identificacion = tipo_identificacion,
                                                identificacion = identificacion,
                                                correo_electronico = correo_persona,
                                                urlCvlac = urlCvlac,
                                                perfil_profesional = perfil,
                                                foto = newImagen,
                                                fecha_ult_modificacion=DateTime.Now
                                            }
                                        };
                                        //Almaceno o actualizo la salaMesa
                                        _repositorio.SaveOrUpdate<Conferencista>(newConferencista);
                                        mensaje = new Mensaje(EnumTipoMensaje.Notificacion, "Notificación", "Conferencista registrada exitosamente.");
                                    }
                                    else
                                    {
                                        mensaje = new Mensaje(EnumTipoMensaje.Error, "Error", "El tipo de identificacion no existe. verifique que el valor sea valido");
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
        /// Edita la informacion relacionada con un conferencista asociado a una institucion.
        /// </summary>
        /// <param name="conferencista">Objeto json relacionado con los parametros de una conferencista.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPost]
        public Mensaje editConferencista([FromBody]UtilsJson.APersona conferencista)
        {
            Mensaje mensaje = null;

            try
            {
                if (conferencista != null)
                {
                    if (!string.IsNullOrWhiteSpace(conferencista.token))
                    {
                        if (AutenticacionToken.validateToken(conferencista.token) == 1)
                        {
                            long id_institucion_c = (!string.IsNullOrWhiteSpace(conferencista.Ainstitucion)) ? long.Parse(conferencista.Ainstitucion) : 0;
                            Institucion institucion = _repositorio.Get<Institucion>(id_institucion_c);
                            Conferencista conferencistaDB = _repositorio.Get<Conferencista>(conferencista.id);
                            if (institucion != null && conferencistaDB != null)
                            {
                                if (AutenticacionToken.validateUserToken(conferencista.token, institucion.logueo.correo_electronico))
                                {
                                    //datos personales
                                    string tipo_identificacion = validarTipoIdentificacion(conferencista.tipo_identificacion);
                                    string identificacion = conferencista.identificacion;
                                    string nombre_persona = conferencista.nombre;
                                    string apellido_persona = conferencista.apellido;
                                    string correo_persona = conferencista.correo_electronico;
                                    string urlCvlac = conferencista.urlCvlac;
                                    string perfil = conferencista.perfil_profesional;
                                    string foto = conferencista.foto;

                                    if (tipo_identificacion != null)
                                    {
                                        Imagen newImagen = null;
                                        if (!string.IsNullOrWhiteSpace(foto))
                                        {
                                            if (conferencistaDB.persona.foto != null)
                                            {
                                                conferencistaDB.persona.foto.imagenBase64 = foto;
                                            }
                                            else
                                            {
                                                newImagen = new Imagen { imagenBase64 = foto };
                                                conferencistaDB.persona.foto = newImagen;
                                            }
                                        }
                                        conferencistaDB.persona.tipo_identificacion = tipo_identificacion;
                                        conferencistaDB.persona.identificacion = identificacion;
                                        conferencistaDB.persona.nombre = nombre_persona;
                                        conferencistaDB.persona.apellido = apellido_persona;
                                        conferencistaDB.persona.correo_electronico = correo_persona;
                                        conferencistaDB.persona.urlCvlac = urlCvlac;
                                        conferencistaDB.persona.perfil_profesional = perfil;
                                        conferencistaDB.fecha_ult_modificacion = DateTime.Now;
                                        //Almaceno o actualizo la salaMesa
                                        _repositorio.SaveOrUpdate<Conferencista>(conferencistaDB);

                                        mensaje = new Mensaje(EnumTipoMensaje.Notificacion, "Notificación", "Conferencista editado exitosamente.");
                                    }
                                    else
                                    {
                                        mensaje = new Mensaje(EnumTipoMensaje.Error, "Error", "El tipo de identificacion no existe. verifique que el valor sea valido");
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
        /// Elimina un conferencista especificado por id .
        /// </summary>
        /// <param name="conferencista">Objeto json con los datos del conferencista.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPost]
        public Mensaje deleteConferencista([FromBody]UtilsJson.APersona conferencista)
        {
            Mensaje mensaje = null;

            try
            {
                if (conferencista != null)
                {
                    if (!string.IsNullOrWhiteSpace(conferencista.token))
                    {
                        if (AutenticacionToken.validateToken(conferencista.token) == 1)
                        {
                            long id_institucion_c = (!string.IsNullOrWhiteSpace(conferencista.Ainstitucion)) ? long.Parse(conferencista.Ainstitucion) : 0;
                            Institucion institucion = _repositorio.Get<Institucion>(id_institucion_c);
                            Conferencista conferencistaDB = _repositorio.Get<Conferencista>(conferencista.id);
                            if (institucion != null && conferencistaDB != null)
                            {
                                if (AutenticacionToken.validateUserToken(conferencista.token, institucion.logueo.correo_electronico))
                                {
                                    _repositorio.Delete<Conferencista>(conferencistaDB.id);
                                    mensaje = new Mensaje(EnumTipoMensaje.Notificacion, "Notificación", "Conferencista fue eliminada exitosamente.");
                                }
                                else
                                {
                                    mensaje = new Mensaje(EnumTipoMensaje.Error, "Error", "No cuenta con los privilegios suficientes");
                                }
                            }
                            else
                            {
                                mensaje = new Mensaje(EnumTipoMensaje.Error, "Error", "No se encontro el conferencista solicitada o esta asociada a otra institucion.");
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


        /// <summary>
        /// Valida si la cadena de caracteres existe dentro de los parametros de tipo de documento presentes en el sistema.
        /// </summary>
        /// <param name="tipo">El tipo de documento a verificar.</param>
        /// <returns></returns>
        private string validarTipoIdentificacion(string tipo)
        {
            foreach (string tipoEnum in EnumTipoIdentificacion.GetNames())
            {
                if (tipoEnum.ToUpper().Equals(tipo.ToUpper()))
                {
                    return EnumTipoIdentificacion.ValueOf(tipoEnum);
                }
            }
            return null;
        
        }
    }
}
