using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Office.Interop.Word;
using ExtractLaws.Entities;
using System.Globalization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ExtractLaws.Models;
using System.Data.SqlClient;
using System.Data;
using System.Data.SqlTypes;

namespace ExtractLaws
{
    public class Executer
    {

        public string FilePath = @"C:\\Costa Rica\\Leyes-Actualizadas";
        string connection;
        private SqlConnection m_db;
        string targetPath = @"C:\\Costa Rica\\Leyes-Actualizadas\\Checked";
        string targetCheckingPath = @"C:\\Costa Rica\\Leyes-Actualizadas\\Checking";

        public ApplicationDbContext _context;

        public ApplicationDbContext Context
        {
            get
            {
                return _context;
            }

            set
            {
                _context = value;
            }
        }

        internal void movingCheckingFile(string name)
        {
            if (!Directory.Exists(targetCheckingPath))
            {
                Directory.CreateDirectory(targetCheckingPath);
            }


            string sourceFile = System.IO.Path.Combine(FilePath, name + ".doc");
            string destFile = System.IO.Path.Combine(targetCheckingPath, name.Trim() + ".doc");

            // To copy a file to another location and 
            // overwrite the destination file if it already exists.

            if (File.Exists(sourceFile))
            {
                File.Move(sourceFile, destFile);
            }
            else
            {
                sourceFile = System.IO.Path.Combine(FilePath, name + ".docx");
                destFile = System.IO.Path.Combine(targetCheckingPath, name.Trim() + ".docx");

                if (File.Exists(sourceFile))
                {
                    File.Move(sourceFile, destFile);
                }
                else
                {
                    sourceFile = System.IO.Path.Combine(FilePath, name + ".rtf");
                    destFile = System.IO.Path.Combine(targetCheckingPath, name.Trim() + ".rtf");

                    File.Move(sourceFile, destFile);
                }

            }


        }

        public Executer()
        {
            var builder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json");
            var config = builder.Build();
            connection = config["Data:DefaultConnection:ConnectionString"];

            IServiceCollection services = new ServiceCollection();
            services.AddEntityFramework()
            .AddSqlServer()
            .AddDbContext<ApplicationDbContext>();

            m_db = new SqlConnection(connection);
            m_db.Open();

        }

        public string getDocuments(string path)
        {
            if (File.Exists(path))
            {
                IEnumerable<string> lines = File.ReadLines(path, Encoding.ASCII);
                var law = lines.First().Trim().ToString();
                return File.ReadAllText(path);
            }
            else
            {
                return null;
            }
        }

        public bool saveDoc(Law newLaw)
        {


            var result = false;

            using (var cmd = m_db.CreateCommand())
            {
                cmd.CommandTimeout = 0;
                cmd.CommandText = @"INSERT INTO Law (Body, Created, DigitalVersionDate, Kind, LawDate, LawName, Publication, Valid) 
                VALUES (@body,
                @created,
                @digitalversiondate,
                @kind,
                @lawdate,
                @lawname,
                @publication,
                @valid)";

                cmd.Parameters.Add("body", SqlDbType.VarChar).Value = newLaw.Body.Trim();
                cmd.Parameters.Add("created", SqlDbType.DateTime).Value = DateTime.Now;
                cmd.Parameters.Add("digitalversiondate", SqlDbType.DateTime).Value = newLaw.DigitalVersionDate;
                cmd.Parameters.Add("kind", SqlDbType.VarChar).Value = newLaw.Kind;
                cmd.Parameters.Add("lawdate", SqlDbType.DateTime).Value = newLaw.LawDate;
                cmd.Parameters.Add("lawname", SqlDbType.VarChar).Value = newLaw.LawName;
                cmd.Parameters.Add("publication", SqlDbType.VarChar).Value = newLaw.Publication;
                cmd.Parameters.Add("valid", SqlDbType.VarChar).Value = newLaw.Valid;

                if (cmd.ExecuteNonQuery() > 0)
                {
                    result = true;
                }


                return result;
            }


            //cmd.CommandText = "INSERT INTO Law (Body, Created, DigitalVersionDate, Kind, LawDate, LawName, Publication, Valid) VALUES ("+ newLaw.Body + "," 
            //    + newLaw.Created + ","+ newLaw.DigitalVersionDate + "," + newLaw.Kind + "," + newLaw.LawDate + "," + newLaw.LawName + "," + newLaw.Publication + "," + newLaw.Valid + ")";


        }

