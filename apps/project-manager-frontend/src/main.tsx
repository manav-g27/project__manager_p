import React from "react";
import ReactDOM from "react-dom/client";
import { createBrowserRouter, RouterProvider } from "react-router-dom";
import Login from "./pages/Login";
import Dashboard from "./pages/Dashboard";
import Project from "./pages/Project";
import "./styles.css";

const router = createBrowserRouter([
  { path: "/", element: <Login/> },
  { path: "/dash", element: <Dashboard/> },
  { path: "/p/:id", element: <Project/> }
]);
ReactDOM.createRoot(document.getElementById("root")!).render(<RouterProvider router={router}/>);
