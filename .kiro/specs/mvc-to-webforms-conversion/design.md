# Design Document

## Overview

This design document outlines the architectural approach for converting Bob's Used Books Classic from ASP.NET MVC to ASP.NET WebForms while maintaining all existing functionality. The conversion will transform the current MVC pattern (Controllers, Views, Models) into WebForms patterns (Pages, Code-behind, Master Pages, User Controls) while preserving the existing service layer, data access layer, and AWS integrations.

## Architecture

### High-Level Architecture Changes

The conversion will maintain the existing layered architecture but replace the MVC presentation layer with WebForms:

```
Current MVC Architecture:
┌─────────────────────────────────────────┐
│ Presentation Layer (MVC)                │
│ ├── Controllers                         │
│ ├── Views (Razor)                       │
│ ├── Models (ViewModels)                 │
│ └── Areas (Admin)                       │
├─────────────────────────────────────────┤
│ Business/Service Layer                  │
│ ├── Domain Services                     │
│ ├── Authentication Services             │
│ └── AWS Service Integrations            │
├─────────────────────────────────────────┤
│ Data Access Layer                       │
│ ├── Entity Framework DbContext          │
│ ├── Repository Pattern                  │
│ └── Domain Models                       │
├─────────────────────────────────────────┤
│ Infrastructure                          │
│ ├── Dependency Injection (Autofac)      │
│ ├── Configuration Management            │
│ └── Logging (NLog)                      │
└─────────────────────────────────────────┘

Target WebForms Architecture:
┌─────────────────────────────────────────┐
│ Presentation Layer (WebForms)           │
│ ├── ASPX Pages                          │
│ ├── Code-behind Files                   │
│ ├── Master Pages                        │
│ ├── User Controls                       │
│ └── Admin Folder (Admin Pages)          │
├─────────────────────────────────────────┤
│ Business/Service Layer (Unchanged)      │
│ ├── Domain Services                     │
│ ├── Authentication Services             │
│ └── AWS Service Integrations            │
├─────────────────────────────────────────┤
│ Data Access Layer (Unchanged)           │
│ ├── Entity Framework DbContext          │
│ ├── Repository Pattern                  │
│ └── Domain Models                       │
├─────────────────────────────────────────┤
│ Infrastructure (Minimal Changes)        │
│ ├── Dependency Injection (Autofac)      │
│ ├── Configuration Management            │
│ └── Logging (NLog)                      │
└─────────────────────────────────────────┘
```

### Page Structure Design

The WebForms application will use the following page structure:

```
WebForms Page Structure:
├── Site.Master (Main master page)
├── Default.aspx (Home page)
├── Search.aspx (Book search)
├── BookDetails.aspx (Book details)
├── ShoppingCart.aspx (Shopping cart)
├── Checkout.aspx (Checkout process)
├── Orders.aspx (Order history)
├── Wishlist.aspx (User wishlist)
├── Address.aspx (Address management)
├── Resale.aspx (Book resale)
├── Login.aspx (Authentication)
├── Register.aspx (User registration)
├── Privacy.aspx (Privacy policy)
└── Admin/ (Administrative pages)
    ├── AdminMaster.Master (Admin master page)
    ├── Dashboard.aspx
    ├── Inventory.aspx
    ├── Orders.aspx
    ├── Offers.aspx
    └── ReferenceData.aspx
```

## Components and Interfaces

### Master Page Design

**Site.Master**
- Contains common layout elements (header, navigation, footer)
- Includes CSS and JavaScript references
- Provides content placeholders for page-specific content
- Handles user authentication state display
- Maintains responsive design and existing styling

**AdminMaster.Master**
- Inherits from Site.Master or provides separate admin layout
- Contains admin-specific navigation and styling
- Includes admin authorization checks
- Provides admin-specific content placeholders

### Page Conversion Mapping

Each MVC controller/action combination will be converted to WebForms pages:

| MVC Controller/Action | WebForms Page | Key Functionality |
|----------------------|---------------|-------------------|
| Home/Index | Default.aspx | Display best-selling books |
| Home/Privacy | Privacy.aspx | Privacy policy display |
| Search/Index | Search.aspx | Book search with filters |
| Search/Details | BookDetails.aspx | Book detail view |
| ShoppingCart/Index | ShoppingCart.aspx | Cart management |
| Checkout/Index | Checkout.aspx | Checkout process |
| Orders/Index | Orders.aspx | Order history |
| Wishlist/Index | Wishlist.aspx | Wishlist management |
| Address/Index | Address.aspx | Address management |
| Resale/Index | Resale.aspx | Book resale functionality |
| Authentication/Login | Login.aspx | User authentication |
| Admin/Dashboard | Admin/Dashboard.aspx | Admin dashboard |
| Admin/Inventory | Admin/Inventory.aspx | Inventory management |
| Admin/Orders | Admin/Orders.aspx | Order management |

### User Control Design

Common UI components will be implemented as User Controls:

**BookListControl.ascx**
- Displays lists of books with consistent formatting
- Supports different display modes (grid, list)
- Handles book selection and actions

**PaginationControl.ascx**
- Provides pagination functionality
- Maintains current page state
- Supports different page sizes

**SearchFilterControl.ascx**
- Contains search filters and criteria
- Maintains filter state across postbacks
- Provides filter reset functionality

**CartSummaryControl.ascx**
- Displays cart summary information
- Updates dynamically with cart changes
- Provides quick cart actions

### Code-behind Architecture

Each page will follow a consistent code-behind pattern:

