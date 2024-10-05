import { MainAppLayout } from "../MainAppLayout/MainAppLayout.component";
import { AuthProvider as QbicAuthProvider } from "../../ContextProviders/AuthProvider/AuthProvider";
import { MenuProvider } from "../../ContextProviders/MenuProvider/MenuProvider";
import { ModalProvider } from "../../ContextProviders/ModalProvider/ModalProvider";
import { ActionProvider } from "../../ContextProviders/ActionProvider/ActionProvider";
import { AuthProvider } from "react-oidc-context";
import { useMainApp } from "../../ContextProviders/MainAppProvider/MainAppProvider";
import { Spinner } from "@cloudscape-design/components";

export const MainBody = () => {
  const { authConfig, isReady } = useMainApp();

  if (!isReady || !authConfig) {
    return <Spinner />;
  }

  const oidcConfig = {
    authority: authConfig.Config.Authority,
    client_id: authConfig.Config.ClientId,
    redirect_uri: authConfig.Config.RedirectUrl,
    scope: authConfig.Config.Scope,
    response_type: authConfig.Config.ResponseType,
  };

  return (
    <AuthProvider {...oidcConfig}>
      <QbicAuthProvider>
        <ModalProvider>
          <ActionProvider>
            <MenuProvider>
              <MainAppLayout />
            </MenuProvider>
          </ActionProvider>
        </ModalProvider>
      </QbicAuthProvider>
    </AuthProvider>
  );
};
