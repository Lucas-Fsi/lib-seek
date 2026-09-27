# Lib-Seek

Lib-Seek é um sistema de gerenciamento de biblioteca, capaz de lidar com: cadastro de livros, empréstimo, devolução e multa por atraso. Este é um projeto de estudo e tem como objetivo o aprendizado de práticas de desenvolvimento com o uso de frameworks e a aplicação de banco de dados.

## Colaboradores

- **Victor**: https://github.com/victorcampesi
- **Lucas**: https://github.com/Lucas-Fsi

## Tecnologias

- Linguagem: C#
- Framework: ASP.NET Core
- ORM: Entity Framework Core
- Banco de dados: PostgreSQL
- Containerização: Docker / Docker Compose

## Funcionalidades

### 📚 Gestão de Acervo

- Cadastro de livros com título, autor, ISBN, ano de publicação e quantidade em estoque
- Listagem completa e busca por título
Edição e remoção de títulos

### 👥 Gestão de Usuários

- Cadastro de novos usuários
- Atualização de dados e exclusão de perfis
- Ativação e desativação de contas

### 🔄 Controle de Empréstimos

- Registro de empréstimos vinculando livro e usuário
- Registro de devoluções com retorno automático ao estoque
- Rastreamento de datas e prazos de devolução

### 💸 Gestão de Multas

- Registro de multas por atraso
- Consulta de pendências por usuário
- Registro de quitação

## Como rodar

### Pré-requisitos

- .NET SDK 10
- Docker

### Passos

``` 
git clone https://github.com/Lucas-Fsi/lib-seek.git

docker-compose up -d

```

- Copie a estrutura que está no arquivo appsettings.json

- Crie um novo arquivo chamado appsettings.Development.json na mesma pasta (src/Lib-SeekApi).

- Insira as credenciais reais do seu banco de dados local.

```
dotnet ef database update

dotnet run
```
- Em seu navegador, acesse o localhost informado pelo terminal, isso abrirá o swagger