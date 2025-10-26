# Jerrygram Frontend

React + TypeScript frontend for Jerrygram, an Instagram + Twitter inspired social media platform.

## Tech Stack

- **React 18** with TypeScript
- **Tailwind CSS** for styling
- **React Router** for navigation
- **Axios** for API calls
- **date-fns** for date formatting
- **react-icons** for icons

## Features Implemented

### Authentication
- User login and registration
- JWT token management
- Protected and public routes
- Auto-redirect based on auth status
- Persistent authentication state

### Feed & Posts
- Home feed with posts from followed users
- Explore page with AI-recommended posts
- Like/unlike functionality
- Post cards with user info and timestamps
- Post creation with image upload
- Post detail view with comments
- Delete own posts
- Real-time like counter

### User Profiles
- View user profiles with stats (posts, followers, following)
- Follow/unfollow users
- Edit own profile
- Grid view of user posts
- Profile picture display

### Comments
- View comments on posts
- Add comments
- Delete own comments
- Real-time comment updates
- Comment timestamps

### Search & Discovery
- Full-text search for users, posts, and hashtags
- Autocomplete suggestions
- Trending searches with rankings
- Popular searches
- Hashtag support
- Real-time search results

### Notifications
- View all notifications (likes, comments, follows)
- Mark notifications as read
- Real-time notification timestamps
- Notification icons by type
- Unread indicator

### Layout & UX
- Responsive header with navigation
- Mobile-friendly hamburger menu
- Instagram-like UI design
- Loading states
- Error handling
- Smooth transitions

## Getting Started

### Prerequisites
- Node.js 16+ installed
- Backend API running on `http://localhost:8080`

### Installation

1. Install dependencies:
```bash
npm install
```

2. Create environment file:
```bash
cp .env.example .env
```

3. Update `.env` with your API URL if different:
```
REACT_APP_API_URL=http://localhost:8080/api
```

### Running the App

Start the development server:
```bash
npm start
```

The app will open at `http://localhost:3000`

### Available Scripts

- `npm start` - Run development server
- `npm build` - Build for production
- `npm test` - Run tests
- `npm run eject` - Eject from Create React App (one-way operation)

## Project Structure

```
src/
├── components/          # Reusable components
│   ├── layout/         # Layout components (Header, Layout)
│   └── post/           # Post-related components (PostCard, CommentSection)
├── contexts/           # React contexts (AuthContext)
├── pages/              # Page components
│   ├── LoginPage.tsx
│   ├── RegisterPage.tsx
│   ├── HomePage.tsx
│   ├── ExplorePage.tsx
│   ├── ProfilePage.tsx
│   ├── CreatePostPage.tsx
│   ├── PostDetailPage.tsx
│   ├── SearchPage.tsx
│   └── NotificationsPage.tsx
├── services/           # API service layers
│   ├── api.ts                  # Axios instance
│   ├── authService.ts          # Authentication
│   ├── postService.ts          # Posts & feed
│   ├── userService.ts          # User profiles
│   ├── searchService.ts        # Search & trending
│   ├── commentService.ts       # Comments
│   └── notificationService.ts  # Notifications
├── types/              # TypeScript type definitions
├── App.tsx             # Main app with routing
└── index.tsx           # Entry point
```

## Key Components

### Authentication Flow
1. Login/Register pages for unauthenticated users
2. JWT token stored in localStorage
3. Axios interceptor adds token to all requests
4. Auto-redirect to login on 401 responses
5. AuthContext provides user state globally

### API Services
- Centralized API client with interceptors
- Service layers for different domains
- Type-safe request/response handling
- Automatic token refresh on 401

### Routing
All routes are protected except login/register:
- `/` - Home feed
- `/explore` - Explore posts
- `/create` - Create new post
- `/search` - Search page
- `/notifications` - Notifications
- `/p/:postId` - Post detail
- `/:username` - User profile

## Features To Be Added

- Direct messaging
- Stories
- Post editing
- Video uploads
- Multiple image uploads
- Story highlights
- Saved posts functionality
- Report/block users
- Account settings page
- Email verification
- Password reset

## Styling Guidelines

This project uses Tailwind CSS with Instagram + Twitter inspired design:
- Primary color: `#0095f6` (Instagram blue)
- Border color: `#dbdbdb`
- Hover color: `#f5f5f5`
- Background: `#fafafa`

## API Integration

The frontend connects to the Jerrygram backend API. Ensure the backend is running before starting the frontend.

Default API URL: `http://localhost:8080/api`

### Available Endpoints

- **Auth**: `/api/auth/login`, `/api/auth/register`
- **Posts**: `/api/posts`, `/api/posts/feed`, `/api/explore`
- **Users**: `/api/users/me`, `/api/users/{username}`
- **Search**: `/api/search`, `/api/search/autocomplete`, `/api/search/popular`
- **Comments**: `/api/posts/{postId}/comments`
- **Notifications**: `/api/notifications`

## Contributing

When adding new features:
1. Create service methods in appropriate service file
2. Define TypeScript types in `types/index.ts`
3. Create reusable components in `components/`
4. Add new pages in `pages/`
5. Update routing in `App.tsx`
6. Follow existing code patterns and naming conventions

## Deployment

### Build for production:
```bash
npm run build
```

The build folder will contain optimized production files ready for deployment.

### Deploy to:
- **Vercel**: `vercel deploy`
- **Netlify**: Connect GitHub repository
- **Azure Static Web Apps**: Use Azure DevOps pipeline
- **AWS S3 + CloudFront**: Upload build folder

## License

MIT
