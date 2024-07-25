import { Outlet, RouterProvider, createHashRouter } from "react-router-dom";
import { Home } from "../Home/Home.component";
import { MainAppLayout } from "../MainAppLayout/MainAppLayout.component";
import { Login } from "../Login/Login.component";
import { AuthProvider } from "../../ContextProviders/AuthProvider/AuthProvider";
import { ViewComponent } from "../View/View.component";
import { MenuProvider } from "../../ContextProviders/MenuProvider/MenuProvider";
import { ModalProvider } from "../../ContextProviders/ModalProvider/ModalProvider";
import { FormComponent } from "../Form/Form.component";
import { ActionProvider } from "../../ContextProviders/ActionProvider/ActionProvider";

// const ROUTES = [
//   { path: "/*", element: <Home /> },
//   { path: "/view/*", element: <ViewComponent /> },
//   { path: "/form/*", element: <FormComponent /> },
//   { path: "/login", element: <Login /> },
// ];

export const MainBody = () => {
  return (
    <AuthProvider>
      <ModalProvider>
        <ActionProvider>
          <MenuProvider>
            <MainAppLayout />
          </MenuProvider>
        </ActionProvider>
      </ModalProvider>
    </AuthProvider>
  );
};

// export const MainBody = () => {
//   const router = createHashRouter([
//     {
//       element: (
//         <AuthProvider>
//           <ModalProvider>
//             <ActionProvider>
//               <MenuProvider>
//                 <MainAppLayout content={<Outlet></Outlet>} />
//               </MenuProvider>
//             </ActionProvider>
//           </ModalProvider>
//         </AuthProvider>
//       ),
//       children: ROUTES,
//     },
//   ]);
//   return <RouterProvider router={router} />;
// };
