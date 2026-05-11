# Projeto Banco - API com Mensageria

Este projeto consiste no desenvolvimento do backend de um banco digital voltado para o processamento assíncrono de contratações de produtos bancários, utilizando a stack .NET 8, Oracle Database e RabbitMQ.

### Integrantes do Grupo
* **Jhonatta Lima Sandes de Oliveira** - RM: 560277
* **Rangel Bernadi Jordão** - RM: 560547
* **Lucas José Lima** - RM: 561160
* 
**Turma:** 2TDSPA

---

## Decisões Arquiteturais
A aplicação foi estruturada como uma Web API RESTful de alta manutenibilidade, priorizando o desacoplamento entre a recepção de pedidos e o processamento pesado.

* **Framework:** .NET 9 / ASP.NET Core Web API.
* **ORM:** Entity Framework Core para manipulação de dados no Oracle.
* **Design Patterns & Princípios:**
    * **Separation of Concerns (SoC):** A lógica de recepção de solicitações (Controller) é isolada da lógica de processamento (Background Service), permitindo que a API responda rapidamente enquanto o sistema processa as regras de negócio em segundo plano.
    * **Resiliência com ACK Manual:** O consumidor RabbitMQ utiliza confirmações manuais (ACK) apenas após a persistência bem-sucedida no banco, garantindo que mensagens não sejam perdidas em caso de falha durante o processamento.
    * **Dependency Injection:** Injeção nativa para serviços como `IContratacaoProducer`, facilitando a testabilidade e o baixo acoplamento.
    * **Observabilidade Integrada:** Uso de OpenTelemetry e Serilog para rastreabilidade ponta a ponta, permitindo o monitoramento de saúde via `/health` e traces exportados para o Jaeger.
    * **DTOs (Records):** Uso de `records` para garantir a imutabilidade dos dados trafegados entre as camadas.

---

## Rotas e Navegação (Endpoints)

| Funcionalidade | Método | Endpoint | Descrição | Processamento |
| :--- | :---: | :--- | :--- | :---: |
| **Cadastrar Cliente PF** | `POST` | `/api/clientes/pf` | Cadastro de pessoa física com CPF único. | Síncrono |
| **Cadastrar Cliente PJ** | `POST` | `/api/clientes/pj` | Cadastro de pessoa jurídica com CNPJ único. | Síncrono |
| **Buscar Cliente** | `GET` | `/api/clientes/{id}` | Retorna dados do cliente e sua agência. | Síncrono |
| **Cadastrar Agência** | `POST` | `/api/agencias` | Registra uma nova agência no sistema. | Síncrono |
| **Solicitar Contratação** | `POST` | `/api/contratacoes` | Valida dados e publica na fila do RabbitMQ. | **Assíncrono** |
| **Status Contratação** | `GET` | `/api/contratacoes/{id}` | Consulta o resultado (Aprovada/Recusada). | Síncrono |
| **Health Check** | `GET` | `/health` | Verifica saúde da API e do banco Oracle. | Técnico |

---

## Produto Bancário Escolhido e Justificativa
Foram implementados os produtos de **Consórcio** e **Empréstimo**.
* **Justificativa:** A escolha visa demonstrar a capacidade do sistema em lidar com regras de negócio distintas. O **Consórcio** exige cálculos de parcelamento linear e gestão de grupos, enquanto o **Empréstimo** implementa uma análise de risco baseada em *score* de crédito e cálculos de juros compostos, evidenciando a robustez do processamento assíncrono para diferentes domínios financeiros.

---

## Decisão de Modelagem de Filas
O projeto utiliza a abordagem de **Fila Única (`contratacao-solicitada`) com Discriminator no Payload**.
* **Justificativa:** Esta decisão foi tomada para centralizar o tráfego de solicitações, facilitando o monitoramento e a escalabilidade inicial. O `ContratacaoConsumer` atua como um orquestrador que, ao ler o discriminator `TipoProduto` da mensagem, direciona o processamento para a lógica específica (Consórcio ou Empréstimo). Isso reduz a sobrecarga de gerenciamento de múltiplas conexões e canais no RabbitMQ para este escopo.

---

## Diagrama de Classes
O diagrama reflete a herança da entidade abstrata `Cliente` para `PessoaFisica` e `PessoaJuridica`, além da relação entre `Agencia`, `Cliente` e `Contratacao`.

![Diagrama de Classes](ProjetoBanco.API/docs/diagrama-classes.png)

---

## Testes Automatizados
O projeto possui testes unitários e integrados utilizando `WebApplicationFactory`, `Moq` e banco em memória `InMemory`.
* **Execução:** `dotnet test`

![Resultado dos Testes](ProjetoBanco.API/docs/tests.png)

## Evidências de Funcionamento

### Painel RabbitMQ (Fila e Unacked)
Abaixo, a comprovação do recebimento de mensagens e o estado da fila:
![RabbitMQ](ProjetoBanco.API/docs/rabbit.png)


![RabbitMQ](ProjetoBanco.API/docs/unacked.png)

### Swagger e Aprovação
Exemplo de contratação processada com sucesso (`APROVADA`):
![Swagger](ProjetoBanco.API/docs/swagger.png)

---

## Como Rodar Localmente

### Pré-requisitos
* .NET SDK 8.0+
* Docker Desktop
* Ferramenta de linha de comando `dotnet-ef`

### Execução
1.  **Subir RabbitMQ e Jaeger (Docker):**
    ```bash
    docker-compose up -d
    ```
    *Isso iniciará o RabbitMQ na porta 5672 (e painel na 15672) e o Jaeger na 4318.*

2.  **Configurar o Banco de Dados:**
    Atualize a `DefaultConnection` no arquivo `appsettings.json` com suas credenciais do Oracle FIAP.

3.  **Atualizar Base de Dados:**
    ```bash
    dotnet ef database update --project ProjetoBanco.API
    ```

4.  **Executar a Aplicação:**
    ```bash
    dotnet run --project ProjetoBanco.API
    ```

---

**Nota sobre Observabilidade:** A API está configurada com OpenTelemetry enviando traces para o Jaeger (porta 4318).