        public void movingFile(string path)
        {
            if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
            }


            string sourceFile = System.IO.Path.Combine(FilePath, path + ".doc");
            string destFile = System.IO.Path.Combine(targetPath, path.Trim() + ".doc");

            // To copy a file to another location and 
            // overwrite the destination file if it already exists.

            if (File.Exists(sourceFile))
            {
                File.Move(sourceFile, destFile);
            }
            else
            {
                sourceFile = System.IO.Path.Combine(FilePath, path + ".docx");
                destFile = System.IO.Path.Combine(targetPath, path.Trim() + ".docx");

                if (File.Exists(sourceFile))
                {
                    File.Move(sourceFile, destFile);
                }
                else
                {
                    sourceFile = System.IO.Path.Combine(FilePath, path + ".rtf");
                    destFile = System.IO.Path.Combine(targetPath, path.Trim() + ".rtf");

                    File.Move(sourceFile, destFile);
                }

            }


        }

        public List<string> getDirectories()
        {
            if (Directory.Exists(FilePath))
            {
                List<string> directories = new List<string>();

                foreach (var arrItem in Directory.GetFiles(FilePath))
                {
                    directories.Add(arrItem);
                }
                return directories;
            }

            return null;
        }

        public Law InteropReading(string path, Application application)
        {
            object confirmConversions = false;
            object readOnly = true;
            object visible = false;
            object missing = Type.Missing;


            Document document = application.Documents.Open(path, ref confirmConversions, ref readOnly, ref missing,
            ref missing, ref missing, ref missing, ref missing,
            ref missing, ref missing, ref missing, ref visible,
            ref missing, ref missing, ref missing, ref missing);
            document.Activate();

            string[] formats = { "dd-MM-yyyy", "dd/MM/yyyy" };

            string text = string.Empty;
            var Rige = string.Empty;
            string Publicacion = string.Empty;
            DateTime Sancion = new DateTime();
            DateTime Digitalizada = new DateTime();

            // Loop through all words in the document.
            int count = document.Words.Count;
            Paragraph val = document.Paragraphs.Last;
            int RangeCount = 8;

            if (count > 48)
            {
                while (!val.Range.Text.StartsWith("Rige") & RangeCount > 0)
                {
                    RangeCount = RangeCount - 1;
                    val = val.Previous();
                }
                if (val.Range.Text.StartsWith("Rige"))
                {
                    Rige = val.Range.Text.Split(':').Last().Trim();
                }

                val = document.Paragraphs.Last;
                RangeCount = 8;

                while (!val.Range.Text.StartsWith("Publicación Decreto") & RangeCount > 0)
                {
                    RangeCount = RangeCount - 1;
                    val = val.Previous();
                }
                if (val.Range.Text.StartsWith("Publicación Decreto"))
                {
                    Publicacion = val.Range.Text.Replace("Publicación Decreto:", "").Trim();
                }
                else
                {
                    val = document.Paragraphs.Last;
                    RangeCount = 8;

                    while (!val.Range.Text.StartsWith("Publicación") & RangeCount > 0)
                    {
                        RangeCount = RangeCount - 1;
                        val = val.Previous();
                    }
                    if (val.Range.Text.StartsWith("Publicación"))
                    {
                        Publicacion = val.Range.Text.Replace("Publicación:", "").Trim();
                    }
                    else
                    {
                        while (!val.Range.Text.StartsWith("Fecha de publicación") & RangeCount > 0)
                        {
                            RangeCount = RangeCount - 1;
                            val = val.Previous();
                        }
                        if (val.Range.Text.StartsWith("Fecha de publicación"))
                        {
                            Publicacion = val.Range.Text.Replace("Fecha de publicación:", "").Trim();
                        }
                    }
                }


                while (!val.Range.Text.StartsWith("Sanción") & RangeCount > 0)
                {
                    RangeCount = RangeCount - 1;
                    val = val.Previous();
                }
                if (val.Range.Text.StartsWith("Sanción"))
                {
                    var sanDate = val.Range.Text.Split(':').Last().Replace(";", "").Replace("–", "-").Replace(".", "").Replace("Sanción", "").Trim().ToString();

                    
                    if (sanDate == "Veto" || sanDate == "VETO" || sanDate == "Resello" || sanDate == "(veto)" || sanDate == "(VETO)")
                    {
                        Sancion = DateTime.Parse(SqlDateTime.MinValue.ToString(), System.Globalization.CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        if (sanDate != "")
                        {
                            sanDate = changeFecha(sanDate);
                            sanDate = checkFormat(sanDate);

                            Sancion = DateTime.ParseExact(sanDate, formats, System.Globalization.CultureInfo.InvariantCulture, DateTimeStyles.None);
                        }
                        else
                        {
                            Sancion = DateTime.Parse(SqlDateTime.MinValue.ToString(), System.Globalization.CultureInfo.InvariantCulture);
                        }
                    }

                }
                else
                {
                    val = document.Paragraphs.Last;
                    RangeCount = 8;

                    while (!val.Range.Text.StartsWith("Resello") & RangeCount > 0)
                    {
                        RangeCount = RangeCount - 1;
                        val = val.Previous();
                    }
                    if (val.Range.Text.StartsWith("Resello"))
                    {
                        var sanDate = val.Range.Text.Split(':').Last().Replace(".", "").Trim().ToString();


                        if (sanDate == "Veto" || sanDate == "VETO")
                        {
                            sanDate = changeFecha(sanDate);
                            sanDate = checkFormat(sanDate);

                            Sancion = DateTime.Parse(SqlDateTime.MinValue.ToString(), System.Globalization.CultureInfo.InvariantCulture);
                        }
                        else
                        {
                            if (sanDate != "")
                            {
                                Sancion = DateTime.ParseExact(sanDate, formats, System.Globalization.CultureInfo.InvariantCulture, DateTimeStyles.None);
                            }
                            else
                            {
                                Sancion = DateTime.Parse(SqlDateTime.MinValue.ToString(), System.Globalization.CultureInfo.InvariantCulture);
                            }
                        }

                    }
                    else
                    {
                        val = document.Paragraphs.Last;
                        RangeCount = 8;

                        while (!val.Range.Text.StartsWith("Veto") & RangeCount > 0)
                        {
                            RangeCount = RangeCount - 1;
                            val = val.Previous();
                        }
                        if (val.Range.Text.StartsWith("Veto"))
                        {
                            var sanDate = val.Range.Text.Split(':').Last().Replace(".", "").Trim().ToString();


                            if (sanDate == "Veto" || sanDate == "VETO")
                            {
                                Sancion = DateTime.Parse(SqlDateTime.MinValue.ToString(), System.Globalization.CultureInfo.InvariantCulture);
                            }
                            else
                            {
                                if (sanDate != "")
                                {
                                    sanDate = changeFecha(sanDate);
                                    sanDate = checkFormat(sanDate);
                                    if (sanDate == "--")
                                    {
                                        Sancion = DateTime.Parse(SqlDateTime.MinValue.ToString(), System.Globalization.CultureInfo.InvariantCulture);
                                    }
                                    else
                                    {
                                        Sancion = DateTime.ParseExact(sanDate, formats, System.Globalization.CultureInfo.InvariantCulture, DateTimeStyles.None);
                                    }
                                }
                                else
                                {
                                    Sancion = DateTime.Parse(SqlDateTime.MinValue.ToString(), System.Globalization.CultureInfo.InvariantCulture);
                                }
                            }

                        }
                        else
                        {
                            val = document.Paragraphs.Last;
                            RangeCount = 8;

                            while (!val.Range.Text.StartsWith("Fecha de sanción") & RangeCount > 0)
                            {
                                RangeCount = RangeCount - 1;
                                val = val.Previous();
                            }
                            if (val.Range.Text.StartsWith("Fecha de sanción"))
                            {
                                var sanDate = val.Range.Text.Split(':').Last().Replace(".", "").Trim().ToString();


                                if (sanDate == "Veto" || sanDate == "VETO")
                                {
                                    Sancion = DateTime.Parse(SqlDateTime.MinValue.ToString(), System.Globalization.CultureInfo.InvariantCulture);
                                }
                                else
                                {
                                    if (sanDate != "")
                                    {
                                        sanDate = sanDate.Replace("(Veto)", "");
                                        sanDate = changeFecha(sanDate);
                                        if (sanDate.Contains("*"))
                                        {
                                            sanDate = "";
                                        }
                                        else
                                        {
                                            sanDate = checkFormat(sanDate);
                                        }


                                        if (sanDate == "--" || sanDate == "")
                                        {
                                            Sancion = DateTime.Parse(SqlDateTime.MinValue.ToString(), System.Globalization.CultureInfo.InvariantCulture);
                                        }
                                        else
                                        {
                                            Sancion = DateTime.ParseExact(sanDate, formats, System.Globalization.CultureInfo.InvariantCulture, DateTimeStyles.None);
                                        }
                                    }
                                    else
                                    {
                                        Sancion = DateTime.Parse(SqlDateTime.MinValue.ToString(), System.Globalization.CultureInfo.InvariantCulture);
                                    }
                                }

                            }
                            else
                            {

                                val = document.Paragraphs.Last;
                                RangeCount = 8;

                                while (!val.Range.Text.StartsWith("SANCIÓN") & RangeCount > 0)
                                {
                                    RangeCount = RangeCount - 1;
                                    val = val.Previous();
                                }
                                if (val.Range.Text.StartsWith("SANCIÓN"))
                                {
                                    var sanDate = val.Range.Text.Split(':').Last().Replace("SANCIÓN", "").Replace(".", "").Trim().ToString();


                                    if (sanDate == "Veto" || sanDate == "VETO")
                                    {
                                        Sancion = DateTime.Parse(SqlDateTime.MinValue.ToString(), System.Globalization.CultureInfo.InvariantCulture);
                                    }
                                    else
                                    {
                                        if (sanDate != "")
                                        {
                                            sanDate = changeFecha(sanDate);
                                            sanDate = checkFormat(sanDate);

                                            Sancion = DateTime.ParseExact(sanDate, formats, System.Globalization.CultureInfo.InvariantCulture, DateTimeStyles.None);
                                        }
                                        else
                                        {
                                            Sancion = DateTime.Parse(SqlDateTime.MinValue.ToString(), System.Globalization.CultureInfo.InvariantCulture);
                                        }
                                    }

                                }
                                else
                                {
                                    val = document.Paragraphs.Last;
                                    RangeCount = 8;

                                    while (!val.Range.Text.StartsWith("VETO") & RangeCount > 0)
                                    {
                                        RangeCount = RangeCount - 1;
                                        val = val.Previous();
                                    }
                                    if (val.Range.Text.StartsWith("VETO"))
                                    {
                                        var sanDate = val.Range.Text.Split(':').Last().Replace(".", "").Trim().ToString();


                                        if (sanDate == "Veto" || sanDate == "VETO" || sanDate == "VETO.-" || sanDate == "VETO-")
                                        {
                                            Sancion = DateTime.Parse(SqlDateTime.MinValue.ToString(), System.Globalization.CultureInfo.InvariantCulture);
                                        }
                                        else
                                        {
                                            if (sanDate != "")
                                            {
                                                sanDate = changeFecha(sanDate);
                                                sanDate = checkFormat(sanDate);

                                                Sancion = DateTime.ParseExact(sanDate, formats, System.Globalization.CultureInfo.InvariantCulture, DateTimeStyles.None);
                                            }
                                            else
                                            {
                                                Sancion = DateTime.Parse(SqlDateTime.MinValue.ToString(), System.Globalization.CultureInfo.InvariantCulture);
                                            }
                                        }

                                    }
                                    else
                                    {
                                        val = document.Paragraphs.Last;
                                        RangeCount = 8;

                                        while (!val.Range.Text.StartsWith("Decreto vetado") & RangeCount > 0)
                                        {
                                            RangeCount = RangeCount - 1;
                                            val = val.Previous();
                                        }
                                        if (val.Range.Text.StartsWith("Decreto vetado"))
                                        {
                                            var sanDate = val.Range.Text.Split(':').Last().Replace(".", "").Trim().ToString();
                                            sanDate = changeFecha(sanDate);
                                            sanDate = checkFormat(sanDate);

                                            Sancion = DateTime.ParseExact(sanDate, formats, System.Globalization.CultureInfo.InvariantCulture, DateTimeStyles.None);
                                        }
                                        else
                                        {

                                            val = document.Paragraphs.Last;
                                            RangeCount = 8;

                                            while (!val.Range.Text.StartsWith("DECRETO LEY VETADO.-") & RangeCount > 0)
                                            {
                                                RangeCount = RangeCount - 1;
                                                val = val.Previous();
                                            }
                                            if (val.Range.Text.StartsWith("DECRETO LEY VETADO.-"))
                                            {
                                                var sanDate = val.Range.Text.Split(':').Last().Replace(".", "").Trim().ToString();

                                                if (sanDate == "DECRETO LEY VETADO-")
                                                {
                                                    Sancion = DateTime.Parse(SqlDateTime.MinValue.ToString(), System.Globalization.CultureInfo.InvariantCulture);
                                                }
                                                else
                                                {
                                                    Sancion = DateTime.Parse(SqlDateTime.MinValue.ToString(), System.Globalization.CultureInfo.InvariantCulture);
                                                }
                                            }
                                            else
                                            {
                                                Sancion = DateTime.Parse(SqlDateTime.MinValue.ToString(), System.Globalization.CultureInfo.InvariantCulture);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                }


                val = document.Paragraphs.Last;
                RangeCount = 8;

                while (!val.Range.Text.StartsWith("Digitalizada") & RangeCount > 0)
                {
                    RangeCount = RangeCount - 1;
                    val = val.Previous();
                }
                if (val.Range.Text.StartsWith("Digitalizada"))
                {

                    var digDate = val.Range.Text.Replace("al", ":").Split(':').Last().Replace(".", "").Trim().ToString();

                    if (digDate != "")
                    {
                        digDate = changeFecha(digDate);
                        if (digDate.Contains('/') & digDate.Contains('-'))
                        {
                            digDate = digDate.Replace('/', '-');
                        }

                        digDate = checkFormat(digDate);

                        Digitalizada = DateTime.ParseExact(digDate, formats, System.Globalization.CultureInfo.InvariantCulture, DateTimeStyles.None);
                    }
                    else
                    {
                        Digitalizada = DateTime.Parse(SqlDateTime.MinValue.ToString(), System.Globalization.CultureInfo.InvariantCulture);
                    }

                }
                else
                {
                    val = document.Paragraphs.Last;
                    RangeCount = 8;

                    while (!val.Range.Text.StartsWith("Actualizada") & RangeCount > 0)
                    {
                        RangeCount = RangeCount - 1;
                        val = val.Previous();
                    }
                    if (val.Range.Text.StartsWith("Actualizada"))
                    {
                        var digDate = val.Range.Text.Replace("  ", " ").Replace("--", "-").Replace(";", "").Replace("Actualizada","").Replace("al", ":").Split(':').Last().Replace(".", "").Trim().ToString();

                        if (digDate != "")
                        {
                            digDate = changeFecha(digDate);
                            digDate = checkFormat(digDate);

                            Digitalizada = DateTime.ParseExact(digDate, formats, System.Globalization.CultureInfo.InvariantCulture, DateTimeStyles.None);
                        }
                        else
                        {
                            Digitalizada = DateTime.Parse(SqlDateTime.MinValue.ToString(), System.Globalization.CultureInfo.InvariantCulture);
                        }

                    }
                    else
                    {
                        Digitalizada = DateTime.Parse(SqlDateTime.MinValue.ToString(), System.Globalization.CultureInfo.InvariantCulture);
                    }

                }


                var Content = document.Content.Text;
                var localUrl = document.FullName;
                var name = document.Name.Split('.').First();

                Law newLaw = new Law(name, Sancion, Content, "Vigentes", Digitalizada, Publicacion, Rige);

                // Write the word.

                Console.WriteLine("Word {0} = {1}", name, localUrl);

                // Close word.

                document.Close();
                return newLaw;
            }

            document.Close();
            return null;
        }

        private string checkFormat(string digDate)
        {
            var format= new String[3];

            if (digDate.Contains('/'))
            {
                 format = digDate.Split('/');
            }
            else
            {
                 format = digDate.Split('-');
            }

            var day = CheckDay(format[0].ToString());
            var year = CheckYear(format[2].ToString());

            digDate = day + "-" + format[1].ToString() + "-" + year;
            return digDate;
        }

        private string CheckYear(string year)
        {
            year = year.Replace("del", "de");

            if (int.Parse(year) >50 & int.Parse(year) <100)
            {
                return "19" + year;
            }
            if (int.Parse(year) > 9 & int.Parse(year) < 16)
            {
                return "20" + year;
            }
            if (int.Parse(year) > 0 & int.Parse(year) < 10)
            {
                if (year.Contains("0")) 
                {
                    return "20" + year;
                }
                else
                {
                    return "200" + year;
                }

            }
            return year;
        }

        private string changeFecha(string date)
        {
            var newDate = date.Replace("del", "de");

            if (newDate.Contains(" de enero de "))
            {
                newDate = newDate.Replace(" de enero de ", "-01-");
            }
            else
            {
                if (newDate.Contains(" de Enero de "))
                {
                    newDate = newDate.Replace(" de Enero de ", "-01-");
                }
            }

            if (newDate.Contains(" de febrero de "))
            {
                newDate = newDate.Replace(" de febrero de ", "-02-");
            }
            else
            {
                if (newDate.Contains(" de Febrero de "))
                {
                    newDate = newDate.Replace(" de Febrero de ", "-02-");
                }
            }

            if (newDate.Contains(" de marzo de "))
            {
                //var start = CheckDay(date.First());
                newDate = newDate.Replace(" de marzo de ", "-03-");
            }
            else
            {
                if (newDate.Contains(" de Marzo de "))
                {
                    newDate = newDate.Replace(" de Marzo de ", "-03-");
                }
            }

            if (newDate.Contains(" de abril de "))
            {
                newDate = newDate.Replace(" de abril de ", "-04-");
            }
            else
            {
                if (newDate.Contains(" de Abril de "))
                {
                    newDate = newDate.Replace(" de Abril de ", "-04-");

                }
            }

            if (date.Contains(" de mayo de "))
            {
                newDate = newDate.Replace(" de mayo de ", "-05-");
            }
            else
            {
                if (newDate.Contains(" de Mayo de "))
                {
                    newDate = newDate.Replace(" de Mayo de ", "-05-");
                }
            }

            if (newDate.Contains(" de junio de "))
            {
                newDate = newDate.Replace(" de junio de ", "-06-");
            }
            else
            {
                if (newDate.Contains(" de Junio de "))
                {
                    newDate = newDate.Replace(" de Junio de ", "-06-");
                }
            }

            if (newDate.Contains(" de julio de "))
            {
                newDate = newDate.Replace(" de julio de ", "-07-");
            }
            else
            {
                if (newDate.Contains(" de Julio de "))
                {
                    newDate = newDate.Replace(" de Julio de ", "-07-");
                }
            }

            if (newDate.Contains(" de agosto de "))
            {
                newDate = newDate.Replace(" de agosto de ", "-08-");
            }
            else
            {
                if (date.Contains(" de Agosto de "))
                {
                    newDate = newDate.Replace(" de Agosto de ", "-08-");
                }
            }

            if (newDate.Contains(" de setiembre de "))
            {
                newDate = newDate.Replace(" de setiembre de ", "-09-");
            }
            else
            {
                if (newDate.Contains(" de Setiembre de "))
                {
                    newDate = newDate.Replace(" de Setiembre de ", "-09-");
                }
            }

            if (newDate.Contains(" de septiembre de "))
            {
                newDate = newDate.Replace(" de septiembre de ", "-09-");
            }
            else
            {
                if (newDate.Contains(" de Septiembre de "))
                {
                    newDate = newDate.Replace(" de Septiembre de ", "-09-");
                }
            }

            if (newDate.Contains(" de octubre de "))
            {
                newDate = newDate.Replace(" de octubre de ", "-10-");
            }
            else
            {
                if (newDate.Contains(" de Octubre de "))
                {
                    newDate = newDate.Replace(" de Octubre de ", "-10-");
                }
            }

            if (newDate.Contains(" de noviembre de "))
            {
                newDate = newDate.Replace(" de noviembre de ", "-11-");
            }
            else
            {
                if (newDate.Contains(" de Noviembre de "))
                {
                    newDate = newDate.Replace(" de Noviembre de ", "-11-");
                }
            }

            if (newDate.Contains(" de diciembre de "))
            {
                newDate = newDate.Replace(" de diciembre de ", "-12-");
            }
            else
            {
                if (newDate.Contains(" de Diciembre de "))
                {
                    newDate = newDate.Replace(" de Diciembre de ", "-12-");
                }
            }

            if (newDate.Contains("-1-"))
            {
                newDate = newDate.Replace("-1-", "-01-");
            }
            else
            {
                if (newDate.Contains("/1/"))
                {
                    newDate = newDate.Replace("/1/", "/01/");
                }
            }

            if (newDate.Contains("-2-"))
            {
                newDate = newDate.Replace("-2-", "-02-");
            }
            else
            {
                if (newDate.Contains("/2/"))
                {
                    newDate = newDate.Replace("/2/", "/02/");
                }
            }

            if (newDate.Contains("-3-"))
            {
                newDate = newDate.Replace("-3-", "-03-");
            }
            else
            {
                if (newDate.Contains("/3/"))
                {
                    newDate = newDate.Replace("/3/", "/03/");
                }
            }


            if (newDate.Contains("-4-"))
            {
                newDate = newDate.Replace("-4-", "-04-");
            }
            else
            {
                if (newDate.Contains("/4/"))
                {
                    newDate = newDate.Replace("/4/", "/04/");
                }
            }

            if (newDate.Contains("-5-"))
            {
                newDate = newDate.Replace("-5-", "-05-");
            }
            else
            {
                if (newDate.Contains("/5/"))
                {
                    newDate = newDate.Replace("/5/", "/05/");
                }
            }

            if (newDate.Contains("-6-"))
            {
                newDate = newDate.Replace("-6-", "-06-");
            }
            else
            {
                if (newDate.Contains("/6/"))
                {
                    newDate = newDate.Replace("/6/", "/06/");
                }
            }

            if (newDate.Contains("-7-"))
            {
                newDate = newDate.Replace("-7-", "-07-");
            }
            else
            {
                if (newDate.Contains("/7/"))
                {
                    newDate = newDate.Replace("/7/", "/07/");
                }
            }

            if (newDate.Contains("-8-"))
            {
                newDate = newDate.Replace("-8-", "-08-");
            }
            else
            {
                if (newDate.Contains("/8/"))
                {
                    newDate = newDate.Replace("/8/", "/08/");
                }
            }

            if (newDate.Contains("-9-"))
            {
                newDate = newDate.Replace("-9-", "-09-");
            }
            else
            {
                if (newDate.Contains("/9/"))
                {
                    newDate = newDate.Replace("/9/", "/09/");
                }
            }

            return newDate.Replace(" ", string.Empty).Replace("Veto", "").Replace("VETO", ""); ;
        }

        private object CheckDay(string day)
        {
            string newDay = day.Replace("del", "de"); ;

            if (day == "1")
            {
                newDay = "01";

            }
            if (day == "2")
            {
                newDay = "02";

            }
            if (day == "3")
            {
                newDay = "03";

            }
            if (day == "4")
            {
                newDay = "04";

            }
            if (day == "5")
            {
                newDay = "05";

            }
            if (day == "6")
            {
                newDay = "06";

            }
            if (day == "7")
            {
                newDay = "07";

            }
            if (day == "8")
            {
                newDay = "08";

            }
            if (day == "9")
            {
                newDay = "09";

            }

            return newDay;
        }
    }
}