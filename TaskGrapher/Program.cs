using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskGrapher
{
    class Program
    {
        private static string output = "#!dot";

        static void Main(string[] args)
        {
            SITSDEVEntities sde = new SITSDEVEntities();
            Console.WriteLine("Please insert the task code");
            promptForInput(sde);
        }

        static void promptForInput(SITSDEVEntities sde)
        {
            var input = Console.ReadLine();
            if (sde.men_tkt.Where(a => a.tkt_code == input).Count() != 1)
            {
                Console.WriteLine("No Tasks found for code: " + input + ". Please try again");
                promptForInput(sde);
            }
            else
            {
                getBuildDetails(sde, input);
                System.IO.File.WriteAllText("output.txt", output);
                System.Diagnostics.Process.Start(@"output.txt");
            }
        }

        static void getBuildDetails(SITSDEVEntities sde, String tkt_code)
        {           
            //begin output file text    
            addToOuput("digraph {label=\"" + sde.men_tkt.Where(a => a.tkt_code == tkt_code).First().tkt_name + " - " + tkt_code + "\"");
            addToOuput("node [fontname = \"Calibri\", fontsize = 16];");
            addToOuput("graph[fontname = \"Calibri\", fontsize = 12];");
            addToOuput("edge[fontname = \"Calibri\", fontsize = 12];");


            //dictionary to translate tte_seq3 -> tte_name - also provides info on conditional steps and hidden steps
            var TTENameSeqDictionary = SitsRepo.getTTENameToSeq3Dictionary(sde, tkt_code);

            //cross reference TEC to make affected TTEs 'conditional'
            foreach (var TEC in sde.men_tec.Where(a => a.tec_tktc == tkt_code))
            {
                TTENameSeqDictionary.Where(a => a.Key == TEC.tec_ttes).First().Value.cond = true;
            }

            //draw out some styling info for each node
            foreach (KeyValuePair<decimal, TTE> T in TTENameSeqDictionary)
            {
                addToOuput(T.Value.TTE_NAME + "[shape=\"" + (T.Value.cond ? "diamond" : "box") + "\", style=\"" + T.Value.TTE_DISP + "\", color=\"" + T.Value.TTE_IUSE + "\"]");
            }

            //print out list of relationships for TTEs AND TECs
            addToOuput(SitsRepo.getTTEsForTask(sde, tkt_code));

            addToOuput(SitsRepo.getTECsForTask(sde, tkt_code, TTENameSeqDictionary));    

            //Close off Output
            addToOuput("}");

        }

        private static void addToOuput(string text)
        {
            output += Environment.NewLine + text;
        }

      
    }

}