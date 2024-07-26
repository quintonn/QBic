import { MainAppLayout } from "../MainAppLayout/MainAppLayout.component";
import { AuthProvider } from "../../ContextProviders/AuthProvider/AuthProvider";
import { MenuProvider } from "../../ContextProviders/MenuProvider/MenuProvider";
import { ModalProvider } from "../../ContextProviders/ModalProvider/ModalProvider";
import { ActionProvider } from "../../ContextProviders/ActionProvider/ActionProvider";

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
