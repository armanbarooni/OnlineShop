# Frontend Integration Guide

## Overview
This document describes the complete integration of the Persian frontend template with the ASP.NET Core backend API. The integration includes authentication, API communication, and static file serving.

## Architecture

### Frontend Structure
```
src/WebAPI/wwwroot/
├── fa/                     # Persian frontend files
│   ├── assets/
│   │   ├── css/
│   │   │   └── app.css    # Tailwind CSS compiled output
│   │   └── fonts/         # Persian fonts
│   ├── login.html         # Persian login page
│   └── dashboard.html     # User dashboard
├── en/                     # English frontend files (future)
└── shared/
    └── js/
        ├── config.js      # API configuration
        ├── auth.js        # Authentication manager
        └── api.js         # API client wrapper
```

### Backend Integration
- **Static File Serving**: ASP.NET Core serves static files from `wwwroot`
- **CORS Configuration**: Allows cross-origin requests from frontend
- **JWT Authentication**: Token-based authentication for API access
- **API Endpoints**: RESTful API for authentication and business logic

## Key Components

### 1. API Configuration (`config.js`)
```javascript
const API_CONFIG = {
    baseURL: 'http://localhost:5162',  // Auto-detects environment
    timeout: 30000,
    endpoints: {
        auth: {
            login: '/api/auth/login',
            register: '/api/auth/register',
            sendOtp: '/api/auth/send-otp',
            verifyOtp: '/api/auth/verify-otp'
        }
    }
};
```

### 2. Authentication Manager (`auth.js`)
- **Token Management**: Stores and refreshes JWT tokens
- **Login Methods**: Email/password and phone/OTP authentication
- **Session Handling**: Automatic token refresh and logout
- **User State**: Manages current user information

### 3. API Client (`api.js`)
- **HTTP Wrapper**: Handles all API requests
- **Authentication**: Automatically adds JWT tokens to requests
- **Error Handling**: Centralized error management
- **Interceptors**: Request/response processing

### 4. Login Page Integration
- **Form Validation**: Client-side validation for user input
- **API Integration**: Connects to backend authentication endpoints
- **User Experience**: Loading states, error messages, success feedback
- **Responsive Design**: Works on desktop and mobile devices

## Authentication Flow

### Email/Password Login
1. User enters email and password
2. Frontend validates input format
3. API call to `/api/auth/login`
4. Backend validates credentials
5. JWT token returned and stored
6. Redirect to dashboard

### Phone/OTP Login
1. User enters phone number
2. API call to `/api/auth/send-otp`
3. OTP sent via SMS
4. User enters OTP code
5. API call to `/api/auth/verify-otp`
6. JWT token returned and stored
7. Redirect to dashboard

## API Endpoints

### Authentication
- `POST /api/auth/login` - Email/password login
- `POST /api/auth/register` - User registration
- `POST /api/auth/send-otp` - Send OTP to phone
- `POST /api/auth/verify-otp` - Verify OTP code

### User Management
- `GET /api/user/profile` - Get user profile
- `PUT /api/user/profile` - Update user profile
- `GET /api/user/addresses` - Get user addresses

### Products
- `GET /api/products` - List products
- `GET /api/products/{id}` - Get product details
- `GET /api/products/search` - Search products

## Configuration

### Backend Configuration (`Program.cs`)
```csharp
// Serve static files from wwwroot
app.UseStaticFiles();

// Serve default files (index.html) for SPA routing
app.UseDefaultFiles();

// CORS configuration
app.UseCors("DefaultCors");
```

### Frontend Configuration
- **Environment Detection**: Automatically detects development/production
- **API Base URL**: Configurable for different environments
- **Timeout Settings**: Request timeout configuration
- **Error Handling**: Centralized error management

## Development Workflow

### 1. Backend Development
```bash
cd src/WebAPI
dotnet run
```

### 2. Frontend Development
```bash
cd front/FA
npm run dev  # Watch mode for CSS compilation
```

### 3. Testing
- **Unit Tests**: Backend API tests
- **Integration Tests**: End-to-end authentication tests
- **Frontend Testing**: Manual testing of login flows

## Deployment

### Production Configuration
1. **Environment Variables**: Set production API URLs
2. **Static File Serving**: Configure for production server
3. **CORS Settings**: Restrict to production domains
4. **SSL/HTTPS**: Enable secure connections

### Build Process
1. **Backend**: `dotnet publish` for production build
2. **Frontend**: Compile Tailwind CSS for production
3. **Static Files**: Copy to `wwwroot` directory

## Security Considerations

### Authentication
- **JWT Tokens**: Secure token-based authentication
- **Token Expiry**: Automatic token refresh
- **HTTPS**: Secure communication in production

### CORS
- **Origin Restrictions**: Limit allowed origins in production
- **Credentials**: Handle authentication cookies properly

### Input Validation
- **Client-side**: Immediate feedback for user experience
- **Server-side**: Backend validation for security

## Troubleshooting

### Common Issues
1. **CORS Errors**: Check CORS configuration in `Program.cs`
2. **Authentication Failures**: Verify JWT configuration
3. **Static File Issues**: Ensure `wwwroot` files are copied
4. **API Connection**: Check API base URL configuration

### Debug Tools
- **Browser DevTools**: Network tab for API calls
- **Console Logs**: Frontend debugging information
- **Backend Logs**: Serilog configuration for debugging

## Future Enhancements

### Planned Features
1. **English Frontend**: Complete English version
2. **Admin Panel**: Administrative interface
3. **Mobile App**: React Native or Flutter app
4. **PWA Support**: Progressive Web App features

### Technical Improvements
1. **State Management**: Redux or Vuex for complex state
2. **Component Library**: Reusable UI components
3. **Testing**: Automated frontend testing
4. **Performance**: Code splitting and optimization

## Support

### Documentation
- **API Documentation**: Swagger/OpenAPI documentation
- **Code Comments**: Inline documentation
- **README Files**: Project setup instructions

### Contact
- **Issues**: GitHub issues for bug reports
- **Discussions**: GitHub discussions for questions
- **Email**: Support email for urgent issues

---

**Last Updated**: January 2025
**Version**: 1.0.0
**Status**: Production Ready
