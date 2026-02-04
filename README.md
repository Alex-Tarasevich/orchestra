# 🎻 Orchestra

### The **A.S.A.P.** (**A**I **S**DLC **A**utomation **P**latform) for modern development.

> **Note:** This solution was built by [spec-workflow](https://github.com/arturhaikou/spec-workflow).

**Orchestra** connects your fragmented tools into a single symphony. It doesn't just track tickets—it solves them. By unifying your trackers (Jira, Linear, GitHub) and assigning intelligent Agents to do the work, Orchestra turns your backlog into a performance.

---

## 🚀 Why Orchestra?

Development shouldn't be about passive tracking; it should be about active orchestration. We built **Orchestra** to be your **A.S.A.P.**—an **A**I **S**DLC **A**utomation **P**latform that bridges the gap between issue creation and code deployment.

### 🎼 The Composition (How it works)

*   **The Instruments (Unified Tracking):** Connect multiple sources like Jira, Trello, Linear, and GitHub Issues into one central view.
*   **The Musicians (AI Agents):** Spin up autonomous agents. Assign them to specific tickets to write code, generate tests, or summarize bugs.
*   **The Sheet Music (Workflows):** Define custom workflows that dictate how agents interact with your tickets and when human review is needed.
*   **The Conductor (You):** Oversee the entire lifecycle from a single dashboard.

---

## ✨ Key Features

- **Unified Connectivity:** Stop tab-switching. View tickets from every tracker in one place.
- **Agent Assignment:** "Hire" an AI agent for a specific ticket type (e.g., *The Bug Fixer*, *The Documentation Writer*).
- **Smart Workflows:** Trigger agent actions automatically when a ticket status changes.
- **A.S.A.P. Execution:** Reduce cycle time from days to minutes with background execution workers.
- **Tool Management System:** Equip agents with the specific tools they need (e.g., Jira API, Git access).
- **Real-Time Feedback:** Watch agents work in real-time via SignalR updates.
- **Jira Integration:** Full two-way conversion of content using a specialized ADF (Atlassian Document Format) generator.

---

## 🏗️ Architecture

Orchestra connects several microservices using **.NET Aspire**:

*   **Orchestrator:** `.NET Aspire` manages service discovery and container lifecycle.
*   **Frontend:** A **React 19** + **Vite** application providing the "Conductor's Dashboard".
*   **API Service:** A **.NET 10** REST API built with Clean Architecture, handling the core business logic.
*   **Worker Service:** Background .NET service dedicated to heavy AI agent processing and database migrations.
*   **ADF Generator:** A specialized Node.js service for converting content to/from Atlassian Document Format (Jira support).
*   **Infrastructure:** **PostgreSQL** for persistence and **Azure OpenAI** for agent intelligence.

---

## 🛠️ Tech Stack

- **Framework:** .NET 10 (Preview), .NET Aspire 13.1 (Preview)
- **Frontend:** React 19, Vite, TypeScript, Tailwind, React Flow, Recharts
- **Database:** PostgreSQL (with Entity Framework Core)
- **AI:** Microsoft Agents Framework, Azure OpenAI
- **Services:** Node.js (ADF Generator)

---

## 📦 Getting Started

### Prerequisites

*   **[.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)** (Preview)
*   **[.NET Aspire](https://aspire.dev)** workload
*   **[Docker Desktop](https://www.docker.com/products/docker-desktop)** (Required for Aspire & PostgreSQL containers)
*   **[Node.js](https://nodejs.org/)** (v22+)
*   Access to an **Azure OpenAI** resource (Deployment name and Endpoint)

### Installation

1.  **Clone the repository:**
    ```bash
    git clone https://github.com/your-org/orchestra.git
    cd orchestra
    ```

2.  **Configure AI Model:**
    Open the file `Orchestra.AppHost/appsettings.json`.
    Change the value of `"model"` to the name of your Azure OpenAI model (e.g., `"gpt-4o"`).
    ```json
    "Parameters": {
      "model": "your-model-name-here"
    }
    ```

3.  **Run the application:**
    Start the entire distributed application.
    ```bash
    aspire run
    ```

4.  **Access the Dashboard:**
    The .NET Aspire dashboard will launch in your browser.

    > **Important:** On the dashboard, look for the **openai** resource configuration. Before clicking on the **ui** project link, you must enter your OpenAI credentials in the format:
    > `Endpoint={your-endpoint};Key={your-key}`

    From there, you can view logs, traces, and click the endpoint link for the **ui** project to access the Orchestra application.
