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
import { AuthContextProvider } from "./ContextProviders/AuthProvider/AuthContextProvider";
import { ApiContextProvider } from "./ContextProviders/ApiContextProvider/ApiContextProvider";
import { AppInfoContextProvider } from "./ContextProviders/AppInfoContextProvider/AppInfoContextProvider";

function App() {
  const [routes, setRoutes] = useState<RouteObject[]>([]);
  const [loadingRoutes, setLoadingRoutes] = useState(true);

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
      setLoadingRoutes(false);
    }, 10);
  }, []);

  const MainBody = ({ isReady }) => {
    if (isReady == true) {
      return <RouterProvider router={router} />;
    }

    return <h1>Loading...</h1>;
  };

  return (
    <AppInfoContextProvider>
      <AuthContextProvider>
        <ApiContextProvider>
          <MainBody isReady={!loadingRoutes} />
        </ApiContextProvider>
      </AuthContextProvider>
    </AppInfoContextProvider>
  );
}

export default App;
