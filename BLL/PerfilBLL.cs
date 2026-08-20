using BE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL
{
    public class PerfilBLL
    {
        public List<PerfilBE> Listar()
        {
            DAL.PerfilDAL PerfilDal = new DAL.PerfilDAL();
            return PerfilDal.Listar();
        }
    }
}
