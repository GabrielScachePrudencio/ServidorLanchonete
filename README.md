# 🌐 Servidor PDV – API ASP.NET

Servidor backend desenvolvido em **ASP.NET / ASP.NET Core**, responsável por centralizar todas as regras de negócio do sistema de **PDV**, fornecendo uma **API REST** para consumo por aplicações desktop (WPF), web e mobile.

---

## 🎯 Objetivo do Servidor

- Centralizar a lógica do sistema
- Gerenciar pedidos, produtos e vendas
- Fornecer endpoints para o PDV
- Garantir integridade e validação dos dados
- Facilitar a integração com múltiplos clientes

---

## 🏗️ Arquitetura

A aplicação segue uma arquitetura baseada em camadas:

- **Controllers** → Recebem as requisições HTTP
- **Services** → Contêm as regras de negócio
- **Repositories** → Acesso ao banco de dados
- **Models** → Entidades do sistema
- **DTOs** → Transferência de dados entre cliente e servidor

---

## 🔄 Funcionamento Geral

1. O cliente (WPF/Web/App) envia uma requisição HTTP
2. O Controller recebe a requisição
3. O Service aplica as regras de negócio
4. O Repository acessa o banco de dados
5. A API retorna a resposta em formato JSON

---

## 🧪 Tecnologias Utilizadas

- C#
- ASP.NET / ASP.NET Core
- Entity Framework Core
- API REST
- JSON
- SQL Server (ou outro banco relacional)

---

## 📁 Estrutura do Projeto

```txt
/Servidor-PDV
 ├── Controllers
 ├── Models
 ├── DTOs
 ├── Services
 ├── Repositories
 ├── Data
 ├── Migrations
 ├── Program.cs
 └── appsettings.json
