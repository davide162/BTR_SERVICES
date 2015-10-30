using BTR_Services.Models;
using BTR_Services.Persistencia;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Xml;
using System.Drawing;
using System.IO;

namespace BTR_Services.Api
{
    public class InstitucionController : ApiController
    {
        private readonly IRepositorio _repositorio;

        public InstitucionController(Repositorio repository)
        {
            _repositorio = repository;
        }

        /// <summary>
        /// Valida el codigo de registro.
        /// </summary>
        /// <param name="code">El codigo.</param>
        /// <returns><c>true</c> if codigo correcto, <c>false</c> otro caso.</returns>
        [HttpPost]
        public bool validateCode([FromBody] string code)
        {
            bool solicitudRol = false;
            try
            {
                if (!string.IsNullOrWhiteSpace(code))
                {
                    //verificacion de existencia y estado del codigo
                    Expression<Func<SolicitudRegistro, bool>> query = u => u.codigo == code && u.estado == true;
                    SolicitudRegistro solicitud = _repositorio.Get<SolicitudRegistro>(query);
                    //Si la solicitud existe y no se ha usado subo a session el codigo
                    if (solicitud != null)
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                SystemLog mSystemLog = new SystemLog();
                mSystemLog.ErrorLog(ex.Message);
            }
            return false;
        }


        /// <summary>
        /// Obtiene todas las instituciones existentes, sin informacion sensible
        /// </summary>
        /// <returns>List&lt;UtilsJson.AInstitucion&gt;.</returns>
        public async Task<List<UtilsJson.AInstitucion>> getInstitutionAll()
        {
            Expression<Func<Persona, bool>> query = (u => u.institucion.estado == true);
            List<Persona> lstInstituciones = _repositorio.Filter<Persona>(query);

            List<UtilsJson.AInstitucion> listado = new List<UtilsJson.AInstitucion>();

            foreach (Persona aux in lstInstituciones)
            {
                Expression<Func<InstitucionSector, bool>> query1 = (u => u.institucion.id == aux.id);
                Expression<Func<InstitucionTipoBiotec, bool>> query2 = (u => u.institucion.id == aux.id);
                listado.Add(convertToInstitucion(aux, _repositorio.Filter<InstitucionSector>(query1), _repositorio.Filter<InstitucionTipoBiotec>(query2), false));
            }
            return listado;
        }


        /// <summary>
        /// Obtiene una institucion especificada por id.
        /// </summary>
        /// <param name="id">Identificador de la institucion.</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<UtilsJson.AInstitucion> getInstitutionId(long id)
        {
            UtilsJson.AInstitucion json = new UtilsJson.AInstitucion();
            Expression<Func<Persona, bool>> query = (u => u.id == id && u.estado == true);
            List<Persona> lstRepre = _repositorio.Filter<Persona>(query);
            if (lstRepre != null)
            {
                if (lstRepre.Count > 0)
                {
                    long id_institucion = lstRepre[0].id;
                    Expression<Func<InstitucionSector, bool>> query1 = (u => u.institucion.id == id_institucion);
                    Expression<Func<InstitucionTipoBiotec, bool>> query2 = (u => u.institucion.id == id_institucion);
                    json = convertToInstitucion(lstRepre[0], _repositorio.Filter<InstitucionSector>(query1), _repositorio.Filter<InstitucionTipoBiotec>(query2), false);
                }
            }
            return json;
        }


        /// <summary>
        /// Obtiene una institucion especificada por id con permisos de usuario autenticado.
        /// </summary>
        /// <param name="token">Token de autenticacion y id.</param>
        /// <returns></returns>
        [HttpPost]
        public UtilsJson.AInstitucion getUsuarioIdAuth([FromBody] UtilsJson.AToken token)
        {
            UtilsJson.AInstitucion json = new UtilsJson.AInstitucion();
            if (token != null)
            {
                if (!string.IsNullOrWhiteSpace(token.token_string))
                {
                    if (AutenticacionToken.validateToken(token.token_string) == 1)
                    {
                        long id_usuario = token.user_id;
                        Expression<Func<Persona, bool>> query = (u => u.institucion.id == id_usuario && u.institucion.estado == true);
                        Persona usuario = _repositorio.Get<Persona>(query);
                        if (usuario != null)
                        {
                            if (usuario.institucion != null)
                            {
                                long id_institucion = usuario.id;
                                Expression<Func<InstitucionSector, bool>> query1 = (u => u.institucion.id == id_institucion);
                                Expression<Func<InstitucionTipoBiotec, bool>> query2 = (u => u.institucion.id == id_institucion);
                                List<InstitucionSector> listSectores = _repositorio.Filter<InstitucionSector>(query1);
                                List<InstitucionTipoBiotec> listBiotec = _repositorio.Filter<InstitucionTipoBiotec>(query2);
                                json = convertToInstitucion(usuario, listSectores, listBiotec, true);
                            }
                        }
                    }
                }
            }
            return json;
        }


        /// <summary>
        /// Crea una nueva institucion en el sistema.
        /// </summary>
        /// <param name="institution">la institution.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPost]
        public Mensaje createInstitution([FromBody]UtilsJson.AInstitucion institution)
        {
            Mensaje mensaje = null;

            try
            {
                if (institution != null)
                {
                    //datos logueo
                    string correo_electronico_l = institution.logueo.correo_electronico;
                    string password1 = institution.logueo.contrasena1;
                    string password2 = institution.logueo.contrasena2;

                    //datos representante
                    string nombre_r = institution.representante.nombre;
                    string apellido_r = institution.representante.apellido;
                    string tipo_identificacion = institution.representante.tipo_identificacion;
                    string identificacion = institution.representante.identificacion;
                    string urlCvlac = institution.representante.urlCvlac;
                    string correo_electronico_r = institution.representante.correo_electronico;

                    //datos institucion
                    string codigo = institution.codigo_registro;
                    string nombre = institution.nombre;
                    string descripcion = institution.descripcion;
                    string correo_electronico = institution.correo_electronico;
                    string direccion_postal = institution.direccion_postal;
                    string facebook = institution.facebook;
                    string fax = institution.fax;
                    string impacto = institution.impacto;
                    string linkedin = institution.linkedin;
                    string naturaleza = institution.naturaleza;
                    string pagina_web = institution.pagina_web;
                    int tamano = institution.tamano;
                    string telefono = institution.telefono;
                    string tipo_institucion = institution.tipo_institucion;
                    string twitter = institution.twitter;
                    string constitucion = institution.constitucion;
                    string fecha_creacion = institution.fecha_creacion;
                    string latitud = institution.latitud;
                    string longitud = institution.longitud;
                    string imagen_base64 = institution.imagen_base64;
                    string tipo_empresa = (!String.IsNullOrWhiteSpace(institution.tipo_empresa)) ? institution.tipo_empresa : EnumTipoEmpresa.Compañia;
                    UtilsJson.AMunicipio municipio = institution.municipio;

                    UtilsJson.ASector[] sectores = institution.sectores;
                    UtilsJson.ATipoBiotecnologia[] Tipos_Biotecnologia = institution.Tipos_Biotecnologia;

                    if (codigo != null)
                    {
                        //verificacion de existencia y estado del codigo
                        Expression<Func<SolicitudRegistro, bool>> query = u => u.codigo == codigo && u.estado == true;
                        SolicitudRegistro solicitud = _repositorio.Get<SolicitudRegistro>(query);
                        //Si la solicitud existe y no se ha usado subo a session el codigo
                        if (solicitud != null)
                        {
                            Municipio municipioDb = null;

                            List<Sector> lstSectores = new List<Sector>();

                            List<TipoBiotecnologia> lstTipoBiotecnologia = new List<TipoBiotecnologia>();

                            if (municipio.id > 0)
                            {
                                municipioDb = _repositorio.Get<Municipio>(municipio.id);
                            }

                            if (sectores != null)
                            {
                                lstSectores = convertToSector(sectores);
                            }

                            if (Tipos_Biotecnologia != null)
                            {
                                lstTipoBiotecnologia = convertToTipoBiotecnologia(Tipos_Biotecnologia);
                            }

                            Institucion institucion_api = new Institucion();
                            if (password1.Equals(password2))
                            {
                                institucion_api.logueo = new LogueoInstitucion
                                {
                                    correo_electronico = correo_electronico_l,
                                    contrasena = CifradoDatos.cifrarPassword(password2),
                                    fecha_ult_modificacion = DateTime.Now,
                                    rol = EnumTipoRol.usuario
                                };

                                institucion_api.descripcion = descripcion;
                                institucion_api.impacto = impacto;
                                institucion_api.correo_electronico = correo_electronico;
                                institucion_api.latitud = latitud;
                                institucion_api.linkedin = linkedin;
                                institucion_api.longitud = longitud;
                                institucion_api.municipio = municipioDb;
                                institucion_api.naturaleza = naturaleza;
                                institucion_api.constitucion = constitucion;
                                institucion_api.nombre = nombre;
                                institucion_api.pagina_web = pagina_web;
                                institucion_api.tamano = tamano;
                                institucion_api.telefono = telefono;
                                institucion_api.fax = fax;
                                institucion_api.direccion_postal = direccion_postal;
                                institucion_api.facebook = facebook;
                                institucion_api.tipo_institucion = tipo_institucion;
                                institucion_api.twitter = twitter;
                                institucion_api.fecha_creacion = (!string.IsNullOrWhiteSpace(fecha_creacion)) ? Convert.ToDateTime(fecha_creacion) : (DateTime?)null;

                                if (institucion_api.banner == null)
                                {
                                    if (!string.IsNullOrWhiteSpace(imagen_base64))
                                    {
                                        institucion_api.banner = new Imagen { imagenBase64 = imagen_base64, fecha_ult_modificacion = DateTime.Now };

                                        //Redimension de la imagen y creacion de icono
                                        string extension = imagen_base64.Split(',')[0];
                                        imagen_base64 = imagen_base64.Split(',')[1];

                                        byte[] bytes = Convert.FromBase64String(imagen_base64);

                                        if (bytes.Length > 0)
                                        {
                                            byte[] filebytesIcon =UtilsHost.Redimensionar(Image.FromStream(new MemoryStream(bytes)), 100, 100, 32);

                                            string encodedDataIcon = extension + "," + Convert.ToBase64String(filebytesIcon, Base64FormattingOptions.None);
                                            institucion_api.icono = new Imagen { imagenBase64 = encodedDataIcon, fecha_ult_modificacion = DateTime.Now };
                                        }
                                    }
                                }
                                else
                                {
                                    if (!string.IsNullOrWhiteSpace(imagen_base64))
                                    {
                                        institucion_api.banner.imagenBase64 = imagen_base64;
                                        institucion_api.banner.fecha_ult_modificacion = DateTime.Now;

                                        //Redimension de la imagen y creacion de icono
                                        string extension = imagen_base64.Split(',')[0];
                                        imagen_base64 = imagen_base64.Split(',')[1];

                                        byte[] bytes = Convert.FromBase64String(imagen_base64);

                                        if (bytes.Length > 0)
                                        {
                                            byte[] filebytesIcon = UtilsHost.Redimensionar(Image.FromStream(new MemoryStream(bytes)), 100, 100, 32);

                                            string encodedDataIcon = extension + "," + Convert.ToBase64String(filebytesIcon, Base64FormattingOptions.None);
                                            institucion_api.icono.imagenBase64 = encodedDataIcon;
                                            institucion_api.icono.fecha_ult_modificacion = DateTime.Now;
                                        }
                                    }
                                }

                                //creo un nuevo representante para la institucion
                                Persona persona = new Persona
                                {
                                    nombre = nombre_r,
                                    apellido = apellido_r,
                                    tipo_identificacion = tipo_identificacion,
                                    identificacion = identificacion,
                                    urlCvlac = urlCvlac,
                                    correo_electronico = correo_electronico_r,
                                    tipoPersona = EnumTipoPersona.director,
                                    fecha_ult_modificacion = DateTime.Now,
                                    estado=true
                                };

                                //Genero el correo para confirmacion para habilitar el perfil
                                StringBuilder bodyMail = new StringBuilder();
                                //Creo un token de autenticacion para habilitar el perfil
                                string codigoEnabled = CifradoDatos.cifrarRSA(institucion_api.logueo.correo_electronico);
                                bodyMail.AppendLine("Para habilitar el perfil de la institucion " + institucion_api.nombre + " dirijase al siguiente enlace.");
                                string informacionHost = UtilsHost.serverInfoCurrent();
                                bodyMail.AppendLine("<a href=\"" + informacionHost + "/Institucion/HabilitarCuenta?tokenEnabled=" + codigoEnabled + "\">Activar perfil.</a>" + "</br>");
                                string subject = "Confirmación y habilitacion de perfil.";
                                Mail mail = new Mail(institucion_api.logueo.correo_electronico, subject, bodyMail);

                                //Verifico si la institucion existe en el sistema
                                List<Mensaje> lstVerificaExiste = existeInstitucion(persona);
                                List<Mensaje> lstVerificaExisteEmail = mail.existeEmail();
                                if (lstVerificaExiste.Count == 0 && lstVerificaExisteEmail.Count == 0)
                                {
                                    //Envio el correo de confirmacion
                                    if (mail.sendMail())
                                    {
                                        persona.institucion = institucion_api;
                                        _repositorio.SaveOrUpdate<Persona>(persona);

                                        //Elimino las relaciones de la entidad institucion con sectores y tipoBiotecnologia
                                        Expression<Func<InstitucionSector, bool>> query3 = u => u.institucion.id == institucion_api.id;
                                        _repositorio.DeleteFilter<InstitucionSector>(query3);
                                        Expression<Func<InstitucionTipoBiotec, bool>> query4 = u => u.institucion.id == institucion_api.id;
                                        _repositorio.DeleteFilter<InstitucionTipoBiotec>(query4);

                                        //almaceno las relaciones de la entidad institucion con sectores y tipoBiotecnologia
                                        foreach (Sector sector in lstSectores)
                                        {
                                            _repositorio.SaveOrUpdate(new InstitucionSector { institucion = institucion_api, sector = sector, fecha_ult_modificacion = DateTime.Now });
                                        }

                                        foreach (TipoBiotecnologia tipo in lstTipoBiotecnologia)
                                        {
                                            _repositorio.SaveOrUpdate(new InstitucionTipoBiotec { institucion = institucion_api, tipoBiotecnologia = tipo, fecha_ult_modificacion = DateTime.Now });
                                        }

                                        // Desabilito el codigoPreRegistro en el poll de codigo para que no se pueda volver a usar
                                        solicitud.estado = false;
                                        solicitud.fecha_ult_modificacion = DateTime.Now;
                                        _repositorio.SaveOrUpdate<SolicitudRegistro>(solicitud);

                                        mensaje = new Mensaje(EnumTipoMensaje.Notificacion, "Notificación", "Verifique el correo de logueo para activar el perfil.");
                                    }
                                    else
                                    {
                                        mensaje = new Mensaje(EnumTipoMensaje.Error, "Error", "Se produjo un error mientras se enviaba el correo. Correo invalido");
                                    }
                                }
                            }
                            else
                            {
                                mensaje = new Mensaje();
                                mensaje.titulo = "Error en validacion de password diferente";
                                mensaje.tipo = "Error";
                                mensaje.cuerpo = "Error en validacion de password diferente";
                            }
                        }
                        else
                        {
                            mensaje = new Mensaje();
                            mensaje.titulo = "Error en validacion de codigo";
                            mensaje.tipo = "Error";
                            mensaje.cuerpo = "Error el codigo no existe o ya fue utilizado";
                        }
                    }
                    else
                    {
                        mensaje = new Mensaje();
                        mensaje.titulo = "Error en validacion de codigo";
                        mensaje.tipo = "Error";
                        mensaje.cuerpo = "Error el codigo es requerido";
                    }
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
        /// Edita una institution institucion.
        /// </summary>
        /// <param name="institution">la institution.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPost]
        public async Task<Mensaje> editInstitution([FromBody]UtilsJson.AInstitucion institution)
        {
            Mensaje mensaje = new Mensaje();

            try
            {
                if (institution != null)
                {
                    if (!string.IsNullOrWhiteSpace(institution.token_string))
                    {
                        if (AutenticacionToken.validateToken(institution.token_string) == 1 && AutenticacionToken.validateUserToken(institution.token_string, institution.logueo.correo_electronico))
                        {
                            string n_correo_electronico_l = string.Empty;
                            string n_password1 = string.Empty;
                            string n_password2 = string.Empty;
                            string contrasena = string.Empty;
                            if (institution.logueo != null)
                            {
                                n_correo_electronico_l = institution.logueo.correo_electronico;
                                n_password1 = institution.logueo.contrasena1;
                                n_password2 = institution.logueo.contrasena2;
                                if (!string.IsNullOrWhiteSpace(n_password1) && !string.IsNullOrWhiteSpace(n_password2))
                                {
                                    contrasena = CifradoDatos.cifrarPassword(n_password1);
                                }
                            }

                            //datos representante
                            string nombre_r = institution.representante.nombre;
                            string apellido_r = institution.representante.nombre;
                            string tipo_identificacion = institution.representante.tipo_identificacion;
                            string identificacion = institution.representante.identificacion;
                            string urlCvlac = institution.representante.urlCvlac;
                            string correo_electronico_r = institution.representante.correo_electronico;

                            //datos institucion
                            string nombre = institution.nombre;
                            string descripcion = institution.descripcion;
                            string correo_electronico = institution.correo_electronico;
                            string direccion_postal = institution.direccion_postal;
                            string facebook = institution.facebook;
                            string fax = institution.fax;
                            string impacto = institution.impacto;
                            string linkedin = institution.linkedin;
                            string naturaleza = institution.naturaleza;
                            string pagina_web = institution.pagina_web;
                            int tamano = institution.tamano;
                            string telefono = institution.telefono;
                            string tipo_institucion = institution.tipo_institucion;
                            string twitter = institution.twitter;
                            string constitucion = institution.constitucion;
                            string fecha_creacion = institution.fecha_creacion;
                            string imagen_base64 = institution.imagen_base64;
                            long municipio_id = institution.municipio.id;

                            UtilsJson.ASector[] sectores = institution.sectores;
                            List<Sector> lstSectores = convertToSector(sectores);

                            UtilsJson.ATipoBiotecnologia[] tipo_biote = institution.Tipos_Biotecnologia;
                            List<TipoBiotecnologia> lstTipoBiotecnologia = convertToTipoBiotecnologia(tipo_biote);
                            //Busco la institucion y el representante asociada al usuario y a la contrasena
                            Expression<Func<Persona, bool>> query = (u => u.institucion.logueo.correo_electronico == institution.logueo.correo_electronico && u.institucion.estado == true);
                            List<Persona> institucion = _repositorio.Filter<Persona>(query);

                            //Si el correo_electronico y la contrasena son validas subo a session la institucion
                            if (institucion != null)
                            {
                                if (institucion.Count > 0)
                                {
                                    Municipio municipioDb = _repositorio.Get<Municipio>(municipio_id);

                                    if (!string.IsNullOrWhiteSpace(contrasena))
                                    {
                                        institucion[0].institucion.logueo.contrasena = contrasena;
                                        institucion[0].institucion.logueo.fecha_ult_modificacion = DateTime.Now;
                                    }


                                    institucion[0].institucion.descripcion = descripcion;
                                    institucion[0].institucion.impacto = impacto;
                                    institucion[0].institucion.correo_electronico = correo_electronico;
                                    institucion[0].institucion.linkedin = linkedin;
                                    institucion[0].institucion.municipio = municipioDb;
                                    institucion[0].institucion.naturaleza = naturaleza;
                                    institucion[0].institucion.constitucion = constitucion;
                                    institucion[0].institucion.nombre = nombre;
                                    institucion[0].institucion.pagina_web = pagina_web;
                                    institucion[0].institucion.tamano = tamano;
                                    institucion[0].institucion.telefono = telefono;
                                    institucion[0].institucion.fax = fax;
                                    institucion[0].institucion.direccion_postal = direccion_postal;
                                    institucion[0].institucion.facebook = facebook;
                                    institucion[0].institucion.tipo_institucion = tipo_institucion;
                                    institucion[0].institucion.twitter = twitter;
                                    institucion[0].institucion.fecha_creacion = (!string.IsNullOrWhiteSpace(fecha_creacion)) ? Convert.ToDateTime(fecha_creacion) : (DateTime?)null;
                                    institucion[0].institucion.fecha_ult_modificacion = DateTime.Now;

                                    if (institucion[0].institucion.banner == null)
                                    {
                                        if (!string.IsNullOrWhiteSpace(imagen_base64))
                                        {
                                            institucion[0].institucion.banner = new Imagen { imagenBase64 = imagen_base64, fecha_ult_modificacion = DateTime.Now };

                                            //Redimension de la imagen y creacion de icono
                                            string extension = imagen_base64.Split(',')[0];
                                            imagen_base64 = imagen_base64.Split(',')[1];

                                            byte[] bytes = Convert.FromBase64String(imagen_base64);

                                            if (bytes.Length > 0)
                                            {
                                                byte[] filebytesIcon = UtilsHost.Redimensionar(Image.FromStream(new MemoryStream(bytes)), 100, 100, 32);

                                                string encodedDataIcon = extension + "," + Convert.ToBase64String(filebytesIcon, Base64FormattingOptions.None);
                                                institucion[0].institucion.icono = new Imagen { imagenBase64 = encodedDataIcon, fecha_ult_modificacion = DateTime.Now };
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (!string.IsNullOrWhiteSpace(imagen_base64))
                                        {
                                            institucion[0].institucion.banner.imagenBase64 = imagen_base64;
                                            institucion[0].institucion.banner.fecha_ult_modificacion = DateTime.Now;

                                            //Redimension de la imagen y creacion de icono
                                            string extension = imagen_base64.Split(',')[0];
                                            imagen_base64 = imagen_base64.Split(',')[1];

                                            byte[] bytes = Convert.FromBase64String(imagen_base64);

                                            if (bytes.Length > 0)
                                            {
                                                byte[] filebytesIcon = UtilsHost.Redimensionar(Image.FromStream(new MemoryStream(bytes)), 100, 100, 32);

                                                string encodedDataIcon = extension + "," + Convert.ToBase64String(filebytesIcon, Base64FormattingOptions.None);
                                                institucion[0].institucion.icono.imagenBase64 = encodedDataIcon;
                                                institucion[0].institucion.icono.fecha_ult_modificacion = DateTime.Now;
                                            }
                                        }
                                    }

                                    //datos del representante
                                    institucion[0].nombre = nombre_r;
                                    institucion[0].apellido = apellido_r;
                                    institucion[0].tipo_identificacion = tipo_identificacion;
                                    institucion[0].identificacion = identificacion;
                                    institucion[0].urlCvlac = urlCvlac;
                                    institucion[0].correo_electronico = correo_electronico_r;
                                    institucion[0].fecha_ult_modificacion = DateTime.Now;

                                    //Envio email confirmacion edicion perfil
                                    StringBuilder bodyMail = new StringBuilder();
                                    bodyMail.AppendLine("Su Perfil ha sido editado exitosamente en biotecred.com .");
                                    bodyMail.AppendLine("Lo invitamos a que inicie sesión y continúe disfrutando de nuestra plataforma.");
                                    string subject = "Confirmación Edición de perfil.";
                                    Mail mail = new Mail(institucion[0].institucion.logueo.correo_electronico, subject, bodyMail);

                                    //Verifico si la institucion existe en el sistema
                                    List<Mensaje> lstVerificaExiste = existeInstitucion(institucion[0]);
                                    List<Mensaje> lstVerificaExisteEmail = mail.existeEmail();

                                    if (lstVerificaExiste.Count == 0 && lstVerificaExisteEmail.Count == 0)
                                    {
                                        institucion[0].fecha_ult_modificacion = DateTime.Now;
                                        _repositorio.SaveOrUpdate<Persona>(institucion[0]);
                                        long id_inst = institucion[0].id;
                                        //Elimino las relaciones de la entidad institucion con sectores y tipoBiotecnologia
                                        Expression<Func<InstitucionSector, bool>> query3 = (u => u.institucion.id == id_inst);
                                        _repositorio.DeleteFilter<InstitucionSector>(query3);
                                        Expression<Func<InstitucionTipoBiotec, bool>> query4 = (u => u.institucion.id == id_inst);
                                        _repositorio.DeleteFilter<InstitucionTipoBiotec>(query4);

                                        //almaceno las relaciones de la entidad institucion con sectores y tipoBiotecnologia
                                        foreach (Sector sector in lstSectores)
                                        {
                                            _repositorio.SaveOrUpdate(new InstitucionSector { institucion = institucion[0].institucion, sector = sector, fecha_ult_modificacion = DateTime.Now });
                                        }

                                        foreach (TipoBiotecnologia tipo in lstTipoBiotecnologia)
                                        {
                                            _repositorio.SaveOrUpdate(new InstitucionTipoBiotec { institucion = institucion[0].institucion, tipoBiotecnologia = tipo, fecha_ult_modificacion = DateTime.Now });
                                        }
                                        //Envio el correo de confirmacion
                                        if (mail.sendMail())
                                        {
                                            mensaje = new Mensaje(EnumTipoMensaje.Notificacion, "Notificación", "Institución editada exitosamente.");
                                        }
                                        else
                                        {
                                            mensaje = new Mensaje(EnumTipoMensaje.Error, "Error", "Se produjo un error mientras se enviaba el correo. Correo invalido");
                                        }
                                    }
                                    else
                                    {
                                        mensaje = new Mensaje(EnumTipoMensaje.Error, "Error validacion", "Existen campos que ya existen en el sistema.");
                                    }
                                }
                                else
                                {
                                    mensaje = new Mensaje(EnumTipoMensaje.Error, "Error Autenticación", "Institucion no encontrada.");
                                }
                            }
                            else
                            {
                                mensaje = new Mensaje(EnumTipoMensaje.Error, "Error Autenticación", "Institución no encontrada.");
                            }
                        }
                        else
                        {
                            mensaje = new Mensaje(EnumTipoMensaje.Expiracion, "Error", "La sesion actual ha expirado. Inicie sesion");
                        }
                    }
                    else
                    {
                        mensaje = new Mensaje(EnumTipoMensaje.Error, "Error Autenticación", "Token de autenticación requerido.");
                    }
                }
                else
                {
                    mensaje = new Mensaje(EnumTipoMensaje.Error, "Error Autenticación", "institucón no encontrada.");
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
        /// Elimina logicamente una institution.
        /// </summary>
        /// <param name="institution">la institution.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPost]
        public Mensaje deleteInstitution([FromBody]UtilsJson.AInstitucion institution)
        {
            Mensaje mensaje = null;
            object[] infoLogin = getExternalIp();

            try
            {
                if (institution != null)
                {
                    if (!string.IsNullOrWhiteSpace(institution.token_string))
                    {
                        if (AutenticacionToken.validateToken(institution.token_string) == 1 && AutenticacionToken.validateUserToken(institution.token_string, institution.logueo.correo_electronico))
                        {
                            //datos logueo
                            string correo_electronico_l = institution.logueo.correo_electronico;
                            string password1 = institution.logueo.contrasena1;
                            string password2 = institution.logueo.contrasena2;

                            if (!string.IsNullOrEmpty(correo_electronico_l) && !string.IsNullOrEmpty(password1) && !string.IsNullOrEmpty(password2))
                            {
                                if (password1.Equals(password2))
                                {
                                    //Cifrado de la contrasena
                                    string contrasena = CifradoDatos.cifrarPassword(password1);

                                    //Busco la institucion asociada al usuario y a la contrasena
                                    Expression<Func<Institucion, bool>> query = (u => u.logueo.correo_electronico == correo_electronico_l && u.logueo.contrasena == password1);
                                    List<Institucion> institucion = _repositorio.Filter<Institucion>(query);

                                    institucion[0].estado = false;
                                    institucion[0].fecha_ult_modificacion = DateTime.Now;
                                    _repositorio.SaveOrUpdate<Institucion>(institucion[0]);

                                    //Creo un token de autenticacion para deshabilitar el perfil
                                    string codigo = CifradoDatos.cifrarRSA(institucion[0].logueo.correo_electronico);

                                    //Envio email confirmacion para deshabilitar el perfil
                                    StringBuilder bodyMail = new StringBuilder();
                                    bodyMail.AppendLine("Para eliminar el perfil de la institucion " + institucion[0].nombre + " dirijase al siguiente enlace.");

                                    string informacionHost = UtilsHost.serverInfoCurrent();
                                    bodyMail.AppendLine("<a href=\"" + informacionHost + "/Institucion/ConfirmationDelete?tokenString=" + codigo + "\">Eliminar perfil.</a>" + "</br>");
                                    bodyMail.AppendLine("Informacion Adicional:" + "</br>");

                                    if (infoLogin != null)
                                    {
                                        bodyMail.AppendLine("Pais:" + infoLogin[2].ToString() + "</br>");
                                        bodyMail.AppendLine("Departamento:" + infoLogin[4].ToString() + "</br>");
                                        bodyMail.AppendLine("Ciudad:" + infoLogin[5].ToString() + "</br>");
                                        bodyMail.AppendLine("Ip Address:" + infoLogin[0].ToString() + "</br>");
                                    }

                                    bodyMail.AppendLine("Fecha:" + DateTime.Now.ToString() + "</br>");

                                    string subject = "Confirmación cancelación de perfil.";

                                    Mail mail = new Mail(institucion[0].logueo.correo_electronico, subject, bodyMail);
                                    mensaje = new Mensaje(EnumTipoMensaje.Notificacion, "Notificacion", "Envio de correo electronico para continuar con el proceso realizado.");

                                }
                                else
                                {
                                    mensaje = new Mensaje(EnumTipoMensaje.Error, "Error Autenticación", "passwords son diferentes.");
                                }
                            }
                            else
                            {
                                mensaje = new Mensaje(EnumTipoMensaje.Error, "Error Autenticación", "passwords son obligatorios.");
                            }
                        }
                        else
                        {
                            mensaje = new Mensaje(EnumTipoMensaje.Expiracion, "Error", "La sesion actual ha expirado. Inicie sesion");
                        }
                    }
                    else
                    {
                        mensaje = new Mensaje(EnumTipoMensaje.Error, "Error Autenticación", "Token de autenticación requerido.");
                    }
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
                mensaje = new Mensaje();
                mensaje.titulo = "Error en tipo de dato";
                mensaje.tipo = "Error";
                mensaje.cuerpo = "Error en la lectura de parametros, verifique que los parametros cumplan con el tipo de dato";
                SystemLog log = new SystemLog();
                log.ErrorLog(sb.ToString());
                throw new Exception(sb.ToString());
            }
            return mensaje;
        }


        /// <summary>
        /// Obtiene las instituciones que cumplan con el filtro especificado.
        /// </summary>
        /// <param name="filter">parametros de filtrado.</param>
        /// <returns></returns>
        [HttpPost]
        public IQueryable<UtilsJson.AInstitucionMapa> getFilterInstitutionMapa([FromBody]UtilsJson.AParamsFilterMapa filter)
        {
            IQueryable<UtilsJson.AInstitucionMapa> listado = null;
            try
            {
                if (filter != null)
                {
                    string opcion = filter.opcion;
                    string sub_opcion = filter.sub_opcion;

                    SqlParameter[] param = new SqlParameter[2];
                    if (opcion == null)
                    {
                        param[0] = new SqlParameter("opcion", DBNull.Value);
                    }
                    else
                    {
                        param[0] = new SqlParameter("opcion", opcion);
                    }
                    if (sub_opcion == null)
                    {
                        param[1] = new SqlParameter("sub_opcion", DBNull.Value);
                    }
                    else
                    {
                        param[1] = new SqlParameter("sub_opcion", sub_opcion);
                    }

                    listado = _repositorio.executeStored<UtilsJson.AInstitucionMapa>("getListingFilterInstitucionMapa", param).Cast<UtilsJson.AInstitucionMapa>().AsQueryable<UtilsJson.AInstitucionMapa>();
                }
            }
            catch (Exception ex)
            {
                SystemLog log = new SystemLog();
                log.ErrorLog(ex.Message);
            }
            return listado;
        }

        /*
         * Verificación de existencia de institucion en el sistema, los parametros tenidos en cuenta para existir son:
         * Nombre institución
         * Email institución
         * Nombre Apellidos Reperesentante institución
         * Cedula Reperesentante institución
         * Email Reperesentante institución
         * Email Logueo
         * param:Representante institucion->Perfil a verificar existencia representante, ligueo,institucion
         * */
        private List<Mensaje> existeInstitucion(Persona institucion)
        {
            List<Mensaje> lstMensajes = new List<Mensaje>();
            Expression<Func<Persona, bool>> query1;
            if (institucion.id > 0)
            {
                query1 = (u => (u.institucion.nombre.ToUpper() == institucion.nombre.ToUpper()
                            || u.institucion.correo_electronico.ToUpper() == institucion.correo_electronico.ToUpper()
                            || u.institucion.logueo.correo_electronico.ToUpper() == institucion.logueo.correo_electronico.ToUpper()
                            )
                            && u.institucion.id != institucion.id
                            && u.institucion.logueo.id != institucion.logueo.id
                            );
            }
            else
            {
                query1 = (u => (u.institucion.nombre.ToUpper() == institucion.nombre.ToUpper()
                            || u.institucion.correo_electronico.ToUpper() == institucion.correo_electronico.ToUpper()
                            || u.institucion.logueo.correo_electronico.ToUpper() == institucion.logueo.correo_electronico.ToUpper()
                            )
                            );
            }
            List<Persona> institucion_existe = _repositorio.Filter<Persona>(query1);
            if (institucion_existe != null)
            {
                foreach (Persona aux in institucion_existe)
                {
                    if (!string.IsNullOrWhiteSpace(aux.nombre) && aux.nombre.ToUpper().Equals(institucion.nombre.ToUpper()))
                    {
                        lstMensajes.Add(new Mensaje(EnumTipoMensaje.Error, "Error", "Ya Existe un perfil de institución con el nombre " + institucion.nombre + ". Verifique los datos suministrados"));
                    }
                    if (!string.IsNullOrWhiteSpace(aux.correo_electronico) && aux.correo_electronico.ToUpper().Equals(institucion.correo_electronico.ToUpper()))
                    {
                        lstMensajes.Add(new Mensaje(EnumTipoMensaje.Error, "Error", "Ya Existe un perfil de institución con el correo electronico " + institucion.correo_electronico + ". Verifique los datos suministrados"));
                    }
                    if (!string.IsNullOrWhiteSpace(aux.logueo.correo_electronico) && aux.logueo.correo_electronico.ToUpper().Equals(institucion.logueo.correo_electronico.ToUpper()))
                    {
                        lstMensajes.Add(new Mensaje(EnumTipoMensaje.Error, "Error", "El correo electronico de logueo " + institucion.logueo.correo_electronico + " ya se encuentra registrado en el sistema. Verifique los datos suministrados"));
                    }
                }
            }
            return lstMensajes;
        }

        /**
       Metodo que obtiene la direccion ip publica del host desde donde se modifica la contraseña
       */
        private object[] getExternalIp()
        {
            string externalIP = string.Empty;
            object[] infoIpAddress = null;
            try
            {
                externalIP = (new WebClient()).DownloadString("http://checkip.dyndns.org/");
                externalIP = (new Regex(@"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}"))
                             .Matches(externalIP)[0].ToString();

                /*-----------------------------------------------------------------------------------------*/
                WebRequest rssReq = WebRequest.Create("http://freegeoip.net/xml/" + externalIP);
                WebProxy px = new WebProxy("http://freegeoip.net/xml/" + externalIP, true);
                rssReq.Proxy = px;
                rssReq.Timeout = 5000;
                WebResponse rep = rssReq.GetResponse();
                XmlTextReader xtr = new XmlTextReader(rep.GetResponseStream());
                DataSet ds = new DataSet();
                ds.ReadXml(xtr);
                infoIpAddress = ds.Tables[0].Rows[0].ItemArray;
            }
            catch (Exception ex)
            {
                SystemLog log = new SystemLog();
                log.ErrorLog(ex.Message);
            }


            return infoIpAddress;
        }

        /// <summary>
        ///  * Verificación de existencia de institucion en el sistema, los parametros tenidos en cuenta para existir son:
        /// Nombre institución
        /// Email institución
        /// Nombre Apellidos Reperesentante institución
        /// Cedula Reperesentante institución
        /// Email Reperesentante institución
        /// Email Logueo
        /// </summary>
        /// <param name=EnumtipoUsuario.institucion>The institucion.</param>
        /// <returns>List&lt;Mensaje&gt;.</returns>
        [HttpPost]
        public List<Mensaje> existeInstitucion([FromBody]UtilsJson.AInstitucion institucion)
        {
            List<Mensaje> listado = new List<Mensaje>();

            Expression<Func<Institucion, bool>> query1;
            if (institucion.id > 0)
            {
                query1 = (u => (u.nombre.ToUpper() == institucion.nombre.ToUpper()
                            || u.correo_electronico.ToUpper() == institucion.correo_electronico.ToUpper()
                            || u.logueo.correo_electronico.ToUpper() == institucion.logueo.correo_electronico.ToUpper())
                            && u.id != institucion.id
                            && u.id != institucion.id
                            && u.logueo.id != institucion.logueo.id
                            );
            }
            else
            {
                query1 = (u => (u.nombre.ToUpper() == institucion.nombre.ToUpper()
                            || u.correo_electronico.ToUpper() == institucion.correo_electronico.ToUpper()
                            || u.logueo.correo_electronico.ToUpper() == institucion.logueo.correo_electronico.ToUpper())
                            );
            }

            List<Institucion> institucion_existe = _repositorio.Filter<Institucion>(query1);

            if (institucion_existe != null)
            {
                foreach (Institucion aux in institucion_existe)
                {
                    if (!string.IsNullOrWhiteSpace(aux.nombre) && aux.nombre.ToUpper().Equals(institucion.nombre.ToUpper()))
                    {
                        listado.Add(new Mensaje(EnumTipoMensaje.Error, "Error", "Ya Existe un perfil de institución con el nombre " + institucion.nombre + ". Verifique los datos suministrados"));
                    }
                    if (!string.IsNullOrWhiteSpace(aux.correo_electronico) && aux.correo_electronico.ToUpper().Equals(institucion.correo_electronico.ToUpper()))
                    {
                        listado.Add(new Mensaje(EnumTipoMensaje.Error, "Error", "Ya Existe un perfil de institución con el correo electronico " + institucion.correo_electronico + ". Verifique los datos suministrados"));
                    }
                    if (!string.IsNullOrWhiteSpace(aux.logueo.correo_electronico) && aux.logueo.correo_electronico.ToUpper().Equals(institucion.logueo.correo_electronico.ToUpper()))
                    {
                        listado.Add(new Mensaje(EnumTipoMensaje.Error, "Error", "El correo electronico de logueo " + institucion.logueo.correo_electronico + " ya se encuentra registrado en el sistema. Verifique los datos suministrados"));
                    }
                }
            }

            //Verifico la existencia del correo
            Mail mail = new Mail(institucion.logueo.correo_electronico, string.Empty, new StringBuilder());
            List<Mensaje> lstVerificaExisteEmail = mail.existeEmail();
            listado.AddRange(lstVerificaExisteEmail);

            return listado;
        }

        /// <summary>
        /// Convierte un objeto tipo institucion a json.
        /// </summary>
        /// <param name=EnumtipoUsuario.institucion>The institucion.</param>
        /// <param name="lst_sector">The lst_sector.</param>
        /// <param name="lst_tipoB">The lst_tipo b.</param>
        /// <returns>UtilsJson.AInstitucion.</returns>
        private static UtilsJson.AInstitucion convertToInstitucion(Persona usuario, List<InstitucionSector> lst_sector, List<InstitucionTipoBiotec> lst_tipoB, bool auth)
        {
            UtilsJson.AInstitucion aux = null;

            if (usuario != null)
            {
                if (usuario.institucion != null)
                {
                    long id_institucion = usuario.institucion.id;
                    string descripcion = usuario.institucion.descripcion;
                    UtilsJson.AMunicipio municipio = convertToAMunicipio(usuario.institucion.municipio);
                    string correo_electronico = usuario.institucion.correo_electronico;
                    string direccion_postal = usuario.institucion.direccion_postal;
                    string facebook = usuario.institucion.facebook;
                    string fax = usuario.institucion.fax;
                    string impacto = usuario.institucion.impacto;
                    string latitud = usuario.institucion.latitud;
                    string linkedin = usuario.institucion.linkedin;
                    string longitud = usuario.institucion.longitud;
                    string naturaleza = usuario.institucion.naturaleza;
                    string nombre = usuario.institucion.nombre;
                    string pagina_web = usuario.institucion.pagina_web;
                    int tamano = usuario.institucion.tamano;
                    string telefono = usuario.institucion.telefono;
                    string tipo_institucion = usuario.institucion.tipo_institucion;
                    string twitter = usuario.institucion.twitter;
                    string constitucion = usuario.institucion.constitucion;
                    string imagen_base64 = (usuario.institucion.banner != null) ? usuario.institucion.banner.imagenBase64 : "";
                    string fecha_creacion = usuario.institucion.fecha_creacion.ToString();
                    long visitas = usuario.institucion.visitas;
                    UtilsJson.ALogueo logueo = (auth) ? convertToALogueoInstitucion(usuario.institucion.logueo) : null;
                    UtilsJson.APersona representante = convertToAPersona(usuario);
                    UtilsJson.ASector[] sectores = convertToASector(lst_sector);
                    UtilsJson.ATipoBiotecnologia[] Tipos_Biotecnologia = convertToATipoBiotecnologia(lst_tipoB);
                    aux = new UtilsJson.AInstitucion
                    {
                        id = id_institucion,
                        descripcion = descripcion,
                        municipio = municipio,
                        correo_electronico = correo_electronico,
                        direccion_postal = direccion_postal,
                        facebook = facebook,
                        fax = fax,
                        impacto = impacto,
                        latitud = latitud,
                        linkedin = linkedin,
                        longitud = longitud,
                        naturaleza = naturaleza,
                        nombre = nombre,
                        pagina_web = pagina_web,
                        tamano = tamano,
                        telefono = telefono,
                        tipo_institucion = tipo_institucion,
                        twitter = twitter,
                        constitucion = constitucion,
                        imagen_base64 = imagen_base64,
                        fecha_creacion = fecha_creacion,
                        visitas = visitas,
                        logueo = logueo,
                        representante = representante,
                        sectores = sectores,
                        Tipos_Biotecnologia = Tipos_Biotecnologia
                    };

                }

                return aux;
            }
            return null;
        }

        /// <summary>
        /// Convierte un tipo Municipio a un objeto json.
        /// </summary>
        /// <param name="municipio">The municipio.</param>
        /// <returns>BTR_Services.Models.UtilsJson.AMunicipio.</returns>
        private static UtilsJson.AMunicipio convertToAMunicipio(Municipio municipio)
        {
            if (municipio != null)
            {
                UtilsJson.AMunicipio aux = new UtilsJson.AMunicipio { id = municipio.id, nombre = municipio.nombre, cod_municipio = municipio.cod_municipio, id_departamento = municipio.departamento.id };
                return aux;
            }
            return null;
        }

        /// <summary>
        /// Convierte un objeto tipo logueo a json.
        /// </summary>
        /// <param name="logueo">The logueo.</param>
        /// <returns>BTR_Services.Models.UtilsJson.ALogueo.</returns>
        private static UtilsJson.ALogueo convertToALogueoInstitucion(LogueoInstitucion logueo)
        {
            if (logueo != null)
            {
                UtilsJson.ALogueo aux = new UtilsJson.ALogueo
                {
                    id = logueo.id,
                    correo_electronico = logueo.correo_electronico,
                    rol = EnumtipoUsuario.institucion
                };
                return aux;
            }
            return null;
        }

        /// <summary>
        /// Converts un objecto tipo representante a json persona.
        /// </summary>
        /// <param name="representante">The representante.</param>
        /// <returns>BTR_Services.Models.UtilsJson.APersona.</returns>
        private static UtilsJson.APersona convertToAPersona(Persona representante)
        {
            if (representante != null)
            {
                UtilsJson.APersona aux = new UtilsJson.APersona
                {
                    id = representante.id,
                    nombre = representante.nombre,
                    apellido = representante.apellido,
                    tipo_identificacion = representante.tipo_identificacion,
                    identificacion = representante.identificacion,
                    correo_electronico = representante.correo_electronico,
                    urlCvlac = representante.urlCvlac,
                    perfil_profesional = representante.perfil_profesional
                };
                if (representante.logueo != null)
                {
                    aux.logueo = new UtilsJson.ALogueo
                    {
                        id = representante.logueo.id,
                        correo_electronico = representante.correo_electronico,
                        rol = representante.logueo.rol
                    };
                }
                if (representante.foto != null)
                {
                    aux.foto = representante.foto.imagenBase64;
                }
                return aux;
            }
            return null;
        }

        /// <summary>
        /// Convierte una lista de tipobiotecnologia a un objeto json.
        /// </summary>
        /// <param name="lst_tipoB">The lst_tipo b.</param>
        /// <returns>BTR_Services.Models.UtilsJson.ATipoBiotecnologia[].</returns>
        private static UtilsJson.ATipoBiotecnologia[] convertToATipoBiotecnologia(List<InstitucionTipoBiotec> lst_tipoB)
        {
            if (lst_tipoB != null)
            {
                List<UtilsJson.ATipoBiotecnologia> listado = new List<UtilsJson.ATipoBiotecnologia>();
                foreach (InstitucionTipoBiotec aux in lst_tipoB)
                {
                    listado.Add(new UtilsJson.ATipoBiotecnologia { id = aux.id, color = aux.tipoBiotecnologia.color, descripcion = aux.tipoBiotecnologia.descripcion });
                }
                return listado.ToArray();
            }
            return null;
        }

        /// <summary>
        /// Convierte una lista de tipo sector a un objeto json.
        /// </summary>
        /// <param name="lst_sectores">The lst_sectores.</param>
        /// <returns>BTR_Services.Models.UtilsJson.ASector[].</returns>
        private static UtilsJson.ASector[] convertToASector(List<InstitucionSector> lst_sectores)
        {
            if (lst_sectores != null)
            {
                List<UtilsJson.ASector> listado = new List<UtilsJson.ASector>();
                foreach (InstitucionSector aux in lst_sectores)
                {
                    listado.Add(new UtilsJson.ASector { id = aux.id, nombre = aux.sector.nombre, descripcion = aux.sector.descripcion });
                }
                return listado.ToArray();
            }
            return null;
        }

        /// <summary>
        /// Convierte una lista de Asector a tipo Sector.
        /// </summary>
        /// <param name="sectores">The sectores.</param>
        /// <returns>List&lt;Sector&gt;.</returns>
        private List<Sector> convertToSector(UtilsJson.ASector[] sectores)
        {
            if (sectores != null)
            {
                List<Sector> listado = new List<Sector>();
                foreach (UtilsJson.ASector aux in sectores)
                {
                    Expression<Func<Sector, bool>> query = (u => u.id == aux.id);
                    List<Sector> sec_aux = _repositorio.Filter<Sector>(query);
                    if (sec_aux != null)
                    {
                        if (sec_aux.Count > 0)
                        {
                            listado.Add(sec_aux[0]);
                        }
                    }
                }
                return listado;
            }

            return null;
        }

        /// <summary>
        /// Convierte unalista de tipo Abiotecnologia a tipo biotecnologia.
        /// </summary>
        /// <param name="biotec">The biotec.</param>
        /// <returns>List&lt;TipoBiotecnologia&gt;.</returns>
        private List<TipoBiotecnologia> convertToTipoBiotecnologia(UtilsJson.ATipoBiotecnologia[] biotec)
        {
            if (biotec != null)
            {
                List<TipoBiotecnologia> listado = new List<TipoBiotecnologia>();
                foreach (UtilsJson.ATipoBiotecnologia aux in biotec)
                {
                    Expression<Func<TipoBiotecnologia, bool>> query = (u => u.id == aux.id);
                    List<TipoBiotecnologia> sec_aux = _repositorio.Filter<TipoBiotecnologia>(query);
                    if (sec_aux != null)
                    {
                        if (sec_aux.Count > 0)
                        {
                            listado.Add(sec_aux[0]);
                        }
                    }
                }
                return listado;
            }

            return null;
        }

    }
}
