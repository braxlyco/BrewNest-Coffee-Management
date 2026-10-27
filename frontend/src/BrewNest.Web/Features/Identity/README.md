# Identity frontend feature

Contains the Blazor UI for the Identity module. Login, logout, current-user security, user management, role management, and permission assignment are implemented here.

- `Pages` contains authorized, thin Razor pages.
- `Services/IdentityApiClient` owns Identity HTTP calls and Problem Details parsing.
- `Services/JwtAuthorizationHandler` attaches access tokens and attempts one refresh when required.
- `State/IdentitySessionState` supplies Blazor authentication state.
- `State/IdentityTokenStore` persists the current browser-tab session.
- `Models` supplies frontend form validation and permission-name constants.

The API is always the authorization authority; hidden links and route policies are usability controls, not security boundaries. Keep feature-specific code in this folder and do not move Identity services into the global services folder.
