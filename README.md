# Crawler-CCEE-PLD-SOMBRA
Retorna em formato txt o Preço de Liquidação das Diferenças (PLD) por submercado. Tais documentos podem ser obtidos no endereço: 
https://www.ccee.org.br/portal/faces/pages_publico/o-que-fazemos/como_ccee_atua/precos/preco_sombra

## Variáveis de ambiente 
Para executar o serviço, as seguintes keys do documento .config devem ser configuradas:
```
DAYS --> Número de dias retroativos na busca do PLD;
PATH --> Diretório onde os documentos .txt devem ser gerados;
FREQUENCY --> Frequência em minutos que o serviço deve ser executado;
```

## Instalação serviço 
```
CCEE.exe install start
```

## Desinstalação serviço
```
CCEE.exe uninstall
```
