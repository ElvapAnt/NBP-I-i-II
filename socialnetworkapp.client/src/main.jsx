import React from 'react';
import ReactDOM from 'react-dom/client';
import './index.css';
import App from './App';
import {
  createBrowserRouter,
  RouterProvider,
} from "react-router-dom";
import Signup from './Pages/Login/Signup'
import Login from './Pages/Login/Login'
import Chat from './Pages/Chat/Chat';
import { ChatLoader } from './Pages/Chat/Chat';
import { Navbar } from './Components/Navbar/Navbar';
import { HomeLoader } from './Pages/Home/Home';
import Home from './Pages/Home/Home';
import Profile from './Pages/Profile/Profile';
import { ProfileLoader } from './Pages/Profile/Profile';
import AddPost from './Pages/AddPost/AddPost';
import { UserContextProvider } from './UserContext';
import PostLikes from './Pages/PostLikes/PostLikes';
import { PostLikesLoader } from './Pages/PostLikes/PostLikes';
import PostComments, { PostCommentsLoader } from './Pages/PostComments/PostComments';
import Search, { SearchLoader } from './Pages/Search/Search';
const root = ReactDOM.createRoot(document.getElementById('root'))
const router = createBrowserRouter([
  {
    path: '/',
    element:<Navbar/>,
    children:[{
      path: "/signup",
      element:<Signup/>
    },
    {
      path: '/login',
      element:<Login/>
    },
    {
      path: '/chat/:chatId?',
      element: <Chat />,
      loader:ChatLoader
      },
      {
        path: '/home',
        element: <Home />,
        loader:HomeLoader
      },
      {
        path: 'profile/:userId',
        element: <Profile />,
        loader: ProfileLoader
      },
      {
        path: '/add_post',
        element:<AddPost/>
      },
      {
        path: '/post/:postId/likes',
        element: <PostLikes />,
        loader: PostLikesLoader
      },
      {
        path: '/post/:postId/comments',
        element: <PostComments/>,
        loader: PostCommentsLoader
      },
      {
        path: '/search/:usernamePattern',
        element: <Search />,
        loader: SearchLoader
    }]
  }
  
])
root.render(
  <UserContextProvider>
    <RouterProvider router={router}/>
  </UserContextProvider>
);