```csharp
public partial class PageName : System.Web.UI.Page
{
    // Dependency injection properties
    public IServiceInterface Service { get; set; }
    
    // Page lifecycle events
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            InitializePage();
        }
    }
    
    // Initialization methods
    private void InitializePage()
    {
        LoadData();
        BindControls();
    }
    
    // Data loading methods
    private async void LoadData()
    {
        // Load data using injected services
    }
    
    // Control binding methods
    private void BindControls()
    {
        // Bind data to server controls
    }
    
    // Event handlers
    protected void ControlName_Event(object sender, EventArgs e)
    {
        // Handle control events
    }
}
```

## Data Models

### ViewModel Adaptation

Existing MVC ViewModels will be adapted for WebForms usage:

**Page-Level ViewModels**
- Convert MVC ViewModels to properties in code-behind
- Maintain data binding patterns using WebForms techniques
- Preserve validation logic using WebForms validation controls

**Data Binding Models**
- Adapt existing ViewModels for use with WebForms data-bound controls
- Implement IEnumerable patterns for list controls
- Maintain existing data transformation logic

### State Management

WebForms state management will be implemented using:

**ViewState**
- Store page-specific state information
- Maintain control state across postbacks
- Minimize ViewState usage for performance

**Session State**
- Store user-specific information (cart, preferences)
- Maintain authentication state
- Implement session timeout handling

**Application State**
- Store application-wide configuration
- Cache frequently accessed data
- Implement cache invalidation strategies

## Error Handling

### Global Error Handling

**Global.asax Error Handling**
```csharp
void Application_Error(object sender, EventArgs e)
{
    Exception exception = Server.GetLastError();
    // Log error using existing NLog infrastructure
    // Redirect to appropriate error page
}
```

**Custom Error Pages**
- Error.aspx for general errors
- NotFound.aspx for 404 errors
- Unauthorized.aspx for authorization errors
- Maintain existing error logging and AWS integration

### Page-Level Error Handling

Each page will implement consistent error handling:
- Try-catch blocks around data operations
- User-friendly error messages
- Logging integration with existing NLog setup
- Graceful degradation for service failures

## Testing Strategy

### Unit Testing Approach

**Code-behind Testing**
- Extract business logic into testable methods
- Mock dependencies using existing patterns
- Test page initialization and data binding logic
- Maintain existing service layer tests

**Integration Testing**
- Test page lifecycle and event handling
- Verify data binding and control interactions
- Test authentication and authorization flows
- Validate AWS service integrations

### Testing Framework Integration

**Existing Test Infrastructure**
- Maintain compatibility with existing test projects
- Preserve service layer and data layer tests
- Adapt controller tests to page tests where applicable

**WebForms-Specific Testing**
- Test master page functionality
- Verify user control behavior
- Test postback and event handling
- Validate state management

### Performance Testing

**Page Performance**
- Monitor page load times and ViewState size
- Test with existing data volumes
- Verify memory usage patterns
- Maintain existing performance benchmarks

**Scalability Testing**
- Test concurrent user scenarios
- Verify session state management
- Test with existing AWS infrastructure
- Maintain existing deployment performance

## Authentication and Authorization Integration

### WebForms Authentication Adaptation

**Forms Authentication**
- Adapt existing OWIN authentication middleware
- Implement WebForms-compatible authentication flow
- Maintain compatibility with AWS Cognito integration
- Preserve local authentication fallback

**Page-Level Authorization**
- Implement authorization checks in Page_Load
- Use existing authorization attributes where possible
- Create WebForms-compatible authorization patterns
- Maintain admin area access control

### Session Management

**Authentication State**
- Store authentication tokens in session
- Implement token refresh logic
- Handle authentication timeouts
- Maintain existing security patterns

## Dependency Injection Integration

### Autofac WebForms Integration

**Container Configuration**
- Adapt existing Autofac configuration for WebForms
- Register page dependencies appropriately
- Maintain existing service registrations
- Implement property injection for pages

**Page Dependency Resolution**
```csharp
public class BasePage : System.Web.UI.Page
{
    protected override void OnPreInit(EventArgs e)
    {
        base.OnPreInit(e);
        // Resolve dependencies using Autofac
        var container = // Get container from application
        container.InjectProperties(this);
    }
}
```

### Service Layer Integration

**Existing Services**
- Maintain all existing service interfaces
- Preserve service implementations
- Adapt service usage patterns for WebForms
- Maintain AWS service integrations

## Migration Strategy

### Phased Conversion Approach

**Phase 1: Infrastructure Setup**
- Configure WebForms project structure
- Set up master pages and basic navigation
- Implement dependency injection integration
- Create base page classes

**Phase 2: Core Pages**
- Convert home page and basic functionality
- Implement authentication pages
- Create common user controls
- Test basic user workflows

**Phase 3: Feature Pages**
- Convert shopping cart and checkout
- Implement search and book details
- Create user management pages
- Test complete user journeys

**Phase 4: Admin Area**
- Convert admin dashboard and navigation
- Implement inventory management
- Create order management pages
- Test admin workflows

**Phase 5: Integration and Testing**
- Complete AWS service integration testing
- Perform comprehensive user acceptance testing
- Optimize performance and resolve issues
- Prepare for deployment

### Rollback Strategy

**Parallel Development**
- Maintain existing MVC application during conversion
- Use feature flags to switch between implementations
- Implement gradual rollout capabilities
- Maintain database compatibility

**Testing and Validation**
- Comprehensive testing at each phase
- User acceptance testing with stakeholders
- Performance validation against existing benchmarks
- Security and compliance verification