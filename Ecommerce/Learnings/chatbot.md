# AI Chatbot Implementation

This document describes how the interactive, role-based AI chatbot is implemented across the Angular frontend and ASP.NET Core backend using Microsoft Semantic Kernel, Groq/Gemini, and role-specific custom UI rendering.

---

## 1. System Architecture & Message Flow Chart

The chatbot architecture coordinates components across the frontend and backend. Below is the step-by-step decision flowchart showing how messages are processed, validated, and how functions are triggered.

### Decision Flowchart

```mermaid
flowchart TD
    Start([User inputs query])
        --> Send[Widget triggers sendMessage]
        --> API[POST ChatbotController.SendMessage]
        --> ResolveContext[Resolve UserId & Role via ICurrentUserService]
        --> Session[Get or Create Chat Session via IChatRepository]
        --> SaveUser[Save User Message to DB if Authenticated]
        --> CheckGuest{Is User a Guest?}

    CheckGuest -- Yes --> GuestGate{Is Query Restricted for Guests?}
    GuestGate -- Yes --> ReturnGuestRestricted[Return - Please log in Response]
    GuestGate -- No --> InvokeLLM

    CheckGuest -- No --> CheckIntent{Is Query Allowed for User Role?}
    CheckIntent -- No --> ReturnDeflect[Save and Return Deflecting Response]
    CheckIntent -- Yes --> InvokeLLM[Invoke Semantic Kernel GetChatMessageContentAsync]

    InvokeLLM --> ToolChoice{Did the LLM Request a Plugin Function?}

    ToolChoice -- Yes --> PluginCall[Invoke Chatbot Plugin Function]
    PluginCall --> ValidateID{Does Customer/Vendor ID Match Current User?}

    ValidateID -- No --> ThrowAuth[Throw UnauthorizedAccessException]
    ValidateID -- Yes --> ExecCore[Execute Domain Service]
    ExecCore --> ReturnTool[Return Serialized Data to LLM Context]
    ReturnTool --> InvokeLLM

    ToolChoice -- No --> ReturnFinal[Generate Final AI Response]
    ReturnFinal --> SaveAI[Save AI Response to DB if Authenticated]
    SaveAI --> ReturnResponse[Return ChatMessageResponse to Client]
    ReturnResponse --> ParseMsg[Client Parses Markdown, Tables, Links, and Images]
    ParseMsg --> Render([Display Chat Bubble to User])
   
```

### Sequence Diagram & Function Triggers

This sequence diagram maps the execution sequence, showing the exact function calls across the client-server boundaries:

```mermaid
sequenceDiagram
    autonumber
    actor User
    participant View as ChatbotWidget Component
    participant Service as ChatbotService (Angular)
    participant Ctrl as ChatbotController (API)
    participant ChatSvc as ChatbotService (BLL)
    participant Repo as ChatRepository
    participant SK as Semantic Kernel (LLM)
    participant Plugins as ChatbotPlugins
    participant CoreSvc as Core Domain Services

    User->>View: Types query & submits
    View->>View: sendMessage() triggers
    View->>Service: sendMessage(message)
    Service->>Ctrl: POST /api/Chatbot/message { message }
    Ctrl->>ChatSvc: ProcessMessageAsync(userId, role, message)
    
    rect rgb(230, 245, 255)
        note over ChatSvc, Repo: Session & Context Prep
        ChatSvc->>Repo: GetSessionWithMessages(userId, role)
        Repo-->>ChatSvc: Return session (or CreateSession)
        ChatSvc->>Repo: AddMessage(sessionId, "User", message) [If Authenticated]
    end

    rect rgb(240, 240, 240)
        note over ChatSvc, SK: Security Check & Filtering
        alt User is Guest
            ChatSvc->>SK: IsGuestQueryRestricted(message, chatCompletionService)
            SK-->>ChatSvc: Verdict: RESTRICTED / ALLOWED
        end
        ChatSvc->>SK: IsQueryValid(message, role, chatCompletionService)
        SK-->>ChatSvc: Verdict: VALID / INVALID
    end

    rect rgb(255, 245, 230)
        note over ChatSvc, CoreSvc: Tool Execution / Function Calling
        ChatSvc->>SK: GetChatMessageContentAsync(chatHistory, settings)
        
        opt LLM requires function call
            SK->>Plugins: Trigger KernelFunction (e.g. SearchProducts, AddToCart, etc.)
            Plugins->>CoreSvc: Validation.ValidateCustomerId/ValidateVendorId
            alt Validation OK
                Plugins->>CoreSvc: Domain Service Method Call (e.g., GetProductsCatalog, AddToCart)
                CoreSvc-->>Plugins: Return data
                Plugins-->>SK: Return tool output
            else Validation Fails
                Plugins-->>SK: UnauthorizedAccessException
            end
        end
        SK-->>ChatSvc: Final reply message content
    end

    ChatSvc->>Repo: AddMessage(sessionId, "AI", replyText) [If Authenticated]
    ChatSvc-->>Ctrl: ChatMessageResponse { reply, sessionId }
    Ctrl-->>Service: Ok(ChatMessageResponse)
    Service-->>View: Observable emission
    View->>View: parseMessage(reply) & Render UI
    View-->>User: Displays response
```

