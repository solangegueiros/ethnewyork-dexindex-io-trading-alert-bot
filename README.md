# Caspian Tradex - Dexindex.io trading alert bot
# ethnewyork 2019

## Description
My project at Hackaton EthNewYork 2019 - Caspian Tradex
Bot for trading on decentralized exchanges using dexindex.io from AirSwap

## Inspiration
Decentralized exchanges can has diferent prices to same asset at same time and is possible to take profits trading it.

## What it does
It check the prices using dexindex.io from AirSwap, verify the cheapest price to buy and the most expensive price to sell. After it save the operation at google sheet. 

You can check the trades and profit here:

https://docs.google.com/spreadsheets/d/1QZSbRHv_GS9o4zQrNSC_JCKajnFUNK75EL7C-O2tfRA/edit?usp=sharing


It can be to trading any asset with ETH, I'm using to trade DAI, but it is configurable.
Caspian Tradex is able to configurate a pause beween the executions and can be use 24h per day.

## How I built it
I use Visual Studio, to build a C# console application.
I am hosting a web server that serves price data in response to HTTP requests, using ethereum-dex-prices-service  - airswap dexindex.io


https://github.com/perich/ethereum-dex-prices-service


## Challenges I ran into
I try to use the airswap's example to build a front-end, but I can not did work.
I do not have time to integrate to check balances, calculates fees and put orders.

## Accomplishments that I'm proud of
I was able to integrate with the web server and consult the quotes, besides calculating the most profitable operation.

## What I learned
I learned how to use dexindex.io, hosting the web server and think about trades and profits.

## What's next for Trading alert bot using airswap dexindex.io
In future it will check the balances at exchanges and will do the trades automatically. 

I would like to integrate with the decentralized exchanges, check balances in each of them, calculates fees and put orders automatically.

It will make money for me while I'm sleeping :)


# Install

## web server ethereum-dex-prices-service
https://github.com/perich/ethereum-dex-prices-service


Follow instructions to star the web server with dex prices
https://github.com/perich/ethereum-dex-prices-service#getting-started

## CaspianTradex


## appsettings.json

At file appsettings.json are defined all configurations

Comments about each parameter are written at file.

## appsettings.json Example

```c#
//CaspianTrade
{
  /* spreadGain is calculated in % 
  */
  "spreadGain": "0.0010",

  /* Pause between executions 
    In miliseconds
    1000 miliseg = 1s
    5000 miliseg = 5s
    10000 miliseg = 10s
    30000 miliseg = 30s
    60000 miliseg 1 min
  "execPause": 60000,
  */
  "execPause": 10000,

  /* O agendador de tarefas executa com a frequencia minima de 5 minutos
      Para executar 5 vezes a cada 5 minutos, o numeroExecucoes deve ser 4 (1 sempre executa, então 5 = 1 + 4)
  */
  "numeroExecucoes": 100000,

  /* debug = true => show messages at console
  */
  "debug": true,

  "market": "DAI-ETH",
  "asset": "DAI",
  "amountDefault": "1",

  "GoogleConfig": {
    "spreadsheetId": "XXX", 
    "sheetTrades": "DaiEth"
  }

}
```

## Compile

At project directory, run:

``` javascript
dotnet publish -c Release -r win7-x64
```


