import { useEffect, useState } from "react";
import {
  Outlet,
  RouteObject,
  RouterProvider,
  createBrowserRouter,
} from "react-router-dom";
import { Home } from "../Home/Home.component";
import { MainAppLayout } from "../MainAppLayout/MainAppLayout.component";
import { Login } from "../Login/Login.component";
import { AuthProvider } from "../../ContextProviders/AuthProvider/AuthProvider";
import { ViewComponent } from "../View/View.component";
import { MenuProvider } from "../../ContextProviders/MenuProvider/MenuProvider";
import { ModalProvider } from "../../ContextProviders/ModalProvider/ModalProvider";
import { store } from "../../App/store";
import { Provider } from "react-redux";
import { FormComponent } from "../Form/Form.component";
import { ActionProvider } from "../../ContextProviders/ActionProvider/ActionProvider";

export const MainBody = () => {
  const [routes, setRoutes] = useState<RouteObject[]>([]);
  const [loadingRoutes, setLoadingRoutes] = useState(true);

  routes.push({ path: "/*", element: <Home /> });
  routes.push({ path: "/view/*", element: <ViewComponent /> });
  routes.push({ path: "/form/*", element: <FormComponent /> });
  routes.push({ path: "/login", element: <Login /> });

  useEffect(() => {
    //TODO: here we can fetch the user's menu
    setTimeout(() => {
      setRoutes([...routes, { path: "/test", element: <h1>Test</h1> }]);
      setLoadingRoutes(false);
    }, 10);
  }, []);

  const router = createBrowserRouter([
    {
      element: (
        <AuthProvider>
          <ModalProvider>
            <ActionProvider>
              <MenuProvider>
                <Provider store={store}>
                  <MainAppLayout content={<Outlet></Outlet>} />
                </Provider>
              </MenuProvider>
            </ActionProvider>
          </ModalProvider>
        </AuthProvider>
      ),
      children: routes,
    },
  ]);

  if (!loadingRoutes) {
    return <RouterProvider router={router} />;
  }

  return <h1>Loading...</h1>;
};
