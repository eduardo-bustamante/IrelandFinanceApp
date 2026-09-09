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
