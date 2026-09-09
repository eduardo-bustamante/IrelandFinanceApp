# 🍀 Ireland Finance — Personal Cash Flow & Runway Manager

[![en](https://img.shields.io/badge/lang-en-red.svg)](#)
[![pt-br](https://img.shields.io/badge/lang-pt--br-green.svg)](README.pt-BR.md)
![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-MVC-blue?logo=dotnet)
![Entity Framework Core](https://img.shields.io/badge/EF_Core-10.0-512BD4?logo=nuget)
![SQL Server](https://img.shields.io/badge/SQL_Server-2022-CC292B?logo=microsoft-sql-server&logoColor=white)
![Bootstrap 5](https://img.shields.io/badge/Bootstrap-5.3-7952B3?logo=bootstrap&logoColor=white)

> A robust, modern personal finance web platform built with .NET 10 and ASP.NET Core, designed for real-world cash flow control, credit card invoice allocation, and runway/burn rate analytics.

---

## 📌 Key Features

* **Real Cash Flow Discipline:**
  * Strict separation between immediate liquid movements (`DebitOrCash`) and future credit obligations.
  * Account balances reflect true settled liquidity without artificial inflation.

* **Automated Credit Card Cycle Engine:**
  * Multi-card support with configurable credit limit, invoice closing day, and due date.
  * Purchases automatically routed to the correct billing cycle based on the transaction date.
  * Real-time visibility into committed credit vs. available balance.

* **Burn Rate & Runway Analytics:**
  * **Daily Burn Rate:** Current month's average daily expense run rate.
  * **Runway Calculation:** Automated survival runway in months based on liquid emergency reserves and core commitments.
  * **Month-End Forecast:** Dynamic projection of total period outflow.

* **Emergency Reserves & Savings Goals:**
  * Short-term and long-term goal tracking with target completion status.
  * Seamless integration with liquid runway calculations.

* **Multi-Currency & Regional Preferences:**
  * Per-user localization supporting `EUR (€)`, `BRL (R$)`, `USD ($)`, and `GBP (£)`.
  * Dedicated regional preferences supporting culture code formatting (`en-IE`, `pt-BR`) and IANA timezones.

* **Modern SaaS Architecture & UI:**
  * Built on a custom *Deep Slate & Emerald* theme with responsive sidebar layout.
  * Completely revamped Identity management console (Profile, Regional Preferences, Security, 2FA, Data Privacy).

---

## 🛠️ Tech Stack

* **Runtime & Framework:** C# / .NET 10, ASP.NET Core MVC, Razor Pages
* **Persistence & ORM:** Entity Framework Core 10, Microsoft SQL Server
* **Identity & Security:** ASP.NET Core Identity (hashed credentials, antiforgery CSRF tokens)
* **Frontend:** Bootstrap 5.3, Bootstrap Icons, Clean CSS3 Design System
* **Data Integrity:** Fully normalized relational schemas with explicit referential integrity (`DeleteBehavior.Restrict` ensuring no cascade cycle failures)

---

## 📂 Architecture Overview

```text
IrelandFinanceApp/
├── Areas/
│   └── Identity/              # Authentication & Identity Subsystem
│       └── Pages/Account/
│           └── Manage/         # Profile, Preferences, Security, 2FA
├── Controllers/
│   ├── HomeController.cs      # Core Dashboard & Financial KPI Engine
│   ├── TransactionsController.cs
│   └── CreditCardsController.cs
├── Data/
│   └── AppDbContext.cs        # EF Core Configurations & Relationship Constraints
├── Models/
│   ├── ApplicationUser.cs     # Identity entity extended with localization preferences
│   ├── CreditCard.cs          # Invoice cycle and closing calculations
│   ├── Transaction.cs         # Double-entry-ready financial ledger
│   ├── Category.cs            # Budget grouping and limits
│   └── SavingsGoal.cs         # Goal accumulation tracking
└── Views/
    ├── Shared/_Layout.cshtml   # SaaS Shell with sidebar and responsive topbar
    └── Home/Index.cshtml       # Executive Dashboard

Versão PT-BR

# 🍀 Ireland Finance — Personal Cash Flow & Runway Manager

![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-MVC-blue?logo=dotnet)
![Entity Framework Core](https://img.shields.io/badge/EF_Core-10.0-512BD4?logo=nuget)
![SQL Server](https://img.shields.io/badge/SQL_Server-2022-CC292B?logo=microsoft-sql-server&logoColor=white)
![Bootstrap 5](https://img.shields.io/badge/Bootstrap-5.3-7952B3?logo=bootstrap&logoColor=white)

Uma plataforma web moderna e robusta para gestão financeira pessoal, projetada especificamente com foco em **fluxo de caixa real**, controle detalhado de **faturas de cartão de crédito** e métricas operacionais essenciais como **Burn Rate** e **Runway** (autonomia financeira).

---

## 📌 Principais Funcionalidades

- **Controle de Fluxo de Caixa Real:**
  - Distinção rigorosa entre movimentações imediatas de conta corrente/espécie (`DebitOrCash`) e compromissos futuros no crédito.
  - O saldo bancário reflete com exatidão entradas e saídas reais, evitando falsa sensação de liquidez.

- **Gestão Inteligente de Cartões de Crédito:**
  - Cadastro de múltiplos cartões com limite de crédito, dia de fechamento e dia de vencimento.
  - Alocação automática de cada despesa na competência correta da fatura com base na data da compra.
  - Visão clara do crédito comprometido vs. limite disponível.

- **Indicadores Financeiros em Tempo Real:**
  - **Daily Burn Rate:** consumo financeiro médio diário no mês corrente.
  - **Runway:** cálculo automático de meses de sobrevivência baseado nas reservas de emergência e nos custos fixos essenciais.
  - **Projeção de Fim de Mês:** estimativa de despesas até o encerramento do ciclo.

- **Metas de Poupança & Reserva de Emergência:**
  - Acompanhamento do progresso de metas de curto e longo prazo.
  - Integração com o cálculo de segurança financeira (Runway).

- **Preferências Regionais & Multi-Moeda:**
  - Suporte completo a formatação monetária configurável por perfil de usuário (€ Euro, R$ Real, $ Dólar, £ Libra).
  - Configuração personalizada de idioma/cultura (`en-IE`, `pt-BR`) e fuso horário IANA para corte preciso de faturas.

- **Design Responsivo & Alta Usabilidade:**
  - Interface no estilo SaaS contemporâneo (*Deep Slate & Emerald*).
  - Sidebar lateral retrátil e adaptável a telas mobile.
  - Módulo de gerenciamento de conta (Identity) com cards elevados e UX moderna.

---

## 🛠️ Tecnologias Utilizadas

- **Backend:** C# / .NET 10, ASP.NET Core MVC, Razor Pages
- **ORM & Banco de Dados:** Entity Framework Core 10, SQL Server (LocalDB / Azure SQL)
- **Autenticação & Autorização:** ASP.NET Core Identity
- **Frontend:** Bootstrap 5, Bootstrap Icons, HTML5, CSS3 Moderno
- **Arquitetura & Boas Práticas:**
  - Modelagem relacional normalizada com integridade referencial protegida (`DeleteBehavior.Restrict` para prevenção de ciclos em cascata).
  - ViewModels desacoplados para renderização performática do Dashboard.

---

## 📂 Estrutura do Projeto

```text
IrelandFinanceApp/
├── Areas/
│   └── Identity/            # Módulo de Autenticação e Configurações de Conta
│       └── Pages/Account/
│           └── Manage/       # Perfil, Preferências Regionais, Segurança
├── Controllers/
│   ├── HomeController.cs    # Métricas do Dashboard, Cash Flow e Runway
│   ├── TransactionsController.cs
│   └── CreditCardsController.cs
├── Data/
│   └── AppDbContext.cs      # Mapeamento do EF Core e regras relacionais
├── Models/
│   ├── ApplicationUser.cs   # Usuário estendido (Moeda, Cultura, Fuso Horário)
│   ├── CreditCard.cs        # Cartões e cálculo de competência de faturas
│   ├── Transaction.cs       # Lançamentos de despesa, receita e pagamento
│   ├── Category.cs          # Categorização e limite orçamentário
│   └── SavingsGoal.cs       # Metas e reserva de emergência
└── Views/
    ├── Shared/_Layout.cshtml # Sidebar e casca estrutural do SaaS
    └── Home/Index.cshtml     # Dashboard principal
