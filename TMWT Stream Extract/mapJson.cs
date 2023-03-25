using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TMWT_Stream_Extract
{
    public class mapJson
    {
        public List<mapInstanceJson> maps { get; set; }
    }

    public class mapInstanceJson
    {
        public string mapName { get; set; }
        public List<double> CPs { get; set; }
        public int Identity { get; set; }
    }
}
