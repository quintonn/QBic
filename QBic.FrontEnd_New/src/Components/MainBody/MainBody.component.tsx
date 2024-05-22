import { useEffect, useState } from "react";
import {
  Outlet,
  RouteObject,
  RouterProvider,
  createBrowserRouter,
} from "react-router-dom";
import { Home } from "../Home/Home.component";
import { MainAppLayout } from "../MainAppLayout/MainAppLayout.component";

export const MainBody = () => {
  const [routes, setRoutes] = useState<RouteObject[]>([]);
  const [loadingRoutes, setLoadingRoutes] = useState(true);

  routes.push({ path: "/*", element: <Home /> });

  useEffect(() => {
    //TODO: here we can fetch the user's menu
    setTimeout(() => {
      setRoutes([...routes, { path: "/test", element: <h1>Test</h1> }]);
      setLoadingRoutes(false);
    }, 10);
  }, []);

  const router = createBrowserRouter([
    {
      element: <MainAppLayout content={<Outlet></Outlet>} />,
      children: routes,
    },
  ]);

  if (!loadingRoutes) {
    return <RouterProvider router={router} />;
  }

  return <h1>Loading...</h1>;
};
