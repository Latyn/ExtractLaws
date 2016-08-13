using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExtractLaws.Models;
using Microsoft.Office.Interop.Word;

namespace ExtractLaws
{
    public class Program
    {

        public static void Main(string[] args)
        {

            Executer main = new Executer();
            Application application = new Application();
            var Objects =  main.getDirectories();
            var name = string.Empty;

            foreach (var item in Objects)
            {
                var doc = main.InteropReading(item.ToString(),application);

                if (doc != null)
                {
                    main.saveDoc(doc);
                    main.movingFile(doc.LawName);
                }
                else
                {
                    name = item.Split('\\').Last();
                    name = name.Split('.').First();
                    main.movingCheckingFile(name);
                }


            }
        }
    }
}
