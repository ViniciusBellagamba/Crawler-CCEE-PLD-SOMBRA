using System;
using System.Linq;
using HtmlAgilityPack;
using RestSharp;
using System.IO;
using System.Globalization;
using System.Configuration;
using System.Net;
using System.Collections.Generic;
using System.Timers;
using System.Threading.Tasks;

namespace CCEE
{
    public class CCEE
    {
        private readonly Timer _timer;
        private readonly double frequency = Math.Abs(Convert.ToDouble(ConfigurationManager.AppSettings["FREQUENCY"]));

        static void Logger(string erro)
        {
            StreamWriter logger = new StreamWriter(Path.Combine(Directory.GetCurrentDirectory(), "log.txt"), true);
            logger.WriteLine(erro);
            logger.WriteLine("********");
            logger.Close();
        }

        static List<string> CookieList()
        {
            ServicePointManager.ServerCertificateValidationCallback +=
(sender, certificate, chain, sslPolicyErrors) => true;

            List<string> Lista = new List<string>();
            var request = (HttpWebRequest)WebRequest.Create("https://www.ccee.org.br/portal/faces/pages_publico/o-que-fazemos/como_ccee_atua/precos/preco_sombra?_adf.ctrl-state=qqqscrbh7_9&_afrLoop=102973988701849");
            request.CookieContainer = new CookieContainer();

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                foreach (Cookie cook in response.Cookies)
                    Lista.Add($"{cook.Name.Trim()}={cook.Value.Trim()}");

                string RequestCook = request.Headers.ToString().Split('{', '}')[0];

                foreach (var x in RequestCook.Split(';'))
                    Lista.Add(x.Replace("\r\n", "").Replace(" ", "").Trim());
            }
            return Lista;
        }

        static string GetCCEE_Sombra(DateTime DataInicio, DateTime DataFinal)
        {
            List<string> cookies = CookieList();

            ServicePointManager.ServerCertificateValidationCallback +=
(sender, certificate, chain, sslPolicyErrors) => true;

            RestClient Client = new RestClient("https://www.ccee.org.br");
            var request = new RestRequest("/portal/faces/oracle/webcenter/portalapp/pages/publico/oquefazemos/produtos/precos/lista_preco_horario_sombra.jspx", Method.POST);
            request.AddParameter("text/plain", $"periodo=" +
                $"{DataInicio:dd\\%\\2\\FMM\\%\\2\\Fyyyy}" +
                $"+-+" +
                $"{DataFinal:dd\\%\\2\\FMM\\%\\2\\Fyyyy}", ParameterType.RequestBody);
            foreach (var cookie in cookies)
            {
                var valore = cookie.Split('=');
                request.AddParameter(valore[0], valore[1], ParameterType.Cookie);
            }
            request.AddHeader("Origin", "https://www.ccee.org.br");
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
            request.AddHeader("Referer", "https://www.ccee.org.br/portal/faces/pages_publico/o-que-fazemos/como_ccee_atua/precos/preco_sombra?_afrLoop=189813672914290&_adf.ctrl-state=10npzs8mcz_14");
            IRestResponse response = Client.Execute(request);
            var content = response.Content;
            return content;
        }

        static void Print_Arquivos_Sombra()
        {
            string path = ConfigurationManager.AppSettings["PATH"];
            string DiasRetroativos = ConfigurationManager.AppSettings["DAYS"];
            int Dias = -Math.Abs(Convert.ToInt32(DiasRetroativos));
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(GetCCEE_Sombra(DateTime.Now.AddDays(Dias), DateTime.Now));

            var ItemList = doc.DocumentNode.SelectNodes("//table[@id='lista_preco_horario_sombra']")//Todos TDs na tabela
.Descendants("tr")
.Where(tr => tr.Elements("td").Count() > 1)
.Select(tr => tr.Elements("td").Select(td => td.InnerText.Trim()).ToList())
.ToList();


            var Datas = doc.DocumentNode.SelectNodes("//table[@id='lista_preco_horario_sombra']")//Header de Datas
.Descendants("tr")
.Where(tr => tr.Elements("th").Count() > 1 && tr.InnerText.Contains("/"))
.Select(tr => tr.Elements("th").Where(td => td.InnerText.Contains("/"))
.Select(td => td.InnerText.Trim()).ToList()).ToList();

            StreamWriter SUL = new StreamWriter(Path.Combine(path, "SOMBRA_SUL.txt"));
            StreamWriter SUDESTE = new StreamWriter(Path.Combine(path, "SOMBRA_SUDESTE.txt"));
            StreamWriter NORTE = new StreamWriter(Path.Combine(path, "SOMBRA_NORTE.txt"));
            StreamWriter NORDESTE = new StreamWriter(Path.Combine(path, "SOMBRA_NORDESTE.txt"));

            foreach (var Linhas in ItemList)
            {
                int i = 0;
                foreach (var Celula in Linhas.Skip(2))
                {
                    string MergeDt = $"{Datas[0][i]} {Linhas[0]}:00:00";
                    DateTime Dt = DateTime.ParseExact(MergeDt, $"dd/MM/yyyy HH:00:00", CultureInfo.InvariantCulture);
                    Dt = Dt.AddHours(3);
                    switch (Linhas[1])
                    {
                        case "SUL":
                            SUL.WriteLine("{0};{1};{2}", Dt.ToString("dd/MM/yyyy HH:mm:ss"), Linhas[1], Celula);
                            break;
                        case "SUDESTE":
                            SUDESTE.WriteLine("{0};{1};{2}", Dt.ToString("dd/MM/yyyy HH:mm:ss"), Linhas[1], Celula);
                            break;
                        case "NORTE":
                            NORTE.WriteLine("{0};{1};{2}", Dt.ToString("dd/MM/yyyy HH:mm:ss"), Linhas[1], Celula);
                            break;
                        case "NORDESTE":
                            NORDESTE.WriteLine("{0};{1};{2}", Dt.ToString("dd/MM/yyyy HH:mm:ss"), Linhas[1], Celula);
                            break;
                    }
                    i++;
                }
            }
            SUL.Close();
            SUDESTE.Close();
            NORTE.Close();
            NORDESTE.Close();
        }

        public CCEE()
        {
            _timer = new Timer(1000 * 60 * frequency) { AutoReset = true };
            _timer.Elapsed += TimerElapsed;
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                Print_Arquivos_Sombra();
            }
            catch (Exception y)
            { Logger(y.ToString()); }
        }

        public void Start()
        {
            Task.Run(() =>Adjust_timer());
        }

        public async void Adjust_timer()
        {
            try
            {
                Print_Arquivos_Sombra();
            }
            catch (Exception y)
            { Logger(y.ToString()); }
            double t = Math.Round(DateTime.Now.Minute % frequency, 0);
            int x = Math.Abs(Convert.ToInt32(frequency - t));
            System.Threading.Thread.Sleep(x * 60 * 1000);
            try
            {
                Print_Arquivos_Sombra();
            }
            catch (Exception y)
            { Logger(y.ToString()); }
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }
    }
}
