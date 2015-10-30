using BTR_Services.Models;
using BTR_Services.Persistencia;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace BTR_Services.Api
{
    public class ContactoController : ApiController
    {
        private readonly IRepositorio _repositorio;

        public ContactoController(Repositorio repository)
        {
            _repositorio = repository;
        }

        public IQueryable<UtilsJson.AContacto> getContactoAll()
        {
            IQueryable<UtilsJson.AContacto> listado = _repositorio.executeStored<UtilsJson.AContacto>("getListingcomentarios", null).Cast<UtilsJson.AContacto>().AsQueryable<UtilsJson.AContacto>();
            return listado;
        }

        public Contacto getContactoId(long id)
        {
            Contacto contacto = null;
            contacto = _repositorio.Get<Contacto>(id);
            return contacto;
        }

        [HttpPost]
        public Mensaje createComentario([FromBody]UtilsJson.AContacto comentario)
        {
            Mensaje mensaje = null;

            try
            {
                //datos logueo
                string nombre = comentario.nombre_contacto;
                string telefono = comentario.telefono_contacto;
                string email = comentario.email_contacto;
                string messangeC = comentario.comentario_contacto;

                Contacto contacto = new Contacto(nombre, telefono, email, messangeC);
                contacto.fecha_ult_modificacion = DateTime.Now;
                _repositorio.SaveOrUpdate(contacto);
                //Envio email confirmacion para habilitar el perfil
                StringBuilder bodyMail = new StringBuilder();
                bodyMail.AppendLine("Comentario enviado por: " + nombre);
                bodyMail.AppendLine("Telefono: " + telefono);
                bodyMail.Append(comentario);
                string subject = "Comentario o sugerencia. " + email;
                Mail mail = new Mail("contacto@biotecred.co", subject, bodyMail);
                mail.sendMail();
                mensaje = new Mensaje(EnumTipoMensaje.Notificacion, "Notificación", "Comentario procesado exitosamente.");
            }
            catch (Exception ex)
            {
                mensaje = new Mensaje(EnumTipoMensaje.Error, "Error", "Ocurrio un error mientras se procesaba su solicitud.");
            }
            return mensaje;
        }
    }
}
