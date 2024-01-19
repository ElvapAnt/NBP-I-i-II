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
const root = ReactDOM.createRoot(document.getElementById('root'))
const router = createBrowserRouter([
  {
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
  }
])
root.render(
    <RouterProvider router={router}/>
);


