using System;
using Topshelf;

namespace CCEE
{
    class Program
    {
        static void Main()
        {
            var exitCode = HostFactory.Run(x =>
            {
                x.Service<CCEE>(s =>
                {
                    s.ConstructUsing(ccee => new CCEE());
                    s.WhenStarted(ccee => ccee.Start());
                    s.WhenStopped(ccee => ccee.Stop());
                });

                x.RunAsLocalSystem();

                x.SetServiceName("CCEE_Service");
                x.SetDisplayName("CCEE");
                x.SetDescription("Retorna em formato txt Preço de Liquidação das Diferenças (PLD) por submercado. Serviço acessa a seguinte página para realizar essa operação: \r\n" +
                    "https://www.ccee.org.br/portal/faces/pages_publico/o-que-fazemos/como_ccee_atua/precos/preco_sombra");
            });

            int exitCodeValue = (int)Convert.ChangeType(exitCode, exitCode.GetTypeCode());
            Environment.ExitCode = exitCodeValue;
        }
    }
}