---

## 2. Frontend Implementation (Angular)

### Chat Widget Component
*   **Files**: 
    *   [chatbot-widget.ts](file:///Users/dharshinik/Desktop/Presidio/Genspark/Ecommerce/frontend/src/app/components/shared/chatbot-widget/chatbot-widget.ts) (Logic)
    *   [chatbot-widget.html](file:///Users/dharshinik/Desktop/Presidio/Genspark/Ecommerce/frontend/src/app/components/shared/chatbot-widget/chatbot-widget.html) (Template)
    *   [chatbot-widget.css](file:///Users/dharshinik/Desktop/Presidio/Genspark/Ecommerce/frontend/src/app/components/shared/chatbot-widget/chatbot-widget.css) (Styles)
*   **Visibility Control**: The widget monitors route changes and automatically hides itself on login and registration pages.
*   **Dynamic Role-based Suggestion Chips**: Reads user roles (Admin, Vendor, Customer, or Guest) from the `AuthService` and presents context-appropriate suggestions (e.g., *Check inventory stock alerts* for Vendors, *Search for laptops* for Customers).
*   **Custom UI Rendering**: 
    *   **Text Blocks**: Normal message paragraphs.
    *   **Comparison Tables**: Renders multi-column comparison tables when product attribute lists are returned.
    *   **Variant Cards**: Parses image syntax `![alt](url)` into visual cards displaying the product image, title, and detailed specifications.
    *   **Actionable Links**: Detects custom links matching patterns like `product:id`, `order:id`, `cart`, `wishlist`, and `settlements` to navigate the user directly to the corresponding application pages.

### Service Client
*   **File**: [chatbot.service.ts](file:///Users/dharshinik/Desktop/Presidio/Genspark/Ecommerce/frontend/src/app/services/chatbot.service.ts)
*   **Endpoints**:
    *   `sendMessage(message: string)` -> `POST /api/Chatbot/message`
    *   `getChatHistory()` -> `GET /api/Chatbot/history`
    *   `clearChat()` -> `POST /api/Chatbot/clear`

---

## 3. Backend Implementation (ASP.NET Core)

### API Controller
*   **File**: [ChatbotController.cs](file:///Users/dharshinik/Desktop/Presidio/Genspark/Ecommerce/backend/Ecommerce.API/Controllers/ChatbotController.cs)
*   Exposes endpoints, maps requests, resolves the user context (retrieving `UserId` and `Role` from `ICurrentUserService`), and routes message payloads to `IChatbotService`.

### Orchestration Service
*   **File**: [ChatbotService.cs](file:///Users/dharshinik/Desktop/Presidio/Genspark/Ecommerce/backend/Ecommerce.BLL/ChatbotService.cs)
*   **Session Management**: Retrieves/creates chat session history using `IChatRepository`. Guest user sessions are ephemeral and are not persisted in the database.
*   **LLM Service Configuration & Fallback**:
    *   Initializes the `KernelBuilder` using **Microsoft Semantic Kernel**.
    *   Checks if `GroqSettings:ApiKey` is configured. If valid, configures `AddOpenAIChatCompletion` using `llama-3.3-70b-versatile` pointed to the Groq API endpoint.
    *   Fallback: If Groq API keys are missing or a request fails (e.g., rate limit exceeded / HTTP 429), it automatically falls back to **Google AI Gemini** (`gemini-2.5-flash`) via `AddGoogleAIGeminiChatCompletion`.
*   **Pre-Filter Security Gates**:
    1.  **Guest Gatekeeper**: Evaluates unauthenticated messages using `IsGuestQueryRestricted`. If a guest attempts to view/modify sensitive areas like their cart or wishlist, it interrupts the flow with a prompt to log in.
    2.  **Intent Guardrail**: Evaluates queries against role-permitted activities in `IsQueryValid`. If off-topic or prompt injection attempts are detected, it returns a polite, role-appropriate deflecting message.
*   **Tool Choice Integration**: Executes AI chat completions with `FunctionChoiceBehavior.Auto()` so the LLM can decide when to execute local plugins/tools.

---

## 4. Semantic Kernel Plugins (Tools)

*   **File**: [ChatbotPlugins.cs](file:///Users/dharshinik/Desktop/Presidio/Genspark/Ecommerce/backend/Ecommerce.BLL/Helper/ChatbotPlugins.cs)
*   Exposes domain services to the LLM using `[KernelFunction]` and `[Description]` annotations.
*   **Features by Role**:
    *   **Customer**:
        *   `SearchProducts(searchTerm)`: Finds matching products or returns the catalog.
        *   `GetProductDetails(productId)`: Returns specific variant specifications.
        *   `GetCart(customerId)`: Fetches current cart items.
        *   `AddToCart(variantId, quantity)`, `RemoveFromCart(cartItemId)`: Modifies cart items.
        *   `GetWishlist(customerId)`, `AddToWishlist(variantId)`, `RemoveFromWishlist(wishlistItemId)`: Wishlist management.
        *   `GetMyOrders(customerId)`, `GetOrderDetails(orderId)`: Order tracking and delivery status.
        *   `GetProductReviews(productId)`: Retrieves reviews for AI summarization.
    *   **Vendor**:
        *   `GetInventoryAlerts(vendorId)`: Finds low stock variants (stock < 10) in the vendor's store.
        *   `GetMySettlements(vendorId)`: Payout statistics.
    *   **Admin**:
        *   `GetPlatformKpis(month)`: Shows key platform statistics, revenue data, and dashboard trends.
        *   `GetVendorTurnoverList()`: Lists registered vendor store names with admin-calculated turnovers.

### Data Isolation & Validation Security
*   **File**: [Validation.cs](file:///Users/dharshinik/Desktop/Presidio/Genspark/Ecommerce/backend/Ecommerce.BLL/Helper/Validation.cs)
*   To prevent prompt injections where the LLM might be tricked into requesting another user's/vendor's data, all parameter inputs representing user IDs are validated in `Validation.ValidateCustomerId(customerId)` and `Validation.ValidateVendorId(vendorId)`.
*   These compare the ID supplied by the LLM against the actual authenticated session user ID resolved by the server's `ICurrentUserService`. If a mismatch is detected, an `UnauthorizedAccessException` is raised immediately.

---

## 5. Prompts & Instruction Rules

*   **File**: [ChatbotPrompt.cs](file:///Users/dharshinik/Desktop/Presidio/Genspark/Ecommerce/backend/Ecommerce.BLL/Helper/ChatbotPrompt.cs)
*   Centralizes role-specific system prompts:
    *   **Admin Prompt**: Restricts scope to KPI indicators, sales figures, and vendor lists, instructing the AI to use Markdown tables and custom routing links.
    *   **Vendor Prompt**: Limits inventory alert access and payouts strictly to their own store.
    *   **Customer Prompt**: Dictates how comparison tables must be formatted (e.g. comparing variants with specific attribute headers) and requests image tags where available.
