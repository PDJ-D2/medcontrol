# MedControl

O MedControl é uma aplicação simples para controle de medicamentos, feita com .NET 10, EF Core, SQLite e React.

A ideia do projeto é ajudar uma pessoa a cadastrar seus remédios, definir horários de uso, acompanhar as doses do dia e controlar o estoque básico de cada medicamento.

## Funcionalidades

- Cadastro de medicamentos
- Edição de medicamentos
- Arquivamento de medicamentos
- Cadastro de horários de uso
- Seleção dos dias da semana para cada horário
- Listagem dos medicamentos ativos
- Agenda de doses do dia
- Marcação de dose como tomada
- Marcação de dose como pulada
- Controle simples de estoque
- Alerta visual para estoque baixo

## Tecnologias usadas

Backend:

- .NET 10
- ASP.NET Core Minimal APIs
- Entity Framework Core
- SQLite

Frontend:

- React
- Vite
- CSS
- lucide-react para ícones

## Estrutura do projeto

```text
MedControl/
├── server/
│   ├── Data/
│   ├── Domain/
│   ├── Features/
│   │   ├── Intakes/
│   │   └── Medications/
│   ├── Program.cs
│   └── MedControl.Api.csproj
│
├── client/
│   ├── src/
│   │   ├── api/
│   │   ├── components/
│   │   ├── constants/
│   │   ├── App.jsx
│   │   ├── main.jsx
│   │   └── styles.css
│   ├── package.json
│   └── vite.config.js
│
├── MedControl.slnx
└── NuGet.Config
```

## Pré-requisitos

Antes de rodar o projeto, instale:

- .NET SDK 10
- Node.js
- npm

Para verificar se estão instalados:

```bash
dotnet --info
node --version
npm --version
```

## Como rodar o backend

Na raiz do projeto, restaure os pacotes:

```bash
dotnet restore MedControl.slnx
```

Depois rode a API:

```bash
dotnet run --project server/MedControl.Api.csproj --launch-profile http
```

A API deve iniciar em:

```text
http://localhost:5243
```

Você pode testar se ela está funcionando acessando:

```text
http://localhost:5243/api/health
```

Resposta esperada:

```json
{
  "status": "ok"
}
```

## Configuração de variáveis locais

Este projeto não versiona dados sensíveis, como chave JWT, senha de e-mail, token SMTP ou banco SQLite local.

O arquivo `server/appsettings.json` contém apenas valores de exemplo e pode ser mantido no repositório público.

Para configurar o projeto na sua máquina:

1. Copie o arquivo:

   server/appsettings.Example.json

2. Renomeie a cópia para:

   server/appsettings.Local.json

3. Preencha os valores locais:

   - Jwt:SigningKey
   - Email:From
   - Email:SmtpHost
   - Email:SmtpPort
   - Email:Username
   - Email:Password

Exemplo:

{
  "Jwt": {
    "SigningKey": "sua-chave-secreta-com-pelo-menos-32-caracteres"
  },
  "Email": {
    "From": "seu-email@gmail.com",
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "UseSsl": true,
    "Username": "seu-email@gmail.com",
    "Password": "sua-senha-de-app-ou-token-smtp"
  }
}

O arquivo `appsettings.Local.json` está no `.gitignore` e não deve ser enviado para o GitHub.

Também são ignorados os arquivos de banco local gerados pelo SQLite:

- *.db
- *.db-shm
- *.db-wal

Em produção, prefira configurar esses valores por variáveis de ambiente:

- Jwt__SigningKey
- Email__From
- Email__SmtpHost
- Email__SmtpPort
- Email__Username
- Email__Password

## Como rodar o frontend

Em outro terminal, entre na pasta do frontend:

```bash
cd client
```

Instale as dependências:

```bash
npm install
```

Rode o projeto:

```bash
npm run dev
```

O frontend deve iniciar em:

```text
http://localhost:5173
```

## Banco de dados

O projeto usa SQLite.

Ao iniciar o backend pela primeira vez, o banco é criado automaticamente pelo EF Core usando `EnsureCreated`.

O arquivo do banco será gerado na pasta do backend com o nome:

```text
medcontrol.db
```

## Principais endpoints da API

Health check:

```http
GET /api/health
```

Listar medicamentos:

```http
GET /api/medications
```

Buscar medicamento por ID:

```http
GET /api/medications/{id}
```

Criar medicamento:

```http
POST /api/medications
```

Editar medicamento:

```http
PUT /api/medications/{id}
```

Arquivar medicamento:

```http
DELETE /api/medications/{id}
```

Listar doses do dia:

```http
GET /api/intakes/today
```

Registrar dose tomada ou pulada:

```http
POST /api/intakes/medications/{medicationId}
```

## Exemplo de cadastro de medicamento

```json
{
  "name": "Vitamina D",
  "dosage": "1 cápsula",
  "instructions": "Tomar após o café da manhã",
  "stockQuantity": 30,
  "lowStockThreshold": 5,
  "startDate": "2026-05-20",
  "endDate": null,
  "isActive": true,
  "schedules": [
    {
      "time": "08:00",
      "days": "EveryDay"
    }
  ]
}
```

## Observações

Este projeto foi criado com foco em simplicidade e utilidade.

Por enquanto é um projeto simples; ainda não possui autenticação, múltiplos usuários, notificações ou migrations formais. A estrutura, porém, foi separada de forma simples para permitir evolução futura.

Melhorias futuras:

- Autenticação de usuários
- Histórico completo de uso
- Notificações por e-mail ou WhatsApp
- Relatórios mensais
- Migrations do EF Core
- Testes automatizados
- Deploy do backend e frontend
- Responsáveis/cuidadores compartilhando o mesmo tratamento

## Objetivo

O objetivo do MedControl é de representar uma aplicação que poderia ajudar pessoas reais a organizarem seus medicamentos no dia a dia.