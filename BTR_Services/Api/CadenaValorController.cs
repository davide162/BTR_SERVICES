using BTR_Services.Models;
using BTR_Services.Persistencia;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace BTR_Services.Api
{
    public class CadenaValorController : ApiController
    {

        private readonly IRepositorio _repositorio;

        public CadenaValorController(Repositorio repository)
        {
            _repositorio = repository;
        }


        [HttpPost]
        public IQueryable<UtilsJson.AInstitucion> getCadenaValorFilter([FromBody] UtilsJson.ACadenaValor cadena_valor)
        {
            IQueryable<UtilsJson.AInstitucion> listado=null;
            try
            {
                string tipo_ente=cadena_valor.tipo_institucion;
                string nombre_institucion=cadena_valor.nombre_institucion;
                long[] id_tipo_biotecnologia =cadena_valor.ids_tipo_biotecnologia;
                long id_sector = cadena_valor.id_sector;
                SqlParameter[] param = new SqlParameter[4];
                if (tipo_ente == null)
                {
                    param[0] = new SqlParameter("tipo", DBNull.Value);
                }
                else
                {
                    param[0] = new SqlParameter("tipo", tipo_ente);
                }

                if (nombre_institucion == null)
                {
                    param[1] = new SqlParameter("nombre", DBNull.Value);
                }
                else
                {
                    param[1] = new SqlParameter("nombre", nombre_institucion);
                }
                if (id_tipo_biotecnologia == null)
                {
                    param[2] = new SqlParameter("biotecnologia", DBNull.Value);
                }
                else
                {
                    string stringids = string.Empty;
                    for (int i = 0; i < id_tipo_biotecnologia.Length; i++)
                    {
                        stringids += id_tipo_biotecnologia[i] + "|";
                    }
                        param[2] = new SqlParameter("biotecnologia", stringids);
                }
                if (id_sector == 0)
                {
                    param[3] = new SqlParameter("sector", DBNull.Value);
                }
                else
                {
                    param[3] = new SqlParameter("sector", id_sector);
                }
                listado = _repositorio.executeStored<UtilsJson.AInstitucion>("getFilterCadenaValor", param).Cast<UtilsJson.AInstitucion>().AsQueryable<UtilsJson.AInstitucion>();

            }
            catch (Exception ex)
            {
                SystemLog log = new SystemLog();
                log.ErrorLog(ex.Message);
            }

            return listado;
        }
    }
}
