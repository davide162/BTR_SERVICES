using BTR_Services.Models;
using BTR_Services.Persistencia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace BTR_Services.Api
{
    public class AutorController : ApiController
    {
        private readonly IRepositorio _repositorio;

        public AutorController(Repositorio repository)
        {
            _repositorio = repository;
        }

        public IQueryable<Persona> getAutoresAll()
        {
            IQueryable<Persona> listado = _repositorio.executeStored<Persona>("getListingAutores", null).Cast<Persona>().AsQueryable<Persona>();
            return listado;
        }

        public UtilsJson.APersona getAutorId(long id)
        {
            Persona autor = null;
            UtilsJson.APersona persona = new UtilsJson.APersona();
            autor = _repositorio.Get<Persona>(id);
            if (autor != null)
            {
                persona.id = autor.id;
                persona.nombre = autor.nombre;
                persona.apellido = autor.apellido;
                persona.tipo_identificacion = autor.tipo_identificacion;
                persona.identificacion = autor.identificacion;
                persona.urlCvlac = autor.urlCvlac;
                persona.correo_electronico = autor.correo_electronico;
                return persona;
            }
            return null;
        }

        [HttpPost]
        public Mensaje createAutor([FromBody]UtilsJson.APersona autor)
        {
            Mensaje mensaje = null;

            try
            {
                /*
                //datos logueo
                string correo_logueo = autor.correo_logueo;
                string contrasena = autor.contrasena;


                //datos publicacion
                string tipo_identificacion = autor.tipo_identificacion;
                double identificacion = autor.identificacion;
                string nombre_persona = autor.nombre;
                string apellido_persona = autor.apellido;
                string correo_persona = autor.correo_electronico;
                string urlCvlac = autor.urlCvlac;

                if (!String.IsNullOrWhiteSpace(correo_logueo) && !String.IsNullOrWhiteSpace(contrasena))
                {
                    //Cifrado de la contrasena
                    contrasena = CifradoDatos.cifrarPassword(contrasena);

                    //Busco la institucion asociada al usuario y a la contrasena
                    Expression<Func<Representante, bool>> query = (u => u.institucion.logueo.correo_electronico == correo_logueo && u.institucion.logueo.contrasena == contrasena && u.institucion.estado == true);

                    List<Representante> institucion = _repositorio.Filter<Representante>(query);
                    
                    if (institucion != null)
                    {
                        if (institucion.Count > 0)
                        {
                            Persona persona = new Persona(nombre_persona, apellido_persona, tipo_identificacion, identificacion, urlCvlac, correo_persona);
                            
                            Autor autorB = new Autor(institucion[0].institucion, persona);

                            _repositorio.SaveOrUpdate<Autor>(autorB);

                            mensaje = new Mensaje(EnumTipoMensaje.Notificacion, "Notificación", "Autor registrada exitosamente.");

                        }
                        else
                        {
                            mensaje = new Mensaje(EnumTipoMensaje.Error, "Error", "Correo logueo y contraseña son requeridos");
                        }
                    }
                    else
                    {
                        mensaje = new Mensaje(EnumTipoMensaje.Error, "Error", "Correo logueo y contraseña son requeridos");
                    }
                }
                else
                {
                    mensaje = new Mensaje(EnumTipoMensaje.Error, "Error", "Correo logueo y contraseña son requeridos");
                }*/
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
        [HttpPost]
        public Mensaje editAutor([FromBody]UtilsJson.APersona autor)
        {
            Mensaje mensaje = null;

            try
            {/*
                //datos logueo
                string correo_logueo = autor.correo_logueo;
                string contrasena = autor.contrasena;


                //datos publicacion
                long id = autor.id;
                string tipo_identificacion = autor.tipo_identificacion;
                double identificacion = autor.identificacion;
                string nombre_persona = autor.nombre;
                string apellido_persona = autor.apellido;
                string correo_persona = autor.correo_electronico;
                string urlCvlac = autor.urlCvlac;

                if (!String.IsNullOrWhiteSpace(correo_logueo) && !String.IsNullOrWhiteSpace(contrasena))
                {
                    //Cifrado de la contrasena
                    contrasena = CifradoDatos.cifrarPassword(contrasena);

                    //Busco la institucion asociada al usuario y a la contrasena
                    Expression<Func<Representante, bool>> query = (u => u.institucion.logueo.correo_electronico == correo_logueo && u.institucion.logueo.contrasena == contrasena && u.institucion.estado == true);

                    List<Representante> institucion = _repositorio.Filter<Representante>(query);

                    Autor autorB = _repositorio.Get<Autor>(id);

                    if (institucion != null && autorB != null)
                    {
                        if (institucion.Count > 0)
                        {
                            if (autorB.institucion.id == institucion[0].institucion.id)
                            {
                                autorB.persona.tipo_identificacion = tipo_identificacion;
                                autorB.persona.identificacion = identificacion;
                                autorB.persona.nombre = nombre_persona;
                                autorB.persona.apellido = apellido_persona;
                                autorB.persona.correo_electronico = correo_persona;
                                autorB.persona.urlCvlac = urlCvlac;
                                
                                _repositorio.SaveOrUpdate<Autor>(autorB);

                                mensaje = new Mensaje(EnumTipoMensaje.Notificacion, "Notificación", "Autor editado exitosamente.");
                            }
                            else
                            {
                                mensaje = new Mensaje(EnumTipoMensaje.Error, "Error", "El autor no hace parte de la institucion especificada");
                            }
                        }
                        else
                        {
                            mensaje = new Mensaje(EnumTipoMensaje.Error, "Error", "Correo logueo y contraseña son requeridos");
                        }
                    }
                    else
                    {
                        mensaje = new Mensaje(EnumTipoMensaje.Error, "Error", "Correo logueo y contraseña son requeridos");
                    }
                }
                else
                {
                    mensaje = new Mensaje(EnumTipoMensaje.Error, "Error", "Correo logueo y contraseña son requeridos");
                }*/
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
        [HttpPost]
        public Mensaje deleteAutor([FromBody]UtilsJson.APersona autor)
        { 
        Mensaje mensaje = null;

            try
            {/*
                //datos logueo
                string correo_logueo = autor.correo_logueo;
                string contrasena = autor.contrasena;


                //datos publicacion
                long id_autor = autor.id;
                

                if (!String.IsNullOrWhiteSpace(correo_logueo) && !String.IsNullOrWhiteSpace(contrasena))
                {
                    //Cifrado de la contrasena
                    contrasena = CifradoDatos.cifrarPassword(contrasena);

                    //Busco la institucion asociada al usuario y a la contrasena
                    Expression<Func<Autor, bool>> query = (u => u.institucion.logueo.correo_electronico == correo_logueo && u.institucion.logueo.contrasena == contrasena && u.institucion.estado == true && u.id == id_autor);

                    List<Autor> autorB = _repositorio.Filter<Autor>(query);

                    if (autorB != null)
                    {
                        if (autorB.Count > 0)
                        {
                            long id_persona = autorB[0].persona.id;
                            _repositorio.Delete<Autor>(autorB[0].id);
                            _repositorio.Delete<Persona>(id_persona);
                            mensaje = new Mensaje(EnumTipoMensaje.Notificacion, "Notificación", "Autor eliminado exitosamente.");
                        }
                        else
                        {
                            mensaje = new Mensaje(EnumTipoMensaje.Error, "Error", "El autor no hace parte de la institucion especificada o no existe");
                        }
                    }
                    else
                    {
                        mensaje = new Mensaje(EnumTipoMensaje.Error, "Error", "El autor no hace parte de la institucion especificada o no existe");
                    }
                }
                else
                {
                mensaje = new Mensaje(EnumTipoMensaje.Error, "Error", "Correo de logueo y contraseña son requeridos");
                }*/
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
                //Guardo los errores que se producieron durante la insercion
                SystemLog log = new SystemLog();
                log.ErrorLog(sb.ToString());
            }
            return mensaje;
        }    
        
        
    }
}
