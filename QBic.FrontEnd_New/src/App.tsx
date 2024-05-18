import "@cloudscape-design/global-styles/index.css";

import { useEffect, useState } from "react";
import {
  Outlet,
  RouteObject,
  RouterProvider,
  createBrowserRouter,
} from "react-router-dom";
import { Home } from "./Components/Home/Home.component";
import { MainAppLayout } from "./Components/MainAppLayout/MainAppLayout.component";

function App() {
  const [routes, setRoutes] = useState<RouteObject[]>([]);
  const [loadingRoutes, setLoadingRoutes] = useState(false);

  routes.push({ path: "/*", element: <Home /> });

  const router = createBrowserRouter([
    {
      element: <MainAppLayout content={<Outlet></Outlet>} />,
      children: routes,
    },
  ]);

  useEffect(() => {
    //TODO: here we can fetch the user's menu
    setTimeout(() => {
      setRoutes([...routes, { path: "/test", element: <h1>Test</h1> }]);
      setLoadingRoutes(true);
    }, 10);
  }, []);

  const MainBody = ({ isReady }) => {
    if (isReady == true) {
      return <RouterProvider router={router} />;
    }

    return <h1>Loading...</h1>;
  };

  return <MainBody isReady={loadingRoutes} />;
}

export default App;
