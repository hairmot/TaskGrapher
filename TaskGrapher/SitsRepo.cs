using System;
using System.Collections.Generic;
using System.Linq;

namespace TaskGrapher
{
    public static class SitsRepo
    {
        public static bool ParseYN(string s)
        {
            return s?.ToLower() == "y";
        }

        public static string StringJoin(this IEnumerable<object> elements, string separator)
        {
            return string.Join(separator, elements);
        }

        public static Dictionary<decimal, TTE> getTTENameToSeq3Dictionary(SITSDEVEntities sde, string tkt_code)
        {
            return sde.men_tte.Where(a => a.tte_tktc == tkt_code)
                .AsEnumerable()
                .Select(a => new {
                    key = a.tte_seq3,
                    value = new TTE()
                    {
                        TTE_NAME = a.tte_step,
                        TTE_DISP = ParseYN(a.tte_disp) ? "" : "dotted",
                        TTE_IUSE = ParseYN(a.tte_iuse) ? "" : "RED"
                    }
                })
                .GroupBy(a => a.key)
                .Select(b => b.First())
                .ToDictionary(a => a.key, a => a.value);
        }

        public static string getTTEsForTask(SITSDEVEntities sde, string tkt_code)
        {
            return
                sde.men_tte.Where(a => a.tte_tktc == tkt_code && a.tte_nxts.Length >= 1)
                .Select(a => a.tte_step + " -> " + a.tte_nxts)
                .StringJoin(Environment.NewLine);
        }

        public static string getTECsForTask(SITSDEVEntities sde, string tkt_code,Dictionary<Decimal, TTE> TTENameSeqDictionary )
        {
            return 
                sde.men_tec
                .Where(a => a.tec_iuse == "Y" && a.tec_tktc == tkt_code)
                .AsEnumerable()
                .Select(a => TTENameSeqDictionary[a.tec_ttes].TTE_NAME + " -> " + a.tec_step + "[label=\"" + a.tec_desc + "\"]" + ";")
                .StringJoin(Environment.NewLine);
        }
    }
}
