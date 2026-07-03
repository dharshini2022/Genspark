## Workflow
```
                    YOUR MAC
┌───────────────────────────────────────────────┐
│                                               │
│  Azure CLI                                    │
│      │                                        │
│      ▼                                        │
│ az deployment group create                    │
│      │                                        │
│      ▼                                        │
│ Azure creates VM + installs Kafka + .NET 8    │
│      │                                        │
│      ▼                                        │
│ ssh azureuser@<VM_IP>                         │
└──────────────┬────────────────────────────────┘
               │ SSH Connection
               ▼
        AZURE VM (Ubuntu)
┌──────────────────────────────────────────────────────────────┐
│                                                              │
│ Part 1                                                       │
│ ──────────────────────────────────────────────────────────   │
│ Verify:                                                      │
│   java -version                                              │
│   dotnet --version                                           │
│   kafka-topics.sh                                            │
│                                                              │
│ Part 2                                                       │
│ ──────────────────────────────────────────────────────────   │
│ dotnet new webapi                                            │
│ Add Kafka package                                             │
│ Create Models                                                 │
│ Create Controller                                              │
│ Replace Program.cs                                             │
│                                                              │
│ dotnet run                                                    │
│                                                              │
│           Web API running on localhost:5100                  │
│                                                              │
│                 │                                             │
│                 ▼                                             │
│          Produces messages                                   │
│                 │                                             │
│                 ▼                                             │
│          Kafka Topic: payment-events                         │
│                                                              │
│ Part 3                                                       │
│ ──────────────────────────────────────────────────────────   │
│ Open SECOND SSH session                                      │
│                                                              │
│ dotnet new console                                           │
│ Add Kafka package                                            │
│ Replace Program.cs                                           │
│                                                              │
│ dotnet run                                                   │
│                                                              │
│      HTTP POST ─────────────► Web API                        │
│                                                              │
│      Kafka Consumer ◄──────── payment-events                 │
│                                                              │
└──────────────────────────────────────────────────────────────┘
```


## Payment Simulator
```
                Payment Simulator
             (This Program - Console App)
                      │
        ┌─────────────┴─────────────┐
        │                           │
        │                           │
        ▼                           ▼
HTTP POST                    Kafka Consumer
/api/payment/process         payment-events
        │                           ▲
        ▼                           │
   Payment Web API ───────────────► Kafka
          │                          Topic
          ▼
 Business Logic
(APPROVED / DECLINED)
```

## Payment Proccessor
```
PaymentSimulator (Console App)
  ┌────────────────────────────────────────────────────────┐
  │  [Producer Thread]  →  POST /api/payment/process      │
  │                         │                              │
  │                    PaymentProcessor (WebAPI)           │
  │                         │  ProduceAsync()             │
  │                         ▼                              │
  │                  Kafka Topic: payment-events           │
  │                         │                              │
  │  [Consumer Thread]  ←  Consume()                       │
  │         [OK] APPROVED  /  [--] DECLINED                │
  └────────────────────────────────────────────────────────┘
  All running on one Azure VM (Ubuntu 22.04, Standard_B2ms)
```